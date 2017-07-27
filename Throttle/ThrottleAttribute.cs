using System;
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global

namespace Throttle
{
    /// <summary>
    /// Apply this attribute to any method with the 'void Method()' signature to convert it into a throttled method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ThrottleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class
        /// with implementation, method and threshold taken from the <see cref="ThrottleDefaultImplementationAttribute"/> and the <see cref="ThrottleDefaultThresholdAttribute"/>.
        /// </summary>
        public ThrottleAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class
        /// with method and threshold taken from the <see cref="ThrottleDefaultImplementationAttribute"/> and the <see cref="ThrottleDefaultThresholdAttribute"/>.
        /// </summary>
        /// <param name="implementation">The type of the throttle implementation.</param>
        public ThrottleAttribute(Type implementation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class
        /// with implementation and threshold taken from the <see cref="ThrottleDefaultImplementationAttribute"/> and the <see cref="ThrottleDefaultThresholdAttribute"/>.
        /// </summary>
        /// <param name="methodName">Name of the method to call on the implementation to trigger the throttle.</param>
        public ThrottleAttribute(string methodName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class
        /// with implementation and method taken from the <see cref="ThrottleDefaultImplementationAttribute"/>.
        /// </summary>
        /// <param name="threshold">The threshold.</param>
        public ThrottleAttribute(int threshold)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class
        /// with the method taken from the <see cref="ThrottleDefaultImplementationAttribute"/>.
        /// </summary>
        /// <param name="implementation">The type of the throttle implementation.</param>
        /// <param name="threshold">The threshold.</param>
        public ThrottleAttribute(Type implementation, int threshold)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class
        /// with the threshold taken from the <see cref="ThrottleDefaultThresholdAttribute"/>.
        /// </summary>
        /// <param name="implementation">The type of the throttle implementation.</param>
        /// <param name="methodName">Name of the method to call on the implementation to trigger the throttle.</param>
        public ThrottleAttribute(Type implementation, string methodName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class
        /// with implementation taken from the <see cref="ThrottleDefaultImplementationAttribute"/>.
        /// </summary>
        /// <param name="threshold">The threshold.</param>
        /// <param name="methodName">Name of the method to call on the implementation to trigger the throttle.</param>
        public ThrottleAttribute(int threshold, string methodName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class.
        /// </summary>
        /// <param name="implementation">The type of the throttle implementation.</param>
        /// <param name="threshold">The threshold.</param>
        /// <param name="methodName">Name of the method to call on the implementation to trigger the throttle.</param>
        public ThrottleAttribute(Type implementation, int threshold, string methodName)
        {
        }
    }
}
