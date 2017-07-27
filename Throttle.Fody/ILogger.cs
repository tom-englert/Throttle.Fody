﻿using Mono.Cecil.Cil;

namespace Throttle.Fody
{
    interface ILogger
    {
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, SequencePoint sequencePoint = null);
    }
}
