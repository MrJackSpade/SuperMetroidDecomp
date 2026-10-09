using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>
    /// Checks the five statically identified installed room-visual JSON families.
    /// Only extracted files are copied; no ROM or gameplay is opened by this fixture.
    /// Defective stock manifests and selected overrides must fail with their path,
    /// while valid historical casing keeps the exact selected-content identity.
    /// </summary>
    private static void VerifyRoomAssetJsonContracts(string installationRoot)
    {
        GameInstallation installed = GameAssetInstallerTooling.ValidateExtractedContent(installationRoot);
        string temporary = Directory.CreateTempSubdirectory("SuperMetroid-room-json-contract-").FullName;
        var accepted = new List<string>();
        int rejected = 0;
        try
        {
            RoomJsonFileContract[] contracts =
            [
                new("backgrounds", installed.RoomBackgroundTilemapDirectory, RoomBackgroundTilemapArtworkFiles.ManifestFileName,
                    (stock, edited) => RoomBackgroundTilemapArtworkFiles.Load(stock, edited).ContentIdentity),
                new("skies", installed.RoomBackgroundTilemapDirectory, RoomSkyTilemapFormat.ManifestFileName,
                    (stock, edited) => RoomSkyTilemapArtworkFiles.Load(stock, edited).ContentIdentity),
                new("metatiles", installed.RoomMetatileDirectory, RoomMetatileArtworkFiles.ManifestFileName,
                    (stock, edited) => RoomMetatileArtworkFiles.Load(stock, edited).ContentIdentity),
                new("layouts", installed.RoomVisualLayoutDirectory, RoomVisualLayoutFiles.ManifestFileName,
                    (stock, edited) => RoomVisualLayoutFiles.Load(stock, edited).ContentIdentity),
                new("X-ray", installed.XrayRevealVisualDirectory, XrayRevealVisualFiles.ManifestFileName,
                    (stock, edited) => XrayRevealVisualFiles.Load(stock, edited).ContentIdentity),
            ];
            foreach (RoomJsonFileContract contract in contracts)
            {
                string stock = Path.Combine(temporary, contract.Name);
                Directory.CreateDirectory(stock);
                foreach (string file in Directory.GetFiles(contract.Source)) File.Copy(file, Path.Combine(stock, Path.GetFileName(file)));
                string manifest = Path.Combine(stock, contract.Manifest);
                string original = File.ReadAllText(manifest);
                string expected = contract.Identity(stock, null);
                Dictionary<string, string> originalHashes = SnapshotRoomJsonFiles(stock);

                // Changing only historical member spelling is not an authored edit.
                File.WriteAllText(manifest, original.Replace("\"version\"", "\"VERSION\"", StringComparison.Ordinal));
                AssertEqual(expected, contract.Identity(stock, null), contract.Name + " retains historical manifest casing");
                File.WriteAllText(manifest, original);
                foreach (string corrupt in RoomJsonCorruptions(original))
                {
                    File.WriteAllText(manifest, corrupt);
                    Reject(contract.Name + " manifest", manifest, () => contract.Identity(stock, null));
                }
                File.WriteAllText(manifest, original);
                AssertRoomJsonUnchanged(originalHashes, stock, contract.Name + " manifest diagnostics leave other stock files intact");

                if (contract.Name is "layouts" or "X-ray")
                {
                    string name = contract.Name == "layouts"
                        ? RoomVisualLayoutFiles.SourceFileName(RoomVisualLayoutFiles.RetailSources.Keys.Min())
                        : XrayRevealVisualFiles.VisualFileName;
                    string visual = File.ReadAllText(Path.Combine(stock, name));
                    string overrides = Directory.CreateDirectory(Path.Combine(temporary, contract.Name + "-overrides")).FullName;
                    string replacement = Path.Combine(overrides, name);
                    string versionField = contract.Name == "layouts" ? "formatVersion" : "version";
                    foreach (string corrupt in RoomJsonCorruptions(visual, versionField)
                        .Concat(RoomJsonMissingVisualFields(visual, contract.Name)))
                    {
                        File.WriteAllText(replacement, corrupt);
                        Reject(contract.Name + " override", replacement, () => contract.Identity(stock, overrides));
                    }
                    File.WriteAllText(replacement, visual.Replace('"' + versionField + '"',
                        '"' + versionField.ToUpperInvariant() + '"', StringComparison.Ordinal));
                    AssertEqual(expected, contract.Identity(stock, overrides), contract.Name + " historical visual casing retains exact content");
                    AssertRoomJsonUnchanged(originalHashes, stock, contract.Name + " rejected overrides never rewrite stock");
                }
            }
            AssertEqual(0, accepted.Count, "room asset loaders must reject malformed documents:\n" + string.Join('\n', accepted));
            Console.WriteLine($"PASS installed room JSON contracts: five manifest families plus layout/X-ray overrides; " +
                $"{rejected} invalid documents rejected with exact paths; stock bytes and selected identities preserved.");
        }
        finally { Directory.Delete(temporary, recursive: true); }

        void Reject(string name, string path, Func<string> load)
        {
            byte[] defective = File.ReadAllBytes(path);
            try { _ = load(); }
            catch (InvalidDataException error)
            {
                AssertTrue(error.ToString().Contains(path, StringComparison.Ordinal), name + " identifies exact file path");
                AssertTrue(File.ReadAllBytes(path).AsSpan().SequenceEqual(defective), name + " retains the invalid authored file");
                rejected++;
                return;
            }
            accepted.Add(name + ": " + path);
        }
    }

    /// <summary>Describes one installed room-asset family and the loader used to calculate its selected-content identity.</summary>
    /// <param name="Name">Short family label used for diagnostics and temporary-directory naming.</param>
    /// <param name="Source">Installed directory containing the family's stock JSON files.</param>
    /// <param name="Manifest">Manifest filename validated before visual documents are loaded.</param>
    /// <param name="Identity">Loader callback returning the content identity for stock data and an optional override directory.</param>
    private sealed record RoomJsonFileContract(string Name, string Source, string Manifest, Func<string, string?, string> Identity);

    /// <summary>Produces malformed manifest or visual JSON variants covering unsupported versions, casing, duplicates, and invalid values.</summary>
    /// <param name="json">Original serialized document to mutate into defective variants.</param>
    /// <param name="versionField">Version-property spelling used by this document family.</param>
    /// <returns>Invalid serialized documents that should be rejected by the corresponding loader.</returns>
    private static IEnumerable<string> RoomJsonCorruptions(string json, string versionField = "version")
    {
        yield return "{\"" + versionField + "\":999," + json[1..];
        yield return "{\"" + versionField.ToUpperInvariant() + "\":999," + json[1..];
        yield return TilemapContractMutate(json, root => root["physics"] = 7);
        foreach (string field in new[] { "sha256", "topLeft", "x", "foregroundVisualWords" })
            if (json.Contains('"' + field + "\":", StringComparison.Ordinal))
            {
                yield return json.Replace('"' + field + "\":", '"' + field + "\":null,\"" + field + "\":", StringComparison.Ordinal);
                yield return json.Replace('"' + field + "\":", '"' + field.ToUpperInvariant() + "\":null,\"" + field + "\":", StringComparison.Ordinal);
            }
        JsonObject root = (JsonObject)JsonNode.Parse(json)!;
        if (root["entries"] is JsonObject entries)
        {
            string first = entries.First().Key;
            yield return TilemapContractMutate(json, copy => copy["entries"]![first]!["damage"] = 7);
        }
        else if (root["entries"] is JsonArray)
            yield return TilemapContractMutate(json, copy => copy["entries"]![0]!["damage"] = 7);
    }

    /// <summary>Produces layout or X-ray override documents with required visual fields removed or invalid dimensions.</summary>
    /// <param name="json">Valid visual document to mutate.</param>
    /// <param name="domain">Visual family name selecting the required-field mutations.</param>
    /// <returns>Defective documents that the family's loader must reject.</returns>
    private static IEnumerable<string> RoomJsonMissingVisualFields(string json, string domain)
    {
        if (domain == "layouts")
        {
            yield return TilemapContractMutate(json, root => root.Remove("foregroundVisualWords"));
            yield return TilemapContractMutate(json, root => root["widthInBlocks"] = 0);
        }
        else
        {
            yield return TilemapContractMutate(json, root => root["entries"]![0]!.AsObject().Remove("topLeft"));
            yield return TilemapContractMutate(json, root => root["entries"]![0]!.AsObject().Remove("btsValues"));
            yield return TilemapContractMutate(json, root => root["rooms"]![0]!["tiles"]![0]!.AsObject().Remove("x"));
            yield return TilemapContractMutate(json, root => root["rooms"]![0]!["tiles"]![0]!.AsObject().Remove("y"));
            yield return TilemapContractMutate(json, root => root["rooms"]![0]!["tiles"]![0]!["collision"] = 7);
        }
    }

    /// <summary>Hashes every top-level file in a room-asset directory to detect unintended stock-file changes.</summary>
    /// <param name="directory">Directory whose files are captured by filename and SHA-256 digest.</param>
    /// <returns>A filename-to-hash snapshot of the directory contents.</returns>
    private static Dictionary<string, string> SnapshotRoomJsonFiles(string directory) => Directory.GetFiles(directory)
        .ToDictionary(path => Path.GetFileName(path), path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));

    /// <summary>Asserts that a room-asset directory still has the same files and bytes represented by a prior snapshot.</summary>
    /// <param name="expected">Filename-to-hash snapshot captured before the loader operation.</param>
    /// <param name="directory">Directory to compare with that snapshot.</param>
    /// <param name="description">Context included in the assertion failure message.</param>
    private static void AssertRoomJsonUnchanged(Dictionary<string, string> expected, string directory, string description)
    {
        Dictionary<string, string> actual = SnapshotRoomJsonFiles(directory);
        AssertTrue(expected.Count == actual.Count && expected.All(pair => actual.TryGetValue(pair.Key, out string? value) && value == pair.Value), description);
    }
}
