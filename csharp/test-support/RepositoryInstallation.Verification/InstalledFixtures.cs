using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.Artwork;
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

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Installation fixtures only the verifiers use; the developer tools link only the shared part.</summary>
internal static class InstalledFixtures
{
    /// <summary>The installation's cartridge bytes Samus's glitch paths read, loaded once.</summary>
    private static readonly Lazy<SamusOverreadCatalog> samusOverreads = new(() => RepositoryInstallation.Installation.LoadSamusOverreads());

    /// <summary>The installation's cartridge bytes Samus's glitch paths read.</summary>
    internal static SamusOverreadCatalog SamusOverreads => samusOverreads.Value;

    /// <summary>The installation's starting and common sprite colours, loaded once.</summary>
    private static readonly Lazy<GameplayBasePaletteCatalog> gameplayBasePalettes = new(() => RepositoryInstallation.Installation.LoadGameplayBasePalettes());

    /// <summary>The installation's starting and common sprite colours.</summary>
    internal static GameplayBasePaletteCatalog GameplayBasePalettes => gameplayBasePalettes.Value;

    /// <summary>Lazy stock character, palette, visual-layout, level-stream and block-sheet catalogs.</summary>
    private static readonly Lazy<RoomAssetCatalogs> roomAssets = new(() => new RoomAssetCatalogs(
        RepositoryInstallation.Installation.LoadRoomCharacters(), RepositoryInstallation.Installation.LoadRoomPalettes(),
        RepositoryInstallation.Installation.LoadRoomVisualLayouts(), RepositoryInstallation.Installation.LoadRoomLevelStreams(),
        RepositoryInstallation.Installation.LoadRoomBlockSheets()));

    /// <summary>Lazy stock block compositions, which extraction composes the block sheets from (#1431).</summary>
    private static readonly Lazy<RoomMetatileCatalog> blockCompositions = new(() =>
        RoomMetatileArtworkFiles.Load(RepositoryInstallation.Installation.RoomMetatileDirectory));

    /// <summary>Lazy map and shared presentation resources.</summary>
    private static readonly Lazy<AreaMapPresentationCatalog> maps = new(() => RepositoryInstallation.Installation.LoadMaps());

    /// <summary>Lazy installed enemy tile and sprite artwork.</summary>
    private static readonly Lazy<InstalledEnemyArtwork> enemyArtwork = new(() => RepositoryInstallation.Installation.LoadEnemyTiles());

    /// <summary>Lazy projectile art, bindings, and tiles.</summary>
    private static readonly Lazy<InstalledProjectilePresentation> projectiles = new(() => RepositoryInstallation.Installation.LoadProjectiles());

    /// <summary>Lazy Samus body and arm-cannon artwork.</summary>
    private static readonly Lazy<SamusBodyArtworkCatalog> samusBody = new(() => RepositoryInstallation.Installation.LoadSamusBodyArt());

    /// <summary>Lazy intro and Ceres-flight cinematic artwork.</summary>
    private static readonly Lazy<IntroCinematicArtworkCatalog> introArtwork = new(() => RepositoryInstallation.Installation.LoadIntroCinematicArt());

    /// <summary>Lazy action that binds all installed host catalogs to a game.</summary>
    private static readonly Lazy<Action<SuperMetroidGame>> gameBindings =
        new(() => InstalledGameBindings.Create(RepositoryInstallation.Installation));

    /// <summary>The installation's stock room catalogs, the cartridge reference extracted from the ROM.</summary>
    internal static RoomAssetCatalogs RoomAssets => roomAssets.Value;

    /// <summary>The installation's stock block compositions, which extraction composes the block sheets from (#1431).</summary>
    internal static RoomMetatileCatalog BlockCompositions => blockCompositions.Value;

    /// <summary>
    /// The stock tiles of every block a room of <paramref name="tileset"/> loads, four per block, as extraction composes
    /// their pictures (#1431): a Zebes room's common room elements before its tileset's, or a Ceres room's tileset alone.
    /// </summary>
    internal static BackgroundTile[] StockBlockTiles(TilesetDefinition tileset, bool ceres) =>
        RoomBlockComposition.StockTiles(BlockCompositions, tileset.Blocks, ceres);

