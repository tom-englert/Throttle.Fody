namespace Tests
{
    using System.Collections.Generic;
    using System.Reflection;

    using Fody;

    using JetBrains.Annotations;

    using Mono.Cecil;

    using Throttle.Fody;

    using TomsToolbox.Core;

    internal class WeaverHelper : DefaultAssemblyResolver
    {
        [NotNull]
        private static readonly Dictionary<string, WeaverHelper> _cache = new Dictionary<string, WeaverHelper>();

        [NotNull]
        private readonly TestResult _testResult;

        [NotNull]
        public Assembly Assembly => _testResult.Assembly;

        [NotNull]
        public static WeaverHelper Create([NotNull] string assemblyName = "AssemblyToProcess")
        {
            lock (typeof(WeaverHelper))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return _cache.ForceValue(assemblyName, _ => new WeaverHelper(assemblyName));
            }
        }

        private WeaverHelper([NotNull] string assemblyName)
        {
            _testResult = new ModuleWeaver().ExecuteTestRun(assemblyName + ".dll", true, null, null, null, new[] { "0x80131869" });
        }
    }
}
