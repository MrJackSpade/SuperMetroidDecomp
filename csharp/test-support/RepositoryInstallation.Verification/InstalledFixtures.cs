using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Installation fixtures only the verifiers use; the developer tools link only the shared part.</summary>
internal static class InstalledFixtures
{
    /// <summary>
    /// A fixture the process loads once, on first use, timed as a shared fixture load of its own under the suite that
    /// first asks for it rather than charged to that suite (#1532), as the installation's own load is.
    /// </summary>
    private static Lazy<T> Shared<T>(string name, Func<T> load) where T : notnull =>
        new(() => SharedFixtureTiming.Load("Fixture: " + name, load));

    /// <summary>The installation's services reproducing Samus's glitch reads, loaded once.</summary>
    private static readonly Lazy<SamusOverreadServices> samusOverreads = Shared<SamusOverreadServices>("Samus overreads", () => RepositoryInstallation.Installation.LoadSamusOverreads());

    /// <summary>The installation's services reproducing Samus's glitch reads.</summary>
    internal static SamusOverreadServices SamusOverreads => samusOverreads.Value;

    /// <summary>The installation's services reproducing what enemies read past their authored data, loaded once.</summary>
    private static readonly Lazy<SuperMetroid.Core.Enemies.EnemyOverreadServices> enemyOverreads = Shared<SuperMetroid.Core.Enemies.EnemyOverreadServices>("enemy overreads", () => RepositoryInstallation.Installation.LoadEnemyOverreads());

    /// <summary>The installation's services reproducing what enemies read past their authored data.</summary>
    internal static SuperMetroid.Core.Enemies.EnemyOverreadServices EnemyOverreads => enemyOverreads.Value;

    /// <summary>The installation's services reproducing Kraid's, Crocomire's and the intro egg shell's reads past their data, loaded once.</summary>
    private static readonly Lazy<SuperMetroid.Core.Compatibility.BossAndIntroOverreadServices> bossAndIntroOverreads =
        Shared<SuperMetroid.Core.Compatibility.BossAndIntroOverreadServices>("boss and intro overreads", () => RepositoryInstallation.Installation.LoadBossAndIntroOverreads());

    /// <summary>The installation's services reproducing Kraid's, Crocomire's and the intro egg shell's reads past their data.</summary>
    internal static SuperMetroid.Core.Compatibility.BossAndIntroOverreadServices BossAndIntroOverreads => bossAndIntroOverreads.Value;

    /// <summary>The installation's continuation of a Samus animation frame past its pose's program, which every Samus takes.</summary>
    internal static SuperMetroid.Core.Compatibility.LaidOutProgramContinuation<SuperMetroid.Core.Samus.Poses.SamusPoseId, SuperMetroid.Core.Samus.Poses.SamusAnimationProgramStep> AnimationContinuation =>
        SamusOverreads.AnimationContinuation;

    /// <summary>Lazy stock visual-layout, level-stream and block-sheet catalogs.</summary>
    private static readonly Lazy<RoomAssetCatalogs> roomAssets = Shared<RoomAssetCatalogs>("room asset catalogs", () => new RoomAssetCatalogs(
        RepositoryInstallation.Installation.LoadRoomVisualLayouts(),
        RepositoryInstallation.Installation.LoadRoomLevelStreams(), RepositoryInstallation.Installation.LoadRoomBlockSheets()));

    /// <summary>
    /// Lazy stock room character catalog, which extraction composes the block sheets from and verification composes VRAM
    /// images of rooms from (#1466); gameplay reads none.
    /// </summary>
    private static readonly Lazy<RoomCharacterAtlases> roomCharacters = Shared<RoomCharacterAtlases>("room characters", () => RepositoryInstallation.Installation.LoadRoomCharacters());

    /// <summary>Lazy stock block compositions, which extraction composes the block sheets from (#1431).</summary>
    private static readonly Lazy<RoomMetatileCatalog> blockCompositions = Shared<RoomMetatileCatalog>("block compositions", () =>
        RoomMetatileArtworkFiles.Load(RepositoryInstallation.Installation.RoomMetatileDirectory));

