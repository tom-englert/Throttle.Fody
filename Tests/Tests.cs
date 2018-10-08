using System;
using System.Reflection;
using System.Threading;
using Tests;

using Xunit;

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

    #if !NETCOREAPP
    public Throttle1(Action callback, System.Windows.Threading.DispatcherPriority threshold)
    {
        _callback = callback;
        _threshold = (int)threshold;
    }
    #endif

    public void Tick()
    {
        if ((++_counter % _threshold) == 0)
            _callback?.Invoke();
    }
}

public class ThrottleTests
{
    readonly Assembly _assembly = WeaverHelper.Create().Assembly;

    [Fact]
    public void ReferenceTest()
    {
        var target = new ClassToProcess();

        for (var outer = 0; outer < 3; outer++)
        {
            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(outer, target.NumberOfWithSimpeThrottleCalls);
                target.WithSimpleThrottle();
            }

            Assert.Equal(outer + 1, target.NumberOfWithSimpeThrottleCalls);
        }
    }

    [Theory]
    [InlineData("ClassToProcess1", 10)]
    [InlineData("ClassToProcess2", 5)]
    [InlineData("ClassToProcess3", 15)]
#if !NETCOREAPP
    [InlineData("ClassToProcess4", 9)]
#endif
    [InlineData("ClassToProcess5", 15)]
    public void TestSimple(string className, int throttleTreshold)
    {
        Test(className, throttleTreshold, target => target.WithSimpleThrottle(), target => target.NumberOfWithSimpeThrottleCalls);
    }

    [Theory]
    [InlineData("ClassToProcess1", 20)]
    [InlineData("ClassToProcess2", 50)]
    [InlineData("ClassToProcess3", 25)]
#if !NETCOREAPP
    [InlineData("ClassToProcess4", 8)]
#endif
    [InlineData("ClassToProcess5", 25)]
    public void TestThreshold(string className, int throttleTreshold)
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
                Assert.Equal(outer, numberOfCalls(target));
                method(target);
            }

            Assert.Equal(outer + 1, numberOfCalls(target));
        }
    }
}
