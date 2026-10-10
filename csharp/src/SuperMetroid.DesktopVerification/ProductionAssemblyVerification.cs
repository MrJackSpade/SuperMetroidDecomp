using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

/// <summary>Checks the actual published binaries without loading a second copy of their dependencies.</summary>
internal static class ProductionAssemblyVerification
{
    /// <summary>Rejects production binaries that contain smoke-test types or reference the verification assembly.</summary>
    /// <param name="paths">File paths to the production assemblies whose metadata should be inspected.</param>
    public static void Run(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            MetadataReader metadata = pe.GetMetadataReader();
            foreach (var handle in metadata.TypeDefinitions)
            {
                string name = metadata.GetString(metadata.GetTypeDefinition(handle).Name);
                if (name.Contains("SmokeTest", StringComparison.Ordinal))
                    throw new InvalidDataException($"Production assembly {path} contains test type {name}.");
            }
            foreach (var handle in metadata.AssemblyReferences)
            {
                string name = metadata.GetString(metadata.GetAssemblyReference(handle).Name);
                if (name.StartsWith("SuperMetroid.", StringComparison.Ordinal) &&
                    name.EndsWith("Verification", StringComparison.Ordinal))
                    throw new InvalidDataException($"Production assembly {path} references test assembly {name}.");
            }
            Console.WriteLine($"PASS production boundary: {Path.GetFileName(path)} has no smoke-test types or verification dependencies.");
        }
    }
}
