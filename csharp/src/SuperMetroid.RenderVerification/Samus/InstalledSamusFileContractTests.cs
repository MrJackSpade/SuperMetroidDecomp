using SuperMetroid.Tooling;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.RenderVerification.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Finite source-inventoried Samus PNG/JSON admission checks. These edit only copied
/// extracted files; no ROM, import, gameplay loop or player state is involved.
/// </summary>
internal static class InstalledSamusFileContractTests
{
    /// <summary>Checks copied Samus stock and override files for provenance, required references, PNG geometry, and exact-path failures.</summary>
    internal static void Run(string installationRoot)
    {
        string source = new GameInstallation(Path.GetFullPath(installationRoot)).SamusBodyDirectory;
        string baseline = SamusBodyArtworkFiles.Load(source, null).ContentIdentity;
        using var temporaryScratch = new TestScratchDirectory("samus-file-contract");
        string temporary = temporaryScratch.Root;
        Directory.CreateDirectory(temporary);
        int rejected = 0, accepted = 0;
        var failures = new List<string>();
        try
        {
            string stock = Directory.CreateDirectory(Path.Combine(temporary, "stock")).FullName;
            string overrides = Directory.CreateDirectory(Path.Combine(temporary, "overrides")).FullName;
            var originals = Directory.GetFiles(source).ToDictionary(path => Path.GetFileName(path),
                File.ReadAllBytes, StringComparer.Ordinal);
            Restore();
            string[] names = originals.Keys.Select(name => name).Order(StringComparer.Ordinal).ToArray();
            foreach (string name in names)
            {
                string path = Path.Combine(stock, name);
                byte[] bad = name.EndsWith(".png", StringComparison.Ordinal)
                    ? "not PNG"u8.ToArray() : "{"u8.ToArray();
                File.Delete(path);
                Reject("missing stock", path);
                Restore();
                File.WriteAllBytes(path, bad);
                Reject("corrupt stock", path);
                Restore();
                if (!IsOverridable(name)) continue;
                string replacement = Path.Combine(overrides, name);
                File.WriteAllBytes(replacement, bad);
                Reject("selected override", replacement, requireInner: true);
                Restore();
                // Hash-valid invalid stock is still rejected before a valid edit.
                File.WriteAllBytes(path, bad);
                SetStockHash(name, bad);
                File.WriteAllBytes(replacement, originals[name]);
                Reject("stock format before valid override", path, requireInner: true);
                Restore();
                File.WriteAllBytes(replacement, originals[name]);
                Require(baseline == Load().ContentIdentity, "identical override changed identity: " + name);
                accepted++;
                Restore();
            }
            foreach (string name in names.Where(name => name.EndsWith(".json", StringComparison.Ordinal)))
            {
                JsonObject original = RequiredJson.Parse(originals[name]).AsObject();
                string path = Path.Combine(IsOverridable(name) ? overrides : stock, name);
                var cases = new List<string> { "null", "[]", Mutate(root => root["gameplayDamage"] = 7) };
                foreach (string property in original.Select(pair => pair.Key))
                    cases.Add(Mutate(root => root.Remove(property)));
                int version = original.Required("version").GetValue<int>();
                for (int old = 0; old < version; old++) cases.Add(Mutate(root => root["version"] = old));
                cases.Add(Mutate(root => root["version"] = version + 1));
                string text = Encoding.UTF8.GetString(originals[name]);
                cases.Add("{\"version\":" + version + "," + text[(text.IndexOf('{') + 1)..]);
                cases.Add("{\"Version\":" + version + "," + text[(text.IndexOf('{') + 1)..]);
                foreach (string bad in cases)
                {
                    File.WriteAllText(path, bad);
                    Reject("JSON contract", path);
                    Restore();
                }
                string Mutate(Action<JsonObject> edit)
                {
                    JsonObject copy = original.DeepClone().AsObject();
                    edit(copy);
                    return copy.ToJsonString();
                }
            }
            // Required references are inventoried from catalog construction, not
            // discovered by playing poses until one happens to request missing art.
            BodyMutation(root => root.Required("poseFirstFrames")[0] = SamusBodyArtworkCatalog.FrameCount, "pose first frame");
            BodyMutation(root => root.Required("spritemapTopBases")[0] = SamusSpritemapArtworkCatalog.PointerCount,
                "spritemap base");
            BodyMutation(root => root.Required("spritemaps").Required(0)["parts"] = null, "spritemap parts");
            BodyMutation(root => root.Required("top")[0] = null, "body definition group");
            BodyMutation(root => root.Required("top").Required(0)[0] = null, "body definition record");
            BodyMutation(root => root.Required("spritemaps")[0] = null, "spritemap record");
            BodyMutation(root => root.Required("top").Required(0).Required(0)["firstSize"] = 0, "DMA size");
            BodyMutation(root => root.Required("landingYOffsets")[0] = 256, "visual landing byte");
            BodyMutation(root => root.Required("spritemaps").AsArray().RemoveAt(0), "required spritemap identity");
            BodyMutation(root => root.Required("hashes").AsObject().Remove("top-00.png"), "required provenance entry");
            foreach (string property in new[] { "topSet", "topPosition", "bottomSet", "bottomPosition" })
                BodyMutation(root => root.Required("frames").Required(0).AsObject().Remove(property), "required frame field");
            foreach (string property in new[] { "x", "y", "attributes" })
                BodyMutation(root => root.Required("spritemaps").Required(0).Required("parts").Required(0).AsObject().Remove(property),
                    "required OAM field");
            foreach (string property in new[] { "sourceAddress", "firstSize", "secondSize" })
                BodyMutation(root => root.Required("top").Required(0).Required(0).AsObject().Remove(property), "required DMA field");
            foreach (string property in new[] { "parts" })
                BodyMutation(root => root.Required("spritemaps").Required(0).AsObject().Remove(property), "required map field");
            foreach (string name in names.Where(name => name.EndsWith(".png", StringComparison.Ordinal)))
            {
                using var stream = new MemoryStream(originals[name], writable: false);
                // IHDR geometry is independently decoded from the already-extracted
                // input for constructing a valid PNG of the wrong dimensions.
                int width = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(originals[name].AsSpan(16, 4));
                int height = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(originals[name].AsSpan(20, 4));
                IndexedPngImage image = IndexedPng.Read(stream, new(checked((ushort)width), checked((ushort)height)));
                string path = Path.Combine(overrides, name);
                using (var output = File.Create(path))
                    IndexedPng.Write(output, new(checked((ushort)(width + 8)), checked((ushort)height)), new byte[(width + 8) * height], image.Colours);
                Reject("PNG geometry", path, requireInner: true);
                Restore();
                if (!(name.StartsWith("top-", StringComparison.Ordinal) || name.StartsWith("bottom-", StringComparison.Ordinal))) continue;
                bool upper = name.StartsWith("top-", StringComparison.Ordinal);
                int set = Convert.ToInt32(name.Substring(name.IndexOf('-') + 1, 2), 16);
                SamusBodyArtworkCatalog body = Load();
                var definitions = upper ? body.TopSet(set) : body.BottomSet(set);
                int position = Enumerable.Range(0, definitions.Count).FirstOrDefault(i =>
                    definitions[i].Planar.Length < SamusBodyArtworkCatalog.BytesPerDefinitionSlot, -1);
                if (position < 0) continue;
                int unusedTile = definitions[position].Planar.Length / 32;
                image.Pixels[(position * 16 + unusedTile / 8 * 8) * width + unusedTile % 8 * 8] = 1;
                using (var output = File.Create(path))
                    IndexedPng.Write(output, new(checked((ushort)width), checked((ushort)height)), image.Pixels, image.Colours);
                Reject("painted unused body character", path, requireInner: true);
                Restore();
            }
            Restore();
            Require(baseline == Load().ContentIdentity, "fixture did not restore selected content");
            foreach (string name in names)
                Require(File.ReadAllBytes(Path.Combine(stock, name)).SequenceEqual(originals[name]),
                    "fixture changed stock bytes: " + name);
            Require(failures.Count == 0, "Samus file contracts:\n" + string.Join('\n', failures));
            Console.WriteLine($"PASS Samus file contracts: {names.Length} inventoried PNG/JSON resources; " +
                $"{rejected} exact-path rejections, {accepted} equivalent overrides; stock-first admission, " +
                "required fields/references, duplicate properties and older formats checked without ROM/gameplay.");

            SamusBodyArtworkCatalog Load() => SamusBodyArtworkFiles.Load(stock, overrides);
            void Restore()
            {
                foreach (var pair in originals) File.WriteAllBytes(Path.Combine(stock, pair.Key), pair.Value);
                foreach (string file in Directory.GetFiles(overrides)) File.Delete(file);
            }
            void Reject(string context, string path, bool requireInner = false)
            {
                byte[]? before = File.Exists(path) ? File.ReadAllBytes(path) : null;
                try { _ = Load(); failures.Add(context + " accepted invalid file: " + path); }
                catch (Exception error) when (error is InvalidDataException or IOException)
                {
                    if (!error.Message.Contains(path, StringComparison.Ordinal))
                        failures.Add(context + " omitted exact path: " + path + " -> " + error.Message);
                    else rejected++;
                    if (requireInner && error is InvalidDataException invalid && invalid.InnerException is null)
                        failures.Add(context + " lost codec exception: " + path);
                }
                if (before is null) Require(!File.Exists(path), "loader silently repaired a missing file");
                else Require(File.ReadAllBytes(path).SequenceEqual(before), "loader changed failing data");
            }
            void BodyMutation(Action<JsonObject> edit, string context)
            {
                JsonObject body = RequiredJson.Parse(originals[SamusBodyArtworkFiles.ManifestFileName]).AsObject();
                edit(body);
                string path = Path.Combine(overrides, SamusBodyArtworkFiles.ManifestFileName);
                File.WriteAllText(path, body.ToJsonString());
                Reject(context, path);
                Restore();
            }
            void SetStockHash(string name, byte[] bytes)
            {
                if (name == SamusBodyArtworkFiles.ManifestFileName) return;
                string manifestName;
                string field;
                if (name.StartsWith("top-", StringComparison.Ordinal) || name.StartsWith("bottom-", StringComparison.Ordinal))
                { manifestName = SamusBodyArtworkFiles.ManifestFileName; field = "hashes"; }
                else if (name == SamusAtmosphericArtworkFiles.ArtworkFileName)
                { manifestName = SamusAtmosphericArtworkFiles.ManifestFileName; field = "artworkSha256"; }
                else if (name == SamusDeathPaletteArtworkFiles.ArtworkFileName)
                { manifestName = SamusDeathPaletteArtworkFiles.ManifestFileName; field = "artworkSha256"; }
                else if (name == SamusDeathTileAtlasFormat.ArtworkFileName)
                { manifestName = SamusDeathTileAtlasFormat.ManifestFileName; field = "artworkSha256"; }
                else
                { manifestName = "samus-arm-cannon-manifest.json"; field = name.EndsWith(".png", StringComparison.Ordinal) ? "pngSha256" : "jsonSha256"; }
                JsonObject manifest = RequiredJson.Parse(originals[manifestName]).AsObject();
                string hash = Convert.ToHexString(SHA256.HashData(bytes));
                if (field == "hashes") manifest.Required(field)[name] = hash;
                else manifest[field] = hash;
                File.WriteAllText(Path.Combine(stock, manifestName), manifest.ToJsonString());
            }
        }
        finally { Directory.Delete(temporary, recursive: true); }
    }

    /// <summary>Allows body manifest and artwork overrides while keeping other provenance manifests stock-owned.</summary>
    private static bool IsOverridable(string name) => name == SamusBodyArtworkFiles.ManifestFileName ||
        (!name.EndsWith("manifest.json", StringComparison.Ordinal));
    /// <summary>Fails the file-contract audit with the specific rejected or altered resource description.</summary>
    private static void Require(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
    }
}
