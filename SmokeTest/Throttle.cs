// ReSharper disable all
namespace SmokeTest
{
    using System;

    /// <summary>
    /// Implements a simple timed throttle.<para/>
    /// Calling <see cref="Tick()"/> multiple times will restart the timer; there will be one single 
    /// call to the action when the delay time has elapsed after the last tick.
    /// </summary>
    public class DummyThrottle
    {
        private readonly Action _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="Throttle"/> class with a default timeout of 100ms.
        /// </summary>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public DummyThrottle(Action target)
            : this(TimeSpan.FromMilliseconds(100), target)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Throttle"/> class.
        /// </summary>
        /// <param name="timeout">The timeout to wait for after the last <see cref="Tick()"/>.</param>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public DummyThrottle(TimeSpan timeout, Action target)
        {
            _target = target;
        }

        /// <summary>
        /// Ticks this instance to trigger the throttle.
        /// </summary>
        public void Tick()
        {
        }
    }
}
