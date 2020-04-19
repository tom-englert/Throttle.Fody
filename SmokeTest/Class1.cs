namespace SmokeTest
{
    using Throttle;

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