    /// <summary>
    /// Lazy map and shared presentation resources, as the import reads them (#1544): the game's presentation with the
    /// colours the fixture sheets are pre-coloured from. Its identity is the game's read's.
    /// </summary>
    private static readonly Lazy<AreaMapPresentation> maps = Shared<AreaMapPresentation>("maps", () =>
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationFiles.LoadForImport(RepositoryInstallation.Installation.MapDirectory));

    /// <summary>Lazy installed enemy tile and sprite artwork.</summary>
    private static readonly Lazy<InstalledEnemyArtwork> enemyArtwork = Shared<InstalledEnemyArtwork>("enemy artwork", () => RepositoryInstallation.Installation.LoadEnemyTiles());

    /// <summary>Lazy projectile art, bindings, and tiles.</summary>
    private static readonly Lazy<InstalledProjectilePresentation> projectiles = Shared<InstalledProjectilePresentation>("projectiles", () => RepositoryInstallation.Installation.LoadProjectiles());

    /// <summary>Lazy Samus body and arm-cannon artwork: the repository installation's, whose game catalog its artwork binds.</summary>
    private static readonly Lazy<SamusBodySources> samusBody = Shared<SamusBodySources>("Samus body", () => RepositoryInstallation.SamusBody);

    /// <summary>Lazy intro and Ceres-flight cinematic artwork.</summary>
    private static readonly Lazy<IntroCinematicArtwork> introArtwork = Shared<IntroCinematicArtwork>("intro artwork", () => SuperMetroid.AssetExtraction.Frontend.Intro.IntroCinematicArtworkFiles.LoadForImport(RepositoryInstallation.Installation.IntroCinematicDirectory));

    /// <summary>
    /// The installed ending Mode 7 floors, loaded once for the checks that compare the cartridge's floor loads (#1532); the
    /// game draws the floors' sheets and never loads them (#1555).
    /// </summary>
    private static readonly Lazy<SuperMetroid.AssetExtraction.Frontend.Ending.EndingFloorCharacters> endingFloors =
        Shared<SuperMetroid.AssetExtraction.Frontend.Ending.EndingFloorCharacters>("ending floors", () => RepositoryInstallation.Installation.LoadEndingMode7Art());

    /// <summary>The installed ending Mode 7 floors the cartridge's floor loads are compared with.</summary>
    internal static SuperMetroid.AssetExtraction.Frontend.Ending.EndingFloorCharacters EndingFloors => endingFloors.Value;

    /// <summary>The installed ending object artwork, loaded once for every ending fixture (#1532).</summary>
    private static readonly Lazy<SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectSourceArtwork> endingObjects =
        Shared<SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectSourceArtwork>("ending objects", () => RepositoryInstallation.Installation.LoadEndingObjectArt());

    /// <summary>The installed ending scene backdrops, loaded once for every ending fixture (#1532, #1544).</summary>
    private static readonly Lazy<SuperMetroid.Core.Frontend.Ending.Artwork.EndingBackdrops> endingBackdrops =
        Shared<SuperMetroid.Core.Frontend.Ending.Artwork.EndingBackdrops>("ending backdrops", () => RepositoryInstallation.Installation.LoadEndingBackdrops());

