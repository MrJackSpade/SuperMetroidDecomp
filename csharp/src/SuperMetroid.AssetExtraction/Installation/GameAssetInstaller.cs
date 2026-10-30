using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Copies a supplied ROM and extracts runtime resources into an app-owned installation.</summary>
public static class GameAssetInstaller
{
    /// <summary>Default per-user Desktop installation root beneath Local Application Data.</summary>
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

    /// <summary>
    /// Uses a complete installation or rebuilds missing/outdated resources from its own validated ROM.
    /// Published content never changes, so any number of processes may open a complete installation
    /// at once; only recovery and extraction write it, and those hold the setup lock.
    /// </summary>
    public static GameInstallation? EnsureInstalled(string root,
        CancellationToken cancellationToken = default, IProgress<string>? progress = null)
    {
        var installation = new GameInstallation(Path.GetFullPath(root));
        if (Inspect(installation, cancellationToken) is { Complete: true }) return installation;
        using FileStream gate = Lock(installation.Root);
        RecoverInterruptedPublish(installation);
        if (Inspect(installation, cancellationToken) is not { } state) return null;
        return state.Complete ? installation : ExtractAndPublish(installation, state.Rom, state.Reusable, cancellationToken, progress);
    }

    /// <summary>
    /// Reads the installation's own ROM and validates its content in one pass, which decides
    /// completeness and which components a repair may keep. Null when no ROM is installed.
    /// </summary>
    private static InstalledState? Inspect(GameInstallation installation, CancellationToken cancellationToken)
    {
        if (!File.Exists(installation.RomPath)) return null;
        byte[] rom;
        using (Stream source = File.OpenRead(installation.RomPath)) rom = SupportedCartridge.Read(source, cancellationToken);
        HashSet<InstallerComponent> reusable = InspectInstalledComponents(installation, out bool complete);
        return new InstalledState(rom, reusable, complete);
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
        if (IsExtractedContentComplete(installation)) return installation;
        using FileStream gate = Lock(installation.Root);
        RecoverInterruptedPublish(installation);
        return IsExtractedContentComplete(installation) ? installation : null;
    }

    /// <summary>Serializes setup, reuses a complete verified installation or stages all components from a newly supplied ROM.</summary>
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

    /// <summary>Requires both an installed cartridge copy and a valid extracted-content receipt and inventory.</summary>
    private static bool IsComplete(GameInstallation installation) =>
        File.Exists(installation.RomPath) && IsExtractedContentComplete(installation);

    /// <summary>Treats missing or malformed installed assets as repairable incompleteness after strict validation.</summary>
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
            // Every component validated above; the receipt records the bytes it published so opening the
            // installation later checks them by digest instead of decoding every resource again (#1430).
            File.WriteAllText(Path.Combine(staging, GameInstallationLayout.ReceiptFileName),
                JsonSerializer.Serialize(new InstallationReceipt(GameInstallationLayout.FormatVersion,
                    SupportedCartridge.Sha256, DigestPublishedFiles(staging))));
            progress?.Report("Finishing setup…");
            cancellationToken.ThrowIfCancellationRequested();
            // Before publishing: a refused rename leaves the old content in place, so the next start retries it.
            RoomArtworkOverrideNames.Migrate(installation);
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

    /// <summary>Restores the previous content directory when a publish stopped after moving the old directory aside.</summary>
    private static void RecoverInterruptedPublish(GameInstallation installation)
    {
        string previous = Path.Combine(installation.Root, GameInstallationLayout.PreviousDirectoryName);
        if (!Directory.Exists(installation.ContentDirectory) && Directory.Exists(previous))
            Directory.Move(previous, installation.ContentDirectory);
    }

