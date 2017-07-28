using System;

namespace Throttle
{
    /// <summary>
    /// Allows you to specify a default for the throttle threshold on assembly or class level, 
    /// so you don't have to specify it in every <see cref="ThrottledAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ThrottleDefaultThresholdAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleDefaultThresholdAttribute"/> class.
        /// </summary>
        /// <param name="threshold">The default threshold.</param>
        public ThrottleDefaultThresholdAttribute(int threshold)
        {
        }
    }
}