    /// <summary>The ending's colours the import decodes from the cartridge and pre-colours the ending's sheets in (#1544).</summary>
    private static readonly Lazy<SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingPaletteSet> endingColours =
        Shared<SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingPaletteSet>("ending colours", () =>
            SuperMetroid.AssetExtraction.Frontend.Ending.EndingPaletteImport.Read(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>The installed ending scene backdrops.</summary>
    internal static SuperMetroid.Core.Frontend.Ending.Artwork.EndingBackdrops EndingBackdrops => endingBackdrops.Value;

    /// <summary>The ending's colours the import decodes from the cartridge, which the installed ending sheets are pre-coloured in.</summary>
    internal static SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingPaletteSet EndingColours => endingColours.Value;

    /// <summary>Lazy action that binds all installed host catalogs to a game.</summary>
    private static readonly Lazy<Action<SuperMetroidGame>> gameBindings =
        new(() => InstalledGameBindings.Create(RepositoryInstallation.Installation));

    /// <summary>The installation's stock room catalogs, the cartridge reference extracted from the ROM.</summary>
    internal static RoomAssetCatalogs RoomAssets => roomAssets.Value;

    /// <summary>The installation's stock room character catalog.</summary>
    internal static RoomCharacterAtlases RoomCharacters => roomCharacters.Value;

    /// <summary>The installation's stock block compositions, which extraction composes the block sheets from (#1431).</summary>
    internal static RoomMetatileCatalog BlockCompositions => blockCompositions.Value;

    /// <summary>
    /// The stock tiles of every block a room of <paramref name="tileset"/> loads, four per block, as extraction composes
    /// their pictures (#1431): a Zebes room's common room elements before its tileset's, or a Ceres room's tileset alone.
    /// </summary>
    internal static BackgroundTile[] StockBlockTiles(TilesetDefinition tileset, bool ceres) =>
        RoomBlockComposition.StockTiles(BlockCompositions, tileset.Blocks, ceres);

    /// <summary>
    /// The installation's stock extracted audio catalog, which the synthesizer's read-only checks compare with uploaded
    /// APU memory, loaded once per process (#1532). Checks that mutate audio definitions load a private copy instead.
    /// </summary>
    internal static ExtractedAudioAssetCatalog Audio => audio.Value;

    /// <summary>Backing loader for <see cref="Audio"/>.</summary>
    private static readonly Lazy<ExtractedAudioAssetCatalog> audio = new(() => SharedFixtureTiming.Load("Fixture: extracted audio",
        () => ExtractedAudioAssetCatalog.Load(RepositoryInstallation.Installation.AudioDirectory)));

    /// <summary>The installation's stock map and presentation catalog, loaded once per process.</summary>
    internal static AreaMapPresentation Maps => maps.Value;

    /// <summary>The installation's stock enemy artwork, loaded once per process.</summary>
    internal static EnemyTileSourceArtwork EnemyTiles => enemyArtwork.Value.Enemies;

    /// <summary>The installation's stock enemy, boss and area artwork, loaded once per process.</summary>
    internal static InstalledEnemyArtwork EnemyArtwork => enemyArtwork.Value;

    /// <summary>Every frame display of the installation's stock enemy artwork.</summary>
    /// <exception cref="InvalidDataException">The installed enemy artwork holds no enemy frames.</exception>
    internal static SuperMetroid.Core.Enemies.Artwork.EnemyFrameDisplays EnemyFrameDisplays => EnemyTiles.FrameDisplays
        ?? throw new InvalidDataException("The installed enemy artwork holds no enemy frames.");

    /// <summary>
    /// The installed catalogs a bare runtime is built over: the Samus, boss and enemy overread services and the installed
    /// enemy sheets and frame displays (#1616).
    /// </summary>
    internal static SuperMetroid.Core.Frontend.GameArtwork BareRuntimeArtwork => bareRuntimeArtwork.Value;

    /// <summary>Backing value of <see cref="BareRuntimeArtwork"/>.</summary>
    private static readonly Lazy<SuperMetroid.Core.Frontend.GameArtwork> bareRuntimeArtwork = new(() => new SuperMetroid.Core.Frontend.GameArtwork
    {
        SamusOverreads = SamusOverreads,
        BossAndIntroOverreads = BossAndIntroOverreads,
        EnemyOverreads = EnemyOverreads,
        EnemySheets = EnemySheets,
        EnemyFrameDisplays = EnemyFrameDisplays,
    });

    /// <summary>The installation's complete game artwork as the game binds it, loaded once per process and only read.</summary>
    private static readonly Lazy<SuperMetroid.Core.Frontend.GameArtwork> gameArtwork =
        Shared<SuperMetroid.Core.Frontend.GameArtwork>("game artwork", () => InstalledGameBindings.Artwork(RepositoryInstallation.Installation));

    /// <summary>The installation's complete game artwork as the game binds it, loaded once per process and only read.</summary>
    internal static SuperMetroid.Core.Frontend.GameArtwork GameArtwork => gameArtwork.Value;

    /// <summary>
    /// Whether <paramref name="tiles"/> is the shared installed enemy artwork's own catalog, which holds the stock content
    /// as loaded; asking does not load it.
    /// </summary>
    internal static bool IsSharedEnemyTiles(EnemyTileSourceArtwork tiles) =>
        enemyArtwork.IsValueCreated && ReferenceEquals(enemyArtwork.Value.Enemies, tiles);

    /// <summary>The installation's stock boss artwork, loaded once per process with the enemy artwork.</summary>
    internal static BossSourceArtwork BossArtwork => enemyArtwork.Value.Bosses;

    /// <summary>The installation's stock area artwork, loaded once per process with the enemy artwork.</summary>
    internal static AreaSourceArtwork AreaArtwork => enemyArtwork.Value.Areas;

    /// <summary>The installation's projectile artwork and frame bindings, loaded once per process.</summary>
    internal static InstalledProjectilePresentation Projectiles => projectiles.Value;

    /// <summary>Projectile draw artwork showing the installed gameplay projectile sheets, each draw its own beams' (#1586).</summary>
    internal static ProjectileDrawArtwork ProjectileArtwork() => new(ProjectileSheets.Gameplay(SpriteSheets));

    /// <summary>The installation's grapple sheet.</summary>
    internal static SpriteSheet GrappleSheet => SpriteSheets.Get(SpriteSheetId.Grapple);

    /// <summary>The installation's pre-coloured sprite sheets, loaded once per process.</summary>
    internal static InstalledSpriteSheets SpriteSheets => spriteSheets.Value;

    /// <summary>The installation's pre-coloured enemy sheets, loaded once per process.</summary>
    internal static SuperMetroid.Core.Enemies.Artwork.InstalledEnemySheets EnemySheets => enemySheets.Value;

    /// <summary>Backing loader for <see cref="EnemySheets"/>.</summary>
    private static readonly Lazy<SuperMetroid.Core.Enemies.Artwork.InstalledEnemySheets> enemySheets =
        Shared<SuperMetroid.Core.Enemies.Artwork.InstalledEnemySheets>("enemy sheets", () => RepositoryInstallation.Installation.LoadEnemySheets());

    /// <summary>
    /// The installation's pre-coloured sprite sheets with <paramref name="reloaded"/> loaded again from its installed files:
    /// a distinct sheet of the same content, so a check that rebinding the sheets reaches a draw can tell the rebound sheet
    /// from the bound one without loading every sheet again (#1532).
    /// </summary>
    internal static InstalledSpriteSheets SpriteSheetsReloading(SpriteSheetId reloaded)
    {
        var sheets = SpriteSheetDefinitions.Sheets.ToDictionary(sheet => sheet, SpriteSheets.Get);
        sheets[reloaded] = SpriteSheetStore.Open(RepositoryInstallation.Installation.SpriteSheetDirectory,
                SpriteSheetFiles.ManifestFileName, SpriteSheetDefinitions.Sheets.Count)
            .Read(SpriteSheetDefinitions.FileNames[reloaded]);
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Backing loader for <see cref="SpriteSheets"/>.</summary>
    private static readonly Lazy<InstalledSpriteSheets> spriteSheets = Shared<InstalledSpriteSheets>("sprite sheets", () => RepositoryInstallation.Installation.LoadSpriteSheets());

    /// <summary>
    /// The installation's boss body sheets, loaded once per process on their own, so a check drawing a boss body does not
    /// load every presentation catalog of <see cref="RepositoryInstallation.Artwork"/> for them.
    /// </summary>
    internal static SuperMetroid.Core.Enemies.BossBodies.BossBodySheets BossBodySheets => bossBodySheets.Value;

    /// <summary>Backing loader for <see cref="BossBodySheets"/>.</summary>
    private static readonly Lazy<SuperMetroid.Core.Enemies.BossBodies.BossBodySheets> bossBodySheets =
        Shared<SuperMetroid.Core.Enemies.BossBodies.BossBodySheets>("boss body sheets", () => RepositoryInstallation.Installation.LoadBossBodySheets());

    /// <summary>The installation's standard OBJ sheet, loaded once per process.</summary>
    internal static SuperMetroid.AssetExtraction.Rooms.Artwork.RoomCharacterAtlas StandardObjects => standardObjects.Value;
    /// <summary>Backing loader for <see cref="StandardObjects"/>.</summary>
    private static readonly Lazy<SuperMetroid.AssetExtraction.Rooms.Artwork.RoomCharacterAtlas> standardObjects =
        Shared<SuperMetroid.AssetExtraction.Rooms.Artwork.RoomCharacterAtlas>("standard objects", () => RepositoryInstallation.Installation.LoadStandardObjects());

    /// <summary>
    /// Binds the sprite sheets a runtime's projectiles draw from; their beam colours are shades pre-coloured on the sheets
    /// (#1512).
    /// </summary>
    internal static void BindProjectileArtwork(SuperMetroid.Core.Runtime.SuperMetroidRuntime runtime)
    {
        runtime.Artwork.SpriteSheets = SpriteSheets;
    }

    /// <summary>The installation's Samus body and arm-cannon artwork as the import reads them, loaded once per process.</summary>
    internal static SamusBodySources SamusBody => samusBody.Value;

    /// <summary>The installation's intro cinematic artwork, loaded once per process.</summary>
    internal static IntroCinematicArtwork IntroArtwork => introArtwork.Value;

    /// <summary>
    /// Creates a Samus projectile system with installed frame bindings, bound to an earthquake of its own as a room binds
    /// its projectiles to the room's, so Super Missile impacts have an earthquake to start.
    /// </summary>
    internal static SamusProjectileSystem CreateProjectileSystem()
    {
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), SamusOverreads, new SuperMetroid.Core.Compatibility.LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount)) { Animation = { FrameBindings = Projectiles.FrameBindings } };
        projectiles.Earthquakes.BindRoomEarthquake(new RoomEarthquake());
        return projectiles;
    }

