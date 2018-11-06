using System;
using System.Linq;
using Fody;
using FodyTools;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Throttle.Fody
{
    public class SystemReferences
    {
        public MethodReference ActionConstructorReference { get; }

        public MethodReference TimeSpanFromMillisecondsReference { get; }

        public MethodReference GenericCompareExchangeMethod { get; }

        public SystemReferences([NotNull] BaseModuleWeaver weaver)
        {
            ActionConstructorReference = weaver.ImportMethod<Action, object, IntPtr>(".ctor");
            TimeSpanFromMillisecondsReference = weaver.ImportMethod(() => TimeSpan.FromMilliseconds(default(double)));

            var interlockedDefinition = weaver.FindType(typeof(System.Threading.Interlocked).FullName);
            var genericCompareExchangeMethodDefinition = interlockedDefinition
                .Methods.First(x =>
                    x.IsStatic &&
                    x.Name == "CompareExchange" &&
                    x.GenericParameters.Count == 1 &&
                    x.Parameters.Count == 3);

            GenericCompareExchangeMethod = weaver.ModuleDefinition.ImportReference(genericCompareExchangeMethodDefinition);
        }
    }
}