    /// <summary>The installation's stock map and presentation catalog, loaded once per process.</summary>
    internal static AreaMapPresentationCatalog Maps => maps.Value;

    /// <summary>The installation's stock enemy artwork, loaded once per process.</summary>
    internal static EnemyTileArtworkCatalog EnemyTiles => enemyArtwork.Value.Enemies;

    /// <summary>The installation's stock enemy, boss and area artwork, loaded once per process.</summary>
    internal static InstalledEnemyArtwork EnemyArtwork => enemyArtwork.Value;

    /// <summary>The installation's stock boss artwork, loaded once per process with the enemy artwork.</summary>
    internal static BossArtworkCatalog BossArtwork => enemyArtwork.Value.Bosses;

    /// <summary>The installation's stock area artwork, loaded once per process with the enemy artwork.</summary>
    internal static AreaArtworkCatalog AreaArtwork => enemyArtwork.Value.Areas;


    /// <summary>The installation's projectile artwork and frame bindings, loaded once per process.</summary>
    internal static InstalledProjectilePresentation Projectiles => projectiles.Value;

    /// <summary>Projectile draw artwork showing the installed gameplay projectile sheet of <paramref name="equippedBeams"/>.</summary>
    internal static ProjectileDrawArtwork ProjectileArtwork(ushort equippedBeams = 0) =>
        new(ProjectileSheets.Gameplay(SpriteSheets), (SamusBeamFlags)equippedBeams);

    /// <summary>The installation's grapple sheet.</summary>
    internal static SpriteSheet GrappleSheet => SpriteSheets.Get(SpriteSheetId.Grapple);

    /// <summary>The installation's pre-coloured sprite sheets, loaded once per process.</summary>
    internal static InstalledSpriteSheets SpriteSheets => spriteSheets.Value;

    /// <summary>The installation's pre-coloured enemy sheets, loaded once per process.</summary>
    internal static SuperMetroid.Core.Enemies.Artwork.InstalledEnemySheets EnemySheets => enemySheets.Value;

    /// <summary>Backing loader for <see cref="EnemySheets"/>.</summary>
    private static readonly Lazy<SuperMetroid.Core.Enemies.Artwork.InstalledEnemySheets> enemySheets =
        new(() => RepositoryInstallation.Installation.LoadEnemySheets());

    /// <summary>Backing loader for <see cref="SpriteSheets"/>.</summary>
    private static readonly Lazy<InstalledSpriteSheets> spriteSheets = new(() => RepositoryInstallation.Installation.LoadSpriteSheets());

    /// <summary>The installation's standard OBJ sheet, loaded once per process.</summary>
    internal static SuperMetroid.Core.Rooms.Artwork.RoomCharacterAtlas StandardObjects => standardObjects.Value;
    /// <summary>Backing loader for <see cref="StandardObjects"/>.</summary>
    private static readonly Lazy<SuperMetroid.Core.Rooms.Artwork.RoomCharacterAtlas> standardObjects =
        new(() => RepositoryInstallation.Installation.LoadStandardObjects());

    /// <summary>Binds the installed standard OBJ sheet, beam and trail artwork and the sprite sheets a runtime's projectiles draw from.</summary>
    internal static void BindProjectileArtwork(SuperMetroid.Core.Runtime.SuperMetroidRuntime runtime)
    {
        runtime.Artwork.StandardObjectArt = StandardObjects;
        runtime.Artwork.BeamPalettes = Projectiles.BeamTiles.Palettes;
        runtime.Artwork.HyperBeamFxColors = Projectiles.BeamTiles.HyperBeamFxColors;
        runtime.Artwork.TrailArtwork = Projectiles.Trails;
        runtime.Artwork.SpriteSheets = SpriteSheets;
    }

    /// <summary>The installation's Samus body and arm-cannon artwork, loaded once per process.</summary>
    internal static SamusBodyArtworkCatalog SamusBody => samusBody.Value;

