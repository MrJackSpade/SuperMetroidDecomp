using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>
    /// Exercises the file boundary inventoried in ProjectilePresentationFiles, not
    /// gameplay or discovery of cartridge reads. Every declared PNG/JSON is rejected
    /// by its exact selected path, even when valid overrides could hide bad stock.
    /// Only this fixture's disposable extracted files are edited or removed.
    /// </summary>
    private static void VerifyProjectileFileContracts(string installationRoot)
    {
        GameInstallation installed = GameAssetInstaller.ValidateExtractedContent(installationRoot);
        string temporary = Directory.CreateTempSubdirectory("SuperMetroid-projectile-file-contract-").FullName;
        var failures = new List<string>();
        int rejections = 0;
        try
        {
            string stock = Directory.CreateDirectory(Path.Combine(temporary, "stock")).FullName;
            string overrides = Directory.CreateDirectory(Path.Combine(temporary, "overrides")).FullName;
            foreach (string source in Directory.GetFiles(installed.ProjectileDirectory))
                File.Copy(source, Path.Combine(stock, Path.GetFileName(source)));
            string manifestPath = Path.Combine(stock, ProjectilePresentationFiles.ManifestFileName);
            byte[] manifestBytes = File.ReadAllBytes(manifestPath);
            JsonObject manifest = JsonNode.Parse(manifestBytes)!.AsObject();
            string baseline = ProjectilePresentationFiles.Load(stock, null).SelectedSha256;
            Dictionary<string, string> originalHashes = SnapshotRoomJsonFiles(stock);
            string[] names = Directory.GetFiles(stock).Select(Path.GetFileName)
                .Where(name => name != ProjectilePresentationFiles.ManifestFileName)
                .Select(name => name!).Order(StringComparer.Ordinal).ToArray();
            var expectedBeams = BeamTileCatalog.Load(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount).Select(BeamTileAtlasDefinitions.SelectionAt)
                .ToDictionary(BeamTileAtlasDefinitions.FileName, index => File.ReadAllBytes(
                    Path.Combine(stock, BeamTileAtlasDefinitions.FileName(index)))));
            var compiledBeams = ProjectilePresentationFiles.Load(stock, null).BeamTiles;
            for (int selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
                AssertTrue(compiledBeams.Resolve(BeamTileCatalog.AssetFor(selection)).Span.SequenceEqual(
                    expectedBeams.Resolve(BeamTileCatalog.AssetFor(selection)).Span),
                    "file-context beam assembly retains every transfer byte and selection");
            var sheets = Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount).Select(BeamTileAtlasDefinitions.SelectionAt).Select(index =>
            {
                using var png = File.OpenRead(Path.Combine(stock, BeamTileAtlasDefinitions.FileName(index)));
                return BeamTileAtlas.Load(png);
            }).ToArray();
            var ownedBeams = BeamTileCatalog.FromAtlases(sheets, compiledBeams.Palettes!, compiledBeams.HyperBeamFxColors!);
            sheets[0] = null!;
            AssertTrue(ownedBeams.Resolve(BeamTileCatalog.AssetFor(0)).Span.SequenceEqual(
                expectedBeams.Resolve(BeamTileCatalog.AssetFor(0)).Span), "beam assembler owns the input array");
            AssertThrows<ArgumentException>(() => BeamTileCatalog.FromAtlases(sheets, compiledBeams.Palettes!,
                compiledBeams.HyperBeamFxColors!), "beam assembler rejects a missing atlas");
            AssertThrows<ArgumentException>(() => BeamTileCatalog.FromAtlases([], compiledBeams.Palettes!,
                compiledBeams.HyperBeamFxColors!), "beam assembler rejects an incomplete selection set");

            foreach (string name in names)
            {
                string stockPath = Path.Combine(stock, name);
                string overridePath = Path.Combine(overrides, name);
                byte[] original = File.ReadAllBytes(stockPath);
                byte[] malformed = name.EndsWith(".png", StringComparison.Ordinal)
                    ? "not an indexed PNG"u8.ToArray() : "{"u8.ToArray();
                try
                {
                    File.Delete(stockPath);
                    Reject("missing stock", stockPath, () => ProjectilePresentationFiles.Load(stock, overrides));
                    File.WriteAllBytes(stockPath, malformed);
                    Reject("stock hash", stockPath, () => ProjectilePresentationFiles.Load(stock, overrides));
                    File.WriteAllBytes(stockPath, original);
                    File.WriteAllBytes(overridePath, malformed);
                    Reject("selected override", overridePath, () => ProjectilePresentationFiles.Load(stock, overrides));

                    // Hash-valid but malformed stock must still fail before a valid
                    // replacement is admitted. The manifest oracle is independent
                    // of the production loader's hash/selection helpers.
                    File.WriteAllBytes(overridePath, original);
                    File.WriteAllBytes(stockPath, malformed);
                    JsonObject revised = manifest.DeepClone().AsObject();
                    SetProjectileTestManifestHash(revised, name, malformed);
                    File.WriteAllText(manifestPath, revised.ToJsonString());
                    Reject("stock format before valid override", stockPath,
                        () => ProjectilePresentationFiles.Load(stock, overrides));
                }
                finally
                {
                    File.WriteAllBytes(stockPath, original);
                    File.WriteAllBytes(manifestPath, manifestBytes);
                    if (File.Exists(overridePath)) File.Delete(overridePath);
                }

                File.WriteAllBytes(overridePath, original);
                AssertEqual(baseline, ProjectilePresentationFiles.Load(stock, overrides).SelectedSha256,
                    "stock-equivalent projectile override preserves byte identity: " + name);
                File.Delete(overridePath);
            }

            var badManifests = new List<string>
            {
                "{", "null", "[]", "{\"version\":999," + System.Text.Encoding.UTF8.GetString(manifestBytes)[1..],
                Mutate(root => root["damage"] = 7),
                Mutate(root => root["version"] = ProjectilePresentationFiles.Version - 1),
                Mutate(root => root["romSha256"] = new string('0', 64)),
            };
            foreach (string property in manifest.Select(pair => pair.Key))
                badManifests.Add(Mutate(root => root.Remove(property)));
            string firstBeam = BeamTileAtlasDefinitions.FileName(0);
            string serialized = System.Text.Encoding.UTF8.GetString(manifestBytes);
            badManifests.Add(serialized.Replace('"' + firstBeam + "\":",
                '"' + firstBeam + "\":\"bad\",\"" + firstBeam + "\":", StringComparison.Ordinal));
            try
            {
                File.Delete(manifestPath);
                Reject("missing manifest", manifestPath, () => ProjectilePresentationFiles.Load(stock, overrides));
                foreach (string bad in badManifests)
                {
                    File.WriteAllText(manifestPath, bad);
                    Reject("manifest", manifestPath, () => ProjectilePresentationFiles.Load(stock, overrides));
                }
                File.WriteAllText(manifestPath, Mutate(root => root["beamHashes"]![firstBeam] = new string('0', 64)));
                Reject("manifest beam hash", Path.Combine(stock, firstBeam),
                    () => ProjectilePresentationFiles.Load(stock, overrides));
            }
            finally { File.WriteAllBytes(manifestPath, manifestBytes); }
            AssertRoomJsonUnchanged(originalHashes, stock, "projectile diagnostics preserve all stock files");
            AssertEqual(baseline, ProjectilePresentationFiles.Load(stock, overrides).SelectedSha256,
                "projectile diagnostics leave selected identity unchanged");
            AssertEqual(0, failures.Count, "projectile file contract failures:\n" + string.Join('\n', failures));
            Console.WriteLine($"PASS projectile file contracts: {names.Length} PNG/JSON resources; " +
                $"{rejections} exact-path failures; stock-first admission, byte identities and authored files preserved.");

            string Mutate(Action<JsonObject> edit)
            {
                JsonObject copy = manifest.DeepClone().AsObject();
                edit(copy);
                return copy.ToJsonString();
            }
        }
        finally { Directory.Delete(temporary, recursive: true); }

        void Reject(string context, string path, Func<InstalledProjectilePresentation> load)
        {
            byte[]? before = File.Exists(path) ? File.ReadAllBytes(path) : null;
            try { _ = load(); failures.Add(context + " accepted invalid data: " + path); }
            catch (Exception error) when (error is InvalidDataException or IOException)
            {
                if (!error.ToString().Contains(path, StringComparison.Ordinal))
                    failures.Add(context + " omitted path: " + path + " -> " + error.Message);
                else rejections++;
                if (context is "selected override" or "stock format before valid override" &&
                    error is InvalidDataException invalid && invalid.InnerException is not InvalidDataException)
                    failures.Add(context + " lost the original codec exception: " + path);
            }
            if (before is not null)
                AssertTrue(File.ReadAllBytes(path).AsSpan().SequenceEqual(before), context + " retains failing file");
            else AssertTrue(!File.Exists(path), context + " never silently repairs missing file");
        }
    }

    /// <summary>Independent correspondence between documented filenames and provenance fields.</summary>
    private static void SetProjectileTestManifestHash(JsonObject manifest, string name, byte[] bytes)
    {
        string hash = Convert.ToHexString(SHA256.HashData(bytes));
        if (manifest["beamHashes"]!.AsObject().ContainsKey(name))
        {
            manifest["beamHashes"]![name] = hash;
            return;
        }
        string field = name switch
        {
            ProjectileSpriteDefinitions.FileName => "contentSha256",
            ProjectileFrameBindingFormat.FileName => "frameBindingsSha256",
            BeamPaletteDefinitions.FileName => "paletteSha256",
            HyperBeamFxColorFormat.FileName => "hyperBeamFxColorsSha256",
            ProjectileTrailVisualDefinitions.FileName => "trailSha256",
            ProjectileTrailAtlasDefinitions.FileName => "trailTilesSha256",
            ChargeFlarePlacementDefinitions.FileName => "flarePlacementSha256",
            ChargeFlareSpriteDefinitions.FileName => "flareCompositionsSha256",
            GrappleTileDefinitions.FileName => "grappleTilesSha256",
            GrappleSpriteDefinitions.FileName => "grappleSpritesSha256",
            GrappleFlarePlacementDefinitions.FileName => "grappleFlareSha256",
            GrappleSwingFrameDefinitions.FileName => "grappleSwingSha256",
            _ => throw new InvalidDataException("Uninventoried projectile resource: " + name),
        };
        AssertTrue(manifest.ContainsKey(field), "every test resource has a declared provenance field");
        manifest[field] = hash;
    }
}
