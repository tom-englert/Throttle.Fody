using Throttle;

using TomsToolbox.Desktop;

namespace NetStandardSmokeTest
{
    public class Class1
    {
        public void Test()
        {
            Throttled();
        }

        [Throttled(typeof(DummyThrottle))]
        public void Throttled()
        {
            
        }
    }
}
