using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Throttle.Fody;

public class ModuleWeaver : ILogger
{
    // Will log an informational message to MSBuild
    public Action<string> LogDebug { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public Action<string> LogError { get; set; }
    public Action<string, SequencePoint> LogErrorPoint { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    public ModuleWeaver()
    {
        // Initialize logging delegates to make testing easier
        LogDebug = LogInfo = LogWarning = LogError = _ => { };
        LogErrorPoint = (_, __) => { };
    }

    public void Execute()
    {
        ModuleDefinition.Process(this);
        ModuleDefinition.RemoveReferences(this);
    }

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