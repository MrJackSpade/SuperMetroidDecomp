using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Top-level C# counterpart of bank $82's <c>RunOneFrameOfGameInner</c> dispatcher.
/// </summary>
/// <remarks>
/// The dispatcher is intentionally narrow while the front end is translated incrementally:
/// every supported state owns its cartridge-derived logic and framebuffer, and unsupported
/// transitions stop on a named native state instead of silently jumping to a debug room.
/// </remarks>
public sealed class SuperMetroidGame
{
    /// <summary>The owner of MenuGamePhases's moved members.</summary>
    [field: NonSerialized]
    private MenuGamePhases MenuPhases => field ??= new MenuGamePhases(this);

    /// <summary>The owner of LoadingGamePhases's moved members.</summary>
    [field: NonSerialized]
    private LoadingGamePhases LoadingPhases => field ??= new LoadingGamePhases(this);

    /// <summary>The owner of GameplayGamePhases's moved members.</summary>
    [field: NonSerialized]
    private GameplayGamePhases GameplayPhases => field ??= new GameplayGamePhases(this);

    /// <summary>The owner of PauseGamePhases's moved members.</summary>
    [field: NonSerialized]
    private PauseGamePhases PausePhases => field ??= new PauseGamePhases(this);

    /// <summary>The owner of DeathGamePhases's moved members.</summary>
    [field: NonSerialized]
    private DeathGamePhases DeathPhases => field ??= new DeathGamePhases(this);

    /// <summary>The owner of DoorGamePhases's moved members.</summary>
    [field: NonSerialized]
    private DoorGamePhases DoorPhases => field ??= new DoorGamePhases(this);

    /// <summary>The owner of CinematicGamePhases's moved members.</summary>
    [field: NonSerialized]
    private CinematicGamePhases CinematicPhases => field ??= new CinematicGamePhases(this);

    /// <summary>The owner of AttractGamePhases's moved members.</summary>
    [field: NonSerialized]
    private AttractGamePhases AttractPhases => field ??= new AttractGamePhases(this);
    /// <summary>Shared fade clock for frontend transitions between rendered phases.</summary>
    internal readonly ScreenFadeTimer screenFade = new();
    /// <summary>Accepted menu NMI counters used by frontend animation and input timing.</summary>
    internal readonly MenuNmiFrameCounters menuNmi = new();

    /// <summary>The installed catalogs currently bound; <see cref="GameArtwork.Unbound"/> until a host binds them.</summary>
    [field: NonSerialized]
    public GameArtwork Artwork { get => field ?? GameArtwork.Unbound; private set; }

