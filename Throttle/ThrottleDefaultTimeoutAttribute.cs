using System;

namespace Throttle
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ThrottleDefaultTimeoutAttribute : Attribute
    {
        public ThrottleDefaultTimeoutAttribute(int timeout)
        {
        }
    }
}
