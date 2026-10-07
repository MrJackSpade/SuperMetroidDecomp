using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// The repository ROM's extracted installation in ignored <c>csharp/test-temp</c>, shared by the
/// verifier and the developer tools. Each process loads its catalogs once; every fixture built
/// here keeps its own bus and mutable runtime state.
/// </summary>
internal static class RepositoryInstallation
{
    private static readonly Lazy<GameInstallation> installation = new(() =>
    {
        string root = Path.GetFullPath("csharp/test-temp/verification-installed-content");
        return GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
    });

    private static readonly Lazy<RoomAssetCatalogs> roomAssets = new(() => new RoomAssetCatalogs(
        Installation.LoadRoomCharacters(), Installation.LoadRoomPalettes(),
        Installation.LoadRoomMetatiles(), Installation.LoadRoomVisualLayouts()));

    private static readonly Lazy<AreaMapPresentationCatalog> maps = new(() => Installation.LoadMaps());

    private static readonly Lazy<EnemyTileArtworkCatalog> enemyTiles = new(() => Installation.LoadEnemyTiles());

    private static readonly Lazy<InstalledProjectilePresentation> projectiles = new(() => Installation.LoadProjectiles());

    private static readonly Lazy<SamusBodyArtworkCatalog> samusBody = new(() => Installation.LoadSamusBodyArt());

    private static readonly Lazy<IntroCinematicArtworkCatalog> introArtwork = new(() => Installation.LoadIntroCinematicArt());

    private static readonly Lazy<Action<SuperMetroidRuntime>> runtimeBindings =
        new(() => InstalledRuntimeBindings.Create(Installation));

    private static readonly Lazy<Action<SuperMetroidGame>> gameBindings =
        new(() => InstalledGameBindings.Create(Installation));

    internal static GameInstallation Installation => installation.Value;

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

    internal static SamusBodyArtworkCatalog SamusBody => samusBody.Value;

    internal static IntroCinematicArtworkCatalog IntroArtwork => introArtwork.Value;

    internal static SamusProjectileSystem CreateProjectileSystem() =>
        new() { FrameBindings = Projectiles.FrameBindings };

    internal static SamusBombProjectileSystem CreateBombSystem() =>
        new() { FrameBindings = Projectiles.FrameBindings, PowerBombExplosion = { PresentationColors = Maps.PowerBombFixedColors } };

    /// <summary>Binds the installation's suit, visor, body and grapple presentation to a standalone Samus.</summary>
    internal static SamusState BindSamus(SamusState samus)
    {
        samus.SuitColors = Maps.SamusSuitColors;
        samus.FullBodyCycleColors = Maps.SamusFullBodyCycleColors;
        samus.ChargeColors = Maps.SamusChargeColors;
        samus.VisorPalette.PresentationColors = Maps.SamusVisorColors;
        samus.Xray.PresentationColors = Maps.SamusVisorColors;
        samus.Drained.PresentationColors = Maps.SamusHyperBeamColors;
        samus.TileTransfers.BindArtwork(SamusBody);
        samus.ArmCannon.Artwork = SamusBody.ArmCannon;
        samus.Grapple.FlarePlacement = Projectiles.GrappleTiles.FlarePlacement;
        samus.Grapple.SwingFrames = Projectiles.GrappleTiles.SwingFrames;
        return samus;
    }

    internal static SamusState CreateSamus() => BindSamus(new SamusState());

    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus,
        CartridgeAudioState? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        new(bus, audio, Maps.PowerBombFixedColors, artwork ?? IntroArtwork, Maps.RoomPaletteFx);

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

    internal static EndingCreditsState CreateEnding(ISnesAddressSpace bus, CartridgeAudioState audio,
        ushort gameTimeHours, ushort gameTimeMinutes, EndingInventorySnapshot inventory = default, bool japaneseText = false)
    {
        var ending = new EndingCreditsState(bus, audio, gameTimeHours, gameTimeMinutes, inventory, japaneseText, Maps.EndingText);
        ending.BindFlightArtwork(IntroArtwork.CeresFlight);
        ending.BindMode7Artwork(Installation.LoadEndingMode7Art());
        ending.BindObjectArtwork(Installation.LoadEndingObjectArt());
        ending.BindPaletteArtwork(Installation.LoadEndingPalettes());
        ending.BindPaletteFxColors(Maps.RoomPaletteFx);
        ending.BindEndingFont(Maps.EndingFont);
        ending.BindStaffCredits(Maps.StaffCredits);
        return ending;
    }

    internal static FileSelectAreaMapGraphics CreateFileSelectAreaMap(ISnesAddressSpace bus, int selectedArea) =>
        FileSelectAreaMapGraphics.FromPresentation(bus, selectedArea, Maps);

    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null) =>
        new(bus, audio, titleGradientPresentation ?? Maps.TitleGradient,
            titlePalettePresentation ?? Maps.TitlePalette, titleGraphicsPresentation ?? Maps.TitleGraphics);

    internal static PauseMenuState CreatePause(ISnesAddressSpace bus, SamusState samus, Bank80SystemState system,
        AreaId areaIndex, byte roomMapX, byte roomMapY, CartridgeAudioState? audio = null,
        SnesVram? gameplayVram = null, MapRevealMode mapRevealMode = MapRevealMode.None) =>
        new(bus, samus, system, areaIndex, roomMapX, roomMapY, audio, gameplayVram, mapRevealMode, Maps);

    /// <summary>
    /// A private copy of the installation for a check that corrupts and restores stock files, so
    /// a failure part-way can never leave the shared installation edited. Disposal deletes it.
    /// </summary>
    internal static PrivateInstallationCopy CreatePrivateCopy() => new(Installation.Root);

    /// <summary>A gameplay runtime bound to the installation's presentation catalogs.</summary>
    internal static SuperMetroidRuntime CreateRuntime(ISnesAddressSpace addressSpace,
        bool playerInvincibilityEnabled = false, bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None, bool preventEscapeTimeout = false)
    {
        var runtime = new SuperMetroidRuntime(addressSpace, playerInvincibilityEnabled,
            infiniteAmmoEnabled, mapRevealMode, preventEscapeTimeout,
            Installation.LoadGameplayBasePalettes());
        runtimeBindings.Value(runtime);
        return runtime;
    }

    /// <summary>Rebinds the host catalogs to a runtime restored from a debugger state, which omits them.</summary>
    internal static void BindRuntime(SuperMetroidRuntime runtime) => runtimeBindings.Value(runtime);

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

/// <summary>A disposable copy of an installation root in the system temporary directory.</summary>
internal sealed class PrivateInstallationCopy : IDisposable
{
    internal PrivateInstallationCopy(string sourceRoot)
    {
        Root = Directory.CreateTempSubdirectory("SuperMetroid-installation-copy-").FullName;
        foreach (string directory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(Root, Path.GetRelativePath(sourceRoot, directory)));
        foreach (string file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(Root, Path.GetRelativePath(sourceRoot, file)));
    }

    internal string Root { get; }

    internal GameInstallation Installation => new(Root);

    public void Dispose() => Directory.Delete(Root, recursive: true);
}
