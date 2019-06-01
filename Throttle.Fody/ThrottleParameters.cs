namespace Throttle.Fody
{
    using JetBrains.Annotations;

    using Mono.Cecil;

    internal struct ThrottleParameters
    {
        public TypeReference Implementation { get; private set; }

        public string MethodName { get; private set; }

        public int? Threshold { get; private set; }

        public bool ConsumeAttribute([NotNull] ICustomAttributeProvider attributeProvider, [NotNull] string attributeName)
        {
            var attribute = attributeProvider.GetAttribute(attributeName);

            if (attribute == null)
                return false;

            Implementation = attribute.GetConstructorArgument<TypeReference>() ?? Implementation;
            MethodName = attribute.GetConstructorArgument<string>() ?? MethodName;
            Threshold = attribute.GetConstructorArgument2<int>() ?? Threshold;

            attributeProvider.CustomAttributes.Remove(attribute);

            return true;
        }

        public void ConsumeDefaultAttributes([NotNull] ICustomAttributeProvider attributeProvider)
        {
            ConsumeAttribute(attributeProvider, "Throttle.ThrottleDefaultImplementationAttribute");
            ConsumeAttribute(attributeProvider, "Throttle.ThrottleDefaultThresholdAttribute");
        }
    }
}
