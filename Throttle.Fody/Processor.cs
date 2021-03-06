﻿namespace Throttle.Fody
{
    using System.Collections.Generic;
    using System.Linq;

    using FodyTools;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    internal static class Processor
    {
        internal static void Process(this ModuleDefinition moduleDefinition, ILogger logger, SystemReferences coreReferences)
        {
            var throttleParameters = new ThrottleParameters();

            throttleParameters.ConsumeDefaultAttributes(moduleDefinition.Assembly);

            var allTypes = moduleDefinition.GetTypes();

            var allClasses = allTypes
                .Where(x => x.IsClass && (x.BaseType != null))
                .ToArray();

            var weavedMethods = new Dictionary<MethodDefinition, MethodDefinition>();

            foreach (var classDefinition in allClasses)
            {
                ProcessClass(classDefinition, throttleParameters, moduleDefinition.SymbolReader, coreReferences, logger, weavedMethods);
            }

            var injectedMethods = new HashSet<MethodDefinition>(weavedMethods.Values);

            foreach (var classDefinition in allClasses)
            {
                PostProcessClass(classDefinition, logger, weavedMethods, injectedMethods);
            }
        }

        private static void PostProcessClass(TypeDefinition classDefinition, ILogger logger, IReadOnlyDictionary<MethodDefinition, MethodDefinition> weavedMethods, ICollection<MethodDefinition> injectedMethods)
        {
            var allMethods = classDefinition.Methods
                .Where(method => method.HasBody)
                .ToArray();

            foreach (var method in allMethods)
            {
                PostProcessMethod(method, logger, weavedMethods, injectedMethods);
            }
        }

        private static void PostProcessMethod(MethodDefinition method, ILogger logger, IReadOnlyDictionary<MethodDefinition, MethodDefinition> weavedMethods, ICollection<MethodDefinition> injectedMethods)
        {
            if (injectedMethods.Contains(method))
                return;

            var instructions = method.Body.Instructions;

            for (var index = 0; index < instructions.Count; index++)
            {
                var instruction = instructions[index];

                if ((instruction.OpCode != OpCodes.Call) && (instruction.OpCode != OpCodes.Callvirt))
                    continue;

                if (!(instruction.Operand is MethodDefinition calledMethod))
                    continue;

                if (!weavedMethods.TryGetValue(calledMethod, out var newMethod))
                    continue;

                logger.LogDebug($"{method.FullName}@{index}: Replace call of {calledMethod} with {newMethod}");

                instructions[index] = Instruction.Create(instruction.OpCode, newMethod);
            }
        }

        private static void ProcessClass(TypeDefinition classDefinition, ThrottleParameters throttleParameters, ISymbolReader symbolReader, SystemReferences systemReferences, ILogger logger, IDictionary<MethodDefinition, MethodDefinition> weavedMethods)
        {
            throttleParameters.ConsumeDefaultAttributes(classDefinition);

            var allMethods = classDefinition.Methods
                .Where(method => method.HasBody)
                .ToArray();

            foreach (var method in allMethods)
            {
                ProcessMethod(method, throttleParameters, symbolReader, systemReferences, logger, weavedMethods);
            }
        }

        private static void ProcessMethod(MethodDefinition method, ThrottleParameters throttleParameters, ISymbolReader symbolReader, SystemReferences systemReferences, ILogger logger, IDictionary<MethodDefinition, MethodDefinition> weavedMethods)
        {
            if (!throttleParameters.ConsumeAttribute(method, "Throttle.ThrottledAttribute"))
                return;

            if (method.Parameters.Any() || method.ReturnType.FullName != "System.Void")
            {
                logger.LogError($"Can't weave throttle into method {method}: It does not have the signature 'void {method.Name}()'.", method.GetEntryPoint(symbolReader));
                return;
            }

            if (throttleParameters.Implementation == null)
            {
                logger.LogError($"Can't weave method {method.FullName} - no throttle implementation is defined.", method.GetEntryPoint(symbolReader));
                return;
            }

            InjectThrottleMethod(method, throttleParameters, symbolReader, systemReferences, logger, weavedMethods);
        }

        private static void InjectThrottleMethod(MethodDefinition method, ThrottleParameters throttleParameters, ISymbolReader symbolReader, SystemReferences systemReferences, ILogger logger, IDictionary<MethodDefinition, MethodDefinition> weavedMethods)
        {
            var classDefinition = method.DeclaringType;
            var moduleDefinition = method.Module;

            var implementationTypeReference = throttleParameters
                .Implementation
                .Import(moduleDefinition);

            if (implementationTypeReference == null)
            {
                logger.LogError($"The type {throttleParameters.Implementation} could not be imported.", method.GetEntryPoint(symbolReader));
                return;
            }

            var implementationTypeDefinition = implementationTypeReference.Resolve();

            var tickMethodName = throttleParameters.MethodName ?? "Tick";

            var tickMethodReference = implementationTypeDefinition
                .Methods
                .FirstOrDefault(m => m.IsPublic && m.Name == tickMethodName && !m.Parameters.Any() && m.ReturnType.FullName == "System.Void")
                .Import(moduleDefinition);

            if (tickMethodReference == null)
            {
                logger.LogError($"The type {implementationTypeDefinition} does not have a public method 'void {tickMethodName}()'.", method.GetEntryPoint(symbolReader));
                return;
            }

            var threshold = throttleParameters.Threshold.GetValueOrDefault();
            var requiredParameterCount = ((threshold > 0) ? 2 : 1);

            var implementationConstructor = implementationTypeDefinition.GetConstructors()
                .FirstOrDefault(c => c.Parameters.Count == requiredParameterCount)
                .Import(moduleDefinition);

            if (implementationConstructor == null)
            {
                logger.LogError($"{implementationTypeDefinition} does not have a constructor with {requiredParameterCount} parameters.", method.GetEntryPoint(symbolReader));
                return;
            }

            var delegateParameter = implementationConstructor.Parameters.FirstOrDefault(p => p.ParameterType.IsActionOrDelegate());
            if (delegateParameter == null)
            {
                logger.LogError($"{implementationConstructor} does not have a parameter of type System.Action or System.Delegate.", method.GetEntryPoint(symbolReader));
                return;
            }

            var thresholdParameter = implementationConstructor.Parameters.FirstOrDefault(p => p.ParameterType.IsIntOrTimeSpan());
            var thresholdParameterIndex = implementationConstructor.Parameters.IndexOf(thresholdParameter);

            if (requiredParameterCount == 2 && thresholdParameter == null)
            {
                logger.LogError($"{implementationConstructor} does not have a parameter of type System.Int32, System.Enum or System.TimeSpan to assign the threshold to.", method.GetEntryPoint(symbolReader));
                return;
            }

            logger.LogInfo($"Weave throttle {implementationTypeDefinition.FullName} into {method.FullName}");

            var compareExchangeMethod = systemReferences.GenericCompareExchangeMethod.MakeGeneric(implementationTypeReference);

            var originalMethodName = method.Name;

            var throttleField = new FieldDefinition($"<{originalMethodName}>" + "_Throttle_Fody_BackingField", FieldAttributes.Private, implementationTypeReference);

            classDefinition.Fields.Add(throttleField);

            var throttledMethodName = $"<{originalMethodName}>_Throttle_Fody_Method";

            var newMethod = new MethodDefinition(originalMethodName, method.Attributes, method.ReturnType)
            {
                Body =
                {
                    MaxStackSize = 3,
                    InitLocals = true
                }
            };

            newMethod.Body.Variables.AddRange(new VariableDefinition(implementationTypeReference), new VariableDefinition(implementationTypeReference));

            var jumpTarget = Instruction.Create(OpCodes.Ldloc_0);

            var instructions = newMethod.Body.Instructions;

            instructions.AddRange(Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, throttleField),
                Instruction.Create(OpCodes.Stloc_0),
                Instruction.Create(OpCodes.Ldloc_0),
                Instruction.Create(OpCodes.Brtrue_S, jumpTarget));

            if (thresholdParameterIndex == 0)
            {
                AddThresholdParameter(instructions, threshold, thresholdParameter?.ParameterType, systemReferences);
            }

            instructions.AddRange(Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldftn, method),
                Instruction.Create(OpCodes.Newobj, systemReferences.ActionConstructorReference));

            if (thresholdParameterIndex == 1)
                AddThresholdParameter(instructions, threshold, thresholdParameter?.ParameterType, systemReferences);

            instructions.AddRange(Instruction.Create(OpCodes.Newobj, implementationConstructor),
                Instruction.Create(OpCodes.Stloc_1),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldflda, throttleField),
                Instruction.Create(OpCodes.Ldloc_1),
                Instruction.Create(OpCodes.Ldnull),
                Instruction.Create(OpCodes.Call, compareExchangeMethod),
                Instruction.Create(OpCodes.Pop),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, throttleField),
                Instruction.Create(OpCodes.Stloc_0),
                jumpTarget, // Instruction.Create(OpCodes.Ldloc_0);
                Instruction.Create(OpCodes.Callvirt, tickMethodReference),
                Instruction.Create(OpCodes.Ret));

            method.Name = throttledMethodName;
            method.IsPrivate = true;

            classDefinition.Methods.Add(newMethod);

            weavedMethods[method] = newMethod;
        }

        private static void AddThresholdParameter(ICollection<Instruction> instructions, int threshold, MemberReference? parameterType, SystemReferences systemReferences)
        {
            if (parameterType == null)
                return;

            if (parameterType.FullName == typeof(System.TimeSpan).FullName)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_R8, (double) threshold));
                instructions.Add(Instruction.Create(OpCodes.Call, systemReferences.TimeSpanFromMillisecondsReference));
            }
            else
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, threshold));
            }
        }
    }
}

