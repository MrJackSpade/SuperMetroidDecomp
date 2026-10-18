using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
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

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Installation fixtures only the verifiers use; the developer tools link only the shared part.</summary>
internal static class InstalledFixtures
{
    /// <summary>The installation's cartridge bytes Samus's glitch paths read, loaded once.</summary>
    private static readonly Lazy<SamusOverreadCatalog> samusOverreads = new(() => RepositoryInstallation.Installation.LoadSamusOverreads());

    /// <summary>The installation's cartridge bytes Samus's glitch paths read.</summary>
    internal static SamusOverreadCatalog SamusOverreads => samusOverreads.Value;

    /// <summary>Lazy stock character, palette, metatile, and visual-layout catalogs.</summary>
    private static readonly Lazy<RoomAssetCatalogs> roomAssets = new(() => new RoomAssetCatalogs(
        RepositoryInstallation.Installation.LoadRoomCharacters(), RepositoryInstallation.Installation.LoadRoomPalettes(),
        RepositoryInstallation.Installation.LoadRoomMetatiles(), RepositoryInstallation.Installation.LoadRoomVisualLayouts(),
        RepositoryInstallation.Installation.LoadRoomLevelStreams()));

    /// <summary>Lazy map and shared presentation resources.</summary>
    private static readonly Lazy<AreaMapPresentationCatalog> maps = new(() => RepositoryInstallation.Installation.LoadMaps());

    /// <summary>Lazy installed enemy tile and sprite artwork.</summary>
    private static readonly Lazy<EnemyTileArtworkCatalog> enemyTiles = new(() => RepositoryInstallation.Installation.LoadEnemyTiles());

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

    /// <summary>The installation's stock map and presentation catalog, loaded once per process.</summary>
    internal static AreaMapPresentationCatalog Maps => maps.Value;

    /// <summary>The installation's stock enemy artwork, loaded once per process.</summary>
    internal static EnemyTileArtworkCatalog EnemyTiles => enemyTiles.Value;


    /// <summary>The installation's projectile artwork and frame bindings, loaded once per process.</summary>
    internal static InstalledProjectilePresentation Projectiles => projectiles.Value;

    /// <summary>
    /// Projectile draw artwork binding <paramref name="compositions"/> (the installed ones when null) to pages of the
    /// installed beam and trail characters over a blank standard OBJ page, for checks of draw geometry and bindings.
    /// </summary>
    internal static ProjectileDrawArtwork ProjectileArtwork(ProjectileSpriteCatalog? compositions = null, ushort equippedBeams = 0) =>
        new(compositions ?? Projectiles.Catalog, projectilePages.Value, (SamusBeamFlags)equippedBeams);

    /// <summary>The installation's standard OBJ sheet, loaded once per process.</summary>
    internal static SuperMetroid.Core.Rooms.Artwork.RoomCharacterAtlas StandardObjects => standardObjects.Value;
    /// <summary>Backing loader for <see cref="StandardObjects"/>.</summary>
    private static readonly Lazy<SuperMetroid.Core.Rooms.Artwork.RoomCharacterAtlas> standardObjects =
        new(() => RepositoryInstallation.Installation.LoadStandardObjects());

    /// <summary>Binds the installed standard OBJ sheet and beam and trail artwork a runtime's projectile pages compose.</summary>
    internal static void BindProjectileArtwork(SuperMetroid.Core.Runtime.SuperMetroidRuntime runtime)
    {
        runtime.Artwork.StandardObjectArt = StandardObjects;
        runtime.Artwork.BeamArtwork = Projectiles.BeamTiles;
        runtime.Artwork.TrailArtwork = Projectiles.Trails;
    }

    /// <summary>The installed beam and trail characters over a blank standard OBJ page.</summary>
    private static readonly Lazy<ProjectileObjectPages> projectilePages = new(() => new(
        SuperMetroid.Core.Assets.SpriteFrameTestArtwork.StandardObjects, Projectiles.BeamTiles,
        Projectiles.Trails.Tiles ?? throw new InvalidOperationException("The installation has no trail characters.")));

    /// <summary>The installation's Samus body and arm-cannon artwork, loaded once per process.</summary>
    internal static SamusBodyArtworkCatalog SamusBody => samusBody.Value;

    /// <summary>The installation's intro cinematic artwork, loaded once per process.</summary>
    internal static IntroCinematicArtworkCatalog IntroArtwork => introArtwork.Value;

    /// <summary>Creates a Samus projectile system with installed frame bindings.</summary>
    internal static SamusProjectileSystem CreateProjectileSystem() =>
        new() { Animation = { FrameBindings = Projectiles.FrameBindings } };

    /// <summary>Creates a bomb system with installed frame bindings and explosion colors.</summary>
    internal static SamusBombProjectileSystem CreateBombSystem() =>
        new() { FrameBindings = Projectiles.FrameBindings, PowerBombExplosion = { PresentationColors = Maps.PowerBombFixedColors } };

