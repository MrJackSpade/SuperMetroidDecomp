using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Installation fixtures only the verifiers use; the developer tools link only the shared part.</summary>
internal static partial class RepositoryInstallation
{
    /// <summary>Room character, palette, metatile, and visual-layout catalogs cached from the installed cartridge assets.</summary>
    private static readonly Lazy<RoomAssetCatalogs> roomAssets = new(() => new RoomAssetCatalogs(
        Installation.LoadRoomCharacters(), Installation.LoadRoomPalettes(),
        Installation.LoadRoomMetatiles(), Installation.LoadRoomVisualLayouts()));

    /// <summary>Map and shared presentation catalogs loaded once for verification fixtures.</summary>
    private static readonly Lazy<AreaMapPresentationCatalog> maps = new(() => Installation.LoadMaps());

    /// <summary>Enemy graphics, palettes, and tile artwork cached from the installation.</summary>
    private static readonly Lazy<EnemyTileArtworkCatalog> enemyTiles = new(() => Installation.LoadEnemyTiles());

    /// <summary>Compiled projectile and beam presentation assets shared by fixture factories.</summary>
    private static readonly Lazy<InstalledProjectilePresentation> projectiles = new(() => Installation.LoadProjectiles());

    /// <summary>Samus body, cannon, and movement artwork loaded from the installation.</summary>
    private static readonly Lazy<SamusBodyArtworkCatalog> samusBody = new(() => Installation.LoadSamusBodyArt());

    /// <summary>Introductory and Ceres cinematic artwork used by standalone state fixtures.</summary>
    private static readonly Lazy<IntroCinematicArtworkCatalog> introArtwork = new(() => Installation.LoadIntroCinematicArt());

    /// <summary>Factory binding the installation's host presentation catalogs to a game instance.</summary>
    private static readonly Lazy<Action<SuperMetroidGame>> gameBindings =
        new(() => InstalledGameBindings.Create(Installation));

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

    /// <summary>The installation's Samus body and cannon artwork.</summary>
    internal static SamusBodyArtworkCatalog SamusBody => samusBody.Value;

    /// <summary>The installation's intro and Ceres cinematic artwork.</summary>
    internal static IntroCinematicArtworkCatalog IntroArtwork => introArtwork.Value;

    /// <summary>Creates a Samus projectile system using the installed projectile frame bindings.</summary>
    internal static SamusProjectileSystem CreateProjectileSystem() =>
        new() { FrameBindings = Projectiles.FrameBindings };

    /// <summary>Creates a bomb system with installed projectile frames and power-bomb presentation colors.</summary>
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

    /// <summary>Creates a Samus state and binds the installation's suit, tile, cannon, and grapple artwork.</summary>
    internal static SamusState CreateSamus() => BindSamus(new SamusState());

    /// <summary>Creates the Ceres destruction sequence with installed palette and room presentation data.</summary>
    /// <param name="bus">Cartridge address space used by the cinematic.</param>
    /// <param name="audio">Optional audio state shared with the frontend.</param>
    /// <param name="artwork">Optional cinematic artwork override; the installed catalog is used when omitted.</param>
    /// <returns>A destruction cinematic configured with installation assets.</returns>
    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus,
        CartridgeAudioState? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        new(bus, audio, Maps.PowerBombFixedColors, artwork ?? IntroArtwork, Maps.RoomPaletteFx);

    /// <summary>Creates an intro sequence and binds installed assets for its characters, beam, Samus, projectiles, and narration.</summary>
    /// <param name="bus">Cartridge address space used by the sequence.</param>
    /// <param name="audio">Optional audio state shared with the frontend.</param>
    /// <param name="introFont">Optional intro font override.</param>
    /// <param name="characterArtwork">Optional cinematic character-art override.</param>
    /// <param name="beamArtwork">Optional beam tile override.</param>
    /// <param name="samusBodyArtwork">Optional Samus body-art override.</param>
    /// <returns>The intro state with required installation presentation bindings.</returns>
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

