using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Rooms.TilemapJsonContractsVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks validation and content identity for the installed room visual JSON and the room JSON the import reads.</summary>
internal static class RoomAssetJsonContractsVerification
{
    /// <summary>
    /// Checks the five statically identified room-visual JSON families: the installed layouts and X-ray reveals, and the
    /// backgrounds, skies and block compositions the import reads (#1638).
    /// Only extracted files are copied; no ROM or gameplay is opened by this fixture.
    /// Defective manifests must fail with their path, while valid historical casing keeps the exact
    /// content identity.
    /// </summary>
    internal static void VerifyRoomAssetJsonContracts(string installationRoot)
    {
        GameInstallation installed = GameAssetInstallerTooling.ValidateExtractedContent(installationRoot);
        using var temporaryScratch = new TestScratchDirectory("room-json-contract");
        string temporary = temporaryScratch.Root;
        Directory.CreateDirectory(temporary);
        var accepted = new List<string>();
        int rejected = 0;
        try
        {
            // The background tilemaps, sky pages and block compositions are import data no installation holds (#1638): their
            // contracts copy the cartridge-derived extraction's.
            string backgrounds = CartridgeImportContent.ImportData(GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
            RoomJsonFileContract[] contracts =
            [
                new("backgrounds", backgrounds, RoomBackgroundTilemapArtworkFiles.ManifestFileName,
                    stock => BackgroundTilemapIdentity(RoomBackgroundTilemapArtworkFiles.LoadForImport(stock))),
                new("skies", backgrounds, RoomSkyTilemapFormat.ManifestFileName,
                    stock => SkyIdentity(RoomSkyTilemapArtworkFiles.LoadForImport(stock))),
                // Block compositions are stock extraction input (#1431): their identity is their tiles.
                new("metatiles", CartridgeImportContent.ImportData(GameInstallationLayout.RoomMetatileDirectoryName), RoomMetatileArtworkFiles.ManifestFileName,
                    stock => BlockCompositionIdentity(RoomMetatileArtworkFiles.LoadForImport(stock))),
                new("layouts", installed.RoomVisualLayoutDirectory, RoomVisualLayoutFiles.ManifestFileName,
                    stock => RoomVisualLayoutFiles.Load(stock).ContentIdentity),
                // The X-ray reveals install no manifest (#1693): their document is the file corrupted.
                new("X-ray", installed.XrayRevealVisualDirectory, XrayRevealVisualFiles.VisualFileName,
                    stock => XrayRevealVisualFiles.Load(stock).ContentIdentity),
            ];
            foreach (RoomJsonFileContract contract in contracts)
            {
                string stock = Path.Combine(temporary, contract.Name);
                Directory.CreateDirectory(stock);
                foreach (string file in Directory.GetFiles(contract.Source)) File.Copy(file, Path.Combine(stock, Path.GetFileName(file)));
                string manifest = Path.Combine(stock, contract.Manifest);
                string original = File.ReadAllText(manifest);
                string expected = contract.Identity(stock);
                Dictionary<string, string> originalHashes = SnapshotRoomJsonFiles(stock);

                // Changing only historical member spelling is not an authored edit.
                File.WriteAllText(manifest, original.Replace("\"version\"", "\"VERSION\"", StringComparison.Ordinal));
                AssertEqual(expected, contract.Identity(stock), contract.Name + " retains historical manifest casing");
                File.WriteAllText(manifest, original);
                foreach (string corrupt in RoomJsonCorruptions(original))
                {
                    File.WriteAllText(manifest, corrupt);
                    Reject(contract.Name + " manifest", manifest, () => contract.Identity(stock));
                }
                File.WriteAllText(manifest, original);
                AssertRoomJsonUnchanged(originalHashes, stock, contract.Name + " manifest diagnostics leave other stock files intact");
            }
            AssertEqual(0, accepted.Count, "room asset loaders must reject malformed documents:\n" + string.Join('\n', accepted));
            Console.WriteLine($"PASS installed room JSON contracts: five manifest families; " +
                $"{rejected} invalid documents rejected with exact paths; stock bytes and identities preserved.");
        }
        finally { Directory.Delete(temporary, recursive: true); }

        void Reject(string name, string path, Func<string> load)
        {
            byte[] defective = File.ReadAllBytes(path);
            try { _ = load(); }
            catch (InvalidDataException error)
            {
                AssertTrue(error.ToString().Contains(path, StringComparison.Ordinal), name + " identifies exact file path");
                AssertTrue(File.ReadAllBytes(path).SequenceEqual(defective), name + " retains the invalid authored file");
                rejected++;
                return;
            }
            accepted.Add(name + ": " + path);
        }
    }

    /// <summary>
    /// A digest of every extracted block composition's tile words, the common room elements' first and then each block
    /// set's in set order, standing for the compositions' content as extraction reads it.
    /// </summary>
    private static string BlockCompositionIdentity(RoomMetatileCatalog catalog)
    {
        byte[] words = [.. BackgroundTileWords.ToBytes(catalog.Cre.BlockTiles.Span), .. catalog.Atlases.OrderBy(entry => entry.Key)
            .SelectMany(entry => BackgroundTileWords.ToBytes(entry.Value.BlockTiles.Span))];
        return Convert.ToHexString(SHA256.HashData(words));
    }

    /// <summary>
    /// The identity of the extracted library background tilemaps, which only the import reads (#1466, #1638): their tiles in
    /// background order.
    /// </summary>
    private static string BackgroundTilemapIdentity(RoomBackgroundTilemaps catalog) => Convert.ToHexString(SHA256.HashData(
        [.. catalog.Tilemaps.OrderBy(entry => entry.Key).SelectMany(entry => BackgroundTileWords.ToBytes(entry.Value.Tiles.Span))]));

    /// <summary>The identity of the extracted sky pages, which only the import reads (#1466, #1638): their tiles in scrolling order.</summary>
    private static string SkyIdentity(RoomSkyTilemaps skies) => Convert.ToHexString(SHA256.HashData(BackgroundTileWords.ToBytes(skies.Pages.Span)));

    /// <summary>Names one room asset family, its stock manifest, and its content identity loader.</summary>
    internal sealed record RoomJsonFileContract(string Name, string Source, string Manifest, Func<string, string> Identity);

    /// <summary>Produces malformed version, duplicate member, and invalid field variants for a room JSON document.</summary>
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
        JsonObject root = (JsonObject)RequiredJson.Parse(json);
        if (root["entries"] is JsonObject entries)
        {
            string first = entries.First().Key;
            yield return TilemapContractMutate(json, copy => copy.Required("entries").Required(first)["damage"] = 7);
        }
        else if (root["entries"] is JsonArray)
            yield return TilemapContractMutate(json, copy => copy.Required("entries").Required(0)["damage"] = 7);
    }

    /// <summary>Hashes each stock file by name so failed loads can be checked for unintended writes.</summary>
    internal static Dictionary<string, string> SnapshotRoomJsonFiles(string directory) => Directory.GetFiles(directory)
        .ToDictionary(path => Path.GetFileName(path), path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));

    /// <summary>Checks that a failed manifest load preserved the stock file set and bytes.</summary>
    internal static void AssertRoomJsonUnchanged(Dictionary<string, string> expected, string directory, string description)
    {
        Dictionary<string, string> actual = SnapshotRoomJsonFiles(directory);
        AssertTrue(expected.Count == actual.Count && expected.All(pair => actual.TryGetValue(pair.Key, out string? value) && value == pair.Value), description);
    }

    /// <summary>Runs room visual JSON contract checks against the installed asset directory.</summary>
    [VerificationCommand("--room-asset-json-contracts")]
    internal static int RoomAssetJsonContractsCommand()
    {
        string roomAssetRoot = RepositoryInstallation.Installation.Root;
        Suite(nameof(VerifyRoomAssetJsonContracts), () => VerifyRoomAssetJsonContracts(roomAssetRoot));
        return 0;
    }
}
