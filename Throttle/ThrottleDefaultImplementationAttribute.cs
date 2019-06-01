using System;
// ReSharper disable UnusedParameter.Local

namespace Throttle
{
    /// <summary>
    /// Allows you to specify a default for the throttle implementation on assembly or class level, 
    /// so you don't have to specify it in every <see cref="ThrottledAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ThrottleDefaultImplementationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleDefaultImplementationAttribute"/> class.
        /// </summary>
        /// <param name="implementation">The type of the throttle implementation.</param>
        public ThrottleDefaultImplementationAttribute(Type implementation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleDefaultImplementationAttribute"/> class.
        /// </summary>
        /// <param name="implementation">The type of the throttle implementation.</param>
        /// <param name="methodName">Name of the method to call on the implementation to trigger the throttle.</param>
        public ThrottleDefaultImplementationAttribute(Type implementation, string methodName)
        {
        }
    }
}
