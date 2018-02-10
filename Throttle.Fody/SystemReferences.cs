using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Mono.Cecil;

namespace Throttle.Fody
{
    public class SystemReferences
    {
        public MethodReference ActionConstructorReference { get; }

        public MethodReference TimeSpanFromMilisecondsReference { get; }

        public MethodReference GenericCompareExchangeMethod { get; }

        public SystemReferences([NotNull] ModuleDefinition moduleDefinition, [NotNull] IAssemblyResolver assemblyResolver)
        {
            var coreTypes = new CoreTypes(moduleDefinition, assemblyResolver);

            ActionConstructorReference = coreTypes.GetMethod<Action, object, IntPtr>(".ctor");
            TimeSpanFromMilisecondsReference = coreTypes.GetMethod<TimeSpan, double>(nameof(TimeSpan.FromMilliseconds));

            var interlockedDefinition = coreTypes.GetTypeDefinition(typeof(System.Threading.Interlocked));
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

        class CoreTypes
        {
            [NotNull, ItemNotNull]
            private readonly TypeDefinition[] _types;
            [NotNull]
            private readonly ModuleDefinition _moduleDefinition;

            public CoreTypes([NotNull] ModuleDefinition moduleDefinition, [NotNull] IAssemblyResolver assemblyResolver)
            {
                _moduleDefinition = moduleDefinition;
                var assemblies = new[] { "mscorlib", "System.Threading", "System.Runtime", "netstandard" };
                _types = assemblies.SelectMany(assembly => GetTypes(assemblyResolver, assembly)).ToArray();
            }

            [NotNull]
            public TypeDefinition GetTypeDefinition([NotNull] Type type)
            {
                return _types.FirstOrDefault(x => x.FullName == type.FullName) ?? throw new InvalidOperationException($"Type {type} not found");
            }

            [NotNull]
            public TypeDefinition GetTypeDefinition<T>()
            {
                return GetTypeDefinition(typeof(T));
            }

            [NotNull]
            public TypeReference GetType(Type type)
            {
                return _moduleDefinition.ImportReference(GetTypeDefinition(type));
            }

            [NotNull]
            public TypeReference GetType<T>()
            {
                return _moduleDefinition.ImportReference(GetTypeDefinition<T>());
            }

            [NotNull]
            public MethodReference GetMethod<T>([NotNull] string name, [NotNull, ItemNotNull] params Type[] parameters)
            {
                return _moduleDefinition.ImportReference(GetTypeDefinition<T>().FindMethod(name, parameters));
            }

            [NotNull]
            public MethodReference GetMethod<T, TP1>([NotNull] string name)
            {
                return GetMethod<T>(name, typeof(TP1));
            }

            [NotNull]
            public MethodReference GetMethod<T, TP1, TP2>([NotNull] string name)
            {
                return GetMethod<T>(name, typeof(TP1), typeof(TP2));
            }

            [NotNull]
            public MethodReference GetMethod<T, TP1, TP2, TP3>([NotNull] string name)
            {
                return GetMethod<T>(name, typeof(TP1), typeof(TP2), typeof(TP3));
            }
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<TypeDefinition> GetTypes([NotNull] IAssemblyResolver assemblyResolver, [NotNull] string name)
        {
            return assemblyResolver.Resolve(new AssemblyNameReference(name, null))?.MainModule.Types ?? Enumerable.Empty<TypeDefinition>();
        }
    }
}