    /// <summary>Creates a bomb system with installed frame bindings and explosion colors.</summary>
    internal static SamusBombProjectileSystem CreateBombSystem() =>
        new() { FrameBindings = Projectiles.FrameBindings, PowerBombExplosion = { PresentationColors = Maps.PowerBombFixedColors } };

    /// <summary>The installed colour tables Samus's sheets are pre-coloured from (#1512), which checks resolve her shades with.</summary>
    internal static SuperMetroid.AssetExtraction.Samus.SamusShadeSources SamusShadeSources => SamusBodySheetFixtures.Inputs.ShadeSources;

    /// <summary>Binds the installation's body and grapple presentation to a standalone Samus.</summary>
    internal static SamusState BindSamus(SamusState samus)
    {
        samus.Presentation.BindArtwork(SamusBody.Gameplay);
        samus.Presentation.BindSheets(SpriteSheets);
        samus.ArmCannon.Artwork = SamusBody.ArmCannon.Gameplay;
        samus.Grapple.FlarePlacement = Projectiles.GrappleTiles.FlarePlacement;
        samus.Grapple.SwingFrames = Projectiles.GrappleTiles.SwingFrames;
        return samus;
    }

    /// <summary>Creates Samus with installation suit, body, beam, visor, and grapple presentation.</summary>
    internal static SamusState CreateSamus() => BindSamus(new SamusState(AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()));

