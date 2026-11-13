using SuperMetroid.AssetExtraction.Artwork;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;

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
    /// Uses a current installation or extracts every resource again from its own validated ROM. Published content never
    /// changes, so any number of processes may open a current installation at once and its receipt alone decides whether
    /// it is current; only recovery and extraction write it, and those hold the setup lock. A damaged file is not looked
    /// for: it fails loudly where it is loaded.
    /// </summary>
    public static GameInstallation? EnsureInstalled(string root,
        CancellationToken cancellationToken = default, IProgress<string>? progress = null)
    {
        var installation = new GameInstallation(Path.GetFullPath(root));
        if (IsComplete(installation)) return installation;
        using FileStream gate = Lock(installation.Root);
        RecoverInterruptedPublish(installation);
        if (IsComplete(installation)) return installation;
        if (!File.Exists(installation.RomPath)) return null;
        byte[] rom;
        using (Stream source = File.OpenRead(installation.RomPath)) rom = SupportedCartridge.Read(source, cancellationToken);
        return ExtractAndPublish(installation, rom, cancellationToken, progress);
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
    /// Opens a current extracted-content installation without requiring the private ROM copy: one whose receipt names
    /// this importer's format and the supported source. Hosts bind these resources to a mutable-memory-only runtime; no
    /// gameplay ROM reader exists.
    /// </summary>
    public static GameInstallation? TryOpenExtractedContent(string root)
    {
        var installation = new GameInstallation(Path.GetFullPath(root));
        if (IsExtractedContentComplete(installation)) return installation;
        using FileStream gate = Lock(installation.Root);
        RecoverInterruptedPublish(installation);
        return IsExtractedContentComplete(installation) ? installation : null;
    }

    /// <summary>Serializes setup, reuses a current installation or extracts every component from a newly supplied ROM.</summary>
    private static GameInstallation InstallValidated(byte[] rom, string root,
        CancellationToken cancellationToken, IProgress<string>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var installation = new GameInstallation(Path.GetFullPath(root));
        using FileStream gate = Lock(installation.Root);
        RecoverInterruptedPublish(installation);
        if (IsComplete(installation)) return installation;
        return ExtractAndPublish(installation, rom, cancellationToken, progress);
    }

    /// <summary>Requires both an installed cartridge copy and a current receipt.</summary>
    private static bool IsComplete(GameInstallation installation) =>
        File.Exists(installation.RomPath) && IsExtractedContentComplete(installation);

    /// <summary>
    /// Whether the content's receipt names this importer's format and the supported source. A missing or unreadable
    /// receipt, or one naming another format or source, means the content is extracted again (owner ruling: extraction
    /// does not validate its own output, so the receipt is the whole of the decision).
    /// </summary>
    private static bool IsExtractedContentComplete(GameInstallation installation)
    {
        string receiptPath = Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName);
        if (!File.Exists(receiptPath)) return false;
        try { return ReceiptMismatchAt(receiptPath) is null; }
        catch (JsonException) { return false; }
    }

    /// <summary>Why the receipt at <paramref name="receiptPath"/> does not name this importer's format and source, or null when it does.</summary>
    /// <exception cref="JsonException">The receipt is not a receipt document.</exception>
    internal static string? ReceiptMismatchAt(string receiptPath) =>
        ReceiptMismatch(JsonSerializer.Deserialize<InstallationReceipt>(File.ReadAllText(receiptPath)), receiptPath);

    /// <summary>Stages a complete content directory extracted from <paramref name="rom"/> and publishes it atomically.</summary>
    private static GameInstallation ExtractAndPublish(GameInstallation installation, byte[] rom,
        CancellationToken cancellationToken, IProgress<string>? progress)
    {
        string staging = Path.Combine(installation.Root, GameInstallationLayout.StagingPrefix + Guid.NewGuid().ToString("N"));
        string previous = Path.Combine(installation.Root, GameInstallationLayout.PreviousDirectoryName);
        Directory.CreateDirectory(staging);
        try
        {
            File.WriteAllBytes(Path.Combine(staging, GameInstallationLayout.RomFileName), rom);
            InstallComponents(rom, staging, cancellationToken, progress);
            // The character and tile pages are the import's own data (#1636): every component that composes from them has
            // finished, so they are deleted before the receipt is written and nothing published holds them.
            Directory.Delete(Path.Combine(staging, GameInstallationLayout.ImportDataDirectoryName), recursive: true);
            // The receipt names the format and source the content was extracted for, which decides whether later opens
            // reuse it.
            File.WriteAllText(Path.Combine(staging, GameInstallationLayout.ReceiptFileName),
                JsonSerializer.Serialize(new InstallationReceipt(GameInstallationLayout.FormatVersion, SupportedCartridge.Sha256)));
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

    /// <summary>
    /// Installs every component into <paramref name="staging"/> (#1530): every extraction and standalone component at once,
    /// as each reads only the cartridge and its own directory, then every composition component at once as soon as the
    /// extraction components it reads have finished, while any standalone component may still run, and then the picture
    /// store once every composition component has composed its sheets (#1635). Reports are serialized,
    /// so a progress listener sees one at a time. A failure cancels the rest, which are awaited; every failure that was not
    /// the cancellation is rethrown, the first as it was thrown when it is the only one.
    /// </summary>
    private static void InstallComponents(byte[] rom, string staging, CancellationToken cancellationToken, IProgress<string>? progress)
    {
        using var failed = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        IProgress<string>? reports = progress is null ? null : new SerializedProgress(progress);
        // Every sheet composer of the run adds its sheets to the run's families, whose pictures the store writes once (#1635).
        var families = new InstalledSheetFamilies();
        // Each component runs on a thread of its own, so it starts at once rather than queueing behind the parallel work
        // inside the components already running.
        Task Start(InstallerComponent component) => Task.Factory.StartNew(() =>
        {
            try { InstallComponent(rom, staging, component, families, failed.Token, reports); }
            catch { failed.Cancel(); throw; }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        InstallerComponent[] first = [.. Components.Where(component => component.Kind is InstallerComponentKind.Extraction or InstallerComponentKind.Standalone)];
        Task[] firstTasks = [.. first.Select(Start)];
        Task extracted = Task.WhenAll(firstTasks.Where((_, index) => first[index].Kind == InstallerComponentKind.Extraction));
        // Composition starts only once every extraction component it reads has finished, and not at all after a failure.
        Task composition = After(extracted, InstallerComponentKind.Composition);
        // The picture store starts only once every sheet family is composed, and not at all after a failure.
        Task store = After(composition, InstallerComponentKind.PictureStore);
        Task[] all = [.. firstTasks, composition, store];
        Task.WhenAll(all).ContinueWith(static _ => { }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)
            .Wait(CancellationToken.None);
        // A component stopped by another's failure reports the cancellation, which is not a failure of its own.
        Exception[] failures = [.. all.SelectMany(task => task.Exception is { } faulted ? faulted.InnerExceptions : [])
            .Where(error => error is not OperationCanceledException)];
        if (failures.Length == 1)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failures[0]).Throw();
        if (failures.Length > 1)
            throw new AggregateException(failures);
        cancellationToken.ThrowIfCancellationRequested();

        // Every component of kind, all at once, once preceding has finished, and none after it failed.
        Task After(Task preceding, InstallerComponentKind kind) => preceding.ContinueWith(done => done.IsCompletedSuccessfully
            ? Task.WhenAll(Components.Where(component => component.Kind == kind).Select(Start))
            : Task.CompletedTask, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
    }

    /// <summary>Extracts each step of <paramref name="component"/> into its directory under <paramref name="staging"/>.</summary>
    internal static void InstallComponent(byte[] rom, string staging, InstallerComponent component, InstalledSheetFamilies families,
        CancellationToken cancellationToken, IProgress<string>? progress)
    {
        string directory = Path.Combine(staging, component.DirectoryName);
        foreach (InstallerStep step in component.Steps)
        {
            progress?.Report(step.Progress);
            cancellationToken.ThrowIfCancellationRequested();
            step.Extract(new CartridgeImportAddressSpace(rom), directory, families, cancellationToken);
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

    /// <summary>The published content's format and the digest of the source revision it was extracted from.</summary>
    private sealed record InstallationReceipt(int FormatVersion, string RomSha256);

    /// <summary>One extraction stage: its progress text and extractor.</summary>
    internal sealed record InstallerStep(
        string Progress,
        Action<CartridgeImportAddressSpace, string, InstalledSheetFamilies, CancellationToken> Extract);

    /// <summary>
    /// One extracted-content directory, and what it reads, which decides what it installs alongside (#1530). Extraction is
    /// deterministic, so its output is checked by verification, not by the installer. Every extraction extracts every
    /// component and publishes them atomically.
    /// </summary>
    internal sealed record InstallerComponent(string DirectoryName, InstallerComponentKind Kind, params InstallerStep[] Steps)
    {
        /// <summary>An extraction component: it reads only the cartridge and its own directory, and composition reads it.</summary>
        internal InstallerComponent(string directoryName, params InstallerStep[] steps) : this(directoryName, InstallerComponentKind.Extraction, steps)
        {
        }
    }

    /// <summary>A report listener that receives one report at a time, however many components report at once.</summary>
    /// <param name="Inner">The listener reported to.</param>
    private sealed record SerializedProgress(IProgress<string> Inner) : IProgress<string>
    {
        /// <summary>Taken around each report.</summary>
        private readonly Lock gate = new();

        /// <inheritdoc />
        public void Report(string value)
        {
            lock (gate)
                Inner.Report(value);
        }
    }

    /// <summary>Adapts an extraction without cancellation parameters into one installer stage.</summary>
    private static InstallerStep Step(string progress, Action<CartridgeImportAddressSpace, string> extract) =>
        new(progress, (bus, directory, _, _) => extract(bus, directory));

    /// <summary>Adapts a sheet composition or the picture store into one installer stage over the run's sheet families (#1635).</summary>
    private static InstallerStep SheetStep(string progress, Action<CartridgeImportAddressSpace, string, InstalledSheetFamilies> compose) =>
        new(progress, (bus, directory, families, _) => compose(bus, directory, families));

    /// <summary>The installation a component directory belongs to.</summary>
    /// <exception cref="InvalidOperationException">The directory has no installation above it.</exception>
    private static string InstallationOf(string componentDirectory) => Path.GetDirectoryName(componentDirectory)
        ?? throw new InvalidOperationException($"Component directory {componentDirectory} has no installation above it.");

    /// <summary>
    /// Every extracted-content component, in extraction order. The sheets come last: they compose from the stock
    /// artwork every earlier component has installed beside them, and the picture store, after them, writes their pictures
    /// and manifests (#1635).
    /// </summary>
    internal static readonly InstallerComponent[] Components =
    [
        new(GameInstallationLayout.AudioDirectoryName, InstallerComponentKind.Standalone,
            Step("Extracting audio…", (bus, d) => SpcAudioAssetExtractor.Extract(bus, d)),
            Step("Rendering audio clips…", (_, d) => AudioClipExporter.Export(d))),
        new(GameInstallationLayout.MapDirectoryName,
            new InstallerStep("Extracting map presentation...",
                (bus, d, _, cancellation) => MapPresentationExtractor.Extract(bus, d, SupportedCartridge.Sha256, cancellation))),
        new(GameInstallationLayout.GameplayBasePaletteDirectoryName,
            Step("Extracting gameplay base palettes...", (bus, d) => GameplayBasePaletteFiles.Extract(bus, d))),
        new(GameInstallationLayout.SamusOverreadDirectoryName,
            Step("Extracting Samus overread bytes...", (bus, d) => SamusOverreadFiles.Extract(bus, d))),
        new(GameInstallationLayout.EnemyOverreadDirectoryName,
            Step("Extracting enemy overreads...", (bus, d) => EnemyOverreadFiles.Extract(bus, d))),
        new(GameInstallationLayout.BossAndIntroOverreadDirectoryName,
            Step("Extracting boss and intro overread reads...", (bus, d) => BossAndIntroOverreadFiles.Extract(bus, d))),
        new(GameInstallationLayout.StraddledTerrainDirectoryName, InstallerComponentKind.Standalone,
            Step("Decoding straddled terrain...", (_, d) => StraddledTerrainFiles.Extract(d))),
        new(GameInstallationLayout.StandardObjectDirectoryName,
            Step("Extracting standard sprite artwork...", (bus, d) => StandardObjectArtworkFiles.Extract(bus, d))),
        new(GameInstallationLayout.ProjectileDirectoryName,
            Step("Extracting projectile compositions...", (bus, d) => ProjectilePresentationFiles.Extract(bus, d))),
        new(GameInstallationLayout.EnemyTileDirectoryName,
            Step("Extracting ordinary enemy tile sheets...", (bus, d) => EnemyTileArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomCharacterDirectoryName,
            Step("Extracting room character artwork...", (bus, d) => RoomCharacterArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.SamusBodyDirectoryName,
            Step("Extracting Samus body artwork...", (bus, d) => SamusBodyArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.IntroCinematicDirectoryName,
            Step("Extracting opening-cinematic character artwork...", (bus, d) => IntroCinematicArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.EndingMode7DirectoryName,
            Step("Extracting ending Mode-7 artwork...", (bus, d) => EndingMode7ArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.EndingObjectDirectoryName,
            Step("Extracting ending object artwork...", (bus, d) => EndingObjectArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.EndingBackdropDirectoryName,
            Step("Extracting ending backdrops...", (bus, d) => EndingBackdropFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomPaletteDirectoryName,
            Step("Extracting room base palettes...", (bus, d) => RoomStaticPaletteArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomMetatileDirectoryName,
            Step("Extracting room block compositions...", (bus, d) => RoomMetatileArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomBackgroundTilemapDirectoryName,
            Step("Extracting room background tilemaps...", (bus, d) => RoomBackgroundTilemapArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256)),
            Step("Extracting scrolling-sky tilemaps...", (bus, d) => RoomSkyTilemapArtworkFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomVisualLayoutDirectoryName,
            Step("Extracting room visual layouts...", (bus, d) => RoomVisualLayoutFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomLevelStreamDirectoryName,
            Step("Extracting room terrain...", (bus, d) => RoomLevelStreamFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName,
            Step("Extracting shot-block PLM visuals...", (bus, d) => RoomObjectShotBlockVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectGrappleBlockVisualDirectoryName,
            Step("Extracting Grapple-block PLM visuals...", (bus, d) => RoomObjectGrappleBlockVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectStationVisualDirectoryName,
            Step("Extracting station PLM visuals...", (bus, d) => RoomObjectStationVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName,
            Step("Extracting blue-door PLM visuals...", (bus, d) => RoomObjectBlueDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectColoredDoorVisualDirectoryName,
            Step("Extracting colored-door PLM visuals...", (bus, d) => RoomObjectColoredDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName,
            Step("Extracting grey-door PLM visuals...", (bus, d) => RoomObjectGreyDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectEyeDoorVisualDirectoryName,
            Step("Extracting eye-door PLM visuals...", (bus, d) => RoomObjectEyeDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectMotherBrainGlassVisualDirectoryName,
            Step("Extracting Mother Brain glass PLM visuals...", (bus, d) => RoomObjectMotherBrainGlassVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectNoobTubeVisualDirectoryName,
            Step("Extracting n00b-tube PLM visuals...", (bus, d) => RoomObjectNoobTubeVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectDownwardGateVisualDirectoryName,
            Step("Extracting downward-gate PLM visuals...", (bus, d) => RoomObjectDownwardGateVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectElevatorPlatformVisualDirectoryName,
            Step("Extracting elevator-platform PLM visuals...", (bus, d) => RoomObjectElevatorPlatformVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectEscapeGateVisualDirectoryName,
            Step("Extracting Mother Brain escape-gate visuals...", (bus, d) => RoomObjectEscapeGateVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectBombTorizoHandVisualDirectoryName,
            Step("Extracting Bomb Torizo hand visuals...", (bus, d) => RoomObjectBombTorizoHandVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectDraygonCannonVisualDirectoryName,
            Step("Extracting Draygon cannon visuals...", (bus, d) => RoomObjectDraygonCannonVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectPhantoonDoorVisualDirectoryName,
            Step("Extracting Phantoon door visuals...", (bus, d) => RoomObjectPhantoonDoorVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectChozoStatueVisualDirectoryName,
            Step("Extracting Chozo statue terrain visuals...", (bus, d) => RoomObjectChozoStatueVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectLinkedRestoreVisualDirectoryName,
            Step("Extracting linked-block restoration visuals...", (bus, d) => RoomObjectLinkedRestoreVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectTourianAccessVisualDirectoryName,
            Step("Extracting Tourian access-floor visuals...", (bus, d) => RoomObjectTourianAccessVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectSpeedBoosterVisualDirectoryName,
            Step("Extracting Speed Booster bomb-reveal visuals...", (bus, d) => RoomObjectSpeedBoosterVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectMaridiaElevatubeVisualDirectoryName,
            Step("Extracting Maridia elevatube visuals...", (bus, d) => RoomObjectMaridiaElevatubeVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectSporeSpawnCeilingVisualDirectoryName,
            Step("Extracting Spore Spawn ceiling visuals...", (bus, d) => RoomObjectSporeSpawnCeilingVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectSamusEaterVisualDirectoryName,
            Step("Extracting Samus Eater plant visuals...", (bus, d) => RoomObjectSamusEaterVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectBotwoonWallVisualDirectoryName,
            Step("Extracting Botwoon wall visuals...", (bus, d) => RoomObjectBotwoonWallVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectKraidVisualDirectoryName,
            Step("Extracting Kraid room-object visuals...", (bus, d) => RoomObjectKraidVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectCrocomireVisualDirectoryName,
            Step("Extracting Crocomire room-object visuals...", (bus, d) => RoomObjectCrocomireVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectMotherBrainFakeDeathVisualDirectoryName,
            Step("Extracting Mother Brain fake-death room visuals...", (bus, d) => RoomObjectMotherBrainFakeDeathVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectCollectibleVisualDirectoryName,
            Step("Extracting collectible PLM visuals...", (bus, d) => RoomObjectCollectibleVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName,
            Step("Extracting reveal and wall PLM visuals...", (bus, d) => RoomObjectRevealAndWallVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.RoomObjectSetupBlockDirectoryName,
            Step("Extracting room-object setup blocks...", (bus, d) => RoomObjectSetupBlockFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.XrayRevealVisualDirectoryName,
            Step("Extracting X-ray reveal visuals...", (bus, d) => XrayRevealVisualFiles.Extract(bus, d, SupportedCartridge.Sha256))),
        new(GameInstallationLayout.SpriteSheetDirectoryName, InstallerComponentKind.Composition,
            SheetStep("Composing sprite sheets...", (bus, d, families) => families.Add(SpriteSheetFiles.Extract(bus, InstallationOf(d), d)))),
        new(GameInstallationLayout.RoomBlockSheetDirectoryName, InstallerComponentKind.Composition,
            SheetStep("Composing room block sheets...", (_, d, families) => families.Add(RoomBlockSheetFiles.Extract(InstallationOf(d), d)))),
        new(GameInstallationLayout.BossBodySheetDirectoryName, InstallerComponentKind.Composition,
            SheetStep("Composing boss body sheets...", (_, d, families) => families.Add(BossBodySheetFiles.Extract(InstallationOf(d), d)))),
        new(GameInstallationLayout.EnemySheetDirectoryName, InstallerComponentKind.Composition,
            SheetStep("Composing enemy sheets...", (_, d, families) => families.Add(EnemySheetFiles.Extract(InstallationOf(d), d)))),
        new(PictureStoreFiles.DirectoryName, InstallerComponentKind.PictureStore,
            SheetStep("Writing the picture store...", (_, d, families) => PictureStoreWriter.Write(InstallationOf(d), families.Every(), SupportedCartridge.Sha256))),
    ];

    /// <summary>Why <paramref name="receipt"/> does not name this importer's format and source, or null when it does.</summary>
    private static string? ReceiptMismatch(InstallationReceipt? receipt, string receiptPath)
    {
        if (receipt is null || receipt.FormatVersion != GameInstallationLayout.FormatVersion)
            return $"Installation receipt {receiptPath} requires format " +
                $"{GameInstallationLayout.FormatVersion}; found {receipt?.FormatVersion.ToString() ?? "null"}. Reimport the supported cartridge.";
        if (receipt.RomSha256 != SupportedCartridge.Sha256)
            return $"Installation receipt {receiptPath} has unsupported source provenance.";
        return null;
    }
}
