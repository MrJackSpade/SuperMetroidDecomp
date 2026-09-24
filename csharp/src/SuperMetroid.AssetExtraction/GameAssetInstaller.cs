using System.Text.Json;
using System.Security.Cryptography;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Copies a supplied ROM and extracts runtime resources into an app-owned installation.</summary>
public static class GameAssetInstaller
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
        return IsComplete(installation) ? installation : ExtractAndPublish(installation, rom, cancellationToken, progress);
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
        return ExtractAndPublish(installation, rom, cancellationToken, progress);
    }

    private static bool IsComplete(GameInstallation installation)
    {
        try
        {
            if (!File.Exists(installation.RomPath)) return false;
            var receipt = JsonSerializer.Deserialize<InstallationReceipt>(
                File.ReadAllText(Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName)));
            if (receipt is null || receipt.FormatVersion != GameInstallationLayout.FormatVersion ||
                receipt.RomSha256 != SupportedCartridge.Sha256) return false;
            if (receipt.RoomArtIndexSha256 != Convert.ToHexString(
                    SHA256.HashData(File.ReadAllBytes(installation.RoomArtIndexPath)))) return false;
            _ = RoomArtIndexFiles.Load(installation.ContentDirectory);
            // This verifies hashes and opens every generated stream and waveform, not just the receipt.
            ExtractedAudioAssetCatalog.Load(installation.AudioDirectory);
            AreaMapPresentationCatalog.ValidateStock(installation.MapDirectory);
            _ = ProjectilePresentationFiles.Load(installation.ProjectileDirectory, null);
            RoomCharacterArtworkFiles.ValidateStock(installation.RoomCharacterDirectory);
            IntroCinematicArtworkFiles.ValidateStock(installation.IntroCinematicDirectory);
            EndingMode7ArtworkFiles.ValidateStock(installation.EndingMode7Directory);
            RoomStaticPaletteArtworkFiles.ValidateStock(installation.RoomPaletteDirectory);
            RoomMetatileArtworkFiles.ValidateStock(installation.RoomMetatileDirectory);
            RoomBackgroundTilemapArtworkFiles.ValidateStock(installation.RoomBackgroundTilemapDirectory);
            RoomVisualLayoutFiles.ValidateStock(installation.RoomVisualLayoutDirectory);
            XrayRevealVisualFiles.ValidateStock(installation.XrayRevealVisualDirectory);
            RoomSkyTilemapArtworkFiles.ValidateStock(installation.RoomBackgroundTilemapDirectory);
            return true;
        }
        catch (IOException) { return false; }
        catch (InvalidDataException) { return false; }
        catch (JsonException) { return false; }
        catch (InvalidOperationException) { return false; }
    }

    private static GameInstallation ExtractAndPublish(GameInstallation installation, byte[] rom,
        CancellationToken cancellationToken, IProgress<string>? progress)
    {
        string staging = Path.Combine(installation.Root, GameInstallationLayout.StagingPrefix + Guid.NewGuid().ToString("N"));
        string previous = Path.Combine(installation.Root, GameInstallationLayout.PreviousDirectoryName);
        Directory.CreateDirectory(staging);
        try
        {
            File.WriteAllBytes(Path.Combine(staging, GameInstallationLayout.RomFileName), rom);
            progress?.Report("Extracting audio…");
            cancellationToken.ThrowIfCancellationRequested();
            string audio = Path.Combine(staging, GameInstallationLayout.AudioDirectoryName);
            SpcAudioAssetExtractor.Extract(new SuperMetroidAddressSpace(rom), audio);
            cancellationToken.ThrowIfCancellationRequested();
            ExtractedAudioAssetCatalog.Load(audio);
            progress?.Report("Extracting map presentation...");
            string maps = Path.Combine(staging, GameInstallationLayout.MapDirectoryName);
            MapPresentationExtractor.Extract(new SuperMetroidAddressSpace(rom), maps,
                SupportedCartridge.Sha256, cancellationToken);
            AreaMapPresentationCatalog.ValidateStock(maps);
            progress?.Report("Extracting projectile compositions...");
            cancellationToken.ThrowIfCancellationRequested();
            ProjectilePresentationFiles.Extract(new SuperMetroidAddressSpace(rom),
                Path.Combine(staging, GameInstallationLayout.ProjectileDirectoryName));
            progress?.Report("Extracting room character artwork...");
            cancellationToken.ThrowIfCancellationRequested();
            string roomCharacters = Path.Combine(staging, GameInstallationLayout.RoomCharacterDirectoryName);
            RoomCharacterArtworkFiles.Extract(new SuperMetroidAddressSpace(rom), roomCharacters,
                SupportedCartridge.Sha256);
            RoomCharacterArtworkFiles.ValidateStock(roomCharacters);
            progress?.Report("Extracting opening-cinematic character artwork...");
            cancellationToken.ThrowIfCancellationRequested();
            string introArtwork = Path.Combine(staging, GameInstallationLayout.IntroCinematicDirectoryName);
            IntroCinematicArtworkFiles.Extract(new SuperMetroidAddressSpace(rom), introArtwork,
                SupportedCartridge.Sha256);
            IntroCinematicArtworkFiles.ValidateStock(introArtwork);
            progress?.Report("Extracting ending Mode-7 artwork...");
            cancellationToken.ThrowIfCancellationRequested();
            string endingArt = Path.Combine(staging, GameInstallationLayout.EndingMode7DirectoryName);
            EndingMode7ArtworkFiles.Extract(new SuperMetroidAddressSpace(rom), endingArt,
                SupportedCartridge.Sha256);
            EndingMode7ArtworkFiles.ValidateStock(endingArt);
            progress?.Report("Extracting room base palettes...");
            cancellationToken.ThrowIfCancellationRequested();
            string roomPalettes = Path.Combine(staging, GameInstallationLayout.RoomPaletteDirectoryName);
            RoomStaticPaletteArtworkFiles.Extract(new SuperMetroidAddressSpace(rom), roomPalettes,
                SupportedCartridge.Sha256);
            RoomStaticPaletteArtworkFiles.ValidateStock(roomPalettes);
            progress?.Report("Extracting room block compositions...");
            cancellationToken.ThrowIfCancellationRequested();
            string roomMetatiles = Path.Combine(staging, GameInstallationLayout.RoomMetatileDirectoryName);
            RoomMetatileArtworkFiles.Extract(new SuperMetroidAddressSpace(rom), roomMetatiles,
                SupportedCartridge.Sha256);
            RoomMetatileArtworkFiles.ValidateStock(roomMetatiles);
            progress?.Report("Extracting room background tilemaps...");
            cancellationToken.ThrowIfCancellationRequested();
            string roomBackgrounds = Path.Combine(staging,
                GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
            RoomBackgroundTilemapArtworkFiles.Extract(new SuperMetroidAddressSpace(rom),
                roomBackgrounds, SupportedCartridge.Sha256);
            RoomBackgroundTilemapArtworkFiles.ValidateStock(roomBackgrounds);
            progress?.Report("Extracting scrolling-sky tilemaps...");
            cancellationToken.ThrowIfCancellationRequested();
            RoomSkyTilemapArtworkFiles.Extract(new SuperMetroidAddressSpace(rom),
                roomBackgrounds, SupportedCartridge.Sha256);
            RoomSkyTilemapArtworkFiles.ValidateStock(roomBackgrounds);
            progress?.Report("Extracting room visual layouts...");
            cancellationToken.ThrowIfCancellationRequested();
            string roomLayouts = Path.Combine(staging,
                GameInstallationLayout.RoomVisualLayoutDirectoryName);
            RoomVisualLayoutFiles.Extract(new SuperMetroidAddressSpace(rom),
                roomLayouts, SupportedCartridge.Sha256);
            RoomVisualLayoutFiles.ValidateStock(roomLayouts);
            progress?.Report("Extracting X-ray reveal visuals...");
            cancellationToken.ThrowIfCancellationRequested();
            string xrayReveals = Path.Combine(staging,
                GameInstallationLayout.XrayRevealVisualDirectoryName);
            XrayRevealVisualFiles.Extract(new SuperMetroidAddressSpace(rom),
                xrayReveals, SupportedCartridge.Sha256);
            XrayRevealVisualFiles.ValidateStock(xrayReveals);
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