    /// <summary>Creates Ceres destruction with installed cinematic art and fixed colors.</summary>
    internal static CeresDestructionCinematicState CreateDestruction(AudioQueues? audio = null, IntroCinematicArtwork? artwork = null) =>
        new(audio, Maps.PowerBombFixedColors, artwork ?? IntroArtwork,
            spriteSheets: CinematicSheetFixtures.WithIntro(artwork ?? IntroArtwork));

    /// <summary>Creates an intro scene with installed character, projectile, narration, and hurt-color art.</summary>
    internal static IntroCinematicState CreateIntro(AudioQueues? audio = null,
        IntroCinematicArtwork? characterArtwork = null,
        InstalledSamusBodyArtwork? samusBodyArtwork = null)
    {
        var intro = new IntroCinematicState(new IntroCinematicOverreads(SamusOverreads, BossAndIntroOverreads.ShellFragmentFallPastCurve), audio,
            characterArtwork ?? IntroArtwork, samusBodyArtwork ?? SamusBody.Gameplay)
        {
            Random = new SharedRandom(),
            UnauthoredBoostCadence = new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            ProjectileFrameBindings = Projectiles.FrameBindings,
            NarrationPresentation = Maps.IntroNarration,
        };
        intro.BindSpriteSheets(CinematicSheetFixtures.WithIntro(characterArtwork ?? IntroArtwork));
        return intro;
    }

