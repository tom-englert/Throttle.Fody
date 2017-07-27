using Mono.Cecil;

namespace Throttle.Fody
{
    internal struct ThrottleParameters
    {
        public TypeDefinition Implementation { get; private set; }

        public string MethodName { get; private set; }

        public int? Threshold { get; private set; }

        public void ReadFromAttribute(CustomAttribute attribute)
        {
            Implementation = attribute.GetConstructorArgument<TypeDefinition>() ?? Implementation;
            MethodName = attribute.GetConstructorArgument<string>() ?? MethodName;
            Threshold = attribute.GetConstructorArgument2<int>() ?? Threshold;
        }

        public void ReadDefaults(ICustomAttributeProvider attributeProvider)
        {
            ReadFromAttribute(attributeProvider.GetAttribute("Throttle.ThrottleDefaultImplementationAttribute"));
            ReadFromAttribute(attributeProvider.GetAttribute("Throttle.ThrottleDefaultThresholdAttribute"));
        }
    }
}
