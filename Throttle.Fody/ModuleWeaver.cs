namespace Throttle.Fody
{
    using System.Collections.Generic;

    using FodyTools;

    public class ModuleWeaver : AbstractModuleWeaver
    {
        public override void Execute()
        {
            ModuleDefinition.Process(this, new SystemReferences(this));
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "System.Threading";
        }

        public override bool ShouldCleanReference => true;
    }
}