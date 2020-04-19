namespace Tests
{
    using System.Collections.Generic;
    using System.Reflection;

    using Fody;

    using Mono.Cecil;

    using Throttle.Fody;

    using TomsToolbox.Core;

    internal class WeaverHelper : DefaultAssemblyResolver
    {
        private static readonly Dictionary<string, WeaverHelper> _cache = new Dictionary<string, WeaverHelper>();

        private readonly TestResult _testResult;

        public Assembly Assembly => _testResult.Assembly;

        public static WeaverHelper Create(string assemblyName = "AssemblyToProcess")
        {
            lock (typeof(WeaverHelper))
            {
                return _cache.ForceValue(assemblyName, _ => new WeaverHelper(assemblyName));
            }
        }

        private WeaverHelper(string assemblyName)
        {
            _testResult = new ModuleWeaver().ExecuteTestRun(assemblyName + ".dll", true, null, null, null, new[] { "0x80131869" });
        }
    }
}
