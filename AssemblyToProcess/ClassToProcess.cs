﻿using System;
using System.Reflection;
using System.Windows.Threading;

using Throttle;
// ReSharper disable UnusedMember.Global

[assembly:AssemblyVersion("0.0.1.*")]
[assembly: ThrottleDefaultImplementation(typeof(AssemblyToProcess.Throttle3))]

namespace AssemblyToProcess
{
    public class ClassToProcess1
    {
        public int NumberOfWithSimpeThrottleCalls { get; private set; }

        [Throttled(typeof(Throttle1))]
        public void WithSimpleThrottle()
        {
            NumberOfWithSimpeThrottleCalls += 1;
        }

        public int NumberOfWithTimerThrottleCalls { get; private set; }

        [Throttled(typeof(Throttle1), 20)]
        public void WithTimerThrottle()
        {
            NumberOfWithTimerThrottleCalls += 1;
        }
    }

    class Throttle1
    {
        private readonly Action _callback;
        private readonly int _threshold;
        private int _counter;


        public Throttle1(Action callback)
            : this(10, callback)
        {
        }

        public Throttle1(int threshold, Action callback)
        {
            _callback = callback;
            _threshold = threshold;
        }

        public void Tick()
        {
            if ((++_counter % _threshold) == 0)
                _callback?.Invoke();
        }
    }

    [ThrottleDefaultImplementation(typeof(Throttle2), nameof(Throttle2.Tock))]
    public class ClassToProcess2
    {
        public int NumberOfWithSimpeThrottleCalls { get; private set; }

        [Throttled]
        public void WithSimpleThrottle()
        {
            NumberOfWithSimpeThrottleCalls += 1;
        }

        public int NumberOfWithTimerThrottleCalls { get; private set; }

        [Throttled(50)]
        public void WithTimerThrottle()
        {
            NumberOfWithTimerThrottleCalls += 1;
        }
    }

    class Throttle2
    {
        private readonly Action _callback;
        private readonly int _threshold;
        private int _counter;

        public Throttle2(Action callback)
            : this(callback, 5)
        {
        }

        public Throttle2(Action callback, int threshold)
        {
            _callback = callback;
            _threshold = threshold;
        }

        public void Tock()
        {
            if ((++_counter % _threshold) == 0)
                _callback?.Invoke();
        }
    }

    public class ClassToProcess3
    {
        public int NumberOfWithSimpeThrottleCalls { get; private set; }

        [Throttled]
        public void WithSimpleThrottle()
        {
            NumberOfWithSimpeThrottleCalls += 1;
        }

        public int NumberOfWithTimerThrottleCalls { get; private set; }

        [Throttled(25)]
        public void WithTimerThrottle()
        {
            NumberOfWithTimerThrottleCalls += 1;
        }
    }

    class Throttle3
    {
        private readonly Action _callback;
        private readonly int _threshold;
        private int _counter;

        public Throttle3(Action callback)
            : this(callback, 15)
        {
        }

        public Throttle3(Action callback, int threshold)
        {
            _callback = callback;
            _threshold = threshold;
        }

        public void Tick()
        {
            if ((++_counter % _threshold) == 0)
                _callback?.Invoke();
        }
    }


    public class ClassToProcess4
    {
        public int NumberOfWithSimpeThrottleCalls { get; private set; }

        [Throttled(typeof(Throttle4))]
        public void WithSimpleThrottle()
        {
            NumberOfWithSimpeThrottleCalls += 1;
        }

        public int NumberOfWithTimerThrottleCalls { get; private set; }

        [Throttled(typeof(Throttle4), (int)DispatcherPriority.DataBind)]
        public void WithTimerThrottle()
        {
            NumberOfWithTimerThrottleCalls += 1;
        }
    }

    class Throttle4
    {
        private readonly Action _callback;
        private readonly int _threshold;
        private int _counter;

        public Throttle4(Action callback)
            : this(callback, DispatcherPriority.Normal)
        {
        }

        public Throttle4(Action callback, DispatcherPriority threshold)
        {
            _callback = callback;
            _threshold = (int)threshold;
        }

        public void Tick()
        {
            if ((++_counter % _threshold) == 0)
                _callback?.Invoke();
        }
    }
}
