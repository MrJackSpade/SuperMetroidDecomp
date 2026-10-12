using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Spatial;

/// <summary>Installation fixtures only the verifiers use; the developer tools link only the shared part.</summary>
internal static class InstalledFixtures
{
    /// <summary>Stores room assets.</summary>
    private static readonly Lazy<RoomAssetCatalogs> roomAssets = new(() => new RoomAssetCatalogs(
        RepositoryInstallation.Installation.LoadRoomCharacters(), RepositoryInstallation.Installation.LoadRoomPalettes(),
        RepositoryInstallation.Installation.LoadRoomMetatiles(), RepositoryInstallation.Installation.LoadRoomVisualLayouts()));

    /// <summary>Stores maps.</summary>
    private static readonly Lazy<AreaMapPresentationCatalog> maps = new(() => RepositoryInstallation.Installation.LoadMaps());

    /// <summary>Stores enemy tiles.</summary>
    private static readonly Lazy<EnemyTileArtworkCatalog> enemyTiles = new(() => RepositoryInstallation.Installation.LoadEnemyTiles());

    /// <summary>Stores projectiles.</summary>
    private static readonly Lazy<InstalledProjectilePresentation> projectiles = new(() => RepositoryInstallation.Installation.LoadProjectiles());

    /// <summary>Stores samus body.</summary>
    private static readonly Lazy<SamusBodyArtworkCatalog> samusBody = new(() => RepositoryInstallation.Installation.LoadSamusBodyArt());

    /// <summary>Stores intro artwork.</summary>
    private static readonly Lazy<IntroCinematicArtworkCatalog> introArtwork = new(() => RepositoryInstallation.Installation.LoadIntroCinematicArt());

    /// <summary>Stores game bindings.</summary>
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

    /// <summary>Gets samus body.</summary>
    internal static SamusBodyArtworkCatalog SamusBody => samusBody.Value;

    /// <summary>Gets intro artwork.</summary>
    internal static IntroCinematicArtworkCatalog IntroArtwork => introArtwork.Value;

    /// <summary>Creates projectile system.</summary>
    internal static SamusProjectileSystem CreateProjectileSystem() =>
        new() { Animation = { FrameBindings = Projectiles.FrameBindings } };

    /// <summary>Creates bomb system.</summary>
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

    /// <summary>Creates samus.</summary>
    internal static SamusState CreateSamus() => BindSamus(new SamusState());

    /// <summary>Creates destruction.</summary>
    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus,
        CartridgeAudioState? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        new(bus, audio, Maps.PowerBombFixedColors, artwork ?? IntroArtwork, Maps.RoomPaletteFx);

    /// <summary>Creates intro.</summary>
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

    /// <summary>Creates ending.</summary>
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

    /// <summary>Creates file select area map.</summary>
    internal static FileSelectAreaMapGraphics CreateFileSelectAreaMap(ISnesAddressSpace bus, int selectedArea) =>
        FileSelectAreaMapGraphics.FromPresentation(bus, selectedArea, Maps);

    /// <summary>Creates title.</summary>
    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null) =>
        new(bus, audio, titleGradientPresentation ?? Maps.TitleGradient,
            titlePalettePresentation ?? Maps.TitlePalette, titleGraphicsPresentation ?? Maps.TitleGraphics);

    /// <summary>Creates pause.</summary>
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

/// <summary>A disposable copy of an installation root in a test scratch directory.</summary>
internal sealed class PrivateInstallationCopy : IDisposable
{
    /// <summary>Stores scratch.</summary>
    private readonly TestScratchDirectory scratch = new("installation-copy");

    /// <summary>Initializes a new private installation copy instance.</summary>
    internal PrivateInstallationCopy(string sourceRoot)
    {
        Root = scratch.Root;
        Directory.CreateDirectory(Root);
        foreach (string directory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(Root, Path.GetRelativePath(sourceRoot, directory)));
        foreach (string file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(Root, Path.GetRelativePath(sourceRoot, file)));
    }

    /// <summary>Gets root.</summary>
    internal string Root { get; }

    /// <summary>Gets installation.</summary>
    internal GameInstallation Installation => new(Root);

    /// <summary>Disposes.</summary>
    public void Dispose() => scratch.Dispose();
}
