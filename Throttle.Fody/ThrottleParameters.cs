namespace Throttle.Fody
{
    using FodyTools;

    using Mono.Cecil;

    internal struct ThrottleParameters
    {
        public TypeReference? Implementation { get; private set; }

        public string? MethodName { get; private set; }

        public int? Threshold { get; private set; }

        public bool ConsumeAttribute(ICustomAttributeProvider attributeProvider, string attributeName)
        {
            var attribute = attributeProvider.GetAttribute(attributeName);

            if (attribute == null)
                return false;

            Implementation = attribute.GetReferenceTypeConstructorArgument<TypeReference>() ?? Implementation;
            MethodName = attribute.GetReferenceTypeConstructorArgument<string>() ?? MethodName;
            Threshold = attribute.GetValueTypeConstructorArgument<int>() ?? Threshold;

            attributeProvider.CustomAttributes.Remove(attribute);

            return true;
        }

        public void ConsumeDefaultAttributes(ICustomAttributeProvider attributeProvider)
        {
            ConsumeAttribute(attributeProvider, "Throttle.ThrottleDefaultImplementationAttribute");
            ConsumeAttribute(attributeProvider, "Throttle.ThrottleDefaultThresholdAttribute");
        }
    }
}
