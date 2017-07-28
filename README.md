### This is an add-in for [Fody](https://github.com/Fody/Fody/) ![badge](https://tom-englert.visualstudio.com/_apis/public/build/definitions/75bf84d2-d359-404a-a712-07c9f693f635/13/badge) [![NuGet Status](http://img.shields.io/nuget/v/Throttle.Fody.svg?style=flat-square)](https://www.nuget.org/packages/Throttle.Fody)
![Icon](Icon.png) 

Injects code do to easily use throttles. Simply add the `Throttled` attribute to a method to wire up everything.

Your code:
```C#
public class Class
{
    public void OnEvent()
    {
        Refresh();
    }

    [Throttled(typeof(DispatcherThrottle)]
    public void Refresh()
    { 
        // do the heavy stuff here
    }
}
```

What gets compiled:
```C#
public class Class
{
    private DispatcherThrottle <Refresh>_throttle_field;

    public void OnEvent()
    {
        Refresh();
    }

    public void Refresh()
    {
        var throttle = <Refresh>_throttle_field;
        if (throttle == null)
        {
            Interlocked.CompareExchange(ref <Refresh>_throttle_field, new DispatcherThrottle(<Refresh>_throttled_method), null);
            throttle = _throttle;
        }
        throttle.Tick();
    }

    private void <Refresh>_throttled_method()
    {
        // do the heavy stuff here
    }
}
```

The original method will be renamed and made private, while a new method with the original name is 
created that sets up the throttle and calls it's `Tick()` method.

Since there are so many different requirements to throttles, e.g. timing, counting or threading thresholds, this 
library does not provide a throttle implementation - it just helps you by avoiding the task of wiring up everything again and again.

Some ready to use throttle implementations are 
the [DispatcherThrottle](https://github.com/tom-englert/TomsToolbox/blob/master/TomsToolbox.Desktop/DispatcherThrottle.cs) 
or the [Timer based throttle](https://github.com/tom-englert/TomsToolbox/blob/master/TomsToolbox.Desktop/Throttle.cs) 
from the [Tom's Toolbox library](https://github.com/tom-englert/TomsToolbox)

### Using it

You can convert any method with no return value an no parameter into a throttled method by simply preceding it with the `[Throttled]` attribute.

There are three configurable parameters:
- The type of the implementation to use (`System.Type`, mandatory)
- The name of the "Tick" method of the throttle implementation (`System.String`, optional, default is "Tick")
- The threshold (`System.Int32`, optional); if you specify the threshold, the throttle implementation must have a constructor with two paramters.

You can specify all parameters directly with the `ThrottledAttribute`:

    [Throttled(typeof(TomsToolbox.Desktop.Throttle), "Tick", 500)]

There are also two optional attributes that allow you to specify defaults for all three values on assembly or class 
level, so you may omit them at the `ThrottledAttribute`:

```C#
[assembly: ThrottleDefaultImplementation(typeof(TomsToolbox.Desktop.Throttle))]
[assembly: ThrottleDefaultThreshold(500)]

class Class 
{
    [Throttled]
    Refresh1()
    {
        // do the heavy stuff here
    }

    [Throttled(50)]
    Refresh2()
    {
        // do the heavy stuff here
    }
}

```
Of course you can mix anything of the above. Any parameter can be overwritten at the higher level. 
 


### Writing you own throttle implementation

You can easily write your own throttle implementation or use any existing one. The requirements for a throttle class are:

- It must have a public constructor with at least a `System.Action` (or `SystemDelegate`) parameter. 
  The original method will be passed as this delegate, so the implementation can call it 
  whenever it decides that the throttle threshold is reached.
- The constructor optionally may have a second parameter that takes the threshold. 
  It can be a `System.Int32`, a `System.TimeSpan` or any `System.Enum`. If the parameter is of type `System.TimeSpan`, 
  the attributes integer value is interpreted as milliseconds.
- It must have a public void method without parameters that is called whenever the wrapper method is called. The name of this method 
  can be configured via the attributes, the default name is "Tick".
  