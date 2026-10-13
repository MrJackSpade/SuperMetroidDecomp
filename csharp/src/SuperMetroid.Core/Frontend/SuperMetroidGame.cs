using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

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
    private readonly FrontendDisplay display;
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
    private TitleSequenceState? title;
    /// <summary>Save-slot selection menu state.</summary>
    private FileSelectMenuState? fileSelect;
    /// <summary>Saved-game map selection and loading screen state.</summary>
    private FileSelectMapMenuState? fileSelectMap;
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
    /// <summary>Ceres elevator departure and blackout transition state.</summary>
    private readonly CeresDepartureState ceresDeparture = new();
    /// <summary>Reserve-tank recovery state that can temporarily own the dispatcher.</summary>
    internal readonly SamusReserveAutoRecoveryState reserveRecovery = new();
    /// <summary>Door approach and destination loading state across gameplay states.</summary>
    internal readonly DoorTransitionState doorTransition = new();

    /// <summary>Current battery-backed save slot selected by the frontend.</summary>
    private int selectedSaveSlot;
    /// <summary>Distinguishes a saved-game load from fresh Ceres setup.</summary>
    internal bool loadingExistingSave;
    /// <summary>Save state retained for restart after the Space-Time intro sequence.</summary>
    private SuperMetroidSaveSlot? spacetimeIntroRestartSlot;
    /// <summary>Brightness during the fade into Landing Site or Ceres arrival.</summary>
    private byte postCeresFadeBrightness;
    /// <summary>Cadence counter for the post-Ceres fade.</summary>
    private int postCeresFadeCounter = 1;
    /// <summary>Selects Ceres arrival rather than Landing Site at the fade destination.</summary>
    private bool gameplayFadeLeadsToCeresArrival;
    /// <summary>Gameplay brightness retained through pause and unpause fades.</summary>
    internal byte pauseBrightness = PauseFadeTiming.FullyLit;
    /// <summary>Palette transition for Samus's death sequence, when active.</summary>
    private CartridgePaletteTransition? deathPaletteFade;
    /// <summary>Palette transition for the escape time-up sequence.</summary>
    private CartridgePaletteTransition? timeUpPaletteFade;
    /// <summary>Brightness during the death-state blackout.</summary>
    internal byte deathFadeBrightness = 15;
    /// <summary>Cadence counter for the death fade.</summary>
    internal int deathFadeCounter;
    /// <summary>Brightness during the ending transition after Zebes escape.</summary>
    private byte endingFadeBrightness = 15;
    /// <summary>Cadence counter for the ending fade.</summary>
    private int endingFadeCounter;
    /// <summary>Audio commands published by the most recent frontend frame.</summary>
    private IReadOnlyList<CartridgeAudioCommand> lastAudioCommands =
        Array.Empty<CartridgeAudioCommand>();
    /// <summary>Runtime publication number last copied into the frontend audio queue.</summary>
    private ulong? lastAudioRuntimeGameplayPublication;
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
        saveRam = new SuperMetroidSaveRam(bus, Artwork.MapPresentation);
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

    /// <summary>
    /// Binds the extracted audio catalog at startup and after debugger-state restoration, so
    /// every following update also synthesizes host PCM (<see cref="CopyAudioFrame"/>).
    /// </summary>
    /// <remarks>
    /// Binding affects presentation only. Each update advances the session's SPC port model and
    /// its sound-effect acknowledgements whether or not audio is bound, so gameplay is identical
    /// with audio enabled, disabled, or without an output device.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="assets"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The session already advanced without synthesis and has no player to continue.</exception>
    public void BindAudio(ExtractedAudioAssetCatalog assets) => audioSession.BindSynthesis(assets);

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
    public FrontendFrame Step(ushort controllerInput)
    {
        // Derive the door-owned native disable flag before any producer can queue
        // audio. This also restores its meaning for legacy snapshots without adding
        // another serialized flag that could disagree with the coroutine phase.
        audio.DoorTransitionSoundsDisabled = GameState == SuperMetroidGameState.LoadingNextRoomB;
        bool pauseInputEpilogue = GameState is SuperMetroidGameState.Pausing or
            SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB or
            SuperMetroidGameState.UnpausingA or SuperMetroidGameState.UnpausingB;
        bool messageWasActive = runtime?.MessageBox.IsActive == true;
        bool doorAudioDispatch = GameState is SuperMetroidGameState.HitDoorBlock or
            SuperMetroidGameState.LoadingNextRoomA or SuperMetroidGameState.LoadingNextRoomB;
        DoorTransitionPhase startingDoorPhase = doorTransition.Phase;
        // An update that resumes a dispatch after its NMI wait never reaches the main loop's
        // music prologue ($88:84B9) or its sound handler ($82:89EF).
        bool resumesNmiWait = NextUpdateResumesNmiWait;
        // MainGameLoop runs the music queue ($82:894B) before dispatching any game state, so
        // a dispatch that waits on HasQueuedMusic sees this update's queue. Sound handlers
        // run after its coroutine returns. An accepted IRQ/NMI alone runs neither handler,
        // and a message box's lag frames run only the box's own calls (below).
        bool prologueMusicDispatch = !resumesNmiWait && !messageWasActive &&
            (!doorAudioDispatch || GameState != SuperMetroidGameState.LoadingNextRoomB ||
             startingDoorPhase is not (DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.FinishDoorLoading));
        IReadOnlyList<CartridgeAudioCommand> prologueMusicCommands = prologueMusicDispatch
            ? audio.AdvanceMusicDispatch() : Array.Empty<CartridgeAudioCommand>();
        // SendAPUData stalls a door dispatch while door IRQs keep NMIs accepted; nothing
        // between this update's own NMI and the upload reads the counters.
        if (doorAudioDispatch && prologueMusicCommands.Any(command => command.Kind == CartridgeAudioCommandKind.Upload))
            runtime!.NmiHandler.AcceptHardwareWaitNmis(DoorMusicUploadNmis.AcceptedNmisDuringUpload(audio.MusicDataIndex));
        var gameplayAudio = new GameplayAudioFramePublication(audio);
        FrameNumber++;
        if (runtime is null)
            menuNmi.Advance();
        AdvanceMenuRandom();
        if (runtime is not null)
            runtime.DispatchGameState = (ushort)GameState;
        switch (GameState)
        {
            case SuperMetroidGameState.Reset:
                demoSet = 0;
                demoScene = 0;
                demoLoadFramesRemaining = -1;
                // `Vector_RESET_Async` ultimately stores state one and initializes
                // `cinematic_function` to `CinematicFunctionOpening` at $8B:9B68.
                audio.Reset();
                // CommonBootSection clears bank $7E on a soft reset, discarding every gameplay
                // owner. The reseeded RNG and NMI counters already live in the menu owners.
                runtime = null;
                title = new TitleSequenceState(
                    bus,
                    audio,
                    Artwork.MapPresentation?.TitleGradient,
                    Artwork.MapPresentation?.TitlePalette,
                    Artwork.MapPresentation?.TitleGraphics);
                lastAudioRuntimeGameplayPublication = null;
                lastAudioRoomStatePointer = null;
                fileSelect = null;
                fileSelectMap = null;
                gameOver = null;
                spacetimeIntroRestartSlot = null;
                GameState = SuperMetroidGameState.OpeningCinematic;
                display.PublishMenu(title);
                break;

            case SuperMetroidGameState.OpeningCinematic:
                title!.Step(controllerInput);
                display.PublishMenu(title);
                if (title.FileSelectRequested)
                {
                    // `$8B:9F52` sets game_state=4 only after the slow fade reaches black.
                    fileSelect = new FileSelectMenuState(bus, audio, Artwork.MapPresentation);
                    GameState = SuperMetroidGameState.FileSelectMenus;
                    display.PublishMenu(fileSelect);
                }
                else if (title.DemoRequested)
                {
                    demoScene = 0;
                    demoLoadFramesRemaining = -1;
                    GameState = SuperMetroidGameState.TransitionToDemoA;
                    display.PublishBlack();
                }
                break;

            case SuperMetroidGameState.FileSelectMenus:
                fileSelect!.Step(controllerInput);
                if (fileSelect.SaveRamChangedThisFrame)
                    SaveRamChanged?.Invoke();
                display.PublishMenu(fileSelect);
                if (fileSelect.TitleRequested)
                {
                    // The assembly calls SoftReset from menu index 33. Re-entering state
                    // zero makes all frontend-owned state disposable and deterministic.
                    GameState = SuperMetroidGameState.Reset;
                }
                else if (fileSelect.NewGameRequested)
                {
                    selectedSaveSlot = fileSelect.SelectedSaveSlot;
                    loadingExistingSave = fileSelect.SelectedSlotContainsSave;
                    saveRam.SelectSlot(selectedSaveSlot);
                    SaveRamChanged?.Invoke();
                    // Existing slots restore the same seven controller words and two
                    // special-settings words that SaveToSram copied from live WRAM. A new
                    // slot begins with NewSaveFile's literal defaults.
                    SuperMetroidSaveSlot? optionSlot = loadingExistingSave
                        ? saveRam.ReadSlot(selectedSaveSlot)
                        : null;
                    options = new GameOptionsMenuState(
                        bus,
                        audio,
                        optionSlot?.ControllerBindings,
                        optionSlot?.IconCancelEnabled ?? false,
                        optionSlot?.MoonwalkEnabled ?? false,
                        japaneseText: optionSlot?.JapaneseText ?? false,
                        mapPresentation: Artwork.MapPresentation);
                    GameState = SuperMetroidGameState.GameOptionsMenu;
                    display.PublishMenu(options);
                }
                break;

            case SuperMetroidGameState.GameOptionsMenu:
                options!.Step(controllerInput);
                display.PublishMenu(options);
                if (options.FileSelectRequested)
                {
                    fileSelect = new FileSelectMenuState(bus, audio, Artwork.MapPresentation);
                    GameState = SuperMetroidGameState.FileSelectMenus;
                    display.PublishMenu(fileSelect);
                }
                else if (options.IntroRequested)
                {
                    if (loadingExistingSave)
                    {
                        SuperMetroidSaveSlot selectedSlot = saveRam.ReadSlot(selectedSaveSlot)
                            ?? throw new InvalidDataException(
                                "The selected save became invalid before startup dispatch.");
                        if (selectedSlot.StartupRoute ==
                            SaveLoadingGameState.OpeningCinematic)
                        {
                            // $82:EEB4 dispatches the zero word written by SpaceTime Beam
                            // to the intro while the already-loaded Samus inventory remains
                            // live. Retain that inventory for Ceres setup, but do not restore
                            // the corrupted progression planes into the fresh run.
                            spacetimeIntroRestartSlot = selectedSlot;
                            if (ConfiguredOptions.SkipOpeningCinematic)
                            {
                                intro = null;
                                GameState = SuperMetroidGameState.SetUpNewGame;
                                display.PublishBlack();
                            }
                            else
                            {
                                intro = CreateIntroCinematic(options.JapaneseText);
                                GameState = SuperMetroidGameState.IntroCinematic;
                                display.PublishIntro(intro);
                            }
                        }
                        else if (selectedSlot.StartupRoute == SaveLoadingGameState.CeresDestruction)
                        {
                            StartSavedCeresDestruction(selectedSlot);
                        }
                        else if (selectedSlot.StartupRoute == SaveLoadingGameState.CeresElevatorArrival)
                        {
                            // The saved $1F word selects loading directly, without a map.
                            GameState = SuperMetroidGameState.SetUpNewGame;
                            display.PublishBlack();
                        }
                        else
                        {
                            fileSelectMap = null;
                            GameState = SuperMetroidGameState.FileSelectMap;
                            display.PublishBlack();
                        }
                    }
                    else if (ConfiguredOptions.SkipOpeningCinematic)
                    {
                        // This is the same dispatcher boundary reached by `$8B:C100` after
                        // the SPACE COLONY fade. Do not fake Start presses or build a host-
                        // authored Ceres room: the following frame will execute the ordinary
                        // new-game loader, elevator arrival, and movement-unlock sequence.
                        intro = null;
                        GameState = SuperMetroidGameState.SetUpNewGame;
                        display.PublishBlack();
                    }
                    else
                    {
                        intro = CreateIntroCinematic(options.JapaneseText);
                        GameState = SuperMetroidGameState.IntroCinematic;
                        display.PublishIntro(intro);
                    }
                }
                break;

            case SuperMetroidGameState.IntroCinematic:
                intro!.Step(controllerInput);
                display.PublishIntro(intro);
                if (intro.CeresFlightFinished)
                {
                    // `$8B:C100` writes state $1F, area six, and load-station zero only
                    // after the SPACE COLONY fade has reached forced blank. Preserve the
                    // intervening dispatcher state instead of constructing a debug room
                    // directly from cinematic code.
                    GameState = SuperMetroidGameState.SetUpNewGame;
                    display.PublishBlack();
                }
                break;

            case SuperMetroidGameState.SetUpNewGame:
                if (ResumesGameLoadingWait)
                {
                    StepGameLoadingWait(controllerInput);
                    break;
                }
                // $82:8000 performs the whole load in one dispatch, then waits for NMIs.
                bool usesCeresArrival = SetupSelectedGame();
                BeginGameLoadingWaits(GameLoadingDefinitions.OrdinaryLoadWaits, usesCeresArrival
                    ? GameLoadingCompletion.CeresArrivalFadeIn
                    : GameLoadingCompletion.MainGameplay);
                break;

            case SuperMetroidGameState.MadeItToCeresElevator:
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AllowCeresElevatorDeparture = false }, Audio = gameplayAudio.GameplayFrameAudio(runtime) });
                display.PublishGameplay(runtime);
                // State $20 is only the departure hold. The arrival descent runs in
                // ordinary state eight while the bank-$86 elevator objects lock Samus.
                if (ceresDeparture.Phase != CeresDeparturePhase.HoldingOnElevator)
                    throw new InvalidOperationException("State $20 requires the Ceres departure hold.");
                if (ceresDeparture.StepHoldAfterGameplay())
                    GameState = SuperMetroidGameState.BlackoutFromCeres;
                break;

            case SuperMetroidGameState.MainGameplay:
                RunMainGameplayState(controllerInput, gameplayAudio);
                break;

            case SuperMetroidGameState.ReserveTanksAuto:
                SamusReserveAutoRecoveryStep reserveStep = default;
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(runtime), AfterAcceptedNmi = () =>
                    {
                        SamusState samus = runtime.Player.Samus
                            ?? throw new InvalidOperationException(
                                "Reserve recovery requires a live Samus state.");
                        reserveStep = reserveRecovery.StepAfterNmi(
                            samus,
                            runtime.Nmi.NmiFrameCounter);
                        if (reserveStep.RefillSoundRequested)
                            audio.QueueSoundAndGetAccumulator(SoundEffectLibrary3Sounds.ReserveRefill, 3,
                                soundSuppressed: runtime.PowerBombExplosionSuppressesSounds);
                        // $82:DC18-$DC24 clears the freeze and restores state eight
                        // BEFORE calling gameplay. Unfreezing after StepFrame lets
                        // Samus resume while projectiles/enemies remain a frame behind.
                        if (reserveStep.Completed)
                        {
                            runtime.Freeze.GameplayTimeFrozen = false;
                            GameState = SuperMetroidGameState.MainGameplay;
                        }
                    } });
                display.PublishGameplay(runtime);
                // $82:DC2B invokes the external low-health check even while the
                // locked Samus beta handler omits it. Publish prior gameplay sounds
                // first; their native queue occupancy affects this final Max6 call.
                CollectTranslatedAudioRequests(gameplayAudio);
                runtime.Player.Samus!.HealthWarning.Update(runtime.Player.Samus.Health, audio,
                    soundSuppressed: runtime.PowerBombExplosionSuppressesSounds);
                break;

            case SuperMetroidGameState.DeathSequenceStart:
                // State $13 deliberately completes one final state-eight pass under the
                // global freeze word, then snapshots the visible palette and makes every
                // target row black except Samus's sixteen-color suit row.
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(runtime) });
                display.PublishGameplay(runtime);
                PrepareDeathPaletteFade();
                SamusState dyingSamus = runtime.Player.Samus
                    ?? throw new InvalidOperationException("Death sequence lost its Samus owner.");
                dyingSamus.SelectedHudItem = 0;
                dyingSamus.AutoCancelHudItemIndex = 0;
                dyingSamus.InvincibilityTimer = NativeCountdown.Start(0);
                dyingSamus.KnockbackTimer = NativeCountdown.Start(0);
                dyingSamus.KnockbackActive = false;
                GameState = SuperMetroidGameState.DeathBlackOutSurroundings;
                break;

            case SuperMetroidGameState.DeathBlackOutSurroundings:
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(runtime) });
                bool paletteBlackoutComplete = StepDeathPaletteFade();
                display.PublishGameplay(runtime);
                if (paletteBlackoutComplete)
                {
                    // `$82:DCE0` cancels all three SFX libraries, then installs the death
                    // music data and track through the ordinary delayed music queue.
                    audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
                    audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
                    audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
                    audio.QueueMusicDelayed8(MusicCommand.Stop);
                    audio.QueueMusicDelayed8(MusicCommand.LoadData(0x39));
                    audio.QueueMusicDelayed(MusicCommand.SelectTrack(5), MusicCommandDelay.FromDelayedYArgument(0x000e));
                    GameState = SuperMetroidGameState.DeathWaitForMusic;
                }
                break;

            case SuperMetroidGameState.DeathWaitForMusic:
                runtime!.DeathSequence.DrawFatalSamusFrame(controllerInput);
                display.PublishGameplay(runtime);
                if (!audio.HasQueuedMusic)
                {
                    runtime.DeathSequence.BeginDeathSequenceAfterMusicWait();
                    GameState = SuperMetroidGameState.DeathPreFlashing;
                }
                break;

            case SuperMetroidGameState.DeathPreFlashing:
            case SuperMetroidGameState.DeathFlashing:
            case SuperMetroidGameState.DeathExplosionWhiteOut:
                SamusDeathSequenceStepResult deathStep =
                    runtime!.DeathSequence.StepDeathSequenceFrame(controllerInput);
                display.PublishGameplay(runtime);
                if (deathStep.PhaseAfterStep == SamusDeathSequencePhase.Flashing)
                    GameState = SuperMetroidGameState.DeathFlashing;
                else if (deathStep.PhaseAfterStep == SamusDeathSequencePhase.SuitExplosion)
                    GameState = SuperMetroidGameState.DeathExplosionWhiteOut;
                else if (deathStep.Completed)
                {
                    deathFadeBrightness = 15;
                    deathFadeCounter = 1;
                    GameState = SuperMetroidGameState.DeathFinalBlackOut;
                }
                break;

            case SuperMetroidGameState.DeathFinalBlackOut:
                runtime!.RunBlankGameplayFrame(controllerInput);
                display.PublishGameplay(runtime);
                if (deathFadeCounter == 0)
                {
                    deathFadeCounter = 1;
                    if (deathFadeBrightness == 1)
                        deathFadeBrightness = 0;
                    else if (deathFadeBrightness != 0)
                        deathFadeBrightness--;
                }
                else
                {
                    deathFadeCounter--;
                }
                ApplyDisplayBrightness(deathFadeBrightness);
                if (deathFadeBrightness == 0)
                {
                    runtime.Freeze.GameplayTimeFrozen = false;
                    // The game-over menu runs no gameplay owner: every exit reloads SRAM or
                    // soft resets. Releasing the runtime hands the live RNG and accepted-NMI
                    // counters to the frontend, which keeps counting them through the menu.
                    ReleaseRuntimePreservingRandom();
                    GameState = SuperMetroidGameState.GameOverMenu;
                }
                break;

            case SuperMetroidGameState.GameOverMenu:
                gameOver ??= new GameOverMenuState(bus, audio, Artwork.MapPresentation,
                    continueLoadsCeresArrival: saveRam.ReadSlot(selectedSaveSlot)?.StartupRoute ==
                        SaveLoadingGameState.CeresElevatorArrival);
                gameOver.Step(controllerInput);
                display.PublishMenu(gameOver);
                if (gameOver.CeresArrivalRequested)
                {
                    // $81:9171 publishes state $1F and reloads the slot in the same dispatch;
                    // the Ceres loader runs on the next update. Bank $7E's gameplay owners are
                    // rebuilt from SRAM, so only the live RNG word survives.
                    gameOver = null;
                    loadingExistingSave = true;
                    ReleaseRuntimePreservingRandom();
                    GameState = SuperMetroidGameState.SetUpNewGame;
                }
                else if (gameOver.ContinueRequested)
                {
                    // Native menu index six publishes state $05 after the fully black
                    // frame. Keep that dispatcher boundary separate from SRAM reload.
                    gameOver = null;
                    GameState = SuperMetroidGameState.FileSelectMap;
                    display.PublishBlack();
                }
                else if (gameOver.TitleRequested)
                {
                    // GameOverMenu_7 invokes SoftReset only after its fade reaches black.
                    gameOver = null;
                    GameState = SuperMetroidGameState.Reset;
                    display.PublishBlack();
                }
                break;

            case SuperMetroidGameState.FileSelectMap:
                loadingExistingSave = true;
                SuperMetroidSaveSlot mapSlot = saveRam.ReadSlot(selectedSaveSlot)
                    ?? throw new InvalidDataException("The selected save became invalid before map selection.");
                if (mapSlot.StartupRoute == SaveLoadingGameState.CeresDestruction)
                {
                    StartSavedCeresDestruction(mapSlot);
                    break;
                }
                if (mapSlot.Area != (ushort)AreaId.Ceres)
                {
                    if (fileSelectMap is null)
                    {
                        // Menu construction consumes only save/map data. A dead runtime
                        // must not keep publishing gameplay audio during map selection.
                        ReleaseRuntimePreservingRandom();
                        fileSelectMap = new FileSelectMapMenuState(bus, audio, mapSlot, controllerInput, Artwork.MapPresentation);
                    }
                    else fileSelectMap.Step(controllerInput);
                    display.PublishFileMap(fileSelectMap);
                    if (fileSelectMap.OptionsRequested)
                    {
                        options = new GameOptionsMenuState(bus, audio, mapSlot.ControllerBindings,
                            mapSlot.IconCancelEnabled, mapSlot.MoonwalkEnabled,
                            japaneseText: mapSlot.JapaneseText,
                            mapPresentation: Artwork.MapPresentation);
                        fileSelectMap = null;
                        GameState = SuperMetroidGameState.GameOptionsMenu;
                        display.PublishMenu(options);
                        break;
                    }
                    if (!fileSelectMap.LoadRequested) break;
                    fileSelectMap = null;
                }
                // Ceres has no world-map entry; preserve its checkpoint arrival path.
                bool continueUsesCeresArrival = SetupSelectedGame();
                if (continueUsesCeresArrival)
                {
                    BeginGameplayFadeIn(leadsToCeresArrival: true);
                    display.PublishBlack();
                }
                else
                {
                    BeginGameplayFadeIn(leadsToCeresArrival: false);
                    display.PublishBlack();
                }
                break;

            case SuperMetroidGameState.PausingDarkening:
                // State $0C continues running ordinary gameplay while INIDISP darkens.
                // This matters for moving enemies/projectiles and is why pause cannot be
                // represented as a desktop-only frozen bitmap.
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(runtime) });
                display.PublishGameplay(runtime);
                if (runtime.RoomLoader.HasPendingDoorTransition)
                    GameState = SuperMetroidGameState.HitDoorBlock;
                if (runtime.ConsumeTimeUpRequest())
                    BeginTimeUp();
                pauseBrightness = screenFade.Advance(pauseBrightness, brightening: false);
                ApplyDisplayBrightness(pauseBrightness);
                if (pauseBrightness == 0)
                {
                    // $82:8CEA increments the state written by gameplay, including a
                    // door hit on this final fade frame ($09 becomes $0A).
                    screenFade.Clear();
                    GameState = GameState.Next();
                }
                break;

            case SuperMetroidGameState.Pausing:
                // `$82:8CEF` owns the force-blank setup frame: pause tiles, base maps,
                // palette, inventory labels, and PPU bases are installed before state $0E.
                // Keep accepting NMI so the controller's previous sample remains truthful.
                runtime!.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                // The pause hook returns directly into `$82:8D16`, which sends the three
                // cartridge stop sequences before any pause graphics are initialized.
                // Letting gameplay SFX continue under frozen gameplay causes stale echo,
                // charge, beam, or environmental voices to pulse throughout the menu.
                audio.QueueCancelSoundEffects();
                CartridgeRoomHeader pauseRoom = runtime.Room.ActiveRoom ??
                    throw new InvalidOperationException(
                        "Pause setup requires an active cartridge room header.");
                pauseMenu = new PauseMenuState(bus, new PauseMenuEntry { Samus = runtime.Player.Samus ?? throw new InvalidOperationException(
                        "Pause setup requires a live Samus state."), System = runtime.System, AreaIndex = pauseRoom.AreaIndex, RoomMapPosition = pauseRoom.MapPosition, Audio = audio, GameplayVram = runtime.Vram, MapRevealMode = ConfiguredOptions.MapReveal, MapPresentation = Artwork.MapPresentation });
                BeginPauseFade(0);
                display.PublishMenu(pauseMenu);
                ApplyDisplayBrightness(pauseBrightness);
                GameState = SuperMetroidGameState.PausedA;
                break;

            case SuperMetroidGameState.PausedA:
                runtime!.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                pauseBrightness = screenFade.Advance(pauseBrightness, brightening: true);
                pauseMenu!.AdvanceAnimations(runtime.Nmi.NmiFrameCounter8, advancePalette: false);
                display.PublishMenu(pauseMenu!);
                ApplyDisplayBrightness(pauseBrightness);
                if (pauseBrightness == PauseFadeTiming.FullyLit)
                    GameState = SuperMetroidGameState.PausedB;
                break;

            case SuperMetroidGameState.PausedB:
                runtime!.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                pauseMenu!.SynchronizeAcceptedHud(runtime.Vram);
                runtime.UpdatePauseHeldInput();
                ushort previousReserveMode = runtime.Player.Samus!.ReserveTankMode;
                bool unpauseRequested = pauseMenu!.Step(
                    runtime.System.TimedHeldInput,
                    runtime.Controller1.NewlyPressed,
                    runtime.Controller1.Current,
                    runtime.Nmi.NmiFrameCounter8);
                // State $0F runs the HUD writer after pause input. Its DMA is visible
                // at the next accepted NMI, including the first unpause fade frame.
                if (previousReserveMode == PauseReserveLabelRomData.AutoMode &&
                    runtime.Player.Samus.ReserveTankMode == PauseReserveTransferRomData.ManualMode)
                    runtime.Hud.ClearAutoReserveIndicator();
                runtime.Hud.UpdateGameplayCounters(bus, runtime.Player.Samus, timeIsFrozen: true);
                runtime.Hud.QueueUpload(bus, runtime.VramWrites);
                display.PublishMenu(pauseMenu);
                if (unpauseRequested)
                {
                    BeginPauseFade(PauseFadeTiming.FullyLit);
                    GameState = SuperMetroidGameState.UnpausingA;
                }
                break;

            case SuperMetroidGameState.UnpausingA:
                runtime!.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                pauseMenu!.SynchronizeAcceptedHud(runtime.Vram);
                pauseMenu!.AdvanceAnimations(runtime.Nmi.NmiFrameCounter8, fadingOut: true, advancePalette: false);
                display.PublishMenu(pauseMenu!);
                pauseBrightness = screenFade.Advance(pauseBrightness, brightening: false);
                ApplyDisplayBrightness(pauseBrightness);
                if (pauseBrightness == 0)
                    GameState = SuperMetroidGameState.UnpausingB;
                break;

            case SuperMetroidGameState.UnpausingB:
                // Native state $11 restores gameplay PPU state, BG2, beam tiles, palette,
                // hooks, HDMA, and animtiles under forced blank. Pause graphics live in a
                // separate PPU image here, so discarding it restores the untouched runtime
                // image without a host-authored reconstruction.
                SamusState resumedSamus = runtime!.Player.Samus ??
                    throw new InvalidOperationException(
                        "Pause teardown requires live Samus equipment state.");
                // EquipmentScreenCategory_ButtonResponse mutates the live equipped word,
                // but projectile character data and CGRAM are cached PPU resources. State
                // `$11` reruns `$90:AC8D` before its accepted NMI; omitting that handoff
                // made the new damage/type word immediate while art stayed stale until the
                // next room load happened to invoke the same routine.
                runtime.SamusPhases.QueueGameplayBeamTilesAndLoadPalette(resumedSamus.EquippedBeams);
                resumedSamus.ReconcilePauseBallEquipment(bus);
                // The native teardown also reconciles equipment-dependent momentum.
                // Keep the speed pair until the next gameplay frame, as the cartridge does.
                resumedSamus.HorizontalSpeed.ReconcilePauseSpeedBoosterState(
                    (resumedSamus.EquippedItems & (ushort)SamusEquipmentFlags.SpeedBooster) != 0);
                // Samus command $0C also calls LoadSamusSuitPalette at $91:E6CB.
                // Equipment bits are live, but the restored gameplay palette is cached.
                resumedSamus.Presentation.LoadSuitPalette(bus, runtime.Cgram);
                runtime.Plms.Stations.ResumeMapStationsAfterUnpause();
                resumedSamus.ScriptLocks.ReleaseRefillStationLockOnUnpause();
                runtime.Enemies.ElevatorBehavior.ClearElevatorFlagsOnGameplayResume();
                runtime!.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                pauseMenu = null;
                BeginPauseFade(0);
                display.PublishBlack();
                GameState = SuperMetroidGameState.Unpausing;
                break;

            case SuperMetroidGameState.Unpausing:
                // `$82:93A4` runs the whole state-eight routine behind the INIDISP fade, so
                // its out-of-health, pause, door and other publications all apply here.
                RunMainGameplayState(controllerInput, gameplayAudio);
                pauseBrightness = screenFade.Advance(pauseBrightness, brightening: true);
                ApplyDisplayBrightness(pauseBrightness);
                if (pauseBrightness == PauseFadeTiming.FullyLit)
                {
                    // $82:93BB explicitly restores state eight on the terminal frame.
                    screenFade.Clear();
                    GameState = SuperMetroidGameState.MainGameplay;
                }
                break;

            case SuperMetroidGameState.BlackoutFromCeres:
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AllowCeresElevatorDeparture = false }, Audio = gameplayAudio.GameplayFrameAudio(runtime) });
                display.PublishGameplay(runtime);
                bool ceresReachedForcedBlank = ceresDeparture.StepFadeAfterGameplay();
                ApplyDisplayBrightness(ceresDeparture.Brightness);
                if (ceresReachedForcedBlank)
                {
                    // GameState_33 saves the live area-six/station-zero slot only after
                    // INIDISP reaches forced blank. This is the durable post-Ridley Ceres
                    // checkpoint the retail loader later replaces with Zebes arrival.
                    AutomaticCheckpointSaver.SaveCeresDeparture(bus, runtime, selectedSaveSlot);
                    SaveRamChanged?.Invoke();
                    runtime.Enemies.RidleyEncounter.CeresStatus = 0;
                    runtime.EscapeTimer.Clear();
                    // $82:83F3-$840B: stop music and silence all three sound libraries.
                    audio.QueueMusicDelayed8(MusicCommand.Stop);
                    audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
                    audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
                    audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
                    ceresDestruction = new CeresDestructionCinematicState(
                        bus, audio, Artwork.MapPresentation?.PowerBombFixedColors,
                        Artwork.IntroCinematicArt, Artwork.MapPresentation?.RoomPaletteFx);
                    GameState = SuperMetroidGameState.CeresGoesBoom;
                    display.PublishBlack();
                }
                break;

            case SuperMetroidGameState.TimeUp:
                // `$82:8411` runs the whole state-eight routine, then advances every palette
                // toward the white target with denominator eight.
                RunMainGameplayState(controllerInput, gameplayAudio);
                if ((timeUpPaletteFade ?? throw new InvalidOperationException(
                        "Time-up white-out started without its target palette.")).Step(runtime!.Cgram))
                {
                    GameState = SuperMetroidGameState.WhitingOutFromTimeUp;
                    screenFade.Clear();
                }
                display.PublishGameplay(runtime);
                break;

            case SuperMetroidGameState.WhitingOutFromTimeUp:
                // `$82:8431` runs no gameplay: it fades the whitened frame to forced blank.
                runtime!.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                pauseBrightness = screenFade.Advance(pauseBrightness, brightening: false);
                ApplyDisplayBrightness(pauseBrightness);
                if (pauseBrightness == 0)
                    FinishTimeUpBlackOut();
                break;

            case SuperMetroidGameState.CeresGoesBoomWithSamus:
                runtime?.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                CeresDestructionCinematicState fatalDestruction = ceresDestruction ??
                    throw new InvalidOperationException("Ceres destruction with Samus has no cinematic.");
                fatalDestruction.Step();
                display.PublishCeresDestruction(fatalDestruction);
                if (fatalDestruction.Finished)
                {
                    // `$8B:C64E`: the fatal cinematic's fade-out ends at state $19, already
                    // in forced blank, after clearing Samus's HUD selection and hurt timers.
                    SamusState lostSamus = runtime?.Player.Samus ?? throw new InvalidOperationException(
                        "Ceres destruction with Samus requires her gameplay state.");
                    lostSamus.SelectedHudItem = 0;
                    lostSamus.AutoCancelHudItemIndex = 0;
                    lostSamus.InvincibilityTimer = NativeCountdown.Start(0);
                    lostSamus.KnockbackTimer = NativeCountdown.Start(0);
                    ceresDestruction = null;
                    BeginForcedBlankDeathBlackOut();
                }
                break;

            case SuperMetroidGameState.CeresGoesBoom:
                // Each update begins with an accepted NMI. A gameplay owner kept from Ceres
                // counts $05B5/$05B6 and latches input; without one the frontend counted it.
                runtime?.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                ceresDestruction ??= new CeresDestructionCinematicState(
                    bus, audio, Artwork.MapPresentation?.PowerBombFixedColors,
                    Artwork.IntroCinematicArt, Artwork.MapPresentation?.RoomPaletteFx);
                ceresDestruction.Step();
                display.PublishCeresDestruction(ceresDestruction);
                if (ceresDestruction.Finished)
                {
                    // CADF forces blank and publishes state six. The loader's special
                    // `$22` branch—not cinematic code—selects area zero/station eighteen.
                    GameState = SuperMetroidGameState.LoadingGameData;
                    display.PublishBlack();
                }
                break;

            case SuperMetroidGameState.LoadingGameData:
                if (ResumesGameLoadingWait)
                {
                    StepGameLoadingWait(controllerInput);
                    break;
                }
                // The `$22` Zebes-landing branch of $82:8000, followed by its NMI waits. The
                // dispatch's own accepted NMI counts in the gameplay owner kept from Ceres.
                runtime!.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                runtime.RoomLoader.InitializePostCeresZebesRoom();
                BeginGameLoadingWaits(GameLoadingDefinitions.ZebesLandingLoadWaits,
                    GameLoadingCompletion.GameplayFadeIn);
                break;

            case SuperMetroidGameState.MainGameplayFadeIn:
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(runtime) });
                display.PublishGameplay(runtime);
                ApplyDisplayBrightness(postCeresFadeBrightness);
                HandleGunshipLandingSave();
                if (postCeresFadeCounter-- <= 0)
                {
                    postCeresFadeCounter = 1;
                    postCeresFadeBrightness = (byte)Math.Min(
                        15,
                        postCeresFadeBrightness + 1);
                    if (postCeresFadeBrightness == 15)
                    {
                        // $82:8106 always selects state seven, which then enters state
                        // eight; the Ceres arrival is ordinary gameplay with Samus locked.
                        GameState = SuperMetroidGameState.MainGameplay;
                        gameplayFadeLeadsToCeresArrival = false;
                        screenFade.Clear();
                        // HandleFadingIn wrote $51 = $0F this frame; the level is published
                        // before it rises, so record the register directly.
                        pauseBrightness = PauseFadeTiming.FullyLit;
                    }
                }
                break;

            case SuperMetroidGameState.HitDoorBlock:
            case SuperMetroidGameState.LoadingNextRoomA:
                if (GameState == SuperMetroidGameState.HitDoorBlock &&
                    !StepHitDoorBlockFunction(controllerInput))
                    break;
                // State $0A ($82:E1B7) follows in the same dispatch and publishes state $0B.
                // MainGameLoop's HDMA pass ($88:84B9) runs before this state handler. A rising
                // liquid's rumble or ambient sound it requests is admitted ahead of the cancels.
                runtime!.DoorTransition.RunDoorSoundWaitPrologue(controllerInput);
                PublishDoorEntryRoomFxSounds(runtime);
                // $82:E1BB sets the door/enemy pause flags, then $82:E1CA-E1D6 run the source
                // enemy/draw owners while sounds are still admitted, so an enemy instruction's
                // sound on this frame is queued before the door's cancels.
                doorTransition.Begin(runtime);
                runtime.DoorTransition.DrawDoorTransitionActors(runEnemyProjectiles: false);
                CollectDoorSoundWaitAudioRequests(runtime);
                // $82:E26B calls $84:8250, which runs Samus code $1D, then $82:E272 queues
                // library-two $71 so movement/charge loops terminate rather than leaking
                // into the next room.
                SamusState doorSamus = runtime.Player.Samus
                    ?? throw new InvalidOperationException("Door transition requires live Samus state.");
                SamusMovementType doorMovementType = doorSamus.PoseMetadata.ReadMovementType(bus);
                // A live power-bomb explosion drops all three requests inside the queue
                // routines, so neither ring holds anything for $E29E to wait on.
                bool doorSoundsSuppressed = runtime.PowerBombExplosionSuppressesSounds;
                if (doorMovementType is
                    SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
                {
                    audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.StopSpinJump,
                        maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
                }
                else if ((controllerInput & (ushort)SnesButton.X) == 0 &&
                    runtime.Projectiles.Charge.FlareCounter < 16)
                {
                    audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.CancelAll,
                        maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
                }
                audio.QueueSoundAndGetAccumulator(SoundEffectLibrary2Sounds.CancelAll,
                    maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
                // $82:E279 follows both entry cancellation commands, not precedes them.
                audio.DoorTransitionSoundsDisabled = true;
                // State $09 calls state $0A synchronously for ordinary doors; state $0A
                // publishes state $0B before returning. Consequently neither intermediate
                // numeric value owns a separately displayed frame.
                GameState = SuperMetroidGameState.LoadingNextRoomB;
                break;

            case SuperMetroidGameState.LoadingNextRoomB:
                doorTransition.Step(runtime!, audio, controllerInput, DoorLoaderProgress,
                    publishSoundWaitAudio: () => CollectDoorSoundWaitAudioRequests(runtime!));
                display.PublishGameplay(runtime!);
                if (doorTransition.Phase == DoorTransitionPhase.Complete)
                {
                    // $82:E72F ORs $1F into $51 before the destination fade-in, so a door
                    // taken mid-pause-fade still leaves the screen fully lit.
                    pauseBrightness = PauseFadeTiming.FullyLit;
                    audio.DoorTransitionSoundsDisabled = false;
                    // The cartridge resumes spin audio only after sound admission is
                    // restored at the final fade boundary, not on each fade frame.
                    if (SamusSpinSoundCommand.Select(bus, runtime!.Player.Samus!) is { } spinSound)
                        audio.QueueSoundAndGetAccumulator(spinSound, maximumQueued: 9,
                            soundSuppressed: runtime.AttractDemo.IsAttractDemo || runtime.PowerBombExplosionSuppressesSounds);
                    GameState = SuperMetroidGameState.MainGameplay;
                }
                break;

            case SuperMetroidGameState.SamusEscapesFromZebes:
                // State $26 calls the complete state-eight gameplay coroutine before
                // HandleFadeOut. The fleeing gunship, room animations, and APU publishers
                // therefore continue behind every darkening frame.
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(runtime) });
                display.PublishGameplay(runtime);
                // HandleFadingOut ($80:8924) reloads the counter from ScreenFadeDelay, which
                // $A2:AD26 zeroed, so the brightness falls one step on every call.
                if (endingFadeCounter-- <= 0)
                {
                    endingFadeCounter = 0;
                    endingFadeBrightness = (byte)Math.Max(0, endingFadeBrightness - 1);
                }
                ApplyDisplayBrightness(endingFadeBrightness);
                if (endingFadeBrightness == 0)
                {
                    // $82:84D3-$8527 resets PPU ownership, stops the escape timer/music,
                    // cancels all three SFX libraries, and publishes state $27. The new
                    // state performs its own cartridge-backed setup on the following call.
                    audio.QueueMusicDelayed8(MusicCommand.Stop);
                    audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
                    audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
                    audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
                    SamusState endingSamus = runtime.Player.Samus ??
                        throw new InvalidOperationException(
                            "Ending inventory calculation requires the live Samus state.");
                    endingCredits = new EndingCreditsState(bus, audio, new EndingCreditsSetup { GameTimeHours = ConfiguredOptions.EndingTimeOverrideMinutes is { } endingMinutes ? (ushort)(endingMinutes / 60) : runtime.GameTime.Hours, GameTimeMinutes = ConfiguredOptions.EndingTimeOverrideMinutes is { } totalMinutes ? (ushort)(totalMinutes % 60) : runtime.GameTime.Minutes, Inventory = new EndingInventorySnapshot(
                            endingSamus.MaxHealth,
                            endingSamus.MaxReserveEnergy,
                            endingSamus.MaxMissiles,
                            endingSamus.MaxSuperMissiles,
                            endingSamus.MaxPowerBombs,
                            endingSamus.CollectedItems,
                            endingSamus.CollectedBeams), JapaneseText = runtime.JapaneseText, CrittersEscaped = runtime.System.HasEvent(EventNumber.CrittersEscaped) });
                    endingCredits.BindEndingText(Artwork.MapPresentation?.EndingText);
                    endingCredits.BindFlightArtwork(Artwork.IntroCinematicArt?.CeresFlight);
                    endingCredits.BindMode7Artwork(Artwork.EndingMode7Art);
                    endingCredits.BindObjectArtwork(Artwork.EndingObjectArt);
                    endingCredits.BindPaletteArtwork(Artwork.EndingPaletteArt);
                    endingCredits.BindPaletteFxColors(Artwork.MapPresentation?.RoomPaletteFx);
                    endingCredits.BindEndingFont(Artwork.MapPresentation?.EndingFont);
                    endingCredits.BindStaffCredits(Artwork.MapPresentation?.StaffCredits);
                    GameState = SuperMetroidGameState.EndingAndCredits;
                    display.PublishBlack();
                }
                break;

            case SuperMetroidGameState.EndingAndCredits:
                EndingCreditsPhase previousEndingPhase = endingCredits!.Phase;
                endingCredits.Step();
                if (previousEndingPhase != EndingCreditsPhase.SeeYouNextMission &&
                    endingCredits.Phase == EndingCreditsPhase.SeeYouNextMission)
                {
                    saveRam.SetGameCompleted(true);
                    SaveRamChanged?.Invoke();
                }
                display.PublishEnding(endingCredits);
                break;

            case SuperMetroidGameState.TransitionToDemoA:
            case SuperMetroidGameState.TransitionToDemoB:
            case SuperMetroidGameState.PlayingDemo:
            case SuperMetroidGameState.TransitionFromDemoA:
            case SuperMetroidGameState.TransitionFromDemoB:
                StepAttractDemo(controllerInput, gameplayAudio);
                break;

            default:
                // GameState has a private setter and every translated producer writes one
                // of the explicit states above. Pause/death/ending will gain named cases as
                // their producers are connected; they cannot currently arise through this
                // dispatcher. An unexpected value is therefore corrupt internal state.
                throw new InvalidDataException(
                    $"Frontend dispatcher received invalid state " +
                    $"${((ushort)GameState):X2} ({GameState}).");
        }

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
            MessageBoxFrameAudio boxAudio = runtime!.MessageBox.LastFrameAudio;
            musicHandler = boxAudio.MusicHandlerCalls != 0;
            soundHandlers = boxAudio.SoundHandlerCalls;
        }
        else if (runtime?.MessageBox.IsActive == true)
        {
            // The opening dispatch's HandleSounds runs only once the routine returns.
            soundHandlers = 0;
        }
        IReadOnlyList<CartridgeAudioCommand> trailingAudioCommands = audio.AdvanceFrame(
            bus, SoundAcknowledgements.ForNextFrame(audioSession.Acknowledgements),
            advanceMusicQueue: musicHandler,
            soundEffectHandlerCalls: soundHandlers);
        lastAudioCommands = prologueMusicCommands.Count == 0 ? trailingAudioCommands
            : [.. prologueMusicCommands, .. trailingAudioCommands];
        // The SPC acts on this update's port writes and uploads during the update's own DSP
        // cycles; its echoes become visible to the next update's sound handlers.
        audioSession.Advance(lastAudioCommands);
        // $82:8AB0 runs after the pause dispatcher returns, even though Samus's
        // draw-time epilogue did not run. Preserve its held-only publication;
        // updating the press edge or auto-jump timer here would invent gameplay.
        if (pauseInputEpilogue)
            runtime!.Player.Samus!.InputSnapshot.PublishMainLoopHeldInput(runtime.Controller1.Current);
        return CurrentFrame;
    }

    /// <summary>Reads frontend status and audio commands without materializing a retained display.</summary>
    public FrontendFrame CurrentFrameMetadata =>
        new(GameState, PhaseName, FrameNumber, Array.Empty<Rgba32>(), lastAudioCommands);

    /// <summary>Reads the current status, last audio commands, and display without advancing the dispatcher.</summary>
    /// <remarks>
    /// Pixels are the producer's reusable 256-by-224 buffer, not a copy; a host retaining them across another frame must copy them.
    /// When a display capture already owns the current image, the pixel array is empty and the host presents that capture instead.
    /// </remarks>
    public FrontendFrame CurrentFrame =>
        new(GameState, PhaseName, FrameNumber,
            display.CurrentPixels,
            lastAudioCommands);

    /// <summary>
    /// Moves already-translated per-frame publishers into the single cartridge queue.
    /// </summary>
    /// <remarks>
    /// This is intentionally a collection seam, not sound policy. Samus, projectiles, and
    /// PLMs retain ownership of the exact library/ID/cap chosen by their ROM routines; the
    /// frontend merely performs the global QueueSfx call those producers requested.
    /// </remarks>
    private void CollectTranslatedAudioRequests(GameplayAudioFramePublication gameplayAudio)
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
                audio.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x09), maximumQueued: 6);
            if (samus.Xray.ConsumeDeactivationSoundRequest())
                audio.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x0a), maximumQueued: 6);
            if (samus.DeathSequence.ConsumeSpinJumpSoundRequest())
                audio.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x32), maximumQueued: 6);
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

    /// <summary>Queues the entry frame's HDMA-pass room-FX sounds while sounds are still enabled.</summary>
    private void PublishDoorEntryRoomFxSounds(SuperMetroidRuntime source)
    {
        if (source.AttractDemo.IsAttractDemo)
            return;
        foreach (var request in source.RoomLayer3Fx.SoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
    }

    /// <summary>Collects only enemy and Samus audio emitted during the door music-wait pass.</summary>
    private void CollectDoorSoundWaitAudioRequests(SuperMetroidRuntime source)
    {
        // This coroutine completes no ordinary gameplay publication. Only the
        // owners run by its enemy/draw pass may refill the rings before the test.
        foreach (EnemySoundRequest request in source.Enemies.Audio.SoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (EnemyMusicRequest request in source.Enemies.Audio.MusicRequests)
            audio.QueueMusicDelayed(request.Command, request.Delay);
        if (source.Player.Samus is { } samus)
            foreach (SamusSoundRequest request in samus.LiquidPhysics.SoundRequests)
                audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                    soundSuppressed: request.SoundSuppressed);
    }

    /// <summary>Human-readable active subphase for diagnostics without changing native state.</summary>
    private string PhaseName => GameState switch
    {
        SuperMetroidGameState.Reset => "Reset",
        SuperMetroidGameState.OpeningCinematic => title!.Phase.ToString(),
        SuperMetroidGameState.FileSelectMenus => fileSelect!.Phase.ToString(),
        SuperMetroidGameState.GameOptionsMenu => options!.Phase.ToString(),
        SuperMetroidGameState.FileSelectMap => fileSelectMap?.Phase.ToString() ?? "Preparing saved-game map",
        SuperMetroidGameState.IntroCinematic => intro!.Phase.ToString(),
        SuperMetroidGameState.SetUpNewGame =>
            loadingExistingSave ? "Loading saved game" : "Loading fresh Ceres game",
        SuperMetroidGameState.MadeItToCeresElevator =>
            ceresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator
                ? $"Ceres elevator departure ({ceresDeparture.HoldFramesRemaining.RawValue})"
                : runtime?.Room.CeresElevatorArrival is { IsComplete: false }
                ? "Ceres elevator arrival"
                : "Ceres controls unlocked",
        SuperMetroidGameState.BlackoutFromCeres =>
            $"Ceres blackout (brightness {ceresDeparture.Brightness})",
        SuperMetroidGameState.CeresGoesBoom =>
            $"Ceres destruction: {ceresDestruction?.Phase}",
        SuperMetroidGameState.LoadingGameData =>
            $"Loading Landing Site ({gameLoadingWaitsRemaining} NMI waits)",
        SuperMetroidGameState.MainGameplayFadeIn =>
            gameplayFadeLeadsToCeresArrival
                ? $"Ceres elevator fade-in (brightness {postCeresFadeBrightness})"
                : $"Landing Site fade-in (brightness {postCeresFadeBrightness})",
        SuperMetroidGameState.MainGameplay => "Gameplay",
        SuperMetroidGameState.HitDoorBlock => "Door collision",
        SuperMetroidGameState.LoadingNextRoomA => "Loading destination room",
        SuperMetroidGameState.LoadingNextRoomB => "Destination room ready",
        SuperMetroidGameState.SamusEscapesFromZebes =>
            $"Zebes escape fade-out ({endingFadeBrightness})",
        SuperMetroidGameState.EndingAndCredits =>
            $"Ending: {endingCredits?.Phase}",
        SuperMetroidGameState.PausingDarkening =>
            $"Pausing: gameplay darken ({pauseBrightness})",
        SuperMetroidGameState.Pausing => "Pausing: load pause assets",
        SuperMetroidGameState.PausedA => $"Paused: fade in ({pauseBrightness})",
        SuperMetroidGameState.PausedB =>
            pauseMenu?.ScreenMode == 1 ? "Paused: equipment" : "Paused: map",
        SuperMetroidGameState.UnpausingA => $"Unpausing: fade out ({pauseBrightness})",
        SuperMetroidGameState.UnpausingB => "Unpausing: restore gameplay",
        SuperMetroidGameState.Unpausing => $"Unpausing: gameplay brighten ({pauseBrightness})",
        SuperMetroidGameState.Unused03 or SuperMetroidGameState.DeathSequenceStart or
            SuperMetroidGameState.DeathBlackOutSurroundings or SuperMetroidGameState.DeathWaitForMusic or
            SuperMetroidGameState.DeathPreFlashing or SuperMetroidGameState.DeathFlashing or
            SuperMetroidGameState.DeathExplosionWhiteOut or SuperMetroidGameState.DeathFinalBlackOut or
            SuperMetroidGameState.GameOverMenu or SuperMetroidGameState.ReserveTanksAuto or
            SuperMetroidGameState.Unused1c or SuperMetroidGameState.DebugGameOverMenu or
            SuperMetroidGameState.TimeUp or SuperMetroidGameState.WhitingOutFromTimeUp or
            SuperMetroidGameState.CeresGoesBoomWithSamus or SuperMetroidGameState.TransitionToDemoA or
            SuperMetroidGameState.TransitionToDemoB or SuperMetroidGameState.PlayingDemo or
            SuperMetroidGameState.TransitionFromDemoA or SuperMetroidGameState.TransitionFromDemoB => GameState.ToString(),
        _ => throw new InvalidOperationException($"Undefined SuperMetroidGameState {GameState}."),
    };

    /// <summary>Checks the native state-eight pause gate, including Samus handler locks and new Start input.</summary>
    private bool CanEnterPause(
        bool messageBoxOwnedFrame,
        bool samusInputLockedAtFrameStart)
    {
        if (runtime?.Player.Samus is not SamusState samus || runtime.Room.ActiveRoom is null)
            return false;

        // `$90:EA45` only runs from SamusNewStateHandler_Normal. Station command six
        // replaces that handler with the inert `$90:E8D6`, so its entire insertion,
        // message/data-load, and retraction sequence bypasses pause-check even though game
        // state remains eight. InputLocked is the semantic representation of that handler
        // replacement; the remaining clauses are the predicate inside EA45 itself.
        return GameState == SuperMetroidGameState.MainGameplay &&
               !messageBoxOwnedFrame &&
               !runtime.MessageBox.IsActive &&
               !samusInputLockedAtFrameStart &&
               !samus.InputLocked &&
               runtime.PowerBombExplosionStatus == 0 &&
               !samus.Xray.TimeIsFrozen &&
               !runtime.RoomLoader.HasPendingDoorTransition &&
               runtime.Room.ActiveRoom.AreaIndex != AreaId.Ceres &&
               (runtime.Controller1.NewlyPressed & (ushort)SnesButton.Start) != 0;
    }

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
    /// <c>GameState_8_MainGameplay</c>: one gameplay frame and the state publications its
    /// routines make. State $12 (<c>$82:93A1</c>) calls the same routine before fading in.
    /// </summary>
    private void RunMainGameplayState(ushort controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // DisplayMessageBox is a nested NMI-waiting coroutine inside gameplay
        // state eight. Its final acknowledgement returns below the normal bank-$90
        // pause-check call site, so Start may neither enter pause while the box is
        // visible nor on the exact frame it closes. Preserve entry state before
        // StepFrame consumes the acknowledgement; checking IsActive afterward alone
        // misses that final suspended frame.
        bool messageBoxOwnedFrame = runtime!.MessageBox.IsActive;
        bool mapStationMessageOwnedFrame = messageBoxOwnedFrame &&
            runtime.MessageBox.MessageId == GameplayMessageIds.MapDataAccessCompleted;
        // Station command six installs an inert new-state handler before this
        // frame. Retraction can restore normal input later in StepFrame, after the
        // cartridge's Samus-handler call site has already passed pause-check.
        bool samusInputLockedAtFrameStart = runtime.Player.Samus?.InputLocked == true;
        runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(runtime) });
        bool timeUp = runtime.ConsumeTimeUpRequest();
        HandleSaveStationPersistence();
        display.PublishGameplay(runtime);
        HandleGunshipLandingSave();
        if (RouteOutOfHealth())
        {
            // `$82:DB69` publishes the next outer state at the end of this already-
            // completed gameplay call. Nothing else may replace it on the trigger
            // frame—not pause, an elevator handoff, or a pending ordinary door.
        }
        else if (timeUp)
        {
            // `$90:E0EC` wrote state $23 inside Samus's handler, after her movement's
            // door hits and before pause-check, which then sees a state other than eight.
            BeginTimeUp();
        }
        else if (runtime.Enemies.GunshipBehavior.LastGunshipEvent == GunshipFrameEvent.EscapeTakeoffCompleted)
        {
            BeginZebesEscapeFade();
        }
        else if (mapStationMessageOwnedFrame && !runtime.MessageBox.IsActive)
        {
            // Bank $85's message-close return explicitly selects state $0C
            // for map acquisition, independently of Start or station input lock.
            // Keep this separate from admission of a manual pause request.
            pauseMenu = null;
            GameState = SuperMetroidGameState.PausingDarkening;
        }
        else if (CanEnterPause(messageBoxOwnedFrame, samusInputLockedAtFrameStart))
        {
            // Samus_PauseCheck at `$90:EA45` executes during the already-completed
            // state-eight frame. It initializes both fade counters and publishes
            // state $0C; the following dispatcher call consumes the first delay.
            screenFade.Reload();
            pauseMenu = null;
            GameState = SuperMetroidGameState.PausingDarkening;
        }
        else if (runtime.RoomMains.LastCeresElevatorShaftRoomMain.DepartureRequestedThisFrame)
        {
            // `$89:ACC3` has already selected standing pose, locked Samus, and
            // published game state $20. The following dispatcher call owns the
            // 60-frame hold; do not decrement it on this trigger frame.
            ceresDeparture.Begin();
            GameState = SuperMetroidGameState.MadeItToCeresElevator;
        }
        else if (runtime.RoomLoader.HasPendingDoorTransition && !runtime.MessageBox.IsActive)
        {
            // `$94:938B/$93CE` changes WRAM game_state during the gameplay call.
            // The already-produced gameplay image remains this frame's image; the
            // following dispatcher call begins state `$09` from that publication.
            // A message box opened later in the same call (a station's completion
            // notice in the PLM handler) suspends that call in bank $85 until it
            // closes, so the door's state waits for the box's final frame.
            GameState = SuperMetroidGameState.HitDoorBlock;
        }
    }

    /// <summary>Freezes gameplay and enters reserve recovery or death after Samus reaches zero health.</summary>
    private bool RouteOutOfHealth()
    {
        if (runtime?.Player.Samus is not SamusState samus || unchecked((short)samus.Health) > 0)
            return false;

        runtime.Freeze.GameplayTimeFrozen = true;
        if ((samus.ReserveTankMode & 1) != 0 && samus.ReserveEnergy != 0)
        {
            reserveRecovery.Begin(samus);
            GameState = SuperMetroidGameState.ReserveTanksAuto;
        }
        else
        {
            // CallSomeSamusCode($11) disables palette effects and installs fatal-input and
            // fatal-draw handlers. Input locking is the shared translated representation;
            // the following named death states own palette and animation side effects.
            samus.InputLocked = true;
            GameState = SuperMetroidGameState.DeathSequenceStart;
        }
        return true;
    }

    /// <summary>
    /// <c>$90:E0EC-$E106</c>: the escape countdown expired. Every target color becomes white
    /// for state $23's gradual change through the shared <c>$7E:C400</c> numerator.
    /// </summary>
    private void BeginTimeUp()
    {
        if (runtime is null)
            throw new InvalidOperationException("Time-up requires a gameplay runtime.");
        var white = new Bgr555[CgramPalettes.ColorCount];
        Array.Fill(white, TimeUpRomData.WhiteColor);
        timeUpPaletteFade = new CartridgePaletteTransition(white, TimeUpRomData.WhiteOutDenominator,
            runtime.Enemies.Presentation.GradualColorChange);
        GameState = SuperMetroidGameState.TimeUp;
    }

    /// <summary>
    /// <c>$82:8444-$84BC</c>, once the time-up fade reaches forced blank: stop the escape
    /// timer and Ceres effects, silence all three sound libraries, then lose Samus with Ceres
    /// or, once the Zebes time bomb is set, cut straight to the death black-out.
    /// </summary>
    private void FinishTimeUpBlackOut()
    {
        if (runtime is null)
            throw new InvalidOperationException("Time-up black-out requires a gameplay runtime.");
        screenFade.Clear();
        timeUpPaletteFade = null;
        runtime.Enemies.RidleyEncounter.CeresStatus = 0;
        runtime.EscapeTimer.ClearStatus();
        audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
        audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
        audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
        display.PublishBlack();
        if (runtime.System.HasEvent(EventNumber.ZebesTimebombSet))
        {
            BeginForcedBlankDeathBlackOut();
            return;
        }
        ceresDestruction = new CeresDestructionCinematicState(
            bus, audio, Artwork.MapPresentation?.PowerBombFixedColors,
            Artwork.IntroCinematicArt, Artwork.MapPresentation?.RoomPaletteFx, withSamus: true);
        GameState = SuperMetroidGameState.CeresGoesBoomWithSamus;
    }

    /// <summary>
    /// Enters state $19 already at forced blank, as both time-up outcomes do. Its
    /// <c>HandleFadingOut</c> finds no brightness left, so that dispatch opens game over.
    /// </summary>
    private void BeginForcedBlankDeathBlackOut()
    {
        deathFadeBrightness = 0;
        deathFadeCounter = 0;
        GameState = SuperMetroidGameState.DeathFinalBlackOut;
        display.PublishBlack();
    }

    /// <summary>Builds state $13's target-palettes image and resets <c>$7E:C400</c>.</summary>
    private void PrepareDeathPaletteFade()
    {
        if (runtime is null)
            throw new InvalidOperationException("Death palette setup requires a runtime.");

        var target = new Bgr555[CgramPalettes.ColorCount];
        ReadOnlySpan<Bgr555> current = new CgramPalettes(runtime.Cgram).Image;
        // `$82:DCA3-$DCB3` restores only palette-buffer colors $C0-$CF into the otherwise
        // black target. Suitless row $F0 and all room/OBJ rows therefore fade away.
        current.Slice(0xc0, 0x10).CopyTo(target.AsSpan(0xc0, 0x10));
        deathPaletteFade = new CartridgePaletteTransition(target, denominator: 6, runtime.Enemies.Presentation.GradualColorChange);
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
    public RenderFrameSnapshot? GetRetainedDisplay(long hostSequence, long hostGeneration)
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
    public CapturedFrontendFrame StepCaptured(ushort controllerInput, long hostSequence, long hostGeneration)
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
    private void ApplyDisplayBrightness(byte brightness)
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

    /// <summary>
    /// Executes <c>AdvancePaletteFadeForAllPalettes</c> with denominator six. The routine
    /// interpolates from the already-updated current color each call, including its initial
    /// no-op step zero and separate completion call after step seven reaches the target.
    /// </summary>
    private bool StepDeathPaletteFade()
    {
        if (runtime is null)
            throw new InvalidOperationException("Death palette fade requires a runtime.");
        return (deathPaletteFade ?? throw new InvalidOperationException(
            "Death palette target was not prepared.")).Step(runtime.Cgram);
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

    /// <summary>
    /// $82:EECC-$EED4 dispatches saved mode $22 to the initial destruction cinematic.
    /// Restore the SRAM mirror/inventory, but do not initialize any Ceres room or actors.
    /// The existing cinematic handoff later loads Zebes station eighteen.
    /// </summary>
    private void StartSavedCeresDestruction(SuperMetroidSaveSlot slot)
    {
        CreateGameplayRuntime();
        runtime!.RoomLoader.RestoreSavedPlayerState(slot, ConfiguredOptions.ResetBossesOnLoad);
        // Unlike an ongoing escape, a file-select resume has a fresh gameplay
        // owner. Construct its ordinary HUD now so the cinematic's later room
        // handoff retains initialized HUD state and standard graphics transfers.
        InitializeSelectedGameHud(slot);
        runtime.NmiHandler.PublishForcedBlankTransfers();
        ApplySelectedGameOptions();
        ceresDestruction = new CeresDestructionCinematicState(
            bus, audio, Artwork.MapPresentation?.PowerBombFixedColors,
            Artwork.IntroCinematicArt, Artwork.MapPresentation?.RoomPaletteFx);
        GameState = SuperMetroidGameState.CeresGoesBoom;
        display.PublishBlack();
    }

    /// <summary>Applies live menu controller and gameplay choices to a newly created runtime.</summary>
    private void ApplySelectedGameOptions()
    {
        // Live edits in options take precedence over the snapshot loaded at file select.
        if (options is null) return;
        runtime!.Options.ControllerBindings = options.ControllerBindings;
        runtime.Options.MoonwalkEnabled = options.MoonwalkEnabled;
        runtime.Options.IconCancelEnabled = options.IconCancelEnabled;
    }

    /// <summary>
    /// Shared saved-game HUD construction for playable and cinematic checkpoints.
    /// The saved inventory supplies counters/icons; native initialization clears
    /// the persisted HUD selection before the first live gameplay update.
    /// </summary>
    private void InitializeSelectedGameHud(SuperMetroidSaveSlot slot) =>
        runtime!.HudSetup.InitializeHud(new HudSnapshot(
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
            audio.QueueMusicDelayed(MusicCommand.SelectTrack(1), MusicCommandDelay.FromDelayedYArgument(0x000e));
            audio.QueueMusicDelayed(
                MusicCommand.SelectTrackOrStop(loadedState.MusicTrackIndex),
                MusicCommandDelay.FromDelayedYArgument(0x0168 + 5));
            lastAudioRoomStatePointer = loadedState.Pointer;
            return false;
        }

        // Native new-game loading first publishes the standard HUD/OBJ transfer, then
        // loads the area-six station-zero room and finally runs SamusCode_08. This is the
        // same cartridge-backed sequence used by the room diagnostic; there is no host
        // terrain image, hand-placed Samus, or alternate playable-only initialization.
        runtime!.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
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

    /// <summary>Starts the opening cinematic in the text language the options menu selected.</summary>
    private IntroCinematicState CreateIntroCinematic(bool japaneseText)
    {
        var cinematic = new IntroCinematicState(
            bus, audio, Artwork.MapPresentation?.IntroFont, Artwork.IntroCinematicArt,
            Artwork.BeamArtwork, Artwork.SamusBodyArt, japaneseText)
        {
            ProjectileCompositions = Artwork.ProjectileCompositions,
            ProjectileFrameBindings = Artwork.ProjectileFrameBindings,
            TrailArtwork = Artwork.TrailArtwork,
            NarrationPresentation = Artwork.MapPresentation?.IntroNarration,
        };
        cinematic.BindSamusHurtColors(Artwork.MapPresentation?.SamusHurtColors);
        return cinematic;
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
    private void BeginGameplayFadeIn(bool leadsToCeresArrival)
    {
        postCeresFadeBrightness = 0;
        postCeresFadeCounter = 1;
        gameplayFadeLeadsToCeresArrival = leadsToCeresArrival;
        GameState = SuperMetroidGameState.MainGameplayFadeIn;
    }

    /// <summary>Persists the automatic Landing Site checkpoint after the gunship lands.</summary>
    private void HandleGunshipLandingSave()
    {
        if (runtime is null ||
            !AutomaticCheckpointSaver.TrySaveGunshipLanding(bus, runtime, selectedSaveSlot))
            return;
        SaveRamChanged?.Invoke();
    }

    /// <summary>Consumes a confirmed station save and writes the selected SRAM slot.</summary>
    private void HandleSaveStationPersistence()
    {
        if (runtime?.MessageBoxes.ConsumeSaveStationPersistenceRequest() is not { } request)
            return;
        SamusState samus = runtime.Player.Samus
            ?? throw new InvalidOperationException(
                "A confirmed save station has no live Samus state.");
        saveRam.SaveSlot(
            selectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(samus, runtime.System, new SaveSnapshotContext { Area = (ushort)request.AreaIndex, SaveStation = request.StationIndex, GameTime = runtime.GameTime, ControllerBindings = runtime.Options.ControllerBindings, MoonwalkEnabled = runtime.Options.MoonwalkEnabled, IconCancelEnabled = runtime.Options.IconCancelEnabled, JapaneseText = runtime.JapaneseText }));
        SaveRamChanged?.Invoke();
    }

    /// <summary>Current attract-demo set selected by the title sequence.</summary>
    private int demoSet;
    /// <summary>Next scene index within the current attract-demo set.</summary>
    internal int demoScene;
    /// <summary>Remaining scripted input frames in the active demo scene.</summary>
    internal int demoFramesRemaining;
    /// <summary>Remaining still-frame hold after a demo scene's scripted input.</summary>
    internal int demoHoldFramesRemaining;
    /// <summary>Remaining blank enemy-transfer waits before the demo becomes visible.</summary>
    private int demoLoadFramesRemaining = -1;
    /// <summary>Whether live controller input canceled the current demo.</summary>
    private bool demoCancelled;
    /// <summary>Whether another scene follows before returning to the title.</summary>
    private bool demoHasNextScene;

    /// <summary>Advances demo loading, scripted gameplay, holds, cancellation, and title return.</summary>
    private void StepAttractDemo(ushort controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        switch (GameState)
        {
            case SuperMetroidGameState.TransitionToDemoA:
                if (demoLoadFramesRemaining < 0)
                {
                    AttractDemoScene scene = StockAttractDemoScenes.Get(demoSet, demoScene)
                        ?? throw new InvalidDataException("Demo loader reached an end-of-set marker instead of a scene.");
                    // A separate runtime owns all demo progression and inventory. Never
                    // restore demo state into a selected save or publish checkpoints.
                    CreateGameplayRuntime(forAttractDemo: true);
                    runtime.AttractDemo.InitializeAttractDemo(scene);
                    demoFramesRemaining = scene.Duration;
                    demoScene++;
                    demoHoldFramesRemaining = 0;
                    demoCancelled = false;
                    demoLoadFramesRemaining = AttractDemoRomData.EnemyTransferFrames;
                    lastAudioRoomStatePointer = null;
                    lastAudioRuntimeGameplayPublication = null;
                }
                else
                {
                    runtime!.RunBlankGameplayFrame(controllerInput);
                    if (--demoLoadFramesRemaining == 0)
                        GameState = SuperMetroidGameState.TransitionToDemoB;
                }
                display.PublishBlack();
                break;

            case SuperMetroidGameState.TransitionToDemoB:
                // State $29 runs one gameplay frame before revealing the room. Its
                // extra frame does not decrement the state-$2A demo countdown.
                runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.AttractFrameAudio(runtime) });
                display.PublishGameplay(runtime);
                GameState = SuperMetroidGameState.PlayingDemo;
                break;

            case SuperMetroidGameState.PlayingDemo:
                if (demoHoldFramesRemaining != 0)
                {
                    // Native is suspended in WaitForNMI here: no actor, animation, or
                    // script advances during the ninety-frame final-image hold.
                    runtime!.NmiHandler.RunNmi(controllerInput, true);
                    if (runtime.Controller1.NewlyPressed != 0)
                        FinishAttractPlayback(cancelled: true);
                    else if (--demoHoldFramesRemaining == 0)
                        FinishAttractPlayback(cancelled: false);
                }
                else
                {
                    runtime!.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.AttractFrameAudio(runtime) });
                    display.PublishGameplay(runtime);
                    // Demo beta has restored the physical controller pair at this point.
                    if (runtime.Controller1.NewlyPressed != 0)
                        FinishAttractPlayback(cancelled: true);
                    else if (--demoFramesRemaining <= 0)
                        demoHoldFramesRemaining = AttractDemoRomData.FinalImageHoldFrames;
                }
                break;

            case SuperMetroidGameState.TransitionFromDemoA:
                demoHasNextScene = false;
                if (!demoCancelled)
                {
                    demoHasNextScene = StockAttractDemoScenes.Get(demoSet, demoScene) is not null;
                    if (!demoHasNextScene)
                    {
                        demoSet = (demoSet + 1) % AvailableDemoSetCount();
                        demoScene = 0;
                    }
                }
                ReleaseRuntimePreservingRandom();
                display.PublishBlack();
                GameState = SuperMetroidGameState.TransitionFromDemoB;
                break;

            case SuperMetroidGameState.TransitionFromDemoB:
                if (demoHasNextScene)
                {
                    demoLoadFramesRemaining = -1;
                    GameState = SuperMetroidGameState.TransitionToDemoA;
                }
                else
                {
                    if (demoCancelled)
                        title = TitleSequenceState.ReturnFromDemo(
                            bus,
                            audio,
                            Artwork.MapPresentation?.TitleGradient,
                            Artwork.MapPresentation?.TitlePalette,
                            Artwork.MapPresentation?.TitleGraphics);
                    else
                    {
                        audio.QueueMusicDelayed8(MusicCommand.Stop);
                        title = new TitleSequenceState(
                            bus,
                            audio,
                            Artwork.MapPresentation?.TitleGradient,
                            Artwork.MapPresentation?.TitlePalette,
                            Artwork.MapPresentation?.TitleGraphics);
                    }
                    GameState = SuperMetroidGameState.OpeningCinematic;
                    display.PublishMenu(title);
                }
                break;
            default:
                throw new InvalidOperationException($"{GameState} is not an attract-demo state.");
        }
    }

    /// <summary>Starts the native transition out of a completed or canceled demo scene.</summary>
    private void FinishAttractPlayback(bool cancelled)
    {
        demoCancelled = cancelled;
        GameState = SuperMetroidGameState.TransitionFromDemoA;
        display.PublishBlack();
    }

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
            $"input=${runtime.Controller1.Current:X4}; newlyPressed=${runtime.Controller1.NewlyPressed:X4}; inputLocked={samus.InputLocked}; " +
            $"drainedPhase={samus.Drained.Phase}; drainedHandler={samus.Drained.GetUpHandler}; " +
            $"escapeTimer={runtime.EscapeTimer.State}; fx={samus.LiquidPhysics.FxType}; medium={samus.LiquidPhysics.LiquidPhysicsType}; " +
            $"waterY=${samus.LiquidPhysics.FxYPosition:X4}; acidY=${samus.LiquidPhysics.LavaAcidYPosition:X4}";
    }

    // NMI continuations still owed by the current $82:8000 loading dispatch, and the
    // dispatcher state it selects after its final wait returns.
    /// <summary>Accepted blank NMI waits remaining in state-five game loading.</summary>
    private int gameLoadingWaitsRemaining;
    /// <summary>Destination to enter when game-loading waits finish.</summary>
    private GameLoadingCompletion gameLoadingCompletion;

    /// <summary>
    /// True when the next update is an NMI continuation inside the loading dispatch rather
    /// than a new main-loop dispatch, so the main loop's RNG call is absent.
    /// </summary>
    internal bool ResumesGameLoadingWait => gameLoadingWaitsRemaining > 0;

    /// <summary>
    /// Records the NMI waits the native loading dispatch performs after the host has
    /// completed the loading work itself.
    /// </summary>
    private void BeginGameLoadingWaits(int waits, GameLoadingCompletion completion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(waits);
        gameLoadingWaitsRemaining = waits;
        gameLoadingCompletion = completion;
        display.PublishBlack();
    }

    /// <summary>
    /// One native loading wait: the accepted NMI reads the controller and performs its
    /// transfers; no dispatcher runs. The last wait returns into the state store.
    /// </summary>
    private void StepGameLoadingWait(ushort controllerInput)
    {
        runtime!.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        if (--gameLoadingWaitsRemaining != 0)
        {
            display.PublishBlack();
            return;
        }
        switch (gameLoadingCompletion)
        {
            case GameLoadingCompletion.CeresArrivalFadeIn:
                BeginGameplayFadeIn(leadsToCeresArrival: true);
                display.PublishBlack();
                break;
            case GameLoadingCompletion.GameplayFadeIn:
                BeginGameplayFadeIn(leadsToCeresArrival: false);
                display.PublishBlack();
                break;
            case GameLoadingCompletion.MainGameplay:
                GameState = SuperMetroidGameState.MainGameplay;
                display.PublishGameplay(runtime);
                break;
            default:
                throw new InvalidDataException($"Invalid game-loading completion {gameLoadingCompletion}.");
        }
    }

    // State $09's door function before state $0A. Every door hit installs $82:E17D; a
    // downward elevator replaces it with the $82:E19F delay.
    /// <summary>Whether room loading is paused for the downward elevator entry wait.</summary>
    private bool waitingForDownwardsElevator;
    /// <summary>Countdown before a downward elevator resumes destination loading.</summary>
    private int downwardsElevatorDelayTimer;

    /// <summary>
    /// Runs this dispatch's state-$09 door function. Returns true when it sets carry, so
    /// state $0A runs in the same dispatch.
    /// </summary>
    private bool StepHitDoorBlockFunction(ushort controllerInput)
    {
        if (!waitingForDownwardsElevator)
        {
            // $82:E17D: an ordinary door finishes at once.
            if (runtime!.Enemies.ElevatorBehavior.ElevatorFlags == 0)
                return true;
            // Samus command 0 locks Samus; an upward elevator then finishes at once.
            runtime.Player.Samus!.InputLocked = true;
            if ((short)runtime.Enemies.ElevatorBehavior.ElevatorDirection < 0)
                return true;
            downwardsElevatorDelayTimer = ElevatorDoorDefinitions.DownwardsElevatorDelayFrames;
            waitingForDownwardsElevator = true;
        }

        // $82:E19F. The elevator gate is still clear, so the ride continues behind it.
        if (--downwardsElevatorDelayTimer < 0)
        {
            waitingForDownwardsElevator = false;
            return true;
        }
        runtime!.DoorTransition.RunDoorSoundWaitFrame(controllerInput);
        display.PublishGameplay(runtime);
        return false;
    }

    /// <summary>
    /// Rebinds host policy after an interactive debugger-state load. Cartridge state,
    /// resource totals, and the retained frame remain intact unless a tester grant is newly enabled. Exact replay must omit
    /// this override unless the recording explicitly supplies different host options.
    /// </summary>
    public void ApplyHostOptions(SuperMetroidGameOptions hostOptions)
    {
        Ensure.NotNull(hostOptions);
        runtime?.ApplyHostOptions(hostOptions);
        ConfiguredOptions = hostOptions;
    }

    /// <summary>Cartridge RNG owner before a gameplay room exists or after runtime release.</summary>
    private Bank80SystemState? menuRandom;

    /// <summary>Uses gameplay RNG when live and otherwise retains the frontend RNG word.</summary>
    internal Bank80SystemState FrontendRandomOwner =>
        runtime?.System ?? (menuRandom ??= new Bank80SystemState());

    /// <summary>Transfers RNG and NMI counters back to the frontend before dropping gameplay state.</summary>
    private void ReleaseRuntimePreservingRandom()
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
    internal bool NextUpdateResumesNmiWait => GameState switch
    {
        SuperMetroidGameState.FileSelectMenus => fileSelect?.ResumesAfterNmiWait == true,
        SuperMetroidGameState.SetUpNewGame or SuperMetroidGameState.LoadingGameData => ResumesGameLoadingWait,
        SuperMetroidGameState.CeresGoesBoom or SuperMetroidGameState.CeresGoesBoomWithSamus =>
            ceresDestruction?.ResumesAfterNmiWait == true,
        SuperMetroidGameState.EndingAndCredits => endingCredits?.ResumesAfterNmiWait == true,
        SuperMetroidGameState.GameOverMenu => gameOver?.ResumesAfterNmiWait == true,
        SuperMetroidGameState.Reset or SuperMetroidGameState.OpeningCinematic or
            SuperMetroidGameState.GameOptionsMenu or SuperMetroidGameState.Unused03 or
            SuperMetroidGameState.FileSelectMap or SuperMetroidGameState.MainGameplayFadeIn or
            SuperMetroidGameState.MainGameplay or SuperMetroidGameState.HitDoorBlock or
            SuperMetroidGameState.LoadingNextRoomA or SuperMetroidGameState.LoadingNextRoomB or
            SuperMetroidGameState.PausingDarkening or SuperMetroidGameState.Pausing or
            SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB or
            SuperMetroidGameState.UnpausingA or SuperMetroidGameState.UnpausingB or
            SuperMetroidGameState.Unpausing or SuperMetroidGameState.DeathSequenceStart or
            SuperMetroidGameState.DeathBlackOutSurroundings or SuperMetroidGameState.DeathWaitForMusic or
            SuperMetroidGameState.DeathPreFlashing or SuperMetroidGameState.DeathFlashing or
            SuperMetroidGameState.DeathExplosionWhiteOut or SuperMetroidGameState.DeathFinalBlackOut or
            SuperMetroidGameState.ReserveTanksAuto or SuperMetroidGameState.Unused1c or
            SuperMetroidGameState.DebugGameOverMenu or SuperMetroidGameState.IntroCinematic or
            SuperMetroidGameState.MadeItToCeresElevator or SuperMetroidGameState.BlackoutFromCeres or
            SuperMetroidGameState.TimeUp or SuperMetroidGameState.WhitingOutFromTimeUp or
            SuperMetroidGameState.SamusEscapesFromZebes or SuperMetroidGameState.TransitionToDemoA or
            SuperMetroidGameState.TransitionToDemoB or SuperMetroidGameState.PlayingDemo or
            SuperMetroidGameState.TransitionFromDemoA or SuperMetroidGameState.TransitionFromDemoB => false,
        _ => throw new InvalidOperationException($"Undefined SuperMetroidGameState {GameState}."),
    };

    /// <summary>
    /// Preserves the bank-$82 main-loop RNG call through title/file selection and
    /// loading. These menu dispatchers return once per frame; they do not run the
    /// gameplay runtime's prologue. SRAM contains no copy of the live RNG word.
    /// </summary>
    internal void AdvanceMenuRandom()
    {
        switch (GameState)
        {
            case SuperMetroidGameState.Reset:
                // Only the reset vector reseeds. A continued save must keep the word
                // produced by gameplay and the game-over/file-map waiting frames.
                menuRandom = new Bank80SystemState();
                menuNmi.Reset();
                // State zero is itself dispatched by MainGameLoop after its first
                // GenerateRandomNumber call; it is not the reset vector's seed store.
                menuRandom.NextRandom();
                runtime?.System.SetRandomNumber(menuRandom.RandomNumber);
                break;
            case SuperMetroidGameState.Pausing:
            // $24 fades without a gameplay call; its HDMA objects still run until it clears them.
            case SuperMetroidGameState.WhitingOutFromTimeUp:
                // HDMA runs before $0D disables it; lava swaps the live RNG bytes.
                runtime!.DoorTransition.AdvanceNonGameplayMainLoopRandom(hdmaObjectsEnabled: true);
                break;
            case SuperMetroidGameState.FileSelectMenus:
            case SuperMetroidGameState.SetUpNewGame:
            case SuperMetroidGameState.LoadingGameData:
            // $82:8B0E shares the intro's state-$1E/$22/$25 handler and its prologue call.
            case SuperMetroidGameState.CeresGoesBoom:
            case SuperMetroidGameState.CeresGoesBoomWithSamus:
            // State $27 is dispatched by MainGameLoop after its RNG call, except while the
            // ending setup resumes inside its own NMI waits.
            case SuperMetroidGameState.EndingAndCredits:
            // Game-over indexes zero and one resume inside their own NMI waits.
            case SuperMetroidGameState.GameOverMenu:
                if (!NextUpdateResumesNmiWait)
                    FrontendRandomOwner.NextRandom();
                break;
            case SuperMetroidGameState.OpeningCinematic:
            case SuperMetroidGameState.GameOptionsMenu:
            case SuperMetroidGameState.FileSelectMap:
            case SuperMetroidGameState.IntroCinematic:
            // State $19 only fades; MainGameLoop's RNG call precedes it as any dispatch.
            case SuperMetroidGameState.DeathFinalBlackOut:
            // $82:894F runs once before each pause-only dispatcher as well.
            // Fade states $0C/$12 run StepFrame and retain its own RNG call.
            case SuperMetroidGameState.PausedA:
            case SuperMetroidGameState.PausedB:
            case SuperMetroidGameState.UnpausingA:
            case SuperMetroidGameState.UnpausingB:
                FrontendRandomOwner.NextRandom();
                break;
            // The gameplay runtime, or no RNG call, owns these states.
            case SuperMetroidGameState.Unused03:
            case SuperMetroidGameState.MainGameplayFadeIn:
            case SuperMetroidGameState.MainGameplay:
            case SuperMetroidGameState.HitDoorBlock:
            case SuperMetroidGameState.LoadingNextRoomA:
            case SuperMetroidGameState.LoadingNextRoomB:
            case SuperMetroidGameState.PausingDarkening:
            case SuperMetroidGameState.Unpausing:
            case SuperMetroidGameState.DeathSequenceStart:
            case SuperMetroidGameState.DeathBlackOutSurroundings:
            case SuperMetroidGameState.DeathWaitForMusic:
            case SuperMetroidGameState.DeathPreFlashing:
            case SuperMetroidGameState.DeathFlashing:
            case SuperMetroidGameState.DeathExplosionWhiteOut:
            case SuperMetroidGameState.ReserveTanksAuto:
            case SuperMetroidGameState.Unused1c:
            case SuperMetroidGameState.DebugGameOverMenu:
            case SuperMetroidGameState.MadeItToCeresElevator:
            case SuperMetroidGameState.BlackoutFromCeres:
            case SuperMetroidGameState.TimeUp:
            case SuperMetroidGameState.SamusEscapesFromZebes:
            case SuperMetroidGameState.TransitionToDemoA:
            case SuperMetroidGameState.TransitionToDemoB:
            case SuperMetroidGameState.PlayingDemo:
            case SuperMetroidGameState.TransitionFromDemoA:
            case SuperMetroidGameState.TransitionFromDemoB:
                break;
            default:
                throw new InvalidOperationException($"Undefined SuperMetroidGameState {GameState}.");
        }
        // Gameplay retains its existing call after accepted NMI/HDMA processing.
        // Do not advance here for runtime frames or nested NMI-waiting coroutines.
    }
}