    /// <summary>Creates ending credits with installed flight, Mode 7, object, palette, and staff art.</summary>
    internal static EndingCreditsState CreateEnding(AudioQueues audio,
        ushort gameTimeHours, ushort gameTimeMinutes, EndingInventorySnapshot inventory = default, bool japaneseText = false)
    {
        var ending = new EndingCreditsState(audio, new EndingCreditsSetup { GameTimeHours = gameTimeHours, GameTimeMinutes = gameTimeMinutes, Inventory = inventory, JapaneseText = japaneseText, EndingText = Maps.EndingText });
        ending.Flyaway.BindFlightArtwork(IntroArtwork.CeresFlight);
        ending.InstalledArtwork.BindFixtureObjectArtwork(endingObjects.Value);
        ending.InstalledArtwork.BindBackdrops(endingBackdrops.Value);
        ending.InstalledArtwork.BindSpaceBackdrops(Maps.EndingSpaceBackdrops);
        return ending;
    }

    /// <summary>Creates a file-select area map from installed presentation resources.</summary>
    internal static FileSelectAreaMapGraphics CreateFileSelectAreaMap(int selectedArea) =>
        FileSelectAreaMapGraphics.FromPresentation(selectedArea, Maps, SpriteSheets);

    /// <summary>Creates the title sequence with installed gradient, palette, and graphics defaults.</summary>
    internal static TitleSequenceState CreateTitle(AudioQueues? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        RgbColor? titleBackdrop = null,
        TitleGraphicsCharacters? titleGraphicsPresentation = null) =>
        new(audio, titleGradientPresentation ?? Maps.TitleGradient,
            titleBackdrop ?? Maps.TitleBackdrop,
            MenuSheetFixtures.WithTitle(titleGraphicsPresentation ?? InstalledAreaMapPresentation.TitleGraphicsOf(Maps)));

    /// <summary>Creates a pause menu while retaining the supplied state and installed map presentation.</summary>
    internal static PauseMenuState CreatePause(PauseMenuEntry entry) =>
        new(new PauseMenuEntry { Samus = entry.Samus, System = entry.System, AreaIndex = entry.AreaIndex, RoomMapPosition = entry.RoomMapPosition, Audio = entry.Audio, GameplayHud = entry.GameplayHud, MapRevealMode = entry.MapRevealMode, MapPresentation = Maps, SpriteSheets = SpriteSheets });

    /// <summary>
    /// A private copy of the installation for a check that corrupts and restores stock files, so
    /// a failure part-way can never leave the shared installation edited. Disposal deletes it.
    /// </summary>
    internal static PrivateInstallationCopy CreatePrivateCopy() => new(RepositoryInstallation.Installation.Root);

