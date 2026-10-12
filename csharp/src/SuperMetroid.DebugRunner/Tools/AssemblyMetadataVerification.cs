using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

/// <summary>
/// Read-only inventory check for linker experiments. Does not load or execute either
/// assembly; checks nonpublic members too because debugger graphs depend on them.
/// Passing proves declaration retention, not runtime reflection or state compatibility.
/// </summary>
internal static class AssemblyMetadataVerification
{
    public static int Run(string original, string linked)
    {
        var before = Read(original);
        var after = Read(linked);
        string[] missing = before.Except(after).Order(StringComparer.Ordinal).ToArray();
        Console.WriteLine($"Metadata declarations: input={before.Count}, output={after.Count}, missing={missing.Length}.");
        foreach (string member in missing.Take(30)) Console.WriteLine($"MISSING {member}");
        if (missing.Length != 0) throw new InvalidDataException("Link output removed reflection-visible declarations.");
        Console.WriteLine("PASS declaration retention; runtime save-state verification remains required.");
        return 0;
    }

    private static HashSet<string> Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        MetadataReader metadata = pe.GetMetadataReader();
        var result = new HashSet<string>(StringComparer.Ordinal);
        string Name(TypeDefinitionHandle handle)
        {
            TypeDefinition type = metadata.GetTypeDefinition(handle);
            var parent = type.GetDeclaringType();
            return (parent.IsNil ? metadata.GetString(type.Namespace) : Name(parent)) + "." + metadata.GetString(type.Name);
        }
        foreach (var handle in metadata.TypeDefinitions)
        {
            TypeDefinition type = metadata.GetTypeDefinition(handle);
            string name = Name(handle);
            result.Add($"type {name}");
            // Count overloads with the same name/arity/parameter count independently;
            // this catches dropped private overloads without relying on token ordering.
            var methods = type.GetMethods().Select(h => metadata.GetMethodDefinition(h))
                .GroupBy(m => $"{metadata.GetString(m.Name)} arity={m.GetGenericParameters().Count} parameters={m.GetParameters().Count}");
            foreach (var group in methods)
                for (int i = 0; i < group.Count(); i++) result.Add($"method {name}::{group.Key} overload={i}");
            foreach (var field in type.GetFields()) result.Add($"field {name}::{metadata.GetString(metadata.GetFieldDefinition(field).Name)}");
            foreach (var property in type.GetProperties()) result.Add($"property {name}::{metadata.GetString(metadata.GetPropertyDefinition(property).Name)}");
            foreach (var ev in type.GetEvents()) result.Add($"event {name}::{metadata.GetString(metadata.GetEventDefinition(ev).Name)}");
        }
        return result;
    }
}