    /// <summary>
    /// Binds installed artwork at startup and after debugger-state restoration. Each catalog whose reference
    /// changed reaches the live runtime and scenes that hold it; unchanged catalogs are left alone.
    /// </summary>
    public void BindArtwork(GameArtwork artwork)
    {
        Ensure.NotNull(artwork);
        GameArtwork previous = Artwork;
        Artwork = artwork;
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
        {
            saveRam.BindMapPresentation(artwork.MapPresentation);
            if (runtime is not null) runtime.Artwork.MapPresentation = artwork.MapPresentation;
            ceresDestruction?.BindFixedColors(artwork.MapPresentation?.PowerBombFixedColors);
            ceresDestruction?.BindPaletteFxColors(artwork.MapPresentation?.RoomPaletteFx);
            pauseMenu?.BindMapPresentation(artwork.MapPresentation);
            fileSelectMap?.BindMapPresentation(artwork.MapPresentation);
            gameOver?.BindMapPresentation(artwork.MapPresentation);
            options?.BindMapPresentation(artwork.MapPresentation);
            fileSelect?.BindMapPresentation(artwork.MapPresentation);
            title?.BindTitleGradient(artwork.MapPresentation?.TitleGradient);
            title?.BindTitleGraphics(artwork.MapPresentation?.TitleGraphics);
            title?.BindTitlePalette(artwork.MapPresentation?.TitlePalette);
            if (intro is not null) intro.NarrationPresentation = artwork.MapPresentation?.IntroNarration;
            intro?.BindIntroFont(artwork.MapPresentation?.IntroFont);
            intro?.BindSamusHurtColors(artwork.MapPresentation?.SamusHurtColors);
            endingCredits?.BindEndingText(artwork.MapPresentation?.EndingText);
            endingCredits?.BindPaletteFxColors(artwork.MapPresentation?.RoomPaletteFx);
            endingCredits?.BindEndingFont(artwork.MapPresentation?.EndingFont);
            endingCredits?.BindStaffCredits(artwork.MapPresentation?.StaffCredits);
        }
        if (!ReferenceEquals(previous.GameplayBasePalettes, artwork.GameplayBasePalettes))
        {
            if (runtime is not null) runtime.Artwork.InitialPaletteArt = artwork.GameplayBasePalettes;
        }
        if (!ReferenceEquals(previous.StandardObjectArt, artwork.StandardObjectArt))
        {
            if (runtime is not null) runtime.Artwork.StandardObjectArt = artwork.StandardObjectArt;
        }
        if (!ReferenceEquals(previous.IntroCinematicArt, artwork.IntroCinematicArt))
        {
            intro?.BindCharacterArtwork(artwork.IntroCinematicArt);
            ceresDestruction?.BindArtwork(artwork.IntroCinematicArt);
            endingCredits?.BindFlightArtwork(artwork.IntroCinematicArt?.CeresFlight);
        }
        if (!ReferenceEquals(previous.SamusBodyArt, artwork.SamusBodyArt))
        {
            if (runtime is not null) runtime.Artwork.SamusBodyArt = artwork.SamusBodyArt;
            intro?.BindSamusBodyArtwork(artwork.SamusBodyArt);
        }
        if (!ReferenceEquals(previous.EndingMode7Art, artwork.EndingMode7Art))
        {
            endingCredits?.BindMode7Artwork(artwork.EndingMode7Art);
        }
        if (!ReferenceEquals(previous.EndingObjectArt, artwork.EndingObjectArt))
        {
            endingCredits?.BindObjectArtwork(artwork.EndingObjectArt);
        }
        if (!ReferenceEquals(previous.EndingPaletteArt, artwork.EndingPaletteArt))
        {
            endingCredits?.BindPaletteArtwork(artwork.EndingPaletteArt);
        }
        if (!ReferenceEquals(previous.RoomCharacterArt, artwork.RoomCharacterArt))
        {
            if (runtime is not null) runtime.Artwork.RoomCharacterArt = artwork.RoomCharacterArt;
        }
        if (!ReferenceEquals(previous.RoomPaletteArt, artwork.RoomPaletteArt))
        {
            if (runtime is not null) runtime.Artwork.RoomPaletteArt = artwork.RoomPaletteArt;
        }
        if (!ReferenceEquals(previous.RoomMetatileArt, artwork.RoomMetatileArt))
        {
            if (runtime is not null) runtime.Artwork.RoomMetatileArt = artwork.RoomMetatileArt;
        }
        if (!ReferenceEquals(previous.RoomVisualLayouts, artwork.RoomVisualLayouts))
        {
            if (runtime is not null) runtime.Artwork.RoomVisualLayouts = artwork.RoomVisualLayouts;
        }
        if (!ReferenceEquals(previous.RoomLevelStreams, artwork.RoomLevelStreams))
        {
            if (runtime is not null) runtime.Artwork.RoomLevelStreams = artwork.RoomLevelStreams;
        }
        if (!ReferenceEquals(previous.RoomPlmShotBlockVisuals, artwork.RoomPlmShotBlockVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmShotBlockVisuals = artwork.RoomPlmShotBlockVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmGrappleBlockVisuals, artwork.RoomPlmGrappleBlockVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmGrappleBlockVisuals = artwork.RoomPlmGrappleBlockVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmStationVisuals, artwork.RoomPlmStationVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmStationVisuals = artwork.RoomPlmStationVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmBlueDoorVisuals, artwork.RoomPlmBlueDoorVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmBlueDoorVisuals = artwork.RoomPlmBlueDoorVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmColoredDoorVisuals, artwork.RoomPlmColoredDoorVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmColoredDoorVisuals = artwork.RoomPlmColoredDoorVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmGreyDoorVisuals, artwork.RoomPlmGreyDoorVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmGreyDoorVisuals = artwork.RoomPlmGreyDoorVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmEyeDoorVisuals, artwork.RoomPlmEyeDoorVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmEyeDoorVisuals = artwork.RoomPlmEyeDoorVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmMotherBrainGlassVisuals, artwork.RoomPlmMotherBrainGlassVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmMotherBrainGlassVisuals = artwork.RoomPlmMotherBrainGlassVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmNoobTubeVisuals, artwork.RoomPlmNoobTubeVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmNoobTubeVisuals = artwork.RoomPlmNoobTubeVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmDownwardGateVisuals, artwork.RoomPlmDownwardGateVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmDownwardGateVisuals = artwork.RoomPlmDownwardGateVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmElevatorPlatformVisuals, artwork.RoomPlmElevatorPlatformVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmElevatorPlatformVisuals = artwork.RoomPlmElevatorPlatformVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmEscapeGateVisuals, artwork.RoomPlmEscapeGateVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmEscapeGateVisuals = artwork.RoomPlmEscapeGateVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmBombTorizoHandVisuals, artwork.RoomPlmBombTorizoHandVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmBombTorizoHandVisuals = artwork.RoomPlmBombTorizoHandVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmDraygonCannonVisuals, artwork.RoomPlmDraygonCannonVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmDraygonCannonVisuals = artwork.RoomPlmDraygonCannonVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmChozoStatueVisuals, artwork.RoomPlmChozoStatueVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmChozoStatueVisuals = artwork.RoomPlmChozoStatueVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmLinkedRestoreVisuals, artwork.RoomPlmLinkedRestoreVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmLinkedRestoreVisuals = artwork.RoomPlmLinkedRestoreVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmTourianAccessVisuals, artwork.RoomPlmTourianAccessVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmTourianAccessVisuals = artwork.RoomPlmTourianAccessVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmSpeedBoosterVisuals, artwork.RoomPlmSpeedBoosterVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmSpeedBoosterVisuals = artwork.RoomPlmSpeedBoosterVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmMaridiaElevatubeVisuals, artwork.RoomPlmMaridiaElevatubeVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmMaridiaElevatubeVisuals = artwork.RoomPlmMaridiaElevatubeVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmSporeSpawnCeilingVisuals, artwork.RoomPlmSporeSpawnCeilingVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmSporeSpawnCeilingVisuals = artwork.RoomPlmSporeSpawnCeilingVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmSamusEaterVisuals, artwork.RoomPlmSamusEaterVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmSamusEaterVisuals = artwork.RoomPlmSamusEaterVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmBotwoonWallVisuals, artwork.RoomPlmBotwoonWallVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmBotwoonWallVisuals = artwork.RoomPlmBotwoonWallVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmKraidVisuals, artwork.RoomPlmKraidVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmKraidVisuals = artwork.RoomPlmKraidVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmCrocomireVisuals, artwork.RoomPlmCrocomireVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmCrocomireVisuals = artwork.RoomPlmCrocomireVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmMotherBrainFakeDeathVisuals, artwork.RoomPlmMotherBrainFakeDeathVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmMotherBrainFakeDeathVisuals = artwork.RoomPlmMotherBrainFakeDeathVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmCollectibleVisuals, artwork.RoomPlmCollectibleVisuals))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmCollectibleVisuals = artwork.RoomPlmCollectibleVisuals;
        }
        if (!ReferenceEquals(previous.RoomPlmDynamicCollectibleArt, artwork.RoomPlmDynamicCollectibleArt))
        {
            if (runtime is not null) runtime.ShotBlockVisuals.RoomPlmDynamicCollectibleArt = artwork.RoomPlmDynamicCollectibleArt;
        }
        if (!ReferenceEquals(previous.XrayRevealVisuals, artwork.XrayRevealVisuals))
        {
            if (runtime is not null) runtime.Xray.XrayRevealVisuals = artwork.XrayRevealVisuals;
        }
        if (!ReferenceEquals(previous.RoomBackgroundTilemapArt, artwork.RoomBackgroundTilemapArt))
        {
            if (runtime is not null) runtime.Artwork.RoomBackgroundTilemapArt = artwork.RoomBackgroundTilemapArt;
        }
        if (!ReferenceEquals(previous.RoomSkyTilemapArt, artwork.RoomSkyTilemapArt))
        {
            if (runtime is not null) runtime.Artwork.RoomSkyTilemapArt = artwork.RoomSkyTilemapArt;
        }
        if (!ReferenceEquals(previous.ProjectileCompositions, artwork.ProjectileCompositions))
        {
            if (runtime is not null) runtime.Artwork.ProjectileCompositions = artwork.ProjectileCompositions;
            if (intro is not null) intro.ProjectileCompositions = artwork.ProjectileCompositions;
        }
        if (!ReferenceEquals(previous.ProjectileFrameBindings, artwork.ProjectileFrameBindings))
        {
            if (runtime is not null) runtime.Artwork.ProjectileFrameBindings = artwork.ProjectileFrameBindings;
            if (intro is not null) intro.ProjectileFrameBindings = artwork.ProjectileFrameBindings;
        }
        if (!ReferenceEquals(previous.BeamArtwork, artwork.BeamArtwork))
        {
            if (runtime is not null) runtime.Artwork.BeamArtwork = artwork.BeamArtwork;
            // A completed intro remains in a restored object graph, but its VRAM
            // and beam DMA are no longer displayed or advanced during gameplay.
            // Rebinding that dormant scene would replay an unrelated cartridge
            // read while loading a gameplay debugger state.
            if (GameState == SuperMetroidGameState.IntroCinematic)
                intro?.BindBeamArtwork(artwork.BeamArtwork);
        }
        if (!ReferenceEquals(previous.EnemyTileArtwork, artwork.EnemyTileArtwork))
        {
            if (runtime is not null)
            {
                runtime.Enemies.TileArtwork = artwork.EnemyTileArtwork;
                runtime.Room.CeresElevatorArrival?.BindProjectileSpritemaps(
                    artwork.EnemyTileArtwork?.ProjectileSpritemaps);
            }
        }
        if (!ReferenceEquals(previous.TrailArtwork, artwork.TrailArtwork))
        {
            if (runtime is not null) runtime.Artwork.TrailArtwork = artwork.TrailArtwork;
            if (intro is not null) intro.TrailArtwork = artwork.TrailArtwork;
        }
        if (!ReferenceEquals(previous.ChargeFlarePlacement, artwork.ChargeFlarePlacement))
        {
            if (runtime is not null) runtime.Artwork.ChargeFlarePlacement = artwork.ChargeFlarePlacement;
        }
        if (!ReferenceEquals(previous.ChargeFlareCompositions, artwork.ChargeFlareCompositions))
        {
            if (runtime is not null) runtime.Artwork.ChargeFlareCompositions = artwork.ChargeFlareCompositions;
        }
        if (!ReferenceEquals(previous.GrappleArtwork, artwork.GrappleArtwork))
        {
            if (runtime is not null) runtime.Artwork.GrappleArtwork = artwork.GrappleArtwork;
        }
    }

    /// <summary>Mutable emulated address space retained for the whole game session.</summary>
    internal readonly ISnesAddressSpace bus;

    /// <summary>Current host options; interactive state restoration rebinds these to the active session.</summary>
    public SuperMetroidGameOptions ConfiguredOptions { get; private set; }
    /// <summary>Owns scene capture and raster publication for the current frontend frame.</summary>
    internal readonly FrontendDisplay display;
    /// <summary>Battery-backed save data and selected-slot operations.</summary>
    internal readonly SuperMetroidSaveRam saveRam;

    /// <summary>Boot-time <see cref="Bank80SystemState.MainGameLoopCarry"/>; fixed for the session.</summary>
    private readonly bool bootMainLoopCarry;
    /// <summary>Boot-time <c>NumberOfDemoSets</c> ($1F59); fixed for the session.</summary>
    private readonly int bootDemoSetCount;
    /// <summary>Native music and SFX queues shared across menus and gameplay.</summary>
    internal readonly CartridgeAudioState audio = new();
    /// <summary>Core-owned SPC port model and synthesizer advanced at the end of every update.</summary>
    internal readonly CartridgeAudioSession audioSession = new();
    /// <summary>Opening logo and title sequence while the corresponding state is active.</summary>
    internal TitleSequenceState? title;
    /// <summary>Save-slot selection menu state.</summary>
    internal FileSelectMenuState? fileSelect;
    /// <summary>Saved-game map selection and loading screen state.</summary>
    internal FileSelectMapMenuState? fileSelectMap;
    /// <summary>Controller and display option menu state.</summary>
    internal GameOptionsMenuState? options;
    /// <summary>Game-over choice screen after death.</summary>
    internal GameOverMenuState? gameOver;
    /// <summary>New-game prologue and flashback cinematic.</summary>
    internal IntroCinematicState? intro;
    /// <summary>Ceres destruction scene after Ridley's escape.</summary>
    internal CeresDestructionCinematicState? ceresDestruction;
    /// <summary>Post-escape ending and staff credits.</summary>
    internal EndingCreditsState? endingCredits;
    /// <summary>Pause-map and equipment menu state.</summary>
    internal PauseMenuState? pauseMenu;
    /// <summary>Live gameplay runtime, absent during frontend-only phases.</summary>
    internal SuperMetroidRuntime? runtime;
    /// <summary>The state of the opening title sequence, which exists only while that phase runs.</summary>
    internal TitleSequenceState ActiveTitle => title ??
        throw new InvalidOperationException("The opening title sequence is not running.");
    /// <summary>The state of the file-select menu, which exists only while that phase runs.</summary>
    internal FileSelectMenuState ActiveFileSelect => fileSelect ??
        throw new InvalidOperationException("The file-select menu is not running.");
    /// <summary>The state of the options menu, which exists only while that phase runs.</summary>
    internal GameOptionsMenuState ActiveOptions => options ??
        throw new InvalidOperationException("The options menu is not running.");
    /// <summary>The state of the intro cinematic, which exists only while that phase runs.</summary>
    internal IntroCinematicState ActiveIntro => intro ??
        throw new InvalidOperationException("The intro cinematic is not running.");
    /// <summary>The state of the ending and credits, which exists only while that phase runs.</summary>
    internal EndingCreditsState ActiveEndingCredits => endingCredits ??
        throw new InvalidOperationException("The ending and credits is not running.");
    /// <summary>The state of the pause menu, which exists only while that phase runs.</summary>
    internal PauseMenuState ActivePauseMenu => pauseMenu ??
        throw new InvalidOperationException("The pause menu is not running.");
    /// <summary>The live gameplay runtime; gameplay phases run only while a game is loaded.</summary>
    internal SuperMetroidRuntime GameplayRuntime => runtime ??
        throw new InvalidOperationException("No gameplay runtime is loaded; this phase runs only during gameplay.");
    /// <summary>Ceres elevator departure and blackout transition state.</summary>
    internal readonly CeresDepartureState ceresDeparture = new();
    /// <summary>Reserve-tank recovery state that can temporarily own the dispatcher.</summary>
    internal readonly SamusReserveAutoRecoveryState reserveRecovery = new();
    /// <summary>Door approach and destination loading state across gameplay states.</summary>
    internal readonly DoorTransitionState doorTransition = new();

    /// <summary>Current battery-backed save slot selected by the frontend.</summary>
    internal int selectedSaveSlot;
    /// <summary>Distinguishes a saved-game load from fresh Ceres setup.</summary>
    internal bool loadingExistingSave;
    /// <summary>Save state retained for restart after the Space-Time intro sequence.</summary>
    internal SuperMetroidSaveSlot? spacetimeIntroRestartSlot;
    /// <summary>Brightness during the fade into Landing Site or Ceres arrival.</summary>
    internal byte postCeresFadeBrightness;
    /// <summary>Cadence counter for the post-Ceres fade.</summary>
    internal int postCeresFadeCounter = 1;
    /// <summary>Selects Ceres arrival rather than Landing Site at the fade destination.</summary>
    internal bool gameplayFadeLeadsToCeresArrival;
    /// <summary>Gameplay brightness retained through pause and unpause fades.</summary>
    internal byte pauseBrightness = PauseFadeTiming.FullyLit;
    /// <summary>Palette transition for Samus's death sequence, when active.</summary>
    internal CartridgePaletteTransition? deathPaletteFade;
    /// <summary>Palette transition for the escape time-up sequence.</summary>
    internal CartridgePaletteTransition? timeUpPaletteFade;
    /// <summary>Brightness during the death-state blackout.</summary>
    internal byte deathFadeBrightness = 15;
    /// <summary>Cadence counter for the death fade.</summary>
    internal int deathFadeCounter;
    /// <summary>Brightness during the ending transition after Zebes escape.</summary>
    internal byte endingFadeBrightness = 15;
    /// <summary>Cadence counter for the ending fade.</summary>
    internal int endingFadeCounter;
    /// <summary>The most recent update's ordered semantic audio events; the session has already rendered them.</summary>
    internal IReadOnlyList<AudioEvent> lastAudioEvents = Array.Empty<AudioEvent>();
    /// <summary>Runtime publication number last copied into the frontend audio queue.</summary>
    internal ulong? lastAudioRuntimeGameplayPublication;
    /// <summary>Room-state pointer associated with the last copied audio publication.</summary>
    internal ushort? lastAudioRoomStatePointer;

    /// <summary>Creates a reset-state session, reading the selected SRAM slot and capturing the native boot carry before any dispatcher frame runs.</summary>
    /// <param name="bus">Retained cartridge address space with <see cref="ISnesMutableMemory"/> support for WRAM and SRAM changes throughout the session.</param>
    /// <param name="gameOptions">Retained host options, or <see langword="null"/> to use a new default options record.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not implement <see cref="ISnesMutableMemory"/>.</exception>
    public SuperMetroidGame(
        ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null)
        : this(bus, gameOptions, renderGameplayFrames: true)
    {
    }

    /// <summary>
    /// Creates the dispatcher with an optional gameplay-software-PPU bypass for friend
    /// verification hosts that inspect cartridge state rather than frame pixels.
    /// </summary>
    /// <remarks>
    /// Frontend screens remain rendered because their state machines own their own
    /// compositors. The expensive room compositor is skipped only after a runtime exists;
    /// production callers always enter through the public rendering constructor above.
    /// </remarks>
    internal SuperMetroidGame(
        ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions,
        bool renderGameplayFrames)
    {
        this.bus = Ensure.NotNull(bus);
        ConfiguredOptions = gameOptions ?? new SuperMetroidGameOptions();
        display = new FrontendDisplay(renderGameplayFrames);
        // The host binds artwork after construction; BindArtwork then rebinds the save RAM's map presentation.
        saveRam = new SuperMetroidSaveRam(bus, mapPresentation: null);
        selectedSaveSlot = saveRam.ReadSelectedSlot();
        // Construction is power-on: boot evaluates SRAM once before entering MainGameLoop.
        bootMainLoopCarry = saveRam.DetermineBootMainLoopCarry();
        bootDemoSetCount = saveRam.DetermineNumberOfDemoSets();
    }

    /// <summary>
    /// Raised after translated code changes battery-backed SRAM. The core owns cartridge
    /// bytes; a desktop/console host owns the choice of durable storage medium.
    /// </summary>
    public event Action? SaveRamChanged;

    /// <summary>Raises <see cref="SaveRamChanged"/> for a phase that changed save RAM.</summary>
    internal void RaiseSaveRamChanged() => SaveRamChanged?.Invoke();

    /// <summary>
    /// Binds the extracted audio catalog at startup and after debugger-state restoration, so
    /// every following update also synthesizes host PCM (<see cref="CopyAudioFrame"/>).
    /// </summary>
    /// <remarks>
    /// Binding affects presentation only. Each update advances the session's SPC port model and
    /// its sound-effect acknowledgements whether or not audio is bound, so gameplay is identical
    /// with audio enabled, disabled, or without an output device.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="clips"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The session already advanced without synthesis and has no player to continue.</exception>
    public void BindAudio(ClipAudioCatalog clips) => audioSession.BindClips(clips);

    /// <summary>Copies the last update's 1,600 interleaved 48-kHz stereo samples into host-owned storage.</summary>
    /// <exception cref="InvalidOperationException">No audio catalog is bound.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> holds fewer than 1,600 samples.</exception>
    public void CopyAudioFrame(Span<short> destination) => audioSession.CopyFrame(destination);

    /// <summary>The SPC acknowledgements each update's sound handlers read; see <see cref="ISoundAcknowledgementSource"/>.</summary>
    public ISoundAcknowledgementSource SoundAcknowledgements { get; set; } = ModeledSoundAcknowledgements.Instance;

    /// <summary>Current value of the native game-state word at WRAM $0998.</summary>
    public SuperMetroidGameState GameState { get; internal set; } = SuperMetroidGameState.Reset;

    /// <summary>Wrapping frame counter corresponding to the game's 16-bit NMI counter.</summary>
    public ushort FrameNumber { get; private set; }

    /// <summary>Current bank-$8F room header pointer, exposed for door-transition watches.</summary>
    public ushort? GameplayActiveRoomPointer => runtime?.Room.ActiveRoom?.Pointer;

    /// <summary>Selected bank-$8F room-state pointer, exposed beside the room identity.</summary>
    public ushort? GameplayActiveRoomStatePointer => runtime?.Room.ActiveRoom?.State.Pointer;

    /// <summary>Validated retail area in the active room header, or null before gameplay.</summary>
    public AreaId? GameplayActiveAreaIndex => runtime?.Room.ActiveRoom?.AreaIndex;

    /// <summary>Native room index within the active area, or null before gameplay.</summary>
    public byte? GameplayActiveRoomIndex => runtime?.Room.ActiveRoom?.RoomIndex;

    /// <summary>Validated logical area/room pair, or null before cartridge gameplay.</summary>
    public RoomIdentity? GameplayActiveRoomIdentity => runtime?.Room.ActiveRoom?.Identity;

    /// <summary>Current bank-$83 entry door pointer, exposed for door-transition watches.</summary>
    public ushort? GameplayActiveDoorPointer => runtime?.Room.ActiveDoor?.Pointer;

    /// <summary>Accepted NMIs taken while a door transition's music upload blocks the main loop.</summary>
    public IDoorMusicUploadNmiSource DoorMusicUploadNmis { get; set; } = LagFreeDoorMusicUploadNmis.Instance;

    /// <summary>How far the cartridge's door loader has progressed; see <see cref="IDoorLoaderProgressSource"/>.</summary>
    public IDoorLoaderProgressSource DoorLoaderProgress { get; set; } = LagFreeDoorLoaderProgress.Instance;

    /// <summary>Runs one dispatcher frame and returns the PPU-visible result.</summary>
    public FrontendFrame Step(SnesButton controllerInput)
    {
        // Derive the door-owned native disable flag before any producer can queue
        // audio. This also restores its meaning for legacy snapshots without adding
        // another serialized flag that could disagree with the coroutine phase.
        GamePhaseAudio phaseAudio = Phases.Of(GameState).Audio;
        audio.DoorTransitionSoundsDisabled = phaseAudio.DoorSoundsDisabled;
        bool pauseInputEpilogue = phaseAudio.PauseInputEpilogue;
        bool messageWasActive = runtime?.MessageBox.IsActive == true;
        bool doorAudioDispatch = phaseAudio.DoorAudioDispatch;
        DoorTransitionPhase startingDoorPhase = doorTransition.Phase;
        // An update that resumes a dispatch after its NMI wait never reaches the main loop's
        // music prologue ($88:84B9) or its sound handler ($82:89EF).
        bool resumesNmiWait = NextUpdateResumesNmiWait;
        // MainGameLoop runs the music queue ($82:894B) before dispatching any game state, so
        // a dispatch that waits on HasQueuedMusic sees this update's queue. Sound handlers
        // run after its coroutine returns. An accepted IRQ/NMI alone runs neither handler,
        // and a message box's lag frames run only the box's own calls (below).
        bool prologueMusicDispatch = !resumesNmiWait && !messageWasActive &&
            (!doorAudioDispatch || !phaseAudio.DoorSoundsDisabled ||
             startingDoorPhase is not (DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.FinishDoorLoading));
        IReadOnlyList<CartridgeAudioCommand> prologueMusicCommands = prologueMusicDispatch
            ? audio.AdvanceMusicDispatch() : Array.Empty<CartridgeAudioCommand>();
        // SendAPUData stalls a door dispatch while door IRQs keep NMIs accepted; nothing
        // between this update's own NMI and the upload reads the counters.
        if (doorAudioDispatch && prologueMusicCommands.Any(command => command.Kind == CartridgeAudioCommandKind.Upload))
            GameplayRuntime.NmiHandler.AcceptHardwareWaitNmis(DoorMusicUploadNmis.AcceptedNmisDuringUpload(audio.MusicDataIndex));
        var gameplayAudio = new GameplayAudioFramePublication(audio);
        FrameNumber++;
        if (runtime is null)
            menuNmi.Advance();
        AdvanceMenuRandom();
        if (runtime is not null)
            runtime.DispatchGameState = GameState;
        Phases.Of(GameState).Step(controllerInput, gameplayAudio);

        CollectTranslatedAudioRequests(gameplayAudio);
        // DisplayMessageBox ($85:8080) calls CancelSoundEffects before its first
        // NMI wait. Publish earlier gameplay requests first, then cancel once at
        // this entry boundary; repeating it during the wait would cancel the
        // message's own cursor/saving sounds. A loaded active message is not an
        // entry and must not inject a new command merely because the host resumed.
        if (!messageWasActive && runtime?.MessageBox.IsActive == true)
            audio.QueueCancelSoundEffects();
        bool musicHandler = false;
        int soundHandlers = !resumesNmiWait && (!doorAudioDispatch ||
            startingDoorPhase is not (DoorTransitionPhase.LoadMoreThingsAndOpenDoor or DoorTransitionPhase.WaitForDoorOpeningScroll))
            ? 1 : 0;
        if (messageWasActive)
        {
            // A message-box lag frame runs exactly the routine's own audio calls.
            MessageBoxFrameAudio boxAudio = GameplayRuntime.MessageBox.LastFrameAudio;
            musicHandler = boxAudio.MusicHandlerCalls != 0;
            soundHandlers = boxAudio.SoundHandlerCalls;
        }
        else if (runtime?.MessageBox.IsActive == true)
        {
            // The opening dispatch's HandleSounds runs only once the routine returns.
            soundHandlers = 0;
        }
        IReadOnlyList<CartridgeAudioCommand> trailingAudioCommands = audio.AdvanceFrame(SoundAcknowledgements.ForNextFrame(audioSession.Acknowledgements),
            advanceMusicQueue: musicHandler,
            soundEffectHandlerCalls: soundHandlers);
        IReadOnlyList<CartridgeAudioCommand> audioCommands = prologueMusicCommands.Count == 0 ? trailingAudioCommands
            : [.. prologueMusicCommands, .. trailingAudioCommands];
        // The SPC acts on this update's port writes and uploads during the update's own DSP
        // cycles; its echoes become visible to the next update's sound handlers.
        lastAudioEvents = audioSession.Advance(audioCommands);
        // $82:8AB0 runs after the pause dispatcher returns, even though Samus's
        // draw-time epilogue did not run. Preserve its held-only publication;
        // updating the press edge or auto-jump timer here would invent gameplay.
        if (pauseInputEpilogue)
            GameplayRuntime.Player.IntroducedSamus.InputSnapshot.PublishMainLoopHeldInput(GameplayRuntime.Controller1.Current);
        return CurrentFrame;
    }

    /// <summary>One phase per game state, in native state order; each runs that state's dispatch.</summary>
    [field: NonSerialized]
    private GamePhaseTable Phases => field ??= new GamePhaseTable(
        [
            new GamePhase(SuperMetroidGameState.Reset,
                MainLoopRandomCall.Reseed,
                step: (_, _) => MenuPhases.DispatchReset(),
                name: () => "Reset"),
            new GamePhase(SuperMetroidGameState.OpeningCinematic,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => MenuPhases.DispatchOpeningCinematic(controllerInput),
                name: () => ActiveTitle.Phase.ToString()),
            new GamePhase(SuperMetroidGameState.GameOptionsMenu,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => MenuPhases.DispatchGameOptionsMenu(controllerInput),
                name: () => ActiveOptions.Phase.ToString()),
            new UnreachableGamePhase(SuperMetroidGameState.Unused03),
            new GamePhase(SuperMetroidGameState.FileSelectMenus,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchFileSelectMenus(controllerInput),
                name: () => ActiveFileSelect.Phase.ToString(),
                resumesNmiWait: () => fileSelect?.ResumesAfterNmiWait == true),
            // The map's final dispatch resumes after its forced-blank NMI wait.
            // The final map dispatch latched its load request, then waited for this NMI ($81:AF8C).
            new GamePhase(SuperMetroidGameState.FileSelectMap,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchFileSelectMap(controllerInput),
                name: () => fileSelectMap?.Phase.ToString() ?? "Preparing saved-game map",
                resumesNmiWait: () => fileSelectMap?.LoadRequested == true),
            new GamePhase(SuperMetroidGameState.LoadingGameData,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => LoadingPhases.DispatchLoadingGameData(controllerInput),
                name: () => $"Loading Landing Site ({gameLoadingWaitsRemaining} NMI waits)",
                resumesNmiWait: () => ResumesGameLoadingWait),
            new GamePhase(SuperMetroidGameState.MainGameplayFadeIn,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => LoadingPhases.DispatchMainGameplayFadeIn(controllerInput, gameplayAudio),
                name: () => gameplayFadeLeadsToCeresArrival ? $"Ceres elevator fade-in (brightness {postCeresFadeBrightness})" : $"Landing Site fade-in (brightness {postCeresFadeBrightness})"),
            new GamePhase(SuperMetroidGameState.MainGameplay,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchMainGameplay(controllerInput, gameplayAudio),
                name: () => "Gameplay"),
            new GamePhase(SuperMetroidGameState.HitDoorBlock,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchDoorCollisionAndLoading(controllerInput),
                name: () => "Door collision",
                audio: new GamePhaseAudio(DoorAudioDispatch: true, DoorSoundsDisabled: false, PauseInputEpilogue: false)),
            new GamePhase(SuperMetroidGameState.LoadingNextRoomA,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchDoorCollisionAndLoading(controllerInput),
                name: () => "Loading destination room",
                audio: new GamePhaseAudio(DoorAudioDispatch: true, DoorSoundsDisabled: false, PauseInputEpilogue: false)),
            new GamePhase(SuperMetroidGameState.LoadingNextRoomB,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchLoadingNextRoomB(controllerInput),
                name: () => "Destination room ready",
                audio: new GamePhaseAudio(DoorAudioDispatch: true, DoorSoundsDisabled: true, PauseInputEpilogue: false)),
            new GamePhase(SuperMetroidGameState.PausingDarkening,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchPausingDarkening(controllerInput, gameplayAudio),
                name: () => $"Pausing: gameplay darken ({pauseBrightness})"),
            new GamePhase(SuperMetroidGameState.Pausing,
                MainLoopRandomCall.NonGameplayHdmaThenAdvance,
                step: (controllerInput, _) => PausePhases.DispatchPausing(controllerInput),
                name: () => "Pausing: load pause assets",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            // $82:894F runs once before each pause-only dispatcher as well.
            // Fade states $0C/$12 run StepFrame and retain its own RNG call.
            new GamePhase(SuperMetroidGameState.PausedA,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => PausePhases.DispatchPausedA(controllerInput),
                name: () => $"Paused: fade in ({pauseBrightness})",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            new GamePhase(SuperMetroidGameState.PausedB,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => PausePhases.DispatchPausedB(controllerInput),
                name: () => pauseMenu?.ScreenMode == 1 ? "Paused: equipment" : "Paused: map",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            new GamePhase(SuperMetroidGameState.UnpausingA,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => PausePhases.DispatchUnpausingA(controllerInput),
                name: () => $"Unpausing: fade out ({pauseBrightness})",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            new GamePhase(SuperMetroidGameState.UnpausingB,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => PausePhases.DispatchUnpausingB(controllerInput),
                name: () => "Unpausing: restore gameplay",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            new GamePhase(SuperMetroidGameState.Unpausing,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchUnpausing(controllerInput, gameplayAudio),
                name: () => $"Unpausing: gameplay brighten ({pauseBrightness})"),
            new GamePhase(SuperMetroidGameState.DeathSequenceStart,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => DeathPhases.DispatchDeathSequenceStart(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.DeathSequenceStart)),
            new GamePhase(SuperMetroidGameState.DeathBlackOutSurroundings,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => DeathPhases.DispatchDeathBlackOutSurroundings(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.DeathBlackOutSurroundings)),
            new GamePhase(SuperMetroidGameState.DeathWaitForMusic,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathWaitForMusic(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathWaitForMusic)),
            new GamePhase(SuperMetroidGameState.DeathPreFlashing,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathPreFlashing)),
            new GamePhase(SuperMetroidGameState.DeathFlashing,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathFlashing)),
            new GamePhase(SuperMetroidGameState.DeathExplosionWhiteOut,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathExplosionWhiteOut)),
            // State $19 only fades; MainGameLoop's RNG call precedes it as any dispatch.
            new GamePhase(SuperMetroidGameState.DeathFinalBlackOut,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFinalBlackOut(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathFinalBlackOut)),
            // Game-over indexes zero and one resume inside their own NMI waits.
            new GamePhase(SuperMetroidGameState.GameOverMenu,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchGameOverMenu(controllerInput),
                name: () => nameof(SuperMetroidGameState.GameOverMenu),
                resumesNmiWait: () => gameOver?.ResumesAfterNmiWait == true),
            new GamePhase(SuperMetroidGameState.ReserveTanksAuto,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchReserveTanksAuto(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.ReserveTanksAuto)),
            new UnreachableGamePhase(SuperMetroidGameState.Unused1c),
            new UnreachableGamePhase(SuperMetroidGameState.DebugGameOverMenu),
            new GamePhase(SuperMetroidGameState.IntroCinematic,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => CinematicPhases.DispatchIntroCinematic(controllerInput),
                name: () => ActiveIntro.Phase.ToString()),
            new GamePhase(SuperMetroidGameState.SetUpNewGame,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => LoadingPhases.DispatchSetUpNewGame(controllerInput),
                name: () => loadingExistingSave ? "Loading saved game" : "Loading fresh Ceres game",
                resumesNmiWait: () => ResumesGameLoadingWait),
            new GamePhase(SuperMetroidGameState.MadeItToCeresElevator,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchMadeItToCeresElevator(controllerInput, gameplayAudio),
                name: () => ceresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator ? $"Ceres elevator departure ({ceresDeparture.HoldFramesRemaining.RawValue})" : runtime?.Room.CeresElevatorArrival is { IsComplete: false } ? "Ceres elevator arrival" : "Ceres controls unlocked"),
            new GamePhase(SuperMetroidGameState.BlackoutFromCeres,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchBlackoutFromCeres(controllerInput, gameplayAudio),
                name: () => $"Ceres blackout (brightness {ceresDeparture.Brightness})"),
            // $82:8B0E shares the intro's state-$1E/$22/$25 handler and its prologue call.
            new GamePhase(SuperMetroidGameState.CeresGoesBoom,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => CinematicPhases.DispatchCeresGoesBoom(controllerInput),
                name: () => $"Ceres destruction: {ceresDestruction?.Phase}",
                resumesNmiWait: () => ceresDestruction?.ResumesAfterNmiWait == true),
            new GamePhase(SuperMetroidGameState.TimeUp,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchTimeUp(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TimeUp)),
            // $24 fades without a gameplay call; its HDMA objects still run until it clears them.
            new GamePhase(SuperMetroidGameState.WhitingOutFromTimeUp,
                MainLoopRandomCall.NonGameplayHdmaThenAdvance,
                step: (controllerInput, _) => CinematicPhases.DispatchWhitingOutFromTimeUp(controllerInput),
                name: () => nameof(SuperMetroidGameState.WhitingOutFromTimeUp)),
            new GamePhase(SuperMetroidGameState.CeresGoesBoomWithSamus,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => CinematicPhases.DispatchCeresGoesBoomWithSamus(controllerInput),
                name: () => nameof(SuperMetroidGameState.CeresGoesBoomWithSamus),
                resumesNmiWait: () => ceresDestruction?.ResumesAfterNmiWait == true),
            new GamePhase(SuperMetroidGameState.SamusEscapesFromZebes,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchSamusEscapesFromZebes(controllerInput, gameplayAudio),
                name: () => $"Zebes escape fade-out ({endingFadeBrightness})"),
            // State $27 is dispatched by MainGameLoop after its RNG call, except while the
            // ending setup resumes inside its own NMI waits.
            new GamePhase(SuperMetroidGameState.EndingAndCredits,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (_, _) => CinematicPhases.DispatchEndingAndCredits(),
                name: () => $"Ending: {endingCredits?.Phase}",
                resumesNmiWait: () => endingCredits?.ResumesAfterNmiWait == true),
            new GamePhase(SuperMetroidGameState.TransitionToDemoA,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionToDemoA)),
            new GamePhase(SuperMetroidGameState.TransitionToDemoB,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionToDemoB)),
            new GamePhase(SuperMetroidGameState.PlayingDemo,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.PlayingDemo)),
            new GamePhase(SuperMetroidGameState.TransitionFromDemoA,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionFromDemoA)),
            new GamePhase(SuperMetroidGameState.TransitionFromDemoB,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionFromDemoB)),
        ]);

    /// <summary>Reads the current status without advancing the dispatcher.</summary>
    public FrontendFrame CurrentFrame => new(GameState, PhaseName, FrameNumber);

    /// <summary>
    /// Moves already-translated per-frame publishers into the single cartridge queue.
    /// </summary>
    /// <remarks>
    /// This is intentionally a collection seam, not sound policy. Samus, projectiles, and
    /// PLMs retain ownership of the exact library/ID/cap chosen by their ROM routines; the
    /// frontend merely performs the global QueueSfx call those producers requested.
    /// </remarks>
    internal void CollectTranslatedAudioRequests(GameplayAudioFramePublication gameplayAudio)
    {
        // Demo loading suppresses room music; native debug_disable_sounds suppresses
        // scene SFX while the title track continues. Do not let runtime publishers
        // replace it with each demo room's normal area music.
        if (runtime?.AttractDemo.IsAttractDemo == true)
            return;
        if (runtime?.Room.ActiveRoom?.State is { } roomState &&
            roomState.Pointer != lastAudioRoomStatePointer)
        {
            // $82:E071 loads data during door setup. $82:E664 selects the track
            // only after the opening scroll and this data queue have completed.
            if (GameState == SuperMetroidGameState.LoadingNextRoomB)
                audio.QueueRoomMusicData(roomState.MusicDataIndex);
            else
                audio.QueueRoomMusic(roomState.MusicDataIndex, roomState.MusicTrackIndex);
            lastAudioRoomStatePointer = roomState.Pointer;
        }
        // Enemy initialization follows the room's music-data queueing in native loading.
        if (runtime is not null)
            foreach (MusicCommand command in runtime.Enemies.Audio.ConsumeInitializationMusicDelayed8())
                audio.QueueMusicDelayed8(command);

        // The message-box selector is an NMI-only owner, not part of the gameplay
        // publication generation below. It has explicit consume semantics so its one-shot
        // request remains audible without making stale enemy/PLM lists eligible again.
        if (runtime?.MessageBoxes.ConsumeMessageBoxSelectionSoundRequest() == true)
            audio.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
        if (runtime?.MessageBox.ConsumeSavingSoundRequest() == true)
            audio.QueueSound(SoundEffectLibrary1Sounds.Saving, maximumQueued: 6);
        if (runtime?.MessageBoxes.ConsumeGunshipExitSoundRequest() == true)
            audio.QueueSound(SoundEffectLibrary3Sounds.GunshipEntrancePad, maximumQueued: 6);
        // Suit setup resumes from the message close boundary, outside normal gameplay
        // publication generations. Consume once so HDMA-only frames cannot repeat it.
        if (runtime?.SuitPickup.ConsumeTransformationSoundRequest() == true)
            audio.QueueSoundAndGetAccumulator(SoundEffectLibrary2Sounds.SuitTransformation, maximumQueued: 6,
                soundSuppressed: runtime.SuitPickup.TransformationSoundSuppressed);

        // Frontend states sometimes accept NMIs without running the state-eight owner list.
        // In particular, the door coroutine waits for all unread sound-ring entries to
        // drain. NMI identity cannot guard this handoff: re-reading the last enemy or PLM
        // publication during each wait NMI perpetually refills that ring. Consume only a
        // newly completed gameplay publication instead.
        if (runtime is null ||
            runtime.NmiHandler.CompletedGameplayAudioPublication ==
                lastAudioRuntimeGameplayPublication)
            return;
        lastAudioRuntimeGameplayPublication =
            runtime.NmiHandler.CompletedGameplayAudioPublication;

        // Bank $88 room-effect actors and bank $8D palette objects run before Samus,
        // projectiles, PLMs, and enemies in state eight. Queue their calls first so a full
        // SFX ring retains the same higher-priority request the cartridge would retain
        // when several producers fire during one frame.
        gameplayAudio.PublishPrefix(runtime);

        if (runtime.Player.Samus is { } samus)
        {
            // These state machines predate the shared SamusSoundRequest list. Consume their
            // one-shot publications here, retaining the exact native library and MaxN entry.
            if (samus.HorizontalSpeed.ConsumeEchoSoundRequest())
                audio.QueueSound(SoundEffectLibrary3Sounds.SpeedBoosterEcho, maximumQueued: 6);
            if (samus.Shinespark.ConsumeStoredShineWarningSoundRequest())
                audio.QueueSoundAndGetAccumulator(ShinesparkSounds.StoredWarning, 9,
                    soundSuppressed: samus.Shinespark.StoredShineWarningSoundSuppressed);
            if (samus.Shinespark.ConsumeChargeCancellationSoundRequest())
                audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.CancelAll, 9,
                    soundSuppressed: samus.Shinespark.ChargeCancellationSoundSuppressed);
            if (samus.Shinespark.ConsumeLaunchSoundRequest())
                audio.QueueSoundAndGetAccumulator(ShinesparkSounds.Launch, 9,
                    soundSuppressed: samus.Shinespark.LaunchSoundSuppressed);
            if (samus.Shinespark.ConsumeCrashSoundRequest())
            {
                audio.QueueSoundAndGetAccumulator(ShinesparkSounds.CrashImpact, 6,
                    soundSuppressed: samus.Shinespark.CrashSoundSuppressed);
                audio.QueueSoundAndGetAccumulator(ShinesparkSounds.CrashEcho, 6,
                    soundSuppressed: samus.Shinespark.CrashSoundSuppressed);
            }
            if (samus.CrystalFlash.ConsumeActivationSoundRequest())
                audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
            if (samus.Xray.ConsumeActivationSoundRequest())
                audio.QueueSound(SoundEffectLibrary1Sounds.XrayStart, maximumQueued: 6);
            if (samus.Xray.ConsumeDeactivationSoundRequest())
                audio.QueueSound(SoundEffectLibrary1Sounds.XrayEnd, maximumQueued: 6);
            if (samus.DeathSequence.ConsumeSpinJumpSoundRequest())
                audio.QueueSound(SoundEffectLibrary1Sounds.StopSpinJump, maximumQueued: 6);
        }

        if (runtime.Projectiles.LastFrameResult.QueuedSoundEffect is { } projectileSound)
        {
            audio.QueueSoundAndGetAccumulator(
                projectileSound,
                runtime.Projectiles.LastFrameResult.QueuedSoundMaximum,
                soundSuppressed: runtime.Projectiles.LastFrameResult.QueuedSoundSuppressed);
        }

        foreach (SamusSoundRequest request in
            runtime.Projectiles.LastFrameResult.AdditionalSoundRequests ?? [])
        {
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }

        foreach (SamusSoundRequest request in runtime.Projectiles.ImpactAudio.ImpactSoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);

        foreach (SamusSoundRequest request in
            runtime.BombProjectiles.LastFrameResult.SoundRequests ?? [])
        {
            audio.QueueSoundAndGetAccumulator(
                request.SoundEffect,
                request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }

        foreach (PlmSoundRequest request in runtime.Plms.Sounds.SoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (PlmMusicRequest request in runtime.Plms.BombTorizoHand.MusicRequests)
            audio.QueueMusicDelayed(request.Command, request.Delay);
        if (runtime.Plms.CollectiblePlms.ConsumeCollectibleFanfareRequest())
            audio.QueuePermanentItemFanfare();

        foreach (EnemySoundRequest request in runtime.Enemies.Audio.SoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (EnemyMusicRequest request in runtime.Enemies.Audio.MusicRequests)
            audio.QueueMusicDelayed(request.Command, request.Delay);
    }

    /// <summary>Human-readable active subphase for diagnostics without changing native state.</summary>
    private string PhaseName => Phases.Of(GameState).Name;

    /// <summary>
    /// Ports the state-publication half of <c>HandleSamusOutOfHealthAndGameTile</c>. Runtime
    /// state eight has already completed its room-main and clock work when this is called.
    /// </summary>
    /// <summary>
    /// Gunship function $A2:AD0E writes game state $26 only after the top hull has crossed
    /// Y=$0100, zeroing ScreenFadeDelay/Counter. It does not freeze time: state $26 keeps
    /// running full gameplay, so Samus and the RNG advance behind the fade. State eight has
    /// already completed on this publication frame; the following call owns the fade.
    /// </summary>
    internal void BeginZebesEscapeFade()
    {
        endingFadeBrightness = 15;
        endingFadeCounter = 0;
        GameState = SuperMetroidGameState.SamusEscapesFromZebes;
    }

    /// <summary>
    /// Republishes the retained immutable display under a fresh host identity without
    /// advancing simulation, audio, or draw-owned state. Used after restoring a captured
    /// debugger state and when attaching a presenter to an already paused game.
    /// </summary>
    /// <remarks>
    /// Returns null when this game has only produced legacy pixels. The host must handle
    /// that explicitly; this method never fabricates a GPU packet from a CPU raster or
    /// calls a scene draw routine to reconstruct an earlier display.
    /// </remarks>
    public RenderFrameSnapshot GetRetainedDisplay(long hostSequence, long hostGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostSequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostGeneration);
        return display.Retained(new(hostSequence, hostGeneration, FrameNumber));
    }

    /// <summary>
    /// Advances one frame, capturing extracted scenes instead of rasterizing them.
    /// Unconverted scenes are explicitly reported as legacy pixel output. The host
    /// supplies sequence/generation outside the saved game graph, so restoring game
    /// state must not rewind presentation identity. This is a staged migration API.
    /// </summary>
    public CapturedFrontendFrame StepCaptured(SnesButton controllerInput, long hostSequence, long hostGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostSequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostGeneration);
        var identity = new RenderFrameIdentity(hostSequence, hostGeneration, unchecked((ushort)(FrameNumber + 1)));
        display.BeginCapture(identity);
        try
        {
            FrontendFrame frame = Step(controllerInput);
            return new(frame, display.CompleteCapture(identity));
        }
        finally { display.EndCapture(); }
    }

    /// <summary>Writes frontend brightness and retains its last level for pause fades.</summary>
    internal void ApplyDisplayBrightness(byte brightness)
    {
        // Every published level is a write of the brightness register $51; pause entry
        // fades from the last one.
        pauseBrightness = brightness;
        display.ApplyBrightness(brightness);
    }

    /// <summary>Starts a pause fade from <paramref name="initialBrightness"/> with both fade words reloaded.</summary>
    internal void BeginPauseFade(byte initialBrightness)
    {
        pauseBrightness = initialBrightness;
        screenFade.Reload();
    }

    /// <summary>Allocates and binds the gameplay owner without selecting a room.</summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(runtime))]
    internal void CreateGameplayRuntime(bool forAttractDemo = false)
    {
        ushort incomingRandom = FrontendRandomOwner.RandomNumber;
        if (runtime is not null)
            menuNmi.RetainFrom(runtime);
        runtime = new SuperMetroidRuntime(bus, ConfiguredOptions.RuntimeOverrides(forAttractDemo), initialPaletteArt: Artwork.GameplayBasePalettes);
        // Runtime allocation is a managed ownership change, not Vector_RESET.
        // Publish before room initialization so random-consuming enemies see it too.
        runtime.System.SetRandomNumber(incomingRandom);
        runtime.System.MainGameLoopCarry = bootMainLoopCarry;
        menuNmi.PublishTo(runtime);
        runtime.Artwork.MapPresentation = Artwork.MapPresentation;
        runtime.Artwork.StandardObjectArt = Artwork.StandardObjectArt;
        runtime.Artwork.SamusBodyArt = Artwork.SamusBodyArt;
        runtime.Artwork.RoomCharacterArt = Artwork.RoomCharacterArt;
        runtime.Artwork.RoomPaletteArt = Artwork.RoomPaletteArt;
        runtime.Artwork.RoomMetatileArt = Artwork.RoomMetatileArt;
        runtime.Artwork.RoomVisualLayouts = Artwork.RoomVisualLayouts;
        runtime.Artwork.RoomLevelStreams = Artwork.RoomLevelStreams;
        runtime.ShotBlockVisuals.RoomPlmShotBlockVisuals = Artwork.RoomPlmShotBlockVisuals;
        runtime.ShotBlockVisuals.RoomPlmGrappleBlockVisuals = Artwork.RoomPlmGrappleBlockVisuals;
        runtime.ShotBlockVisuals.RoomPlmStationVisuals = Artwork.RoomPlmStationVisuals;
        runtime.ShotBlockVisuals.RoomPlmBlueDoorVisuals = Artwork.RoomPlmBlueDoorVisuals;
        runtime.ShotBlockVisuals.RoomPlmColoredDoorVisuals = Artwork.RoomPlmColoredDoorVisuals;
        runtime.ShotBlockVisuals.RoomPlmGreyDoorVisuals = Artwork.RoomPlmGreyDoorVisuals;
        runtime.ShotBlockVisuals.RoomPlmEyeDoorVisuals = Artwork.RoomPlmEyeDoorVisuals;
        runtime.ShotBlockVisuals.RoomPlmMotherBrainGlassVisuals = Artwork.RoomPlmMotherBrainGlassVisuals;
        runtime.ShotBlockVisuals.RoomPlmNoobTubeVisuals = Artwork.RoomPlmNoobTubeVisuals;
        runtime.ShotBlockVisuals.RoomPlmDownwardGateVisuals = Artwork.RoomPlmDownwardGateVisuals;
        runtime.ShotBlockVisuals.RoomPlmElevatorPlatformVisuals = Artwork.RoomPlmElevatorPlatformVisuals;
        runtime.ShotBlockVisuals.RoomPlmEscapeGateVisuals = Artwork.RoomPlmEscapeGateVisuals;
        runtime.ShotBlockVisuals.RoomPlmBombTorizoHandVisuals = Artwork.RoomPlmBombTorizoHandVisuals;
        runtime.ShotBlockVisuals.RoomPlmDraygonCannonVisuals = Artwork.RoomPlmDraygonCannonVisuals;
        runtime.ShotBlockVisuals.RoomPlmChozoStatueVisuals = Artwork.RoomPlmChozoStatueVisuals;
        runtime.ShotBlockVisuals.RoomPlmLinkedRestoreVisuals = Artwork.RoomPlmLinkedRestoreVisuals;
        runtime.ShotBlockVisuals.RoomPlmTourianAccessVisuals = Artwork.RoomPlmTourianAccessVisuals;
        runtime.ShotBlockVisuals.RoomPlmSpeedBoosterVisuals = Artwork.RoomPlmSpeedBoosterVisuals;
        runtime.ShotBlockVisuals.RoomPlmMaridiaElevatubeVisuals = Artwork.RoomPlmMaridiaElevatubeVisuals;
        runtime.ShotBlockVisuals.RoomPlmSporeSpawnCeilingVisuals = Artwork.RoomPlmSporeSpawnCeilingVisuals;
        runtime.ShotBlockVisuals.RoomPlmSamusEaterVisuals = Artwork.RoomPlmSamusEaterVisuals;
        runtime.ShotBlockVisuals.RoomPlmBotwoonWallVisuals = Artwork.RoomPlmBotwoonWallVisuals;
        runtime.ShotBlockVisuals.RoomPlmKraidVisuals = Artwork.RoomPlmKraidVisuals;
        runtime.ShotBlockVisuals.RoomPlmCrocomireVisuals = Artwork.RoomPlmCrocomireVisuals;
        runtime.ShotBlockVisuals.RoomPlmMotherBrainFakeDeathVisuals = Artwork.RoomPlmMotherBrainFakeDeathVisuals;
        runtime.ShotBlockVisuals.RoomPlmCollectibleVisuals = Artwork.RoomPlmCollectibleVisuals;
        runtime.ShotBlockVisuals.RoomPlmDynamicCollectibleArt = Artwork.RoomPlmDynamicCollectibleArt;
        runtime.Xray.XrayRevealVisuals = Artwork.XrayRevealVisuals;
        runtime.Artwork.RoomBackgroundTilemapArt = Artwork.RoomBackgroundTilemapArt;
        runtime.Artwork.RoomSkyTilemapArt = Artwork.RoomSkyTilemapArt;
        runtime.Artwork.ProjectileCompositions = Artwork.ProjectileCompositions;
        runtime.Artwork.ProjectileFrameBindings = Artwork.ProjectileFrameBindings;
        runtime.Artwork.TrailArtwork = Artwork.TrailArtwork;
        runtime.Artwork.ChargeFlarePlacement = Artwork.ChargeFlarePlacement;
        runtime.Artwork.ChargeFlareCompositions = Artwork.ChargeFlareCompositions;
        runtime.Artwork.GrappleArtwork = Artwork.GrappleArtwork;
        runtime.Artwork.BeamArtwork = Artwork.BeamArtwork;
        runtime.Enemies.TileArtwork = Artwork.EnemyTileArtwork;
        // Attract scripts retain the default controls and options of their private runtime.
        if (forAttractDemo)
            return;
        runtime.JapaneseText = options?.JapaneseText ?? false;
        runtime.Options.ControllerBindings = options?.ControllerBindings ?? ControllerBindings.Default;
        runtime.Options.MoonwalkEnabled = options?.MoonwalkEnabled ?? false;
        runtime.Options.IconCancelEnabled = options?.IconCancelEnabled ?? false;
    }

    /// <summary>Applies live menu controller and gameplay choices to a newly created runtime.</summary>
    internal void ApplySelectedGameOptions()
    {
        // Live edits in options take precedence over the snapshot loaded at file select.
        if (options is null) return;
        GameplayRuntime.Options.ControllerBindings = options.ControllerBindings;
        GameplayRuntime.Options.MoonwalkEnabled = options.MoonwalkEnabled;
        GameplayRuntime.Options.IconCancelEnabled = options.IconCancelEnabled;
    }

    /// <summary>
    /// Shared saved-game HUD construction for playable and cinematic checkpoints.
    /// The saved inventory supplies counters/icons; native initialization clears
    /// the persisted HUD selection before the first live gameplay update.
    /// </summary>
    internal void InitializeSelectedGameHud(SuperMetroidSaveSlot slot) =>
        GameplayRuntime.HudSetup.InitializeHud(new HudSnapshot(
            slot.Health,
            slot.MaxHealth,
            slot.Missiles,
            slot.MaxMissiles,
            slot.SuperMissiles,
            slot.MaxSuperMissiles,
            slot.PowerBombs,
            slot.MaxPowerBombs,
            slot.EquippedItems,
            // $91:E00D clears selection before $80:9A79 builds the visible HUD.
            SelectedItem: 0,
            slot.ReserveEnergy,
            slot.ReserveMode));

    /// <summary>Builds fresh Ceres or saved-game runtime state, including the selected HUD and room.</summary>
    internal bool SetupSelectedGame()
    {
        CreateGameplayRuntime();

        if (spacetimeIntroRestartSlot is { } restartSlot)
        {
            // The cart loaded the old inventory before $82:EEB4 selected the intro. Ceres
            // setup then starts from fresh progression while retaining equipment and other
            // resources. The flashback specifically replaces and then clears Missiles.
            runtime.HudSetup.InitializeHud(new HudSnapshot(
                Health: restartSlot.Health,
                MaxHealth: restartSlot.MaxHealth,
                Missiles: 0,
                MaxMissiles: 0,
                SuperMissiles: restartSlot.SuperMissiles,
                MaxSuperMissiles: restartSlot.MaxSuperMissiles,
                PowerBombs: restartSlot.PowerBombs,
                MaxPowerBombs: restartSlot.MaxPowerBombs,
                EquippedItems: restartSlot.EquippedItems,
                SelectedItem: 0,
                ReserveHealth: restartSlot.ReserveEnergy,
                ReserveMode: restartSlot.ReserveMode));
            runtime.NmiHandler.PublishForcedBlankTransfers();
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            SamusState restartedSamus = runtime.Player.Samus
                ?? throw new InvalidOperationException(
                    "SpaceTime restart did not create the Ceres Samus state.");
            RestoreSpacetimeRestartInventory(restartedSamus, restartSlot);
            runtime.Tester.ApplyTesterInventory(restoredInventory: true);
            restartedSamus.SelectedHudItem = 0;
            restartedSamus.AutoCancelHudItemIndex = 0;
            AutomaticCheckpointSaver.SaveCeresArrival(bus, runtime, selectedSaveSlot);
            SaveRamChanged?.Invoke();
            spacetimeIntroRestartSlot = null;
            loadingExistingSave = false;
            return true;
        }

        if (loadingExistingSave)
        {
            SuperMetroidSaveSlot slot = saveRam.ReadSlot(selectedSaveSlot)
                ?? throw new InvalidDataException(
                    $"Selected save slot {selectedSaveSlot} became invalid during startup.");
            InitializeSelectedGameHud(slot);
            runtime.GameTime.Load(slot.GameTimeFrames, slot.GameTimeSeconds,
                slot.GameTimeMinutes, slot.GameTimeHours);
            runtime.NmiHandler.PublishForcedBlankTransfers();

            // Preserve the already-translated Ceres elevator entrance for its checkpoint.
            // Every other station uses the general cartridge-backed loader below.
            if (slot.StartupRoute == SaveLoadingGameState.CeresElevatorArrival)
            {
                // Room selection and actors must see the restored mirror, not a
                // fresh runtime followed by an after-the-fact progression overwrite.
                runtime.RoomLoader.RestoreSavedPlayerState(slot, ConfiguredOptions.ResetBossesOnLoad);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                slot.ApplyTo(runtime.Player.Samus ?? throw new InvalidOperationException(
                    "Ceres initialization did not create Samus."));
                runtime.Tester.ApplyTesterInventory(restoredInventory: true);
                runtime.Player.Samus.SelectedHudItem = 0;
                runtime.Player.Samus.AutoCancelHudItemIndex = 0;
                ApplySelectedGameOptions();
                return true;
            }

            runtime.RoomLoader.InitializeSavedGame(slot, ConfiguredOptions.ResetBossesOnLoad);
            // File selection loaded the saved options before the options menu ran.
            // InitializeSavedGame also serves direct diagnostic loads, so it restores
            // those fields itself. At this frontend boundary, the player's live edits
            // take precedence over that original snapshot, just like cartridge WRAM.
            ApplySelectedGameOptions();
            CartridgeRoomState loadedState = runtime.Room.ActiveRoom?.State
                ?? throw new InvalidOperationException(
                    "Saved-game appearance started without an active room state.");
            // `$92:ED24` starts appearance track one after 14 frames and, on its fifth
            // call, schedules the room track 360 frames later. Mark this room state as
            // already handled so the generic room-change collector cannot overwrite the
            // fanfare with an immediate track request.
            // Loading from file select still has the title's SPC song data installed.
            // Preserve the appearance track sequence, but upload the room's bank first
            // just as ordinary room loading does; track numbers alone are bank-local.
            audio.QueueRoomMusicData(loadedState.MusicDataIndex);
            audio.QueueMusicDelayed(MusicCommand.SelectTrack(1), MusicCommandDelay.FromDelayedYArgument(14));
            audio.QueueMusicDelayed(
                MusicCommand.SelectTrackOrStop(loadedState.MusicTrackIndex),
                MusicCommandDelay.FromDelayedYArgument(360 + 5));
            lastAudioRoomStatePointer = loadedState.Pointer;
            return false;
        }

        // Native new-game loading first publishes the standard HUD/OBJ transfer, then
        // loads the area-six station-zero room and finally runs SamusCode_08. This is the
        // same cartridge-backed sequence used by the room diagnostic; there is no host
        // terrain image, hand-placed Samus, or alternate playable-only initialization.
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.NmiHandler.PublishForcedBlankTransfers();
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // CinematicFunction_Intro_Func73 at `$8B:C100` publishes area six/station zero
        // and calls the ordinary `$81:8000` saver immediately before state $1F. Both the
        // full cinematic and host-configured skip converge here, so neither path can omit
        // the automatic checkpoint.
        AutomaticCheckpointSaver.SaveCeresArrival(bus, runtime, selectedSaveSlot);
        SaveRamChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Applies the inventory that remains live across a SpaceTime-triggered intro. Native
    /// $8B:A395 temporarily replaces both missile words for the Mother Brain flashback and
    /// $8B:B76C clears them afterward; no equivalent stores touch Super Missiles or Power
    /// Bombs. The frontend cinematic uses isolated actor state, so its gameplay consequence
    /// is made explicit at the shared Ceres handoff.
    /// </summary>
    internal static void RestoreSpacetimeRestartInventory(
        SamusState samus,
        SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(slot);
        slot.ApplyTo(samus);
        samus.Missiles = 0;
        samus.MaxMissiles = 0;
    }

    /// <summary>
    /// Enters cartridge game state seven with a fully dark screen and the native two-frame
    /// brightness cadence. A fresh Ceres game must execute complete gameplay/OAM frames in
    /// this state before state $20 owns the elevator arrival; otherwise the level-data pad
    /// is briefly visible before its bank-$86 concealer reaches displayed OAM.
    /// </summary>
    internal void BeginGameplayFadeIn(bool leadsToCeresArrival)
    {
        postCeresFadeBrightness = 0;
        postCeresFadeCounter = 1;
        gameplayFadeLeadsToCeresArrival = leadsToCeresArrival;
        GameState = SuperMetroidGameState.MainGameplayFadeIn;
    }

    /// <summary>Persists the automatic Landing Site checkpoint after the gunship lands.</summary>
    internal void HandleGunshipLandingSave()
    {
        if (runtime is null ||
            !AutomaticCheckpointSaver.TrySaveGunshipLanding(bus, runtime, selectedSaveSlot))
            return;
        SaveRamChanged?.Invoke();
    }

    /// <summary>Current attract-demo set selected by the title sequence.</summary>
    internal int demoSet;
    /// <summary>Next scene index within the current attract-demo set.</summary>
    internal int demoScene;
    /// <summary>Remaining scripted input frames in the active demo scene.</summary>
    internal int demoFramesRemaining;
    /// <summary>Remaining still-frame hold after a demo scene's scripted input.</summary>
    internal int demoHoldFramesRemaining;
    /// <summary>Remaining blank enemy-transfer waits before the demo becomes visible.</summary>
    internal int demoLoadFramesRemaining = -1;
    /// <summary>Whether live controller input canceled the current demo.</summary>
    internal bool demoCancelled;
    /// <summary>Whether another scene follows before returning to the title.</summary>
    internal bool demoHasNextScene;

    /// <summary>Unlocks the fourth demo set only after a valid completed-game save exists.</summary>
    internal int AvailableDemoSetCount() => bootDemoSetCount;

    /// <summary>
    /// Copies diagnostic state without reading cartridge data or invoking gameplay.
    /// Hosts capture this before a frame and again on failure to expose partial mutations.
    /// </summary>
    public string CaptureGameplayFailureContext()
    {
        string location = $"frame=${FrameNumber:X4}; state=${(ushort)GameState:X2} {GameState}; " +
            $"room=$8F:{GameplayActiveRoomPointer:X4}; roomState=$8F:{GameplayActiveRoomStatePointer:X4}; door=$83:{GameplayActiveDoorPointer:X4}";
        if (runtime?.Player.Samus is not { } samus)
            return location + "; Samus=not initialized";
        return location + $"; pose=${(int)samus.Pose:X2}; animationFrame={samus.AnimationFrame}; animationTimer={samus.AnimationFrameTimer.RawValue}; " +
            $"animationList=${samus.AnimationDelayListAddress:X6}; position={samus.Position.X.Whole},{samus.Position.Y.Whole}; " +
            $"input=${runtime.Controller1.Current:X}; newlyPressed=${runtime.Controller1.NewlyPressed:X}; inputLocked={samus.InputLocked}; " +
            $"drainedPhase={samus.Drained.Phase}; drainedHandler={samus.Drained.GetUpHandler}; " +
            $"escapeTimer={runtime.EscapeTimer.State}; fx={samus.LiquidPhysics.FxType}; medium={samus.LiquidPhysics.LiquidPhysicsType}; " +
            $"waterY=${samus.LiquidPhysics.FxYPosition:X4}; acidY=${samus.LiquidPhysics.LavaAcidYPosition:X4}";
    }

    // NMI continuations still owed by the current $82:8000 loading dispatch, and the
    // dispatcher state it selects after its final wait returns.
    /// <summary>Accepted blank NMI waits remaining in state-five game loading.</summary>
    internal int gameLoadingWaitsRemaining;
    /// <summary>Destination to enter when game-loading waits finish.</summary>
    internal GameLoadingCompletion gameLoadingCompletion;
    /// <summary>
    /// The loading state state six branches on (<c>SRAMMirror_LoadingGameState</c>): a save continued
    /// from the file-select map, or the Ceres destruction's Zebes landing.
    /// </summary>
    internal SaveLoadingGameState stateSixLoadingRoute;

    /// <summary>
    /// True when the next update is an NMI continuation inside the loading dispatch rather
    /// than a new main-loop dispatch, so the main loop's RNG call is absent.
    /// </summary>
    internal bool ResumesGameLoadingWait => gameLoadingWaitsRemaining > 0;

    // State $09's door function before state $0A. Every door hit installs $82:E17D; a
    // downward elevator replaces it with the $82:E19F delay.
    /// <summary>Whether room loading is paused for the downward elevator entry wait.</summary>
    internal bool waitingForDownwardsElevator;
    /// <summary>Countdown before a downward elevator resumes destination loading.</summary>
    internal int downwardsElevatorDelayTimer;

    /// <summary>
    /// Rebinds host policy after an interactive debugger-state load. Cartridge state,
    /// resource totals, and the retained frame remain intact unless a tester grant is newly enabled. Exact replay must omit
    /// this override unless the recording explicitly supplies different host options.
    /// </summary>
    public void ApplyHostOptions(SuperMetroidGameOptions hostOptions)
    {
        Ensure.NotNull(hostOptions);
        runtime?.ApplyHostOptions(hostOptions.RuntimeOverrides(forAttractDemo: false));
        ConfiguredOptions = hostOptions;
    }

    /// <summary>Cartridge RNG owner before a gameplay room exists or after runtime release.</summary>
    private Bank80SystemState? menuRandom;

    /// <summary>Uses gameplay RNG when live and otherwise retains the frontend RNG word.</summary>
    internal Bank80SystemState FrontendRandomOwner =>
        runtime?.System ?? (menuRandom ??= new Bank80SystemState());

    /// <summary>Transfers RNG and NMI counters back to the frontend before dropping gameplay state.</summary>
    internal void ReleaseRuntimePreservingRandom()
    {
        (menuRandom ??= new()).SetRandomNumber(FrontendRandomOwner.RandomNumber);
        // File-map selection may release when no runtime was ever created.
        if (runtime is not null)
            menuNmi.RetainFrom(runtime);
        runtime = null;
    }

    /// <summary>
    /// True when the next update resumes a frontend dispatch after its own NMI wait instead
    /// of entering MainGameLoop. Such an update makes no main-loop RNG call.
    /// </summary>
    internal bool NextUpdateResumesNmiWait => Phases.Of(GameState).ResumesNmiWait;

    /// <summary>
    /// Preserves the bank-$82 main-loop RNG call through title/file selection and
    /// loading. These menu dispatchers return once per frame; they do not run the
    /// gameplay runtime's prologue. SRAM contains no copy of the live RNG word.
    /// </summary>
    internal void AdvanceMenuRandom()
    {
        switch (Phases.Of(GameState).RandomCall)
        {
            case MainLoopRandomCall.Reseed:
                // Only the reset vector reseeds. A continued save must keep the word
                // produced by gameplay and the game-over/file-map waiting frames.
                menuRandom = new Bank80SystemState();
                menuNmi.Reset();
                // State zero is itself dispatched by MainGameLoop after its first
                // GenerateRandomNumber call; it is not the reset vector's seed store.
                menuRandom.NextRandom();
                runtime?.System.SetRandomNumber(menuRandom.RandomNumber);
                break;
            case MainLoopRandomCall.NonGameplayHdmaThenAdvance:
                // HDMA runs before $0D disables it; lava swaps the live RNG bytes.
                GameplayRuntime.DoorTransition.AdvanceNonGameplayMainLoopRandom(hdmaObjectsEnabled: true);
                break;
            case MainLoopRandomCall.AdvanceUnlessResumingWait:
                if (!NextUpdateResumesNmiWait)
                    FrontendRandomOwner.NextRandom();
                break;
            case MainLoopRandomCall.Advance:
                FrontendRandomOwner.NextRandom();
                break;
            case MainLoopRandomCall.RuntimeOwned:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MainLoopRandomCall)} {Phases.Of(GameState).RandomCall}.");
        }
        // Gameplay retains its existing call after accepted NMI/HDMA processing.
        // Do not advance here for runtime frames or nested NMI-waiting coroutines.
    }
}