    /// <summary>
    /// A private installation holding copies of only the installation's <paramref name="directories"/>, for a check that
    /// corrupts the installed files of those content families alone; copying a family or two costs
    /// a sliver of copying the whole installation. Disposal deletes it.
    /// </summary>
    /// <param name="directories">Selects each directory to copy, such as <c>installation =&gt; installation.MapDirectory</c>.</param>
    internal static PrivateInstallationCopy CreatePrivateCopyOf(params Func<GameInstallation, string>[] directories) =>
        new(RepositoryInstallation.Installation, directories);

    /// <summary>Binds the host catalogs to a game restored from a save or debugger state.</summary>
    internal static void BindGame(SuperMetroidGame game) => gameBindings.Value(game);

    /// <summary>A frontend game over <paramref name="saves"/>, bound to the installation's presentation catalogs.</summary>
    /// <param name="saves">The saves the game owns.</param>
    /// <param name="assists">The assists the game plays under, or <see langword="null"/> for the unassisted game.</param>
    /// <param name="renderGameplayFrames">Whether gameplay frames are composited.</param>
    /// <param name="hostServices">The hardware-timing policies, or <see langword="null"/> for the lag-free defaults.</param>
    /// <param name="random">The session's random-number word, or <see langword="null"/> for a game that owns its own.</param>
    internal static SuperMetroidGame CreateGame(SuperMetroid.Core.Saves.SaveSlotStore saves,
        SuperMetroid.Core.Gameplay.GameplayAssists? assists = null, bool renderGameplayFrames = true,
        SuperMetroidGameHostServices? hostServices = null, SharedRandom? random = null)
    {
        // A fixture game without a supplied word is a session of its own.
        SuperMetroidGameHostServices services = hostServices ?? SuperMetroidGameHostServices.Default;
        if (!renderGameplayFrames)
            services = services with { GameplayFrames = RetainedGameplayFrames.Instance };
        var game = new SuperMetroidGame(saves, assists, services, random ?? new SharedRandom());
        gameBindings.Value(game);
        return game;
    }

    /// <summary>
    /// The installed content fixture games, runtimes and Samus states are bound to, in a fixed order: what a debugger
    /// graph names by path rather than holding, as a host's store names the content it binds. Each fixture catalog is
    /// named through its loader, so a catalog first loaded after a graph was indexed is named from then on.
    /// </summary>
    internal static object[] BoundContent => boundContent.Value;

    /// <summary>
    /// How many of <see cref="BoundContent"/>'s catalogs have loaded: a graph indexed over the bound content indexes a
    /// catalog's objects once it has loaded (#1532).
    /// </summary>
    internal static int LoadedBoundContent =>
        new[]
        {
            RepositoryInstallation.ArtworkLoader.IsValueCreated, samusOverreads.IsValueCreated, enemyOverreads.IsValueCreated,
            bossAndIntroOverreads.IsValueCreated, roomAssets.IsValueCreated, roomCharacters.IsValueCreated,
            blockCompositions.IsValueCreated, maps.IsValueCreated, enemyArtwork.IsValueCreated, projectiles.IsValueCreated,
            samusBody.IsValueCreated, introArtwork.IsValueCreated, enemySheets.IsValueCreated, spriteSheets.IsValueCreated,
            standardObjects.IsValueCreated, bossBodySheets.IsValueCreated,
        }.Count(loaded => loaded);

    /// <summary>The bound content, gathered once so every graph names it by the same paths.</summary>
    private static readonly Lazy<object[]> boundContent = new(() =>
        [RepositoryInstallation.ArtworkLoader, samusOverreads, enemyOverreads, bossAndIntroOverreads, roomAssets,
            roomCharacters, blockCompositions, maps, enemyArtwork, projectiles, samusBody, introArtwork, enemySheets, spriteSheets,
            standardObjects, bossBodySheets]);
}