    /// <summary>Opens the setup lock exclusively so only one repair or extraction publishes at a time.</summary>
    internal static FileStream Lock(string root)
    {
        Directory.CreateDirectory(root);
        try { return new FileStream(Path.Combine(root, GameInstallationLayout.LockFileName), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException error) { throw new IOException("Game setup is already in use. Close the other setup window and retry.", error); }
    }

    /// <summary>
    /// Published content format, verified source-revision digest, and the SHA-256 of every published file by its
    /// content-relative path.
    /// </summary>
    private sealed record InstallationReceipt(int FormatVersion, string RomSha256, IReadOnlyDictionary<string, string> Files);

    /// <summary>Verified installed ROM plus component reuse decisions from one inspection pass.</summary>
    private sealed record InstalledState(byte[] Rom, HashSet<InstallerComponent> Reusable, bool Complete);

    /// <summary>One extraction stage: its progress text, extractor, and stock validator.</summary>
    private sealed record InstallerStep(
        string Progress,
        Action<CartridgeImportAddressSpace, string, CancellationToken> Extract,
        Action<string> Validate);

    /// <summary>
    /// One extracted-content directory. Its steps' stock validators run when it is extracted; a repair keeps a component
    /// whose published files are intact and re-extracts only the components that are not, then republishes atomically.
    /// </summary>
    private sealed record InstallerComponent(string DirectoryName, params InstallerStep[] Steps);

    /// <summary>Adapts an extraction without cancellation parameters into one installer stage.</summary>
    private static InstallerStep Step(string progress,
        Action<CartridgeImportAddressSpace, string> extract, Action<string> validate) =>
        new(progress, (bus, directory, _) => extract(bus, directory), validate);

    /// <summary>The installation a component directory belongs to.</summary>
    /// <exception cref="InvalidOperationException">The directory has no installation above it.</exception>
    private static string InstallationOf(string componentDirectory) => Path.GetDirectoryName(componentDirectory)
        ?? throw new InvalidOperationException($"Component directory {componentDirectory} has no installation above it.");

    /// <summary>
    /// Every extracted-content component, in extraction order. The sheets come last: they compose from the stock
    /// artwork every earlier component has installed beside them.
    /// </summary>
    private static readonly InstallerComponent[] Components =
    [
        new(GameInstallationLayout.AudioDirectoryName,
            Step("Extracting audio…", (bus, d) => SpcAudioAssetExtractor.Extract(bus, d), d => _ = ExtractedAudioAssetCatalog.Load(d)),
            Step("Rendering audio clips…", (_, d) => AudioClipExporter.Export(d), InstalledAudioClips.ValidateStock)),
        new(GameInstallationLayout.MapDirectoryName,
            new InstallerStep("Extracting map presentation...",
                (bus, d, cancellation) => MapPresentationExtractor.Extract(bus, d, SupportedCartridge.Sha256, cancellation),
                MapPresentationFiles.ValidateStock)),
        new(GameInstallationLayout.GameplayBasePaletteDirectoryName,
            Step("Extracting gameplay base palettes...", (bus, d) => GameplayBasePaletteFiles.Extract(bus, d, SupportedCartridge.Sha256), GameplayBasePaletteFiles.ValidateStock)),
        new(GameInstallationLayout.SamusOverreadDirectoryName,
            Step("Extracting Samus overread bytes...", (bus, d) => SamusOverreadFiles.Extract(bus, d, SupportedCartridge.Sha256), SamusOverreadFiles.ValidateStock)),
        new(GameInstallationLayout.StandardObjectDirectoryName,
            Step("Extracting standard sprite artwork...", (bus, d) => StandardObjectArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), StandardObjectArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.ProjectileDirectoryName,
            Step("Extracting projectile compositions...", (bus, d) => ProjectilePresentationFiles.Extract(bus, d), d => _ = ProjectilePresentationFiles.Load(d, null))),
        new(GameInstallationLayout.EnemyTileDirectoryName,
            Step("Extracting ordinary enemy tile sheets...", (bus, d) => EnemyTileArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), EnemyTileArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomCharacterDirectoryName,
            Step("Extracting room character artwork...", (bus, d) => RoomCharacterArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomCharacterArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.SamusBodyDirectoryName,
            Step("Extracting Samus body artwork...", (bus, d) => SamusBodyArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), SamusBodyArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.IntroCinematicDirectoryName,
            Step("Extracting opening-cinematic character artwork...", (bus, d) => IntroCinematicArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), IntroCinematicArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.EndingMode7DirectoryName,
            Step("Extracting ending Mode-7 artwork...", (bus, d) => EndingMode7ArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), EndingMode7ArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.EndingObjectDirectoryName,
            Step("Extracting ending object artwork...", (bus, d) => EndingObjectArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), EndingObjectArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.EndingPaletteDirectoryName,
            Step("Extracting ending palettes...", (bus, d) => EndingPaletteArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), EndingPaletteArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPaletteDirectoryName,
            Step("Extracting room base palettes...", (bus, d) => RoomStaticPaletteArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomStaticPaletteArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomMetatileDirectoryName,
            Step("Extracting room block compositions...", (bus, d) => RoomMetatileArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomMetatileArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomBackgroundTilemapDirectoryName,
            Step("Extracting room background tilemaps...", (bus, d) => RoomBackgroundTilemapArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomBackgroundTilemapArtworkFiles.ValidateStock),
            Step("Extracting scrolling-sky tilemaps...", (bus, d) => RoomSkyTilemapArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomSkyTilemapArtworkFiles.ValidateStock)),
        new(GameInstallationLayout.RoomVisualLayoutDirectoryName,
            Step("Extracting room visual layouts...", (bus, d) => RoomVisualLayoutFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomVisualLayoutFiles.ValidateStock)),
        new(GameInstallationLayout.RoomLevelStreamDirectoryName,
            Step("Extracting room terrain...", (bus, d) => RoomLevelStreamFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomLevelStreamFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName,
            Step("Extracting shot-block PLM visuals...", (bus, d) => RoomPlmShotBlockVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmShotBlockVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName,
            Step("Extracting Grapple-block PLM visuals...", (bus, d) => RoomPlmGrappleBlockVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmGrappleBlockVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmStationVisualDirectoryName,
            Step("Extracting station PLM visuals...", (bus, d) => RoomPlmStationVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmStationVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName,
            Step("Extracting blue-door PLM visuals...", (bus, d) => RoomPlmBlueDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmBlueDoorVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName,
            Step("Extracting colored-door PLM visuals...", (bus, d) => RoomPlmColoredDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmColoredDoorVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName,
            Step("Extracting grey-door PLM visuals...", (bus, d) => RoomPlmGreyDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmGreyDoorVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName,
            Step("Extracting eye-door PLM visuals...", (bus, d) => RoomPlmEyeDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmEyeDoorVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName,
            Step("Extracting Mother Brain glass PLM visuals...", (bus, d) => RoomPlmMotherBrainGlassVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmMotherBrainGlassVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName,
            Step("Extracting n00b-tube PLM visuals...", (bus, d) => RoomPlmNoobTubeVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmNoobTubeVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName,
            Step("Extracting downward-gate PLM visuals...", (bus, d) => RoomPlmDownwardGateVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmDownwardGateVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName,
            Step("Extracting elevator-platform PLM visuals...", (bus, d) => RoomPlmElevatorPlatformVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmElevatorPlatformVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName,
            Step("Extracting Mother Brain escape-gate visuals...", (bus, d) => RoomPlmEscapeGateVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmEscapeGateVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName,
            Step("Extracting Bomb Torizo hand visuals...", (bus, d) => RoomPlmBombTorizoHandVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmBombTorizoHandVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName,
            Step("Extracting Draygon cannon visuals...", (bus, d) => RoomPlmDraygonCannonVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmDraygonCannonVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName,
            Step("Extracting Chozo statue terrain visuals...", (bus, d) => RoomPlmChozoStatueVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmChozoStatueVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName,
            Step("Extracting linked-block restoration visuals...", (bus, d) => RoomPlmLinkedRestoreVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmLinkedRestoreVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName,
            Step("Extracting Tourian access-floor visuals...", (bus, d) => RoomPlmTourianAccessVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmTourianAccessVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName,
            Step("Extracting Speed Booster bomb-reveal visuals...", (bus, d) => RoomPlmSpeedBoosterVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmSpeedBoosterVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName,
            Step("Extracting Maridia elevatube visuals...", (bus, d) => RoomPlmMaridiaElevatubeVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmMaridiaElevatubeVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName,
            Step("Extracting Spore Spawn ceiling visuals...", (bus, d) => RoomPlmSporeSpawnCeilingVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmSporeSpawnCeilingVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName,
            Step("Extracting Samus Eater plant visuals...", (bus, d) => RoomPlmSamusEaterVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmSamusEaterVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName,
            Step("Extracting Botwoon wall visuals...", (bus, d) => RoomPlmBotwoonWallVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmBotwoonWallVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmKraidVisualDirectoryName,
            Step("Extracting Kraid room-object visuals...", (bus, d) => RoomPlmKraidVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmKraidVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName,
            Step("Extracting Crocomire room-object visuals...", (bus, d) => RoomPlmCrocomireVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmCrocomireVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName,
            Step("Extracting Mother Brain fake-death room visuals...", (bus, d) => RoomPlmMotherBrainFakeDeathVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmMotherBrainFakeDeathVisualFiles.ValidateStock)),
        new(GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName,
            Step("Extracting collectible PLM visuals...", (bus, d) => RoomPlmCollectibleVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), RoomPlmCollectibleVisualFiles.ValidateStock)),
        new(GameInstallationLayout.XrayRevealVisualDirectoryName,
            Step("Extracting X-ray reveal visuals...", (bus, d) => XrayRevealVisualFiles.Extract(bus, d, SupportedCartridge.Sha256), XrayRevealVisualFiles.ValidateStock)),
        new(GameInstallationLayout.SpriteSheetDirectoryName,
            Step("Composing sprite sheets...", (bus, d) => SpriteSheetFiles.Extract(bus, InstallationOf(d), d, SupportedCartridge.Sha256),
                SpriteSheetFiles.ValidateStock)),
        new(GameInstallationLayout.RoomBlockSheetDirectoryName,
            Step("Composing room block sheets...", (_, d) => RoomBlockSheetFiles.Extract(InstallationOf(d), d, SupportedCartridge.Sha256),
                RoomBlockSheetFiles.ValidateStock)),
        new(GameInstallationLayout.BossBodySheetDirectoryName,
            Step("Composing boss body sheets...", (_, d) => BossBodySheetFiles.Extract(InstallationOf(d), d, SupportedCartridge.Sha256),
                BossBodySheetFiles.ValidateStock)),
        new(GameInstallationLayout.EnemySheetDirectoryName,
            Step("Composing enemy sheets...", (_, d) => EnemySheetFiles.Extract(InstallationOf(d), d, SupportedCartridge.Sha256),
                EnemySheetFiles.ValidateStock)),
    ];

    /// <summary>Content failures a repair rebuilds from the ROM, rather than errors to surface.</summary>
    private static bool IsRepairableContentFailure(Exception error) =>
        error is IOException or InvalidDataException or JsonException or InvalidOperationException;

    /// <summary>
    /// Inspects installed content once. Returns the components whose published files are intact, which a repair keeps,
    /// and whether every component is. A receipt from another format or ROM keeps nothing.
    /// </summary>
    private static HashSet<InstallerComponent> InspectInstalledComponents(GameInstallation installation, out bool complete)
    {
        var valid = new HashSet<InstallerComponent>();
        complete = false;
        InstallationReceipt receipt;
        try { receipt = ReadReceipt(installation); }
        catch (Exception error) when (IsRepairableContentFailure(error)) { return valid; }
        foreach (InstallerComponent component in Components)
        {
            try
            {
                VerifyPublishedFiles(installation, receipt, component);
                valid.Add(component);
            }
            catch (Exception error) when (IsRepairableContentFailure(error)) { }
        }
        complete = valid.Count == Components.Length;
        return valid;
    }

    /// <summary>
    /// The SHA-256 of every file under <paramref name="contentDirectory"/>, by content-relative path, except the ROM,
    /// which is checked against the supported cartridge, and the receipt that records them.
    /// </summary>
    private static SortedDictionary<string, string> DigestPublishedFiles(string contentDirectory)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (string file in Directory.EnumerateFiles(contentDirectory, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(contentDirectory, file).Replace('\\', '/');
            if (relative is GameInstallationLayout.RomFileName or GameInstallationLayout.ReceiptFileName)
                continue;
            files.Add(relative, Digest(file));
        }
        return files;
    }

    /// <summary>The SHA-256 of <paramref name="file"/>'s bytes.</summary>
    private static string Digest(string file)
    {
        using FileStream stream = File.OpenRead(file);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    /// <summary>
    /// Requires every file <paramref name="component"/> published to still hold the bytes the receipt recorded. A missing
    /// file or directory fails with the I/O error naming it; changed bytes fail naming the file.
    /// </summary>
    private static void VerifyPublishedFiles(GameInstallation installation, InstallationReceipt receipt, InstallerComponent component)
    {
        string prefix = component.DirectoryName + "/";
        bool published = false;
        foreach ((string relative, string digest) in receipt.Files)
        {
            if (!relative.StartsWith(prefix, StringComparison.Ordinal))
                continue;
            published = true;
            string path = Path.GetFullPath(Path.Combine(installation.ContentDirectory, relative));
            if (!string.Equals(Digest(path), digest, StringComparison.Ordinal))
                throw new InvalidDataException($"Installed file {path} differs from the content published with this installation. " +
                    "Reimport the supported cartridge.");
        }
        if (!published)
            throw new InvalidDataException($"Installation receipt publishes no files for {component.DirectoryName}. Reimport the supported cartridge.");
    }

    /// <summary>Copies a previously validated component tree into the new staging directory.</summary>
    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
    }

    /// <summary>
    /// Shared strict implementation; startup may use its exception to decide whether repair is needed. Every component's
    /// stock validators ran when it was published, and published content never changes, so opening the installation
    /// checks that each published file still holds the bytes the receipt recorded (#1430).
    /// </summary>
    internal static void ValidateRequiredExtractedContent(GameInstallation installation)
    {
        InstallationReceipt receipt = ReadReceipt(installation);
        // This is the startup inventory, not a sample of whichever room was run.
        foreach (InstallerComponent component in Components)
            VerifyPublishedFiles(installation, receipt, component);
    }

    /// <summary>The receipt, which must name this format and source and list the files it published.</summary>
    private static InstallationReceipt ReadReceipt(GameInstallation installation)
    {
        string receiptPath = Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName);
        var receipt = JsonSerializer.Deserialize<InstallationReceipt>(File.ReadAllText(receiptPath));
        if (receipt is null || receipt.FormatVersion != GameInstallationLayout.FormatVersion)
            throw new InvalidDataException($"Installation receipt {receiptPath} requires format " +
                $"{GameInstallationLayout.FormatVersion}; found {receipt?.FormatVersion.ToString() ?? "null"}. Reimport the supported cartridge.");
        if (receipt.RomSha256 != SupportedCartridge.Sha256)
            throw new InvalidDataException($"Installation receipt {receiptPath} has unsupported source provenance.");
        if (receipt.Files is null)
            throw new InvalidDataException($"Installation receipt {receiptPath} lists no published files. Reimport the supported cartridge.");
        return receipt;
    }
}
