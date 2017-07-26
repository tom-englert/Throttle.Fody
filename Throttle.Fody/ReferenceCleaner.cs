using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Collections.Generic;

using Throttle.Fody;

internal static class ReferenceCleaner
{
    private static readonly HashSet<string> attributeNames = new HashSet<string>
    {
        "Throttle.ThrottleDefaultImplementationAttribute",
        "Throttle.ThrottleDefaultTimeoutAttribute"
    };

    private static void ProcessAssembly(ModuleDefinition moduleDefinition)
    {
        foreach (var type in moduleDefinition.GetTypes())
        {
            ProcessType(type);
        }

        RemoveAttributes(moduleDefinition.Assembly.CustomAttributes, attributeNames);
    }

    private static void ProcessType(TypeDefinition type)
    {
        RemoveAttributes(type.CustomAttributes, attributeNames);

        foreach (var property in type.Properties)
        {
            RemoveAttributes(property.CustomAttributes, attributeNames);
        }
        foreach (var field in type.Fields)
        {
            RemoveAttributes(field.CustomAttributes, attributeNames);
        }
    }

    private static void RemoveAttributes(Collection<CustomAttribute> customAttributes, IEnumerable<string> attributeNames)
    {
        var attributes = customAttributes
            .Where(attribute => attributeNames.Contains(attribute.Constructor.DeclaringType.FullName))
            .ToArray();

        foreach (var customAttribute in attributes.ToList())
        {
            customAttributes.Remove(customAttribute);
        }
    }

    public static void RemoveReferences(this ModuleDefinition moduleDefinition, ILogger logger)
    {
        ProcessAssembly(moduleDefinition);

        var referenceToRemove = moduleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == "Throttle");
        if (referenceToRemove == null)
        {
            logger.LogInfo("\tNo reference to 'Throttle' found. References not modified.");
            return;
        }

        logger.LogInfo("\tRemoving reference to 'Throttle'.");
        if (!moduleDefinition.AssemblyReferences.Remove(referenceToRemove))
        {
            logger.LogWarning("\tCould not remove all references to 'Throttle'.");
        }
    }
}