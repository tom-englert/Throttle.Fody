﻿using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Throttle.Fody
{
    internal static class Processor
    {
        internal static void Process(this ModuleDefinition moduleDefinition, ILogger logger)
        {
            var coreReferences = new CoreReferences(moduleDefinition);

            var throttleParameters = new ThrottleParameters();
            throttleParameters.ReadDefaults(moduleDefinition.Assembly);

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

        private static void PostProcessClass(TypeDefinition classDefinition, ILogger logger, IReadOnlyDictionary<MethodDefinition, MethodDefinition> weavedMethods, HashSet<MethodDefinition> injectedMethods)
        {
            var allMethods = classDefinition.Methods
                .Where(method => method.HasBody)
                .ToArray();

            foreach (var method in allMethods)
            {
                PostProcessMethod(method, logger, weavedMethods, injectedMethods);
            }
        }

        private static void PostProcessMethod(MethodDefinition method, ILogger logger, IReadOnlyDictionary<MethodDefinition, MethodDefinition> weavedMethods, HashSet<MethodDefinition> injectedMethods)
        {
            if (injectedMethods.Contains(method))
                return;

            var instructions = method.Body.Instructions;

            for (var index = 0; index < instructions.Count; index++)
            {
                var instruction = instructions[index];

                if ((instruction.OpCode != OpCodes.Call) && (instruction.OpCode != OpCodes.Callvirt))
                    continue;

                var calledMethod = instruction.Operand as MethodDefinition;
                if (calledMethod == null)
                    continue;

                if (!weavedMethods.TryGetValue(calledMethod, out var newMethod))
                    continue;

                instructions[index] = Instruction.Create(instruction.OpCode, newMethod);
            }
        }

        private static void ProcessClass(TypeDefinition classDefinition, ThrottleParameters throttleParameters, ISymbolReader symbolReader, CoreReferences coreReferences, ILogger logger, IDictionary<MethodDefinition, MethodDefinition> weavedMethods)
        {
            throttleParameters.ReadDefaults(classDefinition);

            var allMethods = classDefinition.Methods
                .Where(method => method.HasBody)
                .ToArray();

            foreach (var method in allMethods)
            {
                ProcessMethod(method, throttleParameters, symbolReader, coreReferences, logger, weavedMethods);
            }
        }

        private static void ProcessMethod(MethodDefinition method, ThrottleParameters throttleParameters, ISymbolReader symbolReader, CoreReferences coreReferences, ILogger logger, IDictionary<MethodDefinition, MethodDefinition> weavedMethods)
        {
            var throttleAttribute = method
                .GetAttribute("Throttle.ThrottleAttribute");

            if (throttleAttribute == null)
                return;

            if (method.Parameters.Any() || method.ReturnType.FullName != "System.Void")
            {
                logger.LogError($"Can't weave throttle into method {method}: It does not have the signature 'void {method.Name}()'.", symbolReader.GetEntryPoint(method));
                return;
            }

            throttleParameters.ReadFromAttribute(throttleAttribute);
            method.CustomAttributes.Remove(throttleAttribute);

            if (throttleParameters.Implementation == null)
            {
                logger.LogError($"Can't weave method {method.FullName} - no throttle implementation is defined.", symbolReader.GetEntryPoint(method));
                return;
            }

            InjectThrottleMethod(method, throttleParameters, symbolReader, coreReferences, logger, weavedMethods);
        }

        private static void InjectThrottleMethod(MethodDefinition method, ThrottleParameters throttleParameters, ISymbolReader symbolReader, CoreReferences coreReferences, ILogger logger, IDictionary<MethodDefinition, MethodDefinition> weavedMethods)
        {
            var classDefinition = method.DeclaringType;
            var moduleDefinition = method.Module;

            var throttleImplementationTypeReference = throttleParameters.Implementation;
            var throttleImplementationTypeDefinition = throttleImplementationTypeReference.Resolve();
            throttleImplementationTypeReference = moduleDefinition.ImportReference(throttleImplementationTypeDefinition);

            var throttleTickMethodName = throttleParameters.MethodName ?? "Tick";

            var methodReference = throttleImplementationTypeDefinition.Methods.FirstOrDefault(m => m.IsPublic && m.Name == throttleTickMethodName && !m.Parameters.Any() && m.ReturnType.FullName == "System.Void");
            if (methodReference == null)
            {
                logger.LogError($"The type {throttleImplementationTypeDefinition} does not have a public method 'void {throttleTickMethodName}()'.", symbolReader.GetEntryPoint(method));
                return;
            }

            var throttleTickMethodReference = moduleDefinition.ImportReference(methodReference);
            var threshold = throttleParameters.Threshold.GetValueOrDefault();

            var requiredParameterCount = ((threshold > 0) ? 2 : 1);

            var throttleImplementationConstructor = moduleDefinition.ImportReference(throttleImplementationTypeDefinition.GetConstructors().FirstOrDefault(c => c.Parameters.Count == requiredParameterCount));
            if (throttleImplementationConstructor == null)
            {
                logger.LogError($"{throttleImplementationTypeDefinition} does not have a constructor with {requiredParameterCount} parameters.", symbolReader.GetEntryPoint(method));
                return;
            }

            var delegateParameter = throttleImplementationConstructor.Parameters.FirstOrDefault(p => p.ParameterType.IsActionOrDelegate());
            if (delegateParameter == null)
            {
                logger.LogError($"{throttleImplementationConstructor} does not have a parameter of type System.Action or System.Delegate.", symbolReader.GetEntryPoint(method));
                return;
            }

            var thresholdParamter = throttleImplementationConstructor.Parameters.FirstOrDefault(p => p.ParameterType.IsIntOrTimeSpan());
            var thresholdParameterIndex = throttleImplementationConstructor.Parameters.IndexOf(thresholdParamter);

            if (requiredParameterCount == 2 && thresholdParamter == null)
            {
                logger.LogError($"{throttleImplementationConstructor} does not have a parameter of type System.Int32 or System.TimeSpan to assign the threshold to.", symbolReader.GetEntryPoint(method));
                return;
            }

            logger.LogInfo($"Weave throttle {throttleImplementationTypeDefinition.FullName} into {method.FullName}");

            var compareExchangeMethod = coreReferences.GenericCompareExchangeMethod.MakeGeneric(throttleImplementationTypeReference);

            var originalMethodName = method.Name;

            var throttleField = new FieldDefinition($"<{originalMethodName}>" + "_Throttle_Fody_BackingField", FieldAttributes.Private, throttleImplementationTypeReference);

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

            newMethod.Body.Variables.AddRange(new VariableDefinition(throttleImplementationTypeReference), new VariableDefinition(throttleImplementationTypeReference));

            var jumpTarget = Instruction.Create(OpCodes.Ldloc_0);

            var instructions = newMethod.Body.Instructions;

            instructions.AddRange(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, throttleField),
                Instruction.Create(OpCodes.Stloc_0),
                Instruction.Create(OpCodes.Ldloc_0),
                Instruction.Create(OpCodes.Brtrue_S, jumpTarget));

            if (thresholdParameterIndex == 0)
                AddThresholdParameter(instructions, threshold, thresholdParamter.ParameterType, coreReferences);

            instructions.AddRange(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldftn, method),
                Instruction.Create(OpCodes.Newobj, coreReferences.ActionConstructorReference));

            if (thresholdParameterIndex == 1)
                AddThresholdParameter(instructions, threshold, thresholdParamter.ParameterType, coreReferences);

            instructions.AddRange(
                Instruction.Create(OpCodes.Newobj, throttleImplementationConstructor),
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
                Instruction.Create(OpCodes.Callvirt, throttleTickMethodReference),
                Instruction.Create(OpCodes.Ret));

            method.Name = throttledMethodName;
            method.IsPrivate = true;

            classDefinition.Methods.Add(newMethod);

            weavedMethods[method] = newMethod;
        }

        private static void AddThresholdParameter(Collection<Instruction> instructions, int threshold, TypeReference parameterType, CoreReferences coreReferences)
        {
            if (parameterType.FullName == "System.Int32")
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, threshold));
            }
            else if (parameterType.FullName == "System.TimeSpan")
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_R8, (double)threshold));
                instructions.Add(Instruction.Create(OpCodes.Call, coreReferences.TimeSpanFromMilisecondsReference));
            }
        }
    }
}

