using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Security.Cryptography;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Verification.Frontend.Map;

using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Verifies installer startup, upgrade and player-data preservation on a private installation copy.</summary>
internal static class MapInstallationVerification
{
    /// <summary>
    /// Opt-in installer transaction on a private copy of the shared installation, which the repository's fresh import
    /// produced (#1532): startup that never opens the ROM, cancelled and real format upgrades, the no-op restart and
    /// override preservation. All ROM/audio/fixture data stays in ignored test-temp.
    /// </summary>
    [PrivateInstallation("Repairs and upgrades a private copy of the shared installation to check startup that never opens the ROM, cancelled and real format upgrades, the no-op restart and override preservation.")]
    internal static void VerifyMapInstallation(string sourceRom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopy();
        string root = copy.Root;
        byte[] sourceHash = SHA256.HashData(File.ReadAllBytes(sourceRom));
        Console.WriteLine($"Installation fixture: {root}");
        var installation = copy.Installation;
        if (OperatingSystem.IsWindows())
        {
            // An exclusive handle makes a startup ROM read fail deterministically.
            // A complete installation must instead validate and use extracted assets.
            using var lockedRom = new FileStream(installation.RomPath, FileMode.Open,
                FileAccess.Read, FileShare.None);
            AssertTrue(GameAssetInstaller.OpenOrRepair(root) is not null,
                "complete host startup does not open the installed ROM");
        }
        var stock = installation.LoadMaps();
        var projectileStock = installation.LoadProjectiles();
        Directory.CreateDirectory(installation.ProjectileOverrideDirectory);
        string projectileOverride = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileSpriteDefinitions.FileName);
        var projectileDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileSpriteDefinitions.FileName)));
        var part = (projectileDocument.Required("frames").AsObject().First(pair => (pair.Value ?? throw new InvalidOperationException("Verification requires pair.Value.")).AsArray().Count > 0).Value ?? throw new InvalidOperationException("Verification requires a non-empty frame.")).Required(0);
        int offset = part.Required("offsetX").GetValue<int>();
        part["offsetX"] = offset == 255 ? 254 : offset + 1;
        File.WriteAllText(projectileOverride, projectileDocument.ToJsonString());
        string beamName = BeamTileAtlasDefinitions.FileName(0);
        string beamOverride = Path.Combine(installation.ProjectileOverrideDirectory, beamName);
        IndexedPngImage beamImage;
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, beamName)))
            beamImage = IndexedPng.Read(input, new(BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height));
        beamImage.Pixels[0] ^= 1;
        using (var output = File.Create(beamOverride))
            IndexedPng.Write(output, new(beamImage.Size.X, beamImage.Size.Y), beamImage.Pixels, beamImage.Colours);
        string beamPaletteOverride = Path.Combine(installation.ProjectileOverrideDirectory, BeamPaletteFile.FileName);
        var beamColors = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, BeamPaletteFile.FileName)));
        beamColors.Required("palettes").Required(BeamPaletteFile.Key(0)).Required(0)["red"] = 17;
        File.WriteAllText(beamPaletteOverride, beamColors.ToJsonString());
        string trailOverride = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailVisualDefinitions.FileName);
        var trailDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileTrailVisualDefinitions.FileName)));
        trailDocument.Required("frames").Required(ProjectileTrailVisualDefinitions.FrameNames[0])["flipX"] = true;
        File.WriteAllText(trailOverride, trailDocument.ToJsonString());
        string trailPngOverride = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailAtlasDefinitions.FileName);
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, ProjectileTrailAtlasDefinitions.FileName)))
        {
            var trailImage = IndexedPng.Read(input, new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
            trailImage.Pixels[0] ^= 1;
            using var output = File.Create(trailPngOverride);
            IndexedPng.Write(output, new(trailImage.Size.X, trailImage.Size.Y), trailImage.Pixels, trailImage.Colours);
        }
        var projectileEdited = installation.LoadProjectiles();
        string flareOverride = Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlarePlacementExtractor.FileName);
        var flareDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ChargeFlarePlacementExtractor.FileName)));
        flareDocument.Required("offsets").Required(ChargeFlarePlacementFile.Key(false, 0))["x"] = 17;
        File.WriteAllText(flareOverride, flareDocument.ToJsonString());
        string flareCompositionOverride = Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlareSpriteExtractor.FileName);
        var flareComposition = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ChargeFlareSpriteExtractor.FileName)));
        flareComposition.Required("frames").Required(ChargeFlareSpriteSelection.Selectors[0].Name).Required(0)["offsetX"] = 17;
        File.WriteAllText(flareCompositionOverride, flareComposition.ToJsonString());
        string grappleOverride = Path.Combine(installation.ProjectileOverrideDirectory, GrappleTileDefinitions.FileName);
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, GrappleTileDefinitions.FileName)))
        {
            var grappleImage = IndexedPng.Read(input, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
            grappleImage.Pixels[0] ^= 1;
            using var output = File.Create(grappleOverride);
            IndexedPng.Write(output, new(grappleImage.Size.X, grappleImage.Size.Y), grappleImage.Pixels, grappleImage.Colours);
        }
        string grappleStyleOverride = Path.Combine(installation.ProjectileOverrideDirectory, GrappleSpriteDefinitions.FileName);
        var grappleStyle = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleSpriteDefinitions.FileName)));
        grappleStyle.Required("endpoint")["palette"] = 4;
        File.WriteAllText(grappleStyleOverride, grappleStyle.ToJsonString());
        var grappleFlare = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleFlarePlacementDefinitions.FileName)));
        grappleFlare.Required("offsets").Required(ChargeFlarePlacementFile.Key(false, 0))["x"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleFlarePlacementDefinitions.FileName), grappleFlare.ToJsonString());
        var grappleSwing = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleSwingFrameExtractor.FileName)));
        grappleSwing.Required("frames")[0] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleSwingFrameExtractor.FileName), grappleSwing.ToJsonString());
        projectileEdited = installation.LoadProjectiles();
        AssertTrue(projectileStock.SelectedSha256 != projectileEdited.SelectedSha256, "installed projectile override selected");
        // Synthetic sentinels, not a copy of the player's real files.
        var preserved = new Dictionary<string, byte[]>
        {
            [projectileOverride] = File.ReadAllBytes(projectileOverride),
            [beamOverride] = File.ReadAllBytes(beamOverride),
            [beamPaletteOverride] = File.ReadAllBytes(beamPaletteOverride),
            [trailOverride] = File.ReadAllBytes(trailOverride),
            [trailPngOverride] = File.ReadAllBytes(trailPngOverride),
            [flareOverride] = File.ReadAllBytes(flareOverride),
            [flareCompositionOverride] = File.ReadAllBytes(flareCompositionOverride),
            [grappleOverride] = File.ReadAllBytes(grappleOverride),
            [Path.Combine(root, "SuperMetroid.ini")] = "[Testing]\nInvincibility=true\n"u8.ToArray(),
            [Path.Combine(root, "SuperMetroid.save.json")] = "{\"fixture\":\"player-save\"}"u8.ToArray(),
            [Path.Combine(root, "SuperMetroid.srm")] = "synthetic-legacy-sram-sentinel"u8.ToArray(),
            [Path.Combine(root, "debug-states", "SuperMetroid-debug-slot-0.smstate")] = "synthetic-state-sentinel"u8.ToArray(),
            [Path.Combine(root, "input-recordings", "session.inputs")] = new byte[] { 0, 1, 0, 2 }
        };
        foreach (var pair in preserved)
        {
            Directory.CreateDirectory((Path.GetDirectoryName(pair.Key) ?? throw new InvalidOperationException("Verification requires Path.GetDirectoryName(pair.Key).")));
            File.WriteAllBytes(pair.Key, pair.Value);
        }
        string manifestPath = Path.Combine(installation.MapDirectory, AreaMapCatalogFormat.ManifestFile);
        var manifest = RequiredJson.Parse(File.ReadAllText(manifestPath));
        manifest["version"] = AreaMapCatalogFormat.Version - 1;
        File.WriteAllText(manifestPath, manifest.ToJsonString());
        // The receipt alone decides currency (#1532): content written by an older importer names its older format.
        MarkReceiptOutdated();
        byte[] outdatedManifest = File.ReadAllBytes(manifestPath);

        using (var cancellation = new CancellationTokenSource())
        {
            var cancelBeforePublish = new ImmediateInstallProgress(message =>
            {
                if (message.StartsWith("Finishing setup", StringComparison.Ordinal)) cancellation.Cancel();
            });
            AssertThrows<OperationCanceledException>(() => GameAssetInstaller.EnsureInstalled(root, cancellation.Token, cancelBeforePublish),
                "cancelled full extraction does not publish its staging directory");
        }
        AssertTrue(outdatedManifest.SequenceEqual(File.ReadAllBytes(manifestPath)), "cancelled upgrade retains original installed catalog");
        AssertTrue(!Directory.EnumerateDirectories(root, ".game.install-*").Any(), "cancelled upgrade cleans its own staging directory");
        CheckPreserved();
        var repaired = GameAssetInstaller.EnsureInstalled(root, progress: new ImmediateInstallProgress())
            ?? throw new InvalidOperationException("Installation vanished during map upgrade.");
        AssertEqual(stock.ContentIdentity, repaired.LoadMaps().ContentIdentity, "full installer upgrade reinstalls the same map presentation");
        AssertEqual(projectileEdited.SelectedSha256, repaired.LoadProjectiles().SelectedSha256, "full installer replacement preserves projectile override");
        AssertEqual(AreaMapCatalogFormat.Version, RequiredJson.Parse(File.ReadAllText(manifestPath)).Required("version").GetValue<int>(), "upgrade publishes current catalog version");
        CheckPreserved();
        // An older installation whose map manifest is already current still upgrades the rest of its content.
        string projectileManifestPath = Path.Combine(installation.ProjectileDirectory, ProjectilePresentationFiles.ManifestFileName);
        var oldProjectileManifest = RequiredJson.Parse(File.ReadAllText(projectileManifestPath));
        oldProjectileManifest["version"] = 1;
        oldProjectileManifest.AsObject().Remove("beamHashes");
        File.WriteAllText(projectileManifestPath, oldProjectileManifest.ToJsonString());
        MarkReceiptOutdated();
        var beamUpgrade = GameAssetInstaller.EnsureInstalled(root)
            ?? throw new InvalidOperationException("Installation vanished during beam upgrade.");
        AssertEqual(projectileEdited.SelectedSha256, beamUpgrade.LoadProjectiles().SelectedSha256, "composition-only upgrade retains beam and composition overrides");
        CheckPreserved();
        var unchanged = GameAssetInstaller.EnsureInstalled(root, progress: new ImmediateInstallProgress(_ =>
            throw new InvalidOperationException("Complete installation unexpectedly extracted again.")));
        AssertTrue(unchanged is not null, "complete installation fast path remains available");
        AssertTrue(sourceHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(sourceRom))), "full installer leaves original ROM unchanged");
        Console.WriteLine("Full installer: fresh import, cancelled upgrade, real version replacement, no-op restart and player/override preservation pass.");

        void MarkReceiptOutdated()
        {
            string receiptPath = Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName);
            var receipt = RequiredJson.Parse(File.ReadAllText(receiptPath));
            receipt["FormatVersion"] = GameInstallationLayout.FormatVersion - 1;
            File.WriteAllText(receiptPath, receipt.ToJsonString());
        }

        void CheckPreserved()
        {
            foreach (var pair in preserved)
                AssertTrue(pair.Value.SequenceEqual(File.ReadAllBytes(pair.Key)), $"installer preserves {Path.GetRelativePath(root, pair.Key)} exactly");
        }
    }

    /// <summary>Forwards installer progress to the console and an optional verification callback.</summary>
    internal sealed class ImmediateInstallProgress(Action<string>? onReport = null) : IProgress<string>
    {
        /// <summary>Emits one installation step and forwards it to the registered callback.</summary>
        public void Report(string message) { Console.WriteLine(message); onReport?.Invoke(message); }
    }

    /// <summary>Runs the full map installation audit using the repository's retail cartridge input.</summary>
    [VerificationCommand("--map-installation")]
    internal static int MapInstallationCommand()
    {
        string installationRom = RepositoryRomPath;
        Suite(nameof(VerifyMapInstallation), () => VerifyMapInstallation(installationRom));
        return 0;
    }
}
