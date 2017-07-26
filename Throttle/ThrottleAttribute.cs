using System;

namespace Throttle
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ThrottleAttribute : Attribute
    {
        public ThrottleAttribute()
        {
        }

        public ThrottleAttribute(Type implementation)
        {
        }

        public ThrottleAttribute(string methodName)
        {
        }

        public ThrottleAttribute(int timeout)
        {
        }

        public ThrottleAttribute(Type implementation, int timeout)
        {
        }

        public ThrottleAttribute(Type implementation, string methodName)
        {
        }

        public ThrottleAttribute(int timeout, string methodName)
        {
        }

        public ThrottleAttribute(Type implementation, int timeout, string methodName)
        {
        }
    }
}
