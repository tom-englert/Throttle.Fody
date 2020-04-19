namespace Tests
{
    using System;
    using System.Reflection;

    public static class ExtensionMethods
    {
        public static dynamic GetInstance(this Assembly assembly, string className, params object[] args)
        {
            var type = assembly.GetType(className, true);
            return Activator.CreateInstance(type, args);
        }
    }
}
