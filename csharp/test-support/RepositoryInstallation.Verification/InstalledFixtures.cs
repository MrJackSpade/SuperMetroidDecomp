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

    /// <summary>A standalone enemy system bound to the installation's enemy artwork and presentation colors.</summary>
    internal static RoomEnemySystem CreateEnemySystem()
    {
        var enemies = new RoomEnemySystem { TileArtwork = EnemyTiles };
        enemies.BindMapPresentation(Maps);
        return enemies;
    }

    /// <summary>The installation's projectile artwork and frame bindings, loaded once per process.</summary>
    internal static InstalledProjectilePresentation Projectiles => projectiles.Value;

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
    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus,
        CartridgeAudioState? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        new(bus, audio, Maps.PowerBombFixedColors, artwork ?? IntroArtwork, Maps.RoomPaletteFx);

    /// <summary>Creates an intro scene with installed character, projectile, narration, and hurt-color art.</summary>
    internal static IntroCinematicState CreateIntro(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        IntroFontAtlas? introFont = null, IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamTileCatalog? beamArtwork = null, SamusBodyArtworkCatalog? samusBodyArtwork = null)
    {
        var intro = new IntroCinematicState(bus, audio, introFont ?? Maps.IntroFont,
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
    internal static EndingCreditsState CreateEnding(ISnesAddressSpace bus, CartridgeAudioState audio,
        ushort gameTimeHours, ushort gameTimeMinutes, EndingInventorySnapshot inventory = default, bool japaneseText = false)
    {
        var ending = new EndingCreditsState(bus, audio, new EndingCreditsSetup { GameTimeHours = gameTimeHours, GameTimeMinutes = gameTimeMinutes, Inventory = inventory, JapaneseText = japaneseText, EndingText = Maps.EndingText });
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
    internal static FileSelectAreaMapGraphics CreateFileSelectAreaMap(ISnesAddressSpace bus, int selectedArea) =>
        FileSelectAreaMapGraphics.FromPresentation(bus, selectedArea, Maps);

    /// <summary>Creates the title sequence with installed gradient, palette, and graphics defaults.</summary>
    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null) =>
        new(bus, audio, titleGradientPresentation ?? Maps.TitleGradient,
            titlePalettePresentation ?? Maps.TitlePalette, titleGraphicsPresentation ?? Maps.TitleGraphics);

    /// <summary>Creates a pause menu while retaining the supplied state and installed map presentation.</summary>
    internal static PauseMenuState CreatePause(ISnesAddressSpace bus, PauseMenuEntry entry) =>
        new(bus, new PauseMenuEntry { Samus = entry.Samus, System = entry.System, AreaIndex = entry.AreaIndex, RoomMapPosition = entry.RoomMapPosition, Audio = entry.Audio, GameplayVram = entry.GameplayVram, MapRevealMode = entry.MapRevealMode, MapPresentation = Maps });

    /// <summary>
    /// A private copy of the installation for a check that corrupts and restores stock files, so
    /// a failure part-way can never leave the shared installation edited. Disposal deletes it.
    /// </summary>
    internal static PrivateInstallationCopy CreatePrivateCopy() => new(RepositoryInstallation.Installation.Root);

    /// <summary>Binds the host catalogs to a game restored from a save or debugger state.</summary>
    internal static void BindGame(SuperMetroidGame game) => gameBindings.Value(game);

    /// <summary>A frontend game bound to the installation's presentation catalogs.</summary>
    internal static SuperMetroidGame CreateGame(ISnesAddressSpace addressSpace,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true)
    {
        var game = new SuperMetroidGame(addressSpace, gameOptions, renderGameplayFrames);
        gameBindings.Value(game);
        return game;
    }
}