    /// <summary>Binds the installation's suit, visor, body and grapple presentation to a standalone Samus.</summary>
    internal static SamusState BindSamus(SamusState samus)
    {
        samus.Presentation.SuitColors = Maps.SamusSuitColors;
        samus.Presentation.FullBodyCycleColors = Maps.SamusFullBodyCycleColors;
        samus.Presentation.ChargeColors = Maps.SamusChargeColors;
        samus.VisorPalette.PresentationColors = Maps.SamusVisorColors;
        samus.Xray.PresentationColors = Maps.SamusVisorColors;
        samus.Drained.PresentationColors = Maps.SamusHyperBeamColors;
        samus.Presentation.TileTransfers.BindArtwork(SamusBody);
        samus.ArmCannon.Artwork = SamusBody.ArmCannon;
        samus.Grapple.FlarePlacement = Projectiles.GrappleTiles.FlarePlacement;
        samus.Grapple.SwingFrames = Projectiles.GrappleTiles.SwingFrames;
        return samus;
    }

    /// <summary>Creates Samus with installation suit, body, beam, visor, and grapple presentation.</summary>
    internal static SamusState CreateSamus() => BindSamus(new SamusState());

    /// <summary>Creates Ceres destruction with installed cinematic art, fixed colors, and palette FX.</summary>
    internal static CeresDestructionCinematicState CreateDestruction(CartridgeAudioState? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        new(audio, Maps.PowerBombFixedColors, artwork ?? IntroArtwork, Maps.RoomPaletteFx);

    /// <summary>Creates an intro scene with installed character, projectile, narration, and hurt-color art.</summary>
    internal static IntroCinematicState CreateIntro(CartridgeAudioState? audio = null,
        IntroFontAtlas? introFont = null, IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamTileCatalog? beamArtwork = null, SamusBodyArtworkCatalog? samusBodyArtwork = null)
    {
        var intro = new IntroCinematicState(audio, introFont ?? Maps.IntroFont,
            characterArtwork ?? IntroArtwork, beamArtwork ?? Projectiles.BeamTiles, samusBodyArtwork ?? SamusBody)
        {
            ProjectileCompositions = Projectiles.Catalog,
            ProjectileFrameBindings = Projectiles.FrameBindings,
            TrailArtwork = Projectiles.Trails,
            NarrationPresentation = Maps.IntroNarration,
        };
        intro.BindSamusHurtColors(Maps.SamusHurtColors);
        return intro;
    }

    /// <summary>Creates ending credits with installed flight, Mode 7, object, palette, and staff art.</summary>
    internal static EndingCreditsState CreateEnding(CartridgeAudioState audio,
        ushort gameTimeHours, ushort gameTimeMinutes, EndingInventorySnapshot inventory = default, bool japaneseText = false)
    {
        var ending = new EndingCreditsState(audio, new EndingCreditsSetup { GameTimeHours = gameTimeHours, GameTimeMinutes = gameTimeMinutes, Inventory = inventory, JapaneseText = japaneseText, EndingText = Maps.EndingText });
        ending.BindFlightArtwork(IntroArtwork.CeresFlight);
        ending.BindMode7Artwork(RepositoryInstallation.Installation.LoadEndingMode7Art());
        ending.BindObjectArtwork(RepositoryInstallation.Installation.LoadEndingObjectArt());
        ending.BindPaletteArtwork(RepositoryInstallation.Installation.LoadEndingPalettes());
        ending.BindPaletteFxColors(Maps.RoomPaletteFx);
        ending.BindEndingFont(Maps.EndingFont);
        ending.BindStaffCredits(Maps.StaffCredits);
        return ending;
    }

    /// <summary>Creates a file-select area map from installed presentation resources.</summary>
    internal static FileSelectAreaMapGraphics CreateFileSelectAreaMap(int selectedArea) =>
        FileSelectAreaMapGraphics.FromPresentation(selectedArea, Maps);

    /// <summary>Creates the title sequence with installed gradient, palette, and graphics defaults.</summary>
    internal static TitleSequenceState CreateTitle(CartridgeAudioState? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null) =>
        new(audio, titleGradientPresentation ?? Maps.TitleGradient,
            titlePalettePresentation ?? Maps.TitlePalette, titleGraphicsPresentation ?? Maps.TitleGraphics);

    /// <summary>Creates a pause menu while retaining the supplied state and installed map presentation.</summary>
    internal static PauseMenuState CreatePause(PauseMenuEntry entry) =>
        new(new PauseMenuEntry { Samus = entry.Samus, System = entry.System, AreaIndex = entry.AreaIndex, RoomMapPosition = entry.RoomMapPosition, Audio = entry.Audio, GameplayVram = entry.GameplayVram, MapRevealMode = entry.MapRevealMode, MapPresentation = Maps });

    /// <summary>
    /// A private copy of the installation for a check that corrupts and restores stock files, so
    /// a failure part-way can never leave the shared installation edited. Disposal deletes it.
    /// </summary>
    internal static PrivateInstallationCopy CreatePrivateCopy() => new(RepositoryInstallation.Installation.Root);

    /// <summary>Binds the host catalogs to a game restored from a save or debugger state.</summary>
    internal static void BindGame(SuperMetroidGame game) => gameBindings.Value(game);

    /// <summary>A frontend game over <paramref name="saves"/>, bound to the installation's presentation catalogs.</summary>
    internal static SuperMetroidGame CreateGame(SuperMetroid.Core.Saves.SaveSlotStore saves,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true)
    {
        var game = new SuperMetroidGame(saves, gameOptions, renderGameplayFrames);
        gameBindings.Value(game);
        return game;
    }
}