    /// <summary>Creates ending credits with installed text, flight, Mode 7, object, palette, font, and staff assets.</summary>
    /// <param name="bus">Cartridge address space used by the ending sequence.</param>
    /// <param name="audio">Audio state advanced by the ending.</param>
    /// <param name="gameTimeHours">Recorded play-time hours shown in the credits.</param>
    /// <param name="gameTimeMinutes">Recorded play-time minutes shown in the credits.</param>
    /// <param name="inventory">Inventory snapshot used by the ending presentation.</param>
    /// <param name="japaneseText">Selects the Japanese text variant when enabled.</param>
    /// <returns>The ending state bound to installation assets.</returns>
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

    /// <summary>Creates area-map graphics for file select using the installation's map presentation catalog.</summary>
    /// <param name="bus">Cartridge address space for graphics upload state.</param>
    /// <param name="selectedArea">Area whose map image is selected.</param>
    /// <returns>File-select map graphics configured with installed area art.</returns>
    internal static FileSelectAreaMapGraphics CreateFileSelectAreaMap(ISnesAddressSpace bus, int selectedArea) =>
        FileSelectAreaMapGraphics.FromPresentation(bus, selectedArea, Maps);

    /// <summary>Creates the title sequence, filling omitted presentation overrides from installed title assets.</summary>
    /// <param name="bus">Cartridge address space used by the sequence.</param>
    /// <param name="audio">Optional audio state shared with the frontend.</param>
    /// <param name="titleGradientPresentation">Optional gradient override.</param>
    /// <param name="titlePalettePresentation">Optional palette override.</param>
    /// <param name="titleGraphicsPresentation">Optional title graphics override.</param>
    /// <returns>The configured title sequence state.</returns>
    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null) =>
        new(bus, audio, titleGradientPresentation ?? Maps.TitleGradient,
            titlePalettePresentation ?? Maps.TitlePalette, titleGraphicsPresentation ?? Maps.TitleGraphics);

    /// <summary>Creates a pause-menu state with the installation's map and pause presentation data.</summary>
    /// <param name="bus">Cartridge address space used by pause-menu logic.</param>
    /// <param name="samus">Player state controlled by the pause menu.</param>
    /// <param name="system">Bank-$80 state containing game and pause flags.</param>
    /// <param name="areaIndex">Current room area for map selection.</param>
    /// <param name="roomMapX">Room's horizontal map coordinate.</param>
    /// <param name="roomMapY">Room's vertical map coordinate.</param>
    /// <param name="audio">Optional audio state shared with the frontend.</param>
    /// <param name="gameplayVram">Optional gameplay VRAM snapshot.</param>
    /// <param name="mapRevealMode">Initial map reveal policy for the menu.</param>
    /// <returns>A pause menu state using installation catalogs.</returns>
    internal static PauseMenuState CreatePause(ISnesAddressSpace bus, SamusState samus, Bank80SystemState system,
        AreaId areaIndex, byte roomMapX, byte roomMapY, CartridgeAudioState? audio = null,
        SnesVram? gameplayVram = null, MapRevealMode mapRevealMode = MapRevealMode.None) =>
        new(bus, samus, system, areaIndex, roomMapX, roomMapY, audio, gameplayVram, mapRevealMode, Maps);

    /// <summary>
    /// A private copy of the installation for a check that corrupts and restores stock files, so
    /// a failure part-way can never leave the shared installation edited. Disposal deletes it.
    /// </summary>
    internal static PrivateInstallationCopy CreatePrivateCopy() => new(Installation.Root);

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
    /// <summary>Copies an installation tree into a private temporary directory for checks that mutate stock assets.</summary>
    /// <param name="sourceRoot">Root directory whose files and subdirectories are copied.</param>
    internal PrivateInstallationCopy(string sourceRoot)
    {
        Root = Directory.CreateTempSubdirectory("SuperMetroid-installation-copy-").FullName;
        foreach (string directory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(Root, Path.GetRelativePath(sourceRoot, directory)));
        foreach (string file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(Root, Path.GetRelativePath(sourceRoot, file)));
    }

    /// <summary>Path to this fixture's disposable copy of the installation.</summary>
    internal string Root { get; }

    /// <summary>Gets an installation loader rooted at the private copied directory.</summary>
    internal GameInstallation Installation => new(Root);

    /// <summary>Deletes the temporary installation tree created for this verification fixture.</summary>
    public void Dispose() => Directory.Delete(Root, recursive: true);
}
