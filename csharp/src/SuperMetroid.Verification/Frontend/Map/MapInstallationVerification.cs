using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Security.Cryptography;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Frontend.Map;

using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Verifies installer startup, upgrade and player-data preservation on a private map-only installation.</summary>
internal static class MapInstallationVerification
{
    /// <summary>
    /// The installer's transaction on a private installation of the map component alone (#1680): startup that never opens
    /// the ROM, cancelled and real format upgrades, the no-op restart and player-data preservation. The transaction is the
    /// same whatever components it extracts, so the map component keeps each extraction small. All ROM and fixture data
    /// stays in ignored test-temp.
    /// </summary>
    [PrivateInstallation("Installs the map component alone into a private scratch root and upgrades it, to check the installer's transaction: startup that never opens the ROM, cancelled and real format upgrades, the no-op restart and player-data preservation.")]
    internal static void VerifyMapInstallation(string sourceRom)
    {
        using var scratch = new TestScratchDirectory("map-installation");
        string root = scratch.Root;
        byte[] sourceHash = SHA256.HashData(File.ReadAllBytes(sourceRom));
        GameAssetInstaller.InstallerComponent[] mapComponent =
            [.. GameAssetInstaller.Components.Where(component => component.DirectoryName == GameInstallationLayout.MapDirectoryName)];
        AssertEqual(1, mapComponent.Length, "the installer has one map component");
        GameInstallation installation = GameAssetInstaller.Install(sourceRom, root, mapComponent, CancellationToken.None,
            new ImmediateInstallProgress());
        Console.WriteLine($"Installation fixture: {root}");
        if (OperatingSystem.IsWindows())
        {
            // An exclusive handle makes a startup ROM read fail deterministically.
            // A complete installation must instead validate and use extracted assets.
            using var lockedRom = new FileStream(installation.RomPath, FileMode.Open,
                FileAccess.Read, FileShare.None);
            AssertTrue(GameAssetInstaller.OpenOrRepair(root) is not null,
                "complete host startup does not open the installed ROM");
        }
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
        // An installed map document stands for the content an older importer wrote.
        string documentPath = Path.Combine(installation.MapDirectory, AreaMapCatalogFormat.StationRevealFile);
        File.WriteAllBytes(documentPath, [.. File.ReadAllBytes(documentPath), .. "\n"u8]);
        // The receipt alone decides currency (#1532): content written by an older importer names its older format.
        MarkReceiptOutdated();
        byte[] outdatedDocument = File.ReadAllBytes(documentPath);

        using (var cancellation = new CancellationTokenSource())
        {
            var cancelBeforePublish = new ImmediateInstallProgress(message =>
            {
                if (message.StartsWith("Finishing setup", StringComparison.Ordinal)) cancellation.Cancel();
            });
            AssertThrows<OperationCanceledException>(() => GameAssetInstaller.EnsureInstalled(root, mapComponent, cancellation.Token, cancelBeforePublish),
                "cancelled extraction does not publish its staging directory");
        }
        AssertTrue(outdatedDocument.SequenceEqual(File.ReadAllBytes(documentPath)), "cancelled upgrade retains the installed content");
        AssertTrue(!Directory.EnumerateDirectories(root, ".game.install-*").Any(), "cancelled upgrade cleans its own staging directory");
        CheckPreserved();
        _ = GameAssetInstaller.EnsureInstalled(root, mapComponent, CancellationToken.None, new ImmediateInstallProgress())
            ?? throw new InvalidOperationException("Installation vanished during map upgrade.");
        AssertTrue(!outdatedDocument.SequenceEqual(File.ReadAllBytes(documentPath)), "upgrade replaces the content the older importer wrote");
        CheckPreserved();
        var unchanged = GameAssetInstaller.EnsureInstalled(root, mapComponent, CancellationToken.None, new ImmediateInstallProgress(_ =>
            throw new InvalidOperationException("Complete installation unexpectedly extracted again.")));
        AssertTrue(unchanged is not null, "complete installation fast path remains available");
        AssertTrue(sourceHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(sourceRom))), "installer leaves original ROM unchanged");
        Console.WriteLine("Installer: fresh import, cancelled upgrade, real version replacement, no-op restart and player-data preservation pass.");

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

    /// <summary>Runs the map installation's installer transaction check using the repository's retail cartridge input.</summary>
    [VerificationCommand("--map-installation")]
    internal static int MapInstallationCommand()
    {
        string installationRom = RepositoryRomPath;
        Suite(nameof(VerifyMapInstallation), () => VerifyMapInstallation(installationRom));
        return 0;
    }
}
