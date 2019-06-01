namespace Throttle.Fody
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    internal static class ExtensionMethods
    {
        public static CustomAttribute GetAttribute(this ICustomAttributeProvider attributeProvider, string attributeName)
        {
            return attributeProvider?.CustomAttributes.GetAttribute(attributeName);
        }

        public static CustomAttribute GetAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
        {
            return attributes?.FirstOrDefault(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
        }

        public static bool IsActionOrDelegate(this TypeReference type)
        {
            return type.FullName == typeof(System.Action).FullName || type.FullName == typeof(System.Delegate).FullName;
        }

        public static bool IsIntOrTimeSpan(this TypeReference type)
        {
            if (!type.IsValueType)
                return false;

            if (type.FullName == typeof(System.Int32).FullName || type.FullName == typeof(System.TimeSpan).FullName)
                return true;

            return type.Resolve()?.BaseType.FullName == typeof(System.Enum).FullName;
        }

        public static AssemblyDefinition TryResolve(this IAssemblyResolver assemblyResolver, string assemlbyName)
        {
            try
            {
                return assemblyResolver.Resolve(new AssemblyNameReference(assemlbyName, null));
            }
            catch
            {
                return null;
            }
        }

        public static void AddRange<T>(this Collection<T> collection, params T[] values)
        {
            foreach (var value in values)
            {
                collection.Add(value);
            }
        }

        public static GenericInstanceMethod MakeGeneric(this MethodReference method, TypeReference type)
        {
            var genericMethod = new GenericInstanceMethod(method);

            genericMethod.GenericArguments.Add(type);

            return genericMethod;
        }

        public static T GetConstructorArgument<T>(this CustomAttribute attribute)
            where T : class
        {
            return attribute?.ConstructorArguments?
                .Select(arg => arg.Value as T)
                .FirstOrDefault(value => value != null);
        }

        public static T? GetConstructorArgument2<T>(this CustomAttribute attribute)
            where T : struct
        {
            return attribute?.ConstructorArguments?
                .Select(arg => arg.Value as T?)
                .FirstOrDefault(value => value != null);
        }

        public static SequencePoint GetEntryPoint(this ISymbolReader symbolReader, MethodDefinition method)
        {
            var instruction = method?.Body?.Instructions?.FirstOrDefault();

            if (instruction == null)
                return null;

            return symbolReader?.Read(method)?.GetSequencePoint(instruction);
        }

        public static MethodReference Import(this MethodReference reference, ModuleDefinition module)
        {
            if (reference == null)
                return null;

            return module.ImportReference(reference);
        }

        public static TypeReference Import(this TypeReference reference, ModuleDefinition module)
        {
            if (reference == null)
                return null;

            return module.ImportReference(reference);
        }

    }
}