    /// <summary>The installation's intro cinematic artwork, loaded once per process.</summary>
    internal static IntroCinematicArtworkCatalog IntroArtwork => introArtwork.Value;

    /// <summary>Creates a Samus projectile system with installed frame bindings.</summary>
    internal static SamusProjectileSystem CreateProjectileSystem() =>
        new(new BeamChargeState(), SamusProjectileResidue.None) { Animation = { FrameBindings = Projectiles.FrameBindings }, Overreads = SamusOverreads };

    /// <summary>Creates a bomb system with installed frame bindings and explosion colors.</summary>
    internal static SamusBombProjectileSystem CreateBombSystem() =>
        new() { FrameBindings = Projectiles.FrameBindings, PowerBombExplosion = { PresentationColors = Maps.PowerBombFixedColors } };

    /// <summary>Binds the installation's suit, visor, body and grapple presentation to a standalone Samus.</summary>
    internal static SamusState BindSamus(SamusState samus)
    {
        samus.Presentation.SuitColors = Maps.SamusSuitColors;
        samus.Presentation.FullBodyCycleColors = Maps.SamusFullBodyCycleColors;
        samus.Presentation.ChargeColors = Maps.SamusChargeColors;
        samus.Presentation.AnimationWindows = SamusOverreads.AnimationWindows;
        samus.VisorPalette.PresentationColors = Maps.SamusVisorColors;
        samus.Xray.PresentationColors = Maps.SamusVisorColors;
        samus.Drained.PresentationColors = Maps.SamusHyperBeamColors;
        samus.Presentation.BodyFrames.BindArtwork(SamusBody);
        samus.Presentation.BodyFrames.BindSheets(SpriteSheets);
        samus.ArmCannon.Artwork = SamusBody.ArmCannon;
        samus.Grapple.FlarePlacement = Projectiles.GrappleTiles.FlarePlacement;
        samus.Grapple.SwingFrames = Projectiles.GrappleTiles.SwingFrames;
        return samus;
    }

    /// <summary>Creates Samus with installation suit, body, beam, visor, and grapple presentation.</summary>
    internal static SamusState CreateSamus() => BindSamus(new SamusState());

