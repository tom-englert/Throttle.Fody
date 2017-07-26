using System;

namespace Throttle
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ThrottleDefaultImplementationAttribute : Attribute
    {
        public ThrottleDefaultImplementationAttribute(Type implementation)
        {
        }

        public ThrottleDefaultImplementationAttribute(Type implementation, string methodName)
        {
        }
    }
}
