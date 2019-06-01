namespace Throttle.Fody
{
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal static class ExtensionMethods
    {
        [CanBeNull]
        public static CustomAttribute GetAttribute([CanBeNull] this ICustomAttributeProvider attributeProvider, string attributeName)
        {
            return attributeProvider?.CustomAttributes.GetAttribute(attributeName);
        }

        [CanBeNull]
        private static CustomAttribute GetAttribute([CanBeNull] this IEnumerable<CustomAttribute> attributes, string attributeName)
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

        public static GenericInstanceMethod MakeGeneric(this MethodReference method, TypeReference type)
        {
            var genericMethod = new GenericInstanceMethod(method);

            genericMethod.GenericArguments.Add(type);

            return genericMethod;
        }

        [CanBeNull]
        public static T GetConstructorArgument<T>([CanBeNull] this CustomAttribute attribute)
            where T : class
        {
            return attribute?.ConstructorArguments?
                .Select(arg => arg.Value as T)
                .FirstOrDefault(value => value != null);
        }

        public static T? GetConstructorArgument2<T>([CanBeNull] this CustomAttribute attribute)
            where T : struct
        {
            return attribute?.ConstructorArguments?
                .Select(arg => arg.Value as T?)
                .FirstOrDefault(value => value != null);
        }

        [CanBeNull]
        public static SequencePoint GetEntryPoint([CanBeNull] this ISymbolReader symbolReader, [CanBeNull] MethodDefinition method)
        {
            var instruction = method?.Body?.Instructions?.FirstOrDefault();

            if (instruction == null)
                return null;

            return symbolReader?.Read(method)?.GetSequencePoint(instruction);
        }

        [CanBeNull]
        public static MethodReference Import([CanBeNull] this MethodReference reference, [NotNull] ModuleDefinition module)
        {
            if (reference == null)
                return null;

            return module.ImportReference(reference);
        }

        [ContractAnnotation("reference:notnull => notnull")]
        public static TypeReference Import([CanBeNull] this TypeReference reference, [NotNull] ModuleDefinition module)
        {
            if (reference == null)
                return null;

            return module.ImportReference(reference);
        }

    }
}