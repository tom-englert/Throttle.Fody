using System;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

using NUnit.Framework;

using Tests;

public class ClassToProcess
{
    private Throttle1 _throttle;

    public int NumberOfWithSimpeThrottleCalls { get; private set; }

    public void WithSimpleThrottle()
    {
        var throttle = _throttle;
        if (throttle == null)
        {
            Interlocked.CompareExchange(ref _throttle, new Throttle1(WithSimpleThrottle_X), null);
            throttle = _throttle;
        }
        throttle.Tick();
    }

    private void WithSimpleThrottle_X()
    {
        NumberOfWithSimpeThrottleCalls += 1;
    }
}

class Throttle1
{
    private readonly Action _callback;
    private readonly int _threshold;
    private int _counter;


    public Throttle1(Action callback)
        : this(callback, 10)
    {
    }

    public Throttle1(Action callback, int threshold)
    {
        _callback = callback;
        _threshold = threshold;
    }

    public Throttle1(Action callback, DispatcherPriority threshold)
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

public class ThrottleTests
{
    readonly Assembly _assembly = WeaverHelper.Create().Assembly;

    [Test]
    public void ReferenceTest()
    {
        var target = new ClassToProcess();

        for (var outer = 0; outer < 3; outer++)
        {
            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(outer, target.NumberOfWithSimpeThrottleCalls);
                target.WithSimpleThrottle();
            }

            Assert.AreEqual(outer + 1, target.NumberOfWithSimpeThrottleCalls);
        }
    }

    [Test]
    [TestCase("ClassToProcess1", 10)]
    [TestCase("ClassToProcess2", 5)]
    [TestCase("ClassToProcess3", 15)]
    [TestCase("ClassToProcess4", 9)]
    [TestCase("ClassToProcess5", 15)]
    public void TestSimple(string className, int throttleTreshold)
    {
        Test(className, throttleTreshold, target => target.WithSimpleThrottle(), target => target.NumberOfWithSimpeThrottleCalls);
    }

    [Test]
    [TestCase("ClassToProcess1", 20)]
    [TestCase("ClassToProcess2", 50)]
    [TestCase("ClassToProcess3", 25)]
    [TestCase("ClassToProcess4", 8)]
    [TestCase("ClassToProcess5", 25)]
    public void TestTimer(string className, int throttleTreshold)
    {
        Test(className, throttleTreshold, target => target.WithThesholdThrottle(), target => target.NumberOfWithThesholdThrottleCalls);
    }

    private void Test(string className, int throttleTreshold, Action<dynamic> method, Func<dynamic, int> numberOfCalls)
    {
        var target = _assembly.GetInstance("AssemblyToProcess." + className);

        for (var outer = 0; outer < 3; outer++)
        {
            for (var i = 0; i < throttleTreshold; i++)
            {
                Assert.AreEqual(outer, numberOfCalls(target));
                method(target);
            }

            Assert.AreEqual(outer + 1, numberOfCalls(target));
        }
    }
}
