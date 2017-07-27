using System;

namespace Throttle
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ThrottleDefaultThresholdAttribute : Attribute
    {
        public ThrottleDefaultThresholdAttribute(int threshold)
        {
        }
    }
}
