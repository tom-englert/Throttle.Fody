using System;
using System.Collections.Generic;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

using Throttle.Fody;

public class ModuleWeaver : BaseModuleWeaver, ILogger
{
    public override void Execute()
    {
        ModuleDefinition.Process(this);
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield break;
    }

    public override bool ShouldCleanReference => true;

    void ILogger.LogDebug(string message)
    {
        LogDebug(message);
    }

    void ILogger.LogInfo(string message)
    {
        LogInfo(message);
    }

    void ILogger.LogWarning(string message)
    {
        LogWarning(message);
    }

    void ILogger.LogError(string message, SequencePoint sequencePoint)
    {
        if (sequencePoint != null)
            LogErrorPoint(message, sequencePoint);
        else
            LogError(message);
    }
}