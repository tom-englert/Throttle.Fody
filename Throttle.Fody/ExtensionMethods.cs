namespace Throttle.Fody
{
    using System.Diagnostics.CodeAnalysis;

    using Mono.Cecil;

    internal static class ExtensionMethods
    {
        public static bool IsActionOrDelegate(this TypeReference type)
        {
            return type.FullName == typeof(System.Action).FullName || type.FullName == typeof(System.Delegate).FullName;
        }

        public static bool IsIntOrTimeSpan(this TypeReference type)
        {
            if (!type.IsValueType)
                return false;

            if (type.FullName == typeof(int).FullName || type.FullName == typeof(System.TimeSpan).FullName)
                return true;

            return type.Resolve()?.BaseType.FullName == typeof(System.Enum).FullName;
        }

        public static GenericInstanceMethod MakeGeneric(this MethodReference method, TypeReference type)
        {
            var genericMethod = new GenericInstanceMethod(method);

            genericMethod.GenericArguments.Add(type);

            return genericMethod;
        }

        public static MethodReference? Import(this MethodReference? reference, ModuleDefinition module)
        {
            if (reference == null)
                return null;

            return module.ImportReference(reference);
        }

        [return: NotNullIfNotNull("reference")]
        public static TypeReference? Import(this TypeReference? reference, ModuleDefinition module)
        {
            if (reference == null)
                return null;

            return module.ImportReference(reference);
        }
    }
}