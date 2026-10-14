using SuperMetroid.Tooling;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// Exercises the source-inventoried map/menu/title file boundary using an already
/// extracted bundle. It neither opens a ROM nor executes gameplay to find reads.
/// </summary>
internal static class MapPresentationJsonContractVerification
{
    /// <summary>Checks manifest-listed JSON resources reject duplicate properties and preserve authored override bytes.</summary>
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
            string edits = Directory.CreateDirectory(Path.Combine(temporary, "edits")).FullName;
            foreach (string source in Directory.GetFiles(installation.MapDirectory))
                File.Copy(source, Path.Combine(stock, Path.GetFileName(source)));
            Dictionary<string, string> originalHashes = HashFiles(stock);
            string expected = AreaMapPresentationCatalog.Load(stock, null).ContentIdentity;
            string manifestPath = Path.Combine(stock, AreaMapCatalogFormat.ManifestFile);
            byte[] manifest = File.ReadAllBytes(manifestPath);
            using JsonDocument parsedManifest = JsonDocument.Parse(manifest);
            string[] files = parsedManifest.RootElement.GetProperty("sha256").EnumerateObject()
                .Select(property => property.Name).Where(name => name.EndsWith(".json", StringComparison.Ordinal))
                .Order(StringComparer.Ordinal).ToArray();
            // The production manifest requires every resource. Check every JSON file
            // in that finite registry, not just the initially defective formats.
            foreach (string file in files)
            {
                byte[] original = File.ReadAllBytes(Path.Combine(stock, file));
                using JsonDocument document = JsonDocument.Parse(original);
                foreach (byte[] corrupt in DuplicateDocuments(document.RootElement))
                {
                    bool rulesOnly = file == AreaMapCatalogFormat.StationRevealFile;
                    string path = Path.Combine(rulesOnly ? stock : edits, file);
                    File.WriteAllBytes(path, corrupt);
                    if (rulesOnly) RewriteHash(manifestPath, manifest, file, corrupt);
                    Reject(file, path, () => AreaMapPresentationCatalog.Load(stock, edits));
                    if (rulesOnly)
                    {
                        File.WriteAllBytes(path, original);
                        File.WriteAllBytes(manifestPath, manifest);
                    }
                    else File.Delete(path);
                }
                // Whitespace-only edits must retain their bytes and deterministic
                // selected identity across fresh catalog loads; no repair is allowed.
                if (file != AreaMapCatalogFormat.StationRevealFile)
                {
                    string path = Path.Combine(edits, file);
                    byte[] selected = [.. original, (byte)' ', (byte)'\n'];
                    File.WriteAllBytes(path, selected);
                    string selectedIdentity = AreaMapPresentationCatalog.Load(stock, edits).ContentIdentity;
                    Require(selectedIdentity != expected, file + " participates in selected identity");
                    Require(AreaMapPresentationCatalog.Load(stock, edits).ContentIdentity == selectedIdentity,
                        file + " retains its identity on a fresh reload");
                    Require(File.ReadAllBytes(path).SequenceEqual(selected), file + " retains authored bytes");
                    File.Delete(path);
                }
            }
            using (JsonDocument document = JsonDocument.Parse(manifest))
                foreach (byte[] corrupt in DuplicateDocuments(document.RootElement))
                {
                    File.WriteAllBytes(manifestPath, corrupt);
                    Reject("manifest", manifestPath, () => AreaMapPresentationCatalog.Load(stock, edits));
                }
            File.WriteAllBytes(manifestPath, manifest);
            Require(HashFiles(stock).OrderBy(pair => pair.Key).SequenceEqual(originalHashes.OrderBy(pair => pair.Key)),
                "diagnostics must leave the required stock files untouched");
            Require(AreaMapPresentationCatalog.Load(stock, edits).ContentIdentity == expected,
                "removing overrides restores the exact stock identity");
            Require(accepted.Count == 0, "silently accepted ambiguous JSON:\n" + string.Join('\n', accepted));
            Console.WriteLine($"PASS installed map/menu/title JSON: {files.Length} resource files plus manifest, " +
                $"{rejected} duplicate documents rejected; override bytes, reload identities and stock retained.");
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

    /// <summary>Updates a test copy's stock digest so duplicate-property rejection reaches the JSON parser.</summary>
    private static void RewriteHash(string path, byte[] baseline, string file, byte[] content)
    {
        var manifest = RequiredJson.Parse(baseline);
        manifest.Required("sha256")[file] = Convert.ToHexString(SHA256.HashData(content));
        File.WriteAllText(path, manifest.ToJsonString());
    }

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
