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

            var arg = default(Type);
            GenericCompareExchangeMethod = weaver.ImportMethod(() => System.Threading.Interlocked.CompareExchange(ref arg, arg, arg));
        }
    }
}