    /// <summary>Creates Ceres destruction with installed cinematic art, fixed colors, and palette FX.</summary>
    internal static CeresDestructionCinematicState CreateDestruction(AudioQueues? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        new(audio, Maps.PowerBombFixedColors, artwork ?? IntroArtwork, Maps.RoomPaletteFx,
            spriteSheets: CinematicSheetFixtures.WithIntro(artwork ?? IntroArtwork));

    /// <summary>Creates an intro scene with installed character, projectile, narration, and hurt-color art.</summary>
    internal static IntroCinematicState CreateIntro(AudioQueues? audio = null,
        IntroFontAtlas? introFont = null, IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamPaletteCatalog? beamPalettes = null, SamusBodyArtworkCatalog? samusBodyArtwork = null)
    {
        var intro = new IntroCinematicState(audio, introFont ?? Maps.IntroFont,
            characterArtwork ?? IntroArtwork, beamPalettes ?? Projectiles.BeamTiles.Palettes, samusBodyArtwork ?? SamusBody)
        {
            Random = new SharedRandom(),
            ProjectileCompositions = Projectiles.Catalog,
            ProjectileFrameBindings = Projectiles.FrameBindings,
            TrailArtwork = Projectiles.Trails,
            NarrationPresentation = Maps.IntroNarration,
        };
        intro.BindSamusHurtColors(Maps.SamusHurtColors);
        intro.BindSpriteSheets(CinematicSheetFixtures.WithIntro(characterArtwork ?? IntroArtwork));
        return intro;
    }

    /// <summary>Creates ending credits with installed flight, Mode 7, object, palette, and staff art.</summary>
    internal static EndingCreditsState CreateEnding(AudioQueues audio,
        ushort gameTimeHours, ushort gameTimeMinutes, EndingInventorySnapshot inventory = default, bool japaneseText = false)
    {
        var ending = new EndingCreditsState(audio, new EndingCreditsSetup { GameTimeHours = gameTimeHours, GameTimeMinutes = gameTimeMinutes, Inventory = inventory, JapaneseText = japaneseText, EndingText = Maps.EndingText });
        ending.Flyaway.BindFlightArtwork(IntroArtwork.CeresFlight);
        ending.Stage.BindFloorArtwork(RepositoryInstallation.Installation.LoadEndingMode7Art());
        ending.InstalledArtwork.BindFixtureObjectArtwork(RepositoryInstallation.Installation.LoadEndingObjectArt());
        ending.InstalledArtwork.BindPaletteArtwork(RepositoryInstallation.Installation.LoadEndingPalettes());
        ending.InstalledArtwork.BindPaletteFxColors(Maps.RoomPaletteFx);
        ending.Stage.BindEndingFont(Maps.EndingFont);
        ending.InstalledArtwork.BindStaffCredits(Maps.StaffCredits);
        return ending;
    }

    /// <summary>Creates a file-select area map from installed presentation resources.</summary>
    internal static FileSelectAreaMapGraphics CreateFileSelectAreaMap(int selectedArea) =>
        FileSelectAreaMapGraphics.FromPresentation(selectedArea, Maps, SpriteSheets);

    /// <summary>Creates the title sequence with installed gradient, palette, and graphics defaults.</summary>
    internal static TitleSequenceState CreateTitle(AudioQueues? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null) =>
        new(audio, titleGradientPresentation ?? Maps.TitleGradient,
            titlePalettePresentation ?? Maps.TitlePalette, titleGraphicsPresentation ?? Maps.TitleGraphics,
            MenuSheetFixtures.WithTitle(titleGraphicsPresentation ?? Maps.TitleGraphics));

    /// <summary>Creates a pause menu while retaining the supplied state and installed map presentation.</summary>
    internal static PauseMenuState CreatePause(PauseMenuEntry entry) =>
        new(new PauseMenuEntry { Samus = entry.Samus, System = entry.System, AreaIndex = entry.AreaIndex, RoomMapPosition = entry.RoomMapPosition, Audio = entry.Audio, GameplayLayer3 = entry.GameplayLayer3, MapRevealMode = entry.MapRevealMode, MapPresentation = Maps, SpriteSheets = SpriteSheets });

    /// <summary>
    /// A private copy of the installation for a check that corrupts and restores stock files, so
    /// a failure part-way can never leave the shared installation edited. Disposal deletes it.
    /// </summary>
    internal static PrivateInstallationCopy CreatePrivateCopy() => new(RepositoryInstallation.Installation.Root);

    /// <summary>Binds the host catalogs to a game restored from a save or debugger state.</summary>
    internal static void BindGame(SuperMetroidGame game) => gameBindings.Value(game);

    /// <summary>A frontend game over <paramref name="saves"/>, bound to the installation's presentation catalogs.</summary>
    /// <param name="saves">The saves the game owns.</param>
    /// <param name="gameOptions">The host options, or <see langword="null"/> for defaults.</param>
    /// <param name="renderGameplayFrames">Whether gameplay frames are composited.</param>
    /// <param name="hostServices">The hardware-timing policies, or <see langword="null"/> for the lag-free defaults.</param>
    /// <param name="random">The session's random-number word, or <see langword="null"/> for a game that owns its own.</param>
    internal static SuperMetroidGame CreateGame(SuperMetroid.Core.Saves.SaveSlotStore saves,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true,
        SuperMetroidGameHostServices? hostServices = null, SharedRandom? random = null)
    {
        // A fixture game without a supplied word is a session of its own.
        SuperMetroidGameHostServices services = hostServices ?? SuperMetroidGameHostServices.Default;
        if (!renderGameplayFrames)
            services = services with { GameplayFrames = RetainedGameplayFrames.Instance };
        var game = new SuperMetroidGame(saves, gameOptions, services, random ?? new SharedRandom());
        gameBindings.Value(game);
        return game;
    }
}
