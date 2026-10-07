using System.Text.Json;
using System.Security.Cryptography;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Copies a supplied ROM and extracts runtime resources into an app-owned installation.</summary>
public static partial class GameAssetInstaller
{
    public static string DesktopRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SuperMetroid");

    /// <summary>Copies, never moves, a user's input. The input file is closed before replacing installed content.</summary>
    public static GameInstallation Install(string sourcePath, string root,
        CancellationToken cancellationToken = default, IProgress<string>? progress = null)
    {
        byte[] rom;
        progress?.Report("Checking ROM…");
        using (Stream source = File.OpenRead(sourcePath)) rom = SupportedCartridge.Read(source, cancellationToken);
        return InstallValidated(rom, root, cancellationToken, progress);
    }

    /// <summary>Supports Android document-provider streams, including streams without seek or length support.</summary>
    public static GameInstallation Install(Stream source, string root,
        CancellationToken cancellationToken = default, IProgress<string>? progress = null)
    {
        progress?.Report("Checking ROM…");
        return InstallValidated(SupportedCartridge.Read(source, cancellationToken), root, cancellationToken, progress);
    }

    /// <summary>Uses a complete installation or rebuilds missing/outdated resources from its own validated ROM.</summary>
    public static GameInstallation? EnsureInstalled(string root,
        CancellationToken cancellationToken = default, IProgress<string>? progress = null)
    {
        var installation = new GameInstallation(Path.GetFullPath(root));
        using FileStream gate = Lock(installation.Root);
        RecoverInterruptedPublish(installation);
        if (!File.Exists(installation.RomPath)) return null;
        byte[] rom;
        using (Stream source = File.OpenRead(installation.RomPath)) rom = SupportedCartridge.Read(source, cancellationToken);
        // One validation pass decides completeness and which components a repair may keep.
        HashSet<InstallerComponent> reusable = InspectInstalledComponents(installation, out bool complete);
        return complete ? installation : ExtractAndPublish(installation, rom, reusable, cancellationToken, progress);
    }

    /// <summary>
    /// Host startup: use a complete, validated extracted-content installation
    /// without opening the private ROM. Only an incomplete installation enters
    /// the importer's cartridge-backed repair path.
    /// </summary>
    public static GameInstallation? OpenOrRepair(string root,
        CancellationToken cancellationToken = default, IProgress<string>? progress = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GameInstallation? installed = TryOpenExtractedContent(root);
        if (installed is not null) return installed;
        GameInstallation? repaired = EnsureInstalled(root, cancellationToken, progress);
        if (repaired is not null) return repaired;
        return null;
    }

    /// <summary>
    /// Opens a complete extracted-content installation without requiring the private ROM copy.
    /// This validates presentation assets and their source-revision receipt. Hosts bind
    /// these resources to a mutable-memory-only runtime; no gameplay ROM reader exists.
    /// </summary>
    public static GameInstallation? TryOpenExtractedContent(string root)
    {
        var installation = new GameInstallation(Path.GetFullPath(root));
        using FileStream gate = Lock(installation.Root);
        RecoverInterruptedPublish(installation);
        return IsExtractedContentComplete(installation) ? installation : null;
    }

    private static GameInstallation InstallValidated(byte[] rom, string root,
        CancellationToken cancellationToken, IProgress<string>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var installation = new GameInstallation(Path.GetFullPath(root));
        using FileStream gate = Lock(installation.Root);
        RecoverInterruptedPublish(installation);
        if (IsComplete(installation))
        {
            try
            {
                using Stream existing = File.OpenRead(installation.RomPath);
                SupportedCartridge.Read(existing, cancellationToken);
                return installation;
            }
            catch (InvalidDataException) { /* Replace an invalid installed image with the verified input. */ }
        }
        // A newly supplied cartridge rebuilds every component.
        return ExtractAndPublish(installation, rom, new HashSet<InstallerComponent>(), cancellationToken, progress);
    }

    private static bool IsComplete(GameInstallation installation) =>
        File.Exists(installation.RomPath) && IsExtractedContentComplete(installation);

    private static bool IsExtractedContentComplete(GameInstallation installation)
    {
        try
        {
            ValidateRequiredExtractedContent(installation);
            return true;
        }
        catch (Exception error) when (IsRepairableContentFailure(error)) { return false; }
    }

    /// <summary>
    /// Stages a complete content directory and publishes it atomically. Components in
    /// <paramref name="reusable"/> already validated in the current content and are copied;
    /// every other component is extracted from <paramref name="rom"/> and validated.
    /// </summary>
    private static GameInstallation ExtractAndPublish(GameInstallation installation, byte[] rom,
        IReadOnlySet<InstallerComponent> reusable, CancellationToken cancellationToken, IProgress<string>? progress)
    {
        string staging = Path.Combine(installation.Root, GameInstallationLayout.StagingPrefix + Guid.NewGuid().ToString("N"));
        string previous = Path.Combine(installation.Root, GameInstallationLayout.PreviousDirectoryName);
        Directory.CreateDirectory(staging);
        try
        {
            File.WriteAllBytes(Path.Combine(staging, GameInstallationLayout.RomFileName), rom);
            foreach (InstallerComponent component in Components)
            {
                string directory = Path.Combine(staging, component.DirectoryName);
                if (reusable.Contains(component))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    CopyDirectory(Path.Combine(installation.ContentDirectory, component.DirectoryName), directory);
                    continue;
                }
                foreach (InstallerStep step in component.Steps)
                {
                    progress?.Report(step.Progress);
                    cancellationToken.ThrowIfCancellationRequested();
                    step.Extract(new CartridgeImportAddressSpace(rom), directory, cancellationToken);
                    step.Validate(directory);
                }
            }
            progress?.Report("Indexing room artwork by room ID...");
            cancellationToken.ThrowIfCancellationRequested();
            RoomArtIndexFiles.Extract(staging);
            File.WriteAllText(Path.Combine(staging, GameInstallationLayout.ReceiptFileName),
                JsonSerializer.Serialize(new InstallationReceipt(GameInstallationLayout.FormatVersion,
                    SupportedCartridge.Sha256,
                    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(
                        Path.Combine(staging, RoomArtIndexFiles.FileName)))))));
            progress?.Report("Finishing setup…");
            cancellationToken.ThrowIfCancellationRequested();
            // These are fixed app-owned content directories. Player saves, recordings,
            // configuration and the original chosen ROM are outside this transaction.
            if (Directory.Exists(previous)) Directory.Delete(previous, recursive: true);
            if (Directory.Exists(installation.ContentDirectory)) Directory.Move(installation.ContentDirectory, previous);
            try { Directory.Move(staging, installation.ContentDirectory); }
            catch
            {
                RecoverInterruptedPublish(installation);
                throw;
            }
            return installation;
        }
        finally
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true);
        }
    }

    private static void RecoverInterruptedPublish(GameInstallation installation)
    {
        string previous = Path.Combine(installation.Root, GameInstallationLayout.PreviousDirectoryName);
        if (!Directory.Exists(installation.ContentDirectory) && Directory.Exists(previous))
            Directory.Move(previous, installation.ContentDirectory);
    }

    private static FileStream Lock(string root)
    {
        Directory.CreateDirectory(root);
        try { return new FileStream(Path.Combine(root, GameInstallationLayout.LockFileName), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException error) { throw new IOException("Game setup is already in use. Close the other setup window and retry.", error); }
    }

    private sealed record InstallationReceipt(int FormatVersion, string RomSha256,
        string RoomArtIndexSha256);
}
