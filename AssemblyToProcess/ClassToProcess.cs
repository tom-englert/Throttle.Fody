using System;
using System.Reflection;

using Throttle;

[assembly:AssemblyVersion("0.0.1.*")]
[assembly: ThrottleDefaultImplementation(typeof(AssemblyToProcess.Throttle3))]

namespace AssemblyToProcess
{
    public class ClassToProcess1
    {
        public int NumberOfWithSimpeThrottleCalls { get; private set; }

        [Throttle(typeof(Throttle1))]
        public void WithSimpleThrottle()
        {
            NumberOfWithSimpeThrottleCalls += 1;
        }

        public int NumberOfWithTimerThrottleCalls { get; private set; }

        [Throttle(typeof(Throttle1), 20)]
        public void WithTimerThrottle()
        {
            NumberOfWithTimerThrottleCalls += 1;
        }
    }

    class Throttle1
    {
        private readonly Action _callback;
        private readonly int _timeout;
        private int _counter;


        public Throttle1(Action callback)
            : this(10, callback)
        {
        }

        public Throttle1(int timeout, Action callback)
        {
            _callback = callback;
            _timeout = timeout;
        }

        public void Tick()
        {
            if ((++_counter % _timeout) == 0)
                _callback();
        }
    }

    [ThrottleDefaultImplementation(typeof(Throttle2), nameof(Throttle2.Tock))]
    public class ClassToProcess2
    {
        public int NumberOfWithSimpeThrottleCalls { get; private set; }

        [Throttle]
        public void WithSimpleThrottle()
        {
            NumberOfWithSimpeThrottleCalls += 1;
        }

        public int NumberOfWithTimerThrottleCalls { get; private set; }

        [Throttle(50)]
        public void WithTimerThrottle()
        {
            NumberOfWithTimerThrottleCalls += 1;
        }
    }

    class Throttle2
    {
        private readonly Action _callback;
        private readonly int _timeout;
        private int _counter;


        public Throttle2(Action callback)
            : this(callback, 5)
        {
        }

        public Throttle2(Action callback, int timeout)
        {
            _callback = callback;
            _timeout = timeout;
        }

        public void Tock()
        {
            if ((++_counter % _timeout) == 0)
                _callback();
        }
    }

    public class ClassToProcess3
    {
        public int NumberOfWithSimpeThrottleCalls { get; private set; }

        [Throttle]
        public void WithSimpleThrottle()
        {
            NumberOfWithSimpeThrottleCalls += 1;
        }

        public int NumberOfWithTimerThrottleCalls { get; private set; }

        [Throttle(25)]
        public void WithTimerThrottle()
        {
            NumberOfWithTimerThrottleCalls += 1;
        }
    }

    class Throttle3
    {
        private readonly Action _callback;
        private readonly int _timeout;
        private int _counter;


        public Throttle3(Action callback)
            : this(callback, 15)
        {
        }

        public Throttle3(Action callback, int timeout)
        {
            _callback = callback;
            _timeout = timeout;
        }

        public void Tick()
        {
            if ((++_counter % _timeout) == 0)
                _callback();
        }
    }

}
