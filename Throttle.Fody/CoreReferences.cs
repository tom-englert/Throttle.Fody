using System;
using System.Linq;

using Mono.Cecil;

namespace Throttle.Fody
{
    public class CoreReferences
    {
        public MethodReference ActionConstructorReference { get; }

        public MethodReference TimeSpanFromMilisecondsReference { get; }

        public MethodReference GenericCompareExchangeMethod { get; }

        public CoreReferences(ModuleDefinition moduleDefinition)
        {
            var assemblyResolver = moduleDefinition.AssemblyResolver;

            var msCoreLibDefinition = assemblyResolver.TryResolve("mscorlib");
            var systemDefinition = assemblyResolver.TryResolve("System");
            var systemRuntimeDefinition = assemblyResolver.TryResolve("System.Runtime");
            var systemThreadingDefinition = assemblyResolver.TryResolve("System.Threading");
            var systemCoreDefinition = moduleDefinition.AssemblyResolver.TryResolve("System.Core");

            var actionTypeDefinition = ResolveType(x => x.Name == nameof(Action), msCoreLibDefinition, systemDefinition, systemCoreDefinition, systemRuntimeDefinition);
            var actionConstructor = actionTypeDefinition.Methods.First(x => x.IsConstructor);
            ActionConstructorReference = moduleDefinition.ImportReference(actionConstructor);

            var timeSpanTypeDefinition = ResolveType(x => x.Name == nameof(TimeSpan), msCoreLibDefinition, systemDefinition, systemCoreDefinition, systemRuntimeDefinition);
            var timeSpanFromMilisecondsDefinition = timeSpanTypeDefinition.Methods.First(m => m.Name == nameof(TimeSpan.FromMilliseconds));
            TimeSpanFromMilisecondsReference = moduleDefinition.ImportReference(timeSpanFromMilisecondsDefinition);

            var interlockedDefinition = ResolveType(x => x.FullName == "System.Threading.Interlocked", msCoreLibDefinition, systemThreadingDefinition);
            var genericCompareExchangeMethodDefinition = interlockedDefinition
                .Methods.First(x =>
                    x.IsStatic &&
                    x.Name == "CompareExchange" &&
                    x.GenericParameters.Count == 1 &&
                    x.Parameters.Count == 3);

            GenericCompareExchangeMethod = moduleDefinition.ImportReference(genericCompareExchangeMethodDefinition);
        }

        TypeDefinition ResolveType(Func<TypeDefinition, bool> predicate, params AssemblyDefinition[] assemblies)
        {
            return assemblies
                .Where(asm => asm != null).
                SelectMany(a => a.MainModule.Types)
                .FirstOrDefault(predicate);
        }
    }
}