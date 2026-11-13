using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.IntegrationVerification.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Exercises the source-inventoried map/menu/title file boundary using an already
/// extracted bundle. It neither opens a ROM nor executes gameplay to find reads.
/// </summary>
internal static class MapPresentationJsonContractVerification
{
    /// <summary>Checks that every manifest-listed JSON resource, edited into ambiguity in a scratch copy, is rejected and left as written.</summary>
    internal static int Run(string installationRoot)
    {
        GameInstallation installation = GameAssetInstallerTooling.ValidateExtractedContent(installationRoot);
        using var temporaryScratch = new TestScratchDirectory("presentation-json");
        string temporary = temporaryScratch.Root;
        Directory.CreateDirectory(temporary);
        var accepted = new List<string>();
        int rejected = 0;
        try
        {
            string stock = Directory.CreateDirectory(Path.Combine(temporary, "stock")).FullName;
            foreach (string source in Directory.GetFiles(installation.MapDirectory))
                File.Copy(source, Path.Combine(stock, Path.GetFileName(source)));
            Dictionary<string, string> originalHashes = HashFiles(stock);
            // The pages (#1636) and the colour documents only the import reads (#1637) are import data no installation holds:
            // a copy of the cartridge-derived extraction's is read beside the copy, and its documents are edited like the rest.
            string pages = Directory.CreateDirectory(Path.Combine(temporary, "import-data")).FullName;
            foreach (string source in Directory.GetFiles(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.MapDirectoryName)))
                File.Copy(source, Path.Combine(pages, Path.GetFileName(source)));
            Dictionary<string, string> originalImportHashes = HashFiles(pages);
            string expected = MapPresentationFiles.LoadForImport(stock, pages).Game.ContentIdentity;
            string manifestPath = Path.Combine(stock, AreaMapCatalogFormat.ManifestFile);
            byte[] manifest = File.ReadAllBytes(manifestPath);
            using JsonDocument parsedManifest = JsonDocument.Parse(manifest);
            string[] files = parsedManifest.RootElement.GetProperty("sha256").EnumerateObject()
                .Select(property => property.Name).Where(name => name.EndsWith(".json", StringComparison.Ordinal))
                // The import-only documents are import data no installation holds (#1638), so no installed copy is edited.
                .Where(name => !MapPresentationFiles.ImportDocumentFiles.Contains(name))
                .Order(StringComparer.Ordinal).ToArray();
            // The production manifest requires every resource. Check every JSON file
            // in that finite registry, not just the initially defective formats.
            foreach (string file in files)
            {
                // Each listed document is in the copy of the installed directory or in the copy of the import data, never both.
                bool installedDocument = File.Exists(Path.Combine(stock, file));
                bool importDocument = File.Exists(Path.Combine(pages, file));
                Require(installedDocument != importDocument, $"{file} is in exactly one of the installed directory and the import data");
                string path = Path.Combine(installedDocument ? stock : pages, file);
                byte[] original = File.ReadAllBytes(path);
                using JsonDocument document = JsonDocument.Parse(original);
                foreach (byte[] corrupt in DuplicateDocuments(document.RootElement))
                {
                    File.WriteAllBytes(path, corrupt);
                    Reject(file, path, () => MapPresentationFiles.LoadForImport(stock, pages));
                    File.WriteAllBytes(path, original);
                }
            }
            using (JsonDocument document = JsonDocument.Parse(manifest))
                foreach (byte[] corrupt in DuplicateDocuments(document.RootElement))
                {
                    File.WriteAllBytes(manifestPath, corrupt);
                    Reject("manifest", manifestPath, () => MapPresentationFiles.LoadForImport(stock, pages));
                }
            File.WriteAllBytes(manifestPath, manifest);
            Require(HashFiles(stock).OrderBy(pair => pair.Key).SequenceEqual(originalHashes.OrderBy(pair => pair.Key)),
                "the diagnostics put every installed file back as it was");
            Require(HashFiles(pages).OrderBy(pair => pair.Key).SequenceEqual(originalImportHashes.OrderBy(pair => pair.Key)),
                "the diagnostics put every import-data file back as it was");
            Require(MapPresentationFiles.LoadForImport(stock, pages).Game.ContentIdentity == expected,
                "the restored files load with their original identity");
            Require(accepted.Count == 0, "silently accepted ambiguous JSON:\n" + string.Join('\n', accepted));
            Console.WriteLine($"PASS installed map/menu/title JSON: {files.Length} resource files plus manifest, " +
                $"{rejected} duplicate documents rejected.");
            return 0;
        }
        finally { Directory.Delete(temporary, recursive: true); }

        void Reject(string name, string path, Action load)
        {
            byte[] defective = File.ReadAllBytes(path);
            try { load(); }
            catch (InvalidDataException error)
            {
                Require(error.ToString().Contains("Duplicate", StringComparison.OrdinalIgnoreCase),
                    name + " must identify ambiguity, not an unrelated decoder failure: " + error);
                Require(File.ReadAllBytes(path).SequenceEqual(defective), name + " must retain defective bytes");
                rejected++;
                return;
            }
            accepted.Add(name);
        }
    }

    /// <summary>Captures installed stock file digests to detect unintended mutation during validation.</summary>
    private static Dictionary<string, string> HashFiles(string directory) => Directory.GetFiles(directory)
        .ToDictionary(path => Path.GetFileName(path), path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));

    /// <summary>Creates ambiguous root and nested JSON documents from an otherwise valid resource.</summary>
    private static IEnumerable<byte[]> DuplicateDocuments(JsonElement root)
    {
        // Repeating a valid property is still ambiguous authored JSON. Before
        // the fix deserialization silently overwrote the earlier value at root
        // and nested boundaries. Dictionaries are included as objects.
        yield return DuplicateAt(root, []);
        string[]? nested = FirstNestedObject(root);
        if (nested is not null) yield return DuplicateAt(root, nested);
    }

    /// <summary>Finds a populated nested object, traversing dictionary values and array elements.</summary>
    private static string[]? FirstNestedObject(JsonElement root, string[]? path = null)
    {
        path ??= [];
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (path.Length > 0 && root.EnumerateObject().Any()) return path;
            foreach (JsonProperty property in root.EnumerateObject())
                if (FirstNestedObject(property.Value, [.. path, property.Name]) is { } found) return found;
        }
        else if (root.ValueKind == JsonValueKind.Array)
        {
            int index = 0;
            foreach (JsonElement element in root.EnumerateArray())
            {
                if (FirstNestedObject(element, [.. path, (index++).ToString()]) is { } found) return found;
            }
        }
        return null;
    }

    /// <summary>Copies JSON while writing the first property twice at a selected object path.</summary>
    private static byte[] DuplicateAt(JsonElement root, string[] target)
    {
        using var bytes = new MemoryStream();
        using var writer = new Utf8JsonWriter(bytes);
        Write(root, []);
        writer.Flush();
        return bytes.ToArray();

        void Write(JsonElement element, string[] path)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                writer.WriteStartObject();
                bool duplicate = path.SequenceEqual(target);
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (duplicate)
                    {
                        writer.WritePropertyName(property.Name);
                        property.Value.WriteTo(writer);
                        duplicate = false;
                    }
                    writer.WritePropertyName(property.Name);
                    Write(property.Value, [.. path, property.Name]);
                }
                writer.WriteEndObject();
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                writer.WriteStartArray();
                int index = 0;
                foreach (JsonElement child in element.EnumerateArray()) Write(child, [.. path, (index++).ToString()]);
                writer.WriteEndArray();
            }
            else element.WriteTo(writer);
        }
    }

    /// <summary>Fails the integration check with its specific JSON contract message.</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
