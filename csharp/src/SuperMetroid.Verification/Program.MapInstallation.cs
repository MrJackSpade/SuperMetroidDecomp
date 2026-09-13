using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>Opt-in full importer transaction; all ROM/audio/fixture data stays in ignored test-temp.</summary>
    private static void VerifyMapInstallation(string sourceRom)
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp", "map-installation-" + Guid.NewGuid().ToString("N")));
        byte[] sourceHash = SHA256.HashData(File.ReadAllBytes(sourceRom));
        Console.WriteLine($"Full installation fixture: {root}");
        var installation = GameAssetInstaller.Install(sourceRom, root, progress: new ImmediateInstallProgress());
        var stock = installation.LoadMaps();
        Directory.CreateDirectory(installation.MapOverrideDirectory);
        string paletteOverride = Path.Combine(installation.MapOverrideDirectory, MapStaticPalettesFormat.FileName);
        var colors = JsonNode.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapStaticPalettesFormat.FileName)))!;
        int oldRed = colors["pause"]![0]!["red"]!.GetValue<int>();
        colors["pause"]![0]!["red"] = (oldRed + 1) % 32;
        File.WriteAllText(paletteOverride, colors.ToJsonString());
        string stationOverride = Path.Combine(installation.MapOverrideDirectory, MapStationLayoutFormat.FileName);
        var stations = JsonNode.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapStationLayoutFormat.FileName)))!;
        stations["markers"]!["Brinstar.Missile.0"]!["x"] = 80;
        File.WriteAllText(stationOverride, stations.ToJsonString());
        string landmarkOverride = Path.Combine(installation.MapOverrideDirectory, MapLandmarkFormat.FileName);
        var landmarks = JsonNode.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapLandmarkFormat.FileName)))!;
        landmarks["markers"]!["Boss.Phantoon"]!["x"] = 160;
        File.WriteAllText(landmarkOverride, landmarks.ToJsonString());
        string saveMarkerOverride = Path.Combine(installation.MapOverrideDirectory, MapSaveMarkerFormat.FileName);
        var saveMarkers = JsonNode.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapSaveMarkerFormat.FileName)))!;
        saveMarkers["markers"]!["Maridia.Save.0"]!["x"] = 104;
        File.WriteAllText(saveMarkerOverride, saveMarkers.ToJsonString());
        var edited = installation.LoadMaps();
        AssertEqual(104, edited.SaveMarkers.Get(SuperMetroid.Core.Game.AreaId.Maridia, 0).X, "full installation consumes save-marker override");
        AssertEqual(160, edited.Landmarks.Get("Boss.Phantoon").X, "full installation consumes landmark override");
        AssertEqual(80, edited.Stations.Get("Brinstar.Missile.0").X, "full installation consumes station position override");
        AssertTrue(stock.ContentIdentity != edited.ContentIdentity, "full installation consumes edited palette override");
        // Synthetic sentinels, not a copy of the player's real files.
        var preserved = new Dictionary<string, byte[]>
        {
            [paletteOverride] = File.ReadAllBytes(paletteOverride),
            [stationOverride] = File.ReadAllBytes(stationOverride),
            [landmarkOverride] = File.ReadAllBytes(landmarkOverride),
            [saveMarkerOverride] = File.ReadAllBytes(saveMarkerOverride),
            [Path.Combine(root, "SuperMetroid.ini")] = "[Testing]\nInvincibility=true\n"u8.ToArray(),
            [Path.Combine(root, "SuperMetroid.save.json")] = "{\"fixture\":\"player-save\"}"u8.ToArray(),
            [Path.Combine(root, "SuperMetroid.srm")] = "synthetic-legacy-sram-sentinel"u8.ToArray(),
            [Path.Combine(root, "debug-states", "SuperMetroid-debug-slot-0.smstate")] = "synthetic-state-sentinel"u8.ToArray(),
            [Path.Combine(root, "input-recordings", "session.inputs")] = new byte[] { 0, 1, 0, 2 }
        };
        foreach (var pair in preserved)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pair.Key)!);
            File.WriteAllBytes(pair.Key, pair.Value);
        }
        string manifestPath = Path.Combine(installation.MapDirectory, AreaMapCatalogFormat.ManifestFile);
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
        manifest["version"] = AreaMapCatalogFormat.Version - 1;
        File.WriteAllText(manifestPath, manifest.ToJsonString());
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
        AssertTrue(outdatedManifest.AsSpan().SequenceEqual(File.ReadAllBytes(manifestPath)), "cancelled upgrade retains original installed catalog");
        AssertTrue(!Directory.EnumerateDirectories(root, ".game.install-*").Any(), "cancelled upgrade cleans its own staging directory");
        CheckPreserved();
        var repaired = GameAssetInstaller.EnsureInstalled(root, progress: new ImmediateInstallProgress())
            ?? throw new InvalidOperationException("Installation vanished during map upgrade.");
        AssertEqual(edited.ContentIdentity, repaired.LoadMaps().ContentIdentity, "full installer upgrade retains selected override identity");
        AssertEqual(AreaMapCatalogFormat.Version, JsonNode.Parse(File.ReadAllText(manifestPath))!["version"]!.GetValue<int>(), "upgrade publishes current catalog version");
        CheckPreserved();
        var unchanged = GameAssetInstaller.EnsureInstalled(root, progress: new ImmediateInstallProgress(_ =>
            throw new InvalidOperationException("Complete installation unexpectedly extracted again.")));
        AssertTrue(unchanged is not null, "complete installation fast path remains available");
        AssertTrue(sourceHash.AsSpan().SequenceEqual(SHA256.HashData(File.ReadAllBytes(sourceRom))), "full installer leaves original ROM unchanged");
        File.WriteAllText(paletteOverride, "broken override");
        _ = GameAssetInstaller.EnsureInstalled(root);
        AssertThrows<InvalidDataException>(() => repaired.LoadMaps(), "startup preserves and reports invalid user override instead of repairing it");
        AssertEqual("broken override", File.ReadAllText(paletteOverride), "installer never replaces broken user override");
        Console.WriteLine("Full installer: fresh import, cancelled upgrade, real version replacement, no-op restart and player/override preservation pass.");

        void CheckPreserved()
        {
            foreach (var pair in preserved)
                AssertTrue(pair.Value.AsSpan().SequenceEqual(File.ReadAllBytes(pair.Key)), $"installer preserves {Path.GetRelativePath(root, pair.Key)} exactly");
        }
    }

    private sealed class ImmediateInstallProgress(Action<string>? onReport = null) : IProgress<string>
    {
        public void Report(string message) { Console.WriteLine(message); onReport?.Invoke(message); }
    }
}
