namespace Throttle.Fody
{
    using System;

    using FodyTools;

    using JetBrains.Annotations;

    using Mono.Cecil;

    internal class SystemReferences
    {
        public MethodReference ActionConstructorReference { get; }

        public MethodReference TimeSpanFromMillisecondsReference { get; }

        public MethodReference GenericCompareExchangeMethod { get; }

        public SystemReferences([NotNull] ITypeSystem weaver)
        {
            ActionConstructorReference = weaver.ImportMethod<Action, object, IntPtr>(".ctor");
            TimeSpanFromMillisecondsReference = weaver.ImportMethod(() => TimeSpan.FromMilliseconds(default));

            var arg = default(Type);
            GenericCompareExchangeMethod = weaver.ImportMethod(() => System.Threading.Interlocked.CompareExchange(ref arg, arg, arg));
        }
    }
}