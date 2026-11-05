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
    /// player-data preservation. All ROM/audio/fixture data stays in ignored test-temp.
    /// </summary>
    [PrivateInstallation("Repairs and upgrades a private copy of the shared installation to check startup that never opens the ROM, cancelled and real format upgrades, the no-op restart and player-data preservation.")]
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
        // Synthetic sentinels, not a copy of the player's real files.
        var preserved = new Dictionary<string, byte[]>
        {
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
        AssertEqual(AreaMapCatalogFormat.Version, RequiredJson.Parse(File.ReadAllText(manifestPath)).Required("version").GetValue<int>(), "upgrade publishes current catalog version");
        CheckPreserved();
        var unchanged = GameAssetInstaller.EnsureInstalled(root, progress: new ImmediateInstallProgress(_ =>
            throw new InvalidOperationException("Complete installation unexpectedly extracted again.")));
        AssertTrue(unchanged is not null, "complete installation fast path remains available");
        AssertTrue(sourceHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(sourceRom))), "full installer leaves original ROM unchanged");
        Console.WriteLine("Full installer: fresh import, cancelled upgrade, real version replacement, no-op restart and player-data preservation pass.");

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
