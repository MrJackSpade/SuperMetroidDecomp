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
    private readonly ISnesAddressSpace bus;
    private readonly SuperMetroidGameOptions gameOptions;
    private readonly SuperMetroidSaveRam saveRam;
    private readonly CartridgeAudioState audio = new();
    private TitleSequenceState? title;
    private FileSelectMenuState? fileSelect;
    private GameOptionsMenuState? options;
    private GameOverMenuState? gameOver;
    private IntroCinematicState? intro;
    private CeresDestructionCinematicState? ceresDestruction;
    private EndingCreditsState? endingCredits;
    private PauseMenuState? pauseMenu;
    private SuperMetroidRuntime? runtime;
    private readonly CeresDepartureState ceresDeparture = new();
    private readonly SamusReserveAutoRecoveryState reserveRecovery = new();
    private readonly DoorTransitionState doorTransition = new();
    private Rgba32[] lastPixels = CreateBlackFrame();
    private int selectedSaveSlot;
    private bool loadingExistingSave;
    private int postCeresLoadFramesRemaining = -1;
    private byte postCeresFadeBrightness;
    private int postCeresFadeCounter = 1;
    private byte pauseBrightness = 15;
    private CartridgePaletteTransition? deathPaletteFade;
    private byte deathFadeBrightness = 15;
    private int deathFadeCounter;
    private byte endingFadeBrightness = 15;
    private int endingFadeCounter;
    private IReadOnlyList<CartridgeAudioCommand> lastAudioCommands =
        Array.Empty<CartridgeAudioCommand>();
    private CartridgeAudioAcknowledgements audioAcknowledgements;
    private ushort? lastAudioRuntimeNmiFrame;
    private ushort? lastAudioRoomStatePointer;

    public SuperMetroidGame(
        ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.gameOptions = gameOptions ?? new SuperMetroidGameOptions();
        saveRam = new SuperMetroidSaveRam(bus);
        selectedSaveSlot = saveRam.ReadSelectedSlot();
    }

    /// <summary>
    /// Raised after translated code changes battery-backed SRAM. The core owns cartridge
    /// bytes; a desktop/console host owns the choice of durable storage medium.
    /// </summary>
    public event Action? SaveRamChanged;

    /// <summary>
    /// Publishes the APU output-port bytes observed after the preceding audio frame.
    /// </summary>
    /// <remarks>
    /// Retail bank $82 waits for the SPC to echo each SFX request and its following zero
    /// before reusing that library port. A headless host may leave these at zero; music is
    /// independent, while SFX will safely remain in its acknowledgement state.
    /// </remarks>
    public void SetAudioAcknowledgements(CartridgeAudioAcknowledgements acknowledgements) =>
        audioAcknowledgements = acknowledgements;

    /// <summary>Current value of the native game-state word at WRAM $0998.</summary>
    public SuperMetroidGameState GameState { get; private set; } = SuperMetroidGameState.Reset;

    /// <summary>Wrapping frame counter corresponding to the game's 16-bit NMI counter.</summary>
    public ushort FrameNumber { get; private set; }

    /// <summary>
    /// Debugger-visible hit counter owned by the live intro actor, or zero outside that
    /// sequence. Exposing the actor's real counter keeps capture assertions out of gameplay.
    /// </summary>
    public ushort IntroMotherBrainHitCount => intro?.MotherBrainHitCount ?? 0;

    /// <summary>Live ordinary projectiles in the active intro flashback.</summary>
    public ushort IntroActiveProjectileCount => intro?.ActiveFlashbackProjectileCount ?? 0;

    /// <summary>Live fourth-hit cinematic explosion actors, or zero outside the intro.</summary>
    public int IntroMotherBrainExplosionCount => intro?.ActiveMotherBrainExplosionCount ?? 0;

    /// <summary>World X from the active SR388 intro demo, or zero before it is created.</summary>
    public ushort IntroBabyDiscoverySamusX => intro?.BabyDiscoverySamusX ?? 0;

    /// <summary>Live result of the SR388 egg actor's Samus-X proximity test.</summary>
    public bool IntroBabyDiscoveryEggHatchingStarted =>
        intro?.BabyDiscoveryEggHatchingStarted ?? false;

    /// <summary>Live shell fragments from the SR388 egg's native particle burst.</summary>
    public int IntroBabyDiscoveryEggParticleCount =>
        intro?.ActiveBabyDiscoveryEggParticleCount ?? 0;

    /// <summary>The bank-$8B Ceres-flight subphase while game state remains $1E.</summary>
    public string IntroCeresFlightPhaseName => intro?.CeresFlightPhaseName ?? string.Empty;

    /// <summary>World X owned by the live gameplay Samus, or zero before state $1F.</summary>
    public ushort GameplaySamusX => runtime?.Samus?.XPosition ?? 0;

    /// <summary>World Y owned by the live gameplay Samus, or zero before state $1F.</summary>
    public ushort GameplaySamusY => runtime?.Samus?.YPosition ?? 0;

    /// <summary>Live layer-1 camera Y, exposed for debugger watches and ROM smoke captures.</summary>
    public ushort GameplayCameraY => runtime?.Camera?.YPosition ?? 0;

    /// <summary>The BG1/M7 vertical-scroll mirror published from the gameplay camera.</summary>
    public ushort GameplayBg1VerticalScroll => runtime?.BackgroundScroll.Bg1VerticalScroll ?? 0;

    /// <summary>Ordinary tilemap uploads produced by the most recent gameplay scroll pass.</summary>
    public int GameplayBackgroundUpdateCount => runtime?.LastBackgroundUpdateCount ?? 0;

    /// <summary>Live ordinary beam/missile slots in the gameplay projectile owner.</summary>
    public ushort GameplayProjectileCount => runtime?.Projectiles.ProjectileCounter ?? 0;

    /// <summary>Most recent projectile slot allocated by the gameplay alpha handler.</summary>
    public int? GameplayLastFiredProjectileSlot => runtime?.Projectiles.LastFrameResult.FiredSlot;

    /// <summary>The live cartridge pose byte, or zero before the gameplay runtime exists.</summary>
    public byte GameplaySamusPose => runtime?.Samus?.Pose ?? 0;

    /// <summary>Whether the Ceres elevator has restored ordinary player movement.</summary>
    public bool GameplayMovementEnabled => runtime?.GroundedSamusMovementEnabled ?? false;

    /// <summary>Current bank-$8F room header pointer, exposed for door-transition watches.</summary>
    public ushort? GameplayActiveRoomPointer => runtime?.ActiveRoom?.Pointer;

    /// <summary>Selected bank-$8F room-state pointer, exposed beside the room identity.</summary>
    public ushort? GameplayActiveRoomStatePointer => runtime?.ActiveRoom?.State.Pointer;

    /// <summary>Native area byte in the active room header, or null before gameplay.</summary>
    public byte? GameplayActiveAreaIndex => runtime?.ActiveRoom?.AreaIndex;

    /// <summary>Native room index within the active area, or null before gameplay.</summary>
    public byte? GameplayActiveRoomIndex => runtime?.ActiveRoom?.RoomIndex;

    /// <summary>Current bank-$83 entry door pointer, exposed for door-transition watches.</summary>
    public ushort? GameplayActiveDoorPointer => runtime?.ActiveDoor?.Pointer;

    /// <summary>
    /// Read-only access point for friend verification assemblies that need to observe the
    /// complete cartridge-owned room graph while sending input through <see cref="Step"/>.
    /// Production hosts never use this reference, and route audits must not invoke runtime
    /// mutators through it; its purpose is to avoid manufacturing hundreds of writable
    /// frontend proxy properties merely for collision-aware controller planning.
    /// </summary>
    internal SuperMetroidRuntime? RuntimeForVerification => runtime;

    /// <summary>
    /// Exposes the cartridge coroutine phase only to the friend verification/debug hosts.
    /// Production UI code continues to consume the coarser public game state.
    /// </summary>
    internal DoorTransitionPhase DoorTransitionPhaseForVerification => doorTransition.Phase;

    /// <summary>Live equipped-item word, including Morph Ball and Bomb bits.</summary>
    public ushort GameplayEquippedItems => runtime?.Samus?.EquippedItems ?? 0;

    /// <summary>Live collected-item word displayed by the pause equipment screen.</summary>
    public ushort GameplayCollectedItems => runtime?.Samus?.CollectedItems ?? 0;

    /// <summary>Live energy restored from SRAM and consumed by gameplay damage.</summary>
    public ushort GameplayHealth => runtime?.Samus?.Health ?? 0;

    /// <summary>Saved maximum-energy word restored alongside current energy.</summary>
    public ushort GameplayMaxHealth => runtime?.Samus?.MaxHealth ?? 0;

    /// <summary>Saved gameplay minutes, exposed for frontend reload regression watches.</summary>
    public ushort GameplayTimeMinutes => runtime?.GameTime.Minutes ?? 0;

    /// <summary>Saved gameplay hours, exposed for frontend reload regression watches.</summary>
    public ushort GameplayTimeHours => runtime?.GameTime.Hours ?? 0;

    /// <summary>Saved gameplay seconds, exposed for frontend reload regression watches.</summary>
    public ushort GameplayTimeSeconds => runtime?.GameTime.Seconds ?? 0;

    /// <summary>Saved subsecond gameplay frame word.</summary>
    public ushort GameplayTimeFrames => runtime?.GameTime.Frames ?? 0;

    /// <summary>Queries one live area-boss mask without exposing writable system arrays.</summary>
    public bool GameplayHasBossBits(int areaIndex, BossBits bits) =>
        runtime?.System.HasAnyBossBits(areaIndex, bits) ?? false;

    /// <summary>Queries a restored explored-map cell through the native 64-by-32 layout.</summary>
    public bool GameplayIsMapTileExplored(int areaIndex, int mapX, int mapY) =>
        runtime?.System.IsMapTileExplored(areaIndex, mapX, mapY) ?? false;

    /// <summary>Current pause page: zero for map, one for equipment, or -1 outside pause.</summary>
    public int PauseScreenMode => pauseMenu?.ScreenMode ?? -1;

    /// <summary>Equipment selector category, matching the low byte of native word $0754.</summary>
    public int PauseSelectedEquipmentCategory => pauseMenu?.SelectedCategory ?? -1;

    /// <summary>Equipment selector item, matching the high byte of native word $0754.</summary>
    public int PauseSelectedEquipmentItem => pauseMenu?.SelectedItem ?? -1;

    /// <summary>Runs one dispatcher frame and returns the PPU-visible result.</summary>
    public FrontendFrame Step(ushort controllerInput)
    {
        FrameNumber++;
        switch (GameState)
        {
            case SuperMetroidGameState.Reset:
                // `Vector_RESET_Async` ultimately stores state one and initializes
                // `cinematic_function` to `CinematicFunctionOpening` at $8B:9B68.
                audio.Reset();
                title = new TitleSequenceState(bus, audio);
                lastAudioRuntimeNmiFrame = null;
                lastAudioRoomStatePointer = null;
                fileSelect = null;
                gameOver = null;
                GameState = SuperMetroidGameState.OpeningCinematic;
                lastPixels = title.Render();
                break;

            case SuperMetroidGameState.OpeningCinematic:
                title!.Step(controllerInput);
                lastPixels = title.Render();
                if (title.FileSelectRequested)
                {
                    // `$8B:9F52` sets game_state=4 only after the slow fade reaches black.
                    fileSelect = new FileSelectMenuState(bus, audio);
                    GameState = SuperMetroidGameState.FileSelectMenus;
                    lastPixels = fileSelect.Render();
                }
                break;

            case SuperMetroidGameState.FileSelectMenus:
                fileSelect!.Step(controllerInput);
                if (fileSelect.SaveRamChangedThisFrame)
                    SaveRamChanged?.Invoke();
                lastPixels = fileSelect.Render();
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
                        optionSlot?.MoonwalkEnabled ?? false);
                    GameState = SuperMetroidGameState.GameOptionsMenu;
                    lastPixels = options.Render();
                }
                break;

            case SuperMetroidGameState.GameOptionsMenu:
                options!.Step(controllerInput);
                lastPixels = options.Render();
                if (options.FileSelectRequested)
                {
                    fileSelect = new FileSelectMenuState(bus, audio);
                    GameState = SuperMetroidGameState.FileSelectMenus;
                    lastPixels = fileSelect.Render();
                }
                else if (options.IntroRequested)
                {
                    if (loadingExistingSave || gameOptions.SkipOpeningCinematic)
                    {
                        // This is the same dispatcher boundary reached by `$8B:C100` after
                        // the SPACE COLONY fade. Do not fake Start presses or build a host-
                        // authored Ceres room: the following frame will execute the ordinary
                        // new-game loader, elevator arrival, and movement-unlock sequence.
                        intro = null;
                        GameState = SuperMetroidGameState.SetUpNewGame;
                        lastPixels = CreateBlackFrame();
                    }
                    else
                    {
                        intro = new IntroCinematicState(bus, audio);
                        GameState = SuperMetroidGameState.IntroCinematic;
                        lastPixels = intro.Render();
                    }
                }
                break;

            case SuperMetroidGameState.IntroCinematic:
                intro!.Step(controllerInput);
                lastPixels = intro.Render();
                if (intro.CeresFlightFinished)
                {
                    // `$8B:C100` writes state $1F, area six, and load-station zero only
                    // after the SPACE COLONY fade has reached forced blank. Preserve the
                    // intervening dispatcher state instead of constructing a debug room
                    // directly from cinematic code.
                    GameState = SuperMetroidGameState.SetUpNewGame;
                    lastPixels = CreateBlackFrame();
                }
                break;

            case SuperMetroidGameState.SetUpNewGame:
                bool usesCeresArrival = SetupSelectedGame();
                GameState = usesCeresArrival
                    ? SuperMetroidGameState.MadeItToCeresElevator
                    : SuperMetroidGameState.MainGameplay;
                lastPixels = usesCeresArrival
                    ? CreateBlackFrame()
                    : SuperMetroidRuntimeFrameRenderer.Render(runtime!);
                break;

            case SuperMetroidGameState.MadeItToCeresElevator:
                runtime!.StepFrame(
                    controllerInput,
                    allowCeresElevatorDeparture: false);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                if (ceresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator)
                {
                    if (ceresDeparture.StepHoldAfterGameplay())
                        GameState = SuperMetroidGameState.BlackoutFromCeres;
                }
                else if (runtime.GroundedSamusMovementEnabled)
                {
                    // The bank-$86 elevator objects restore Samus's ordinary frame
                    // handler only after the native 60-frame wait and 72-pixel descent.
                    GameState = SuperMetroidGameState.MainGameplay;
                }
                break;

            case SuperMetroidGameState.MainGameplay:
                runtime!.StepFrame(controllerInput);
                HandleSaveStationPersistence();
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                HandleGunshipLandingSave();
                if (RouteOutOfHealth())
                {
                    // `$82:DB69` publishes the next outer state at the end of this already-
                    // completed gameplay call. Nothing else may replace it on the trigger
                    // frame—not pause, an elevator handoff, or a pending ordinary door.
                }
                else if (runtime.Enemies.LastGunshipEvent == GunshipFrameEvent.EscapeTakeoffCompleted)
                {
                    // Gunship function $A2:AD0E writes game state $26 only after the top
                    // hull has crossed Y=$0100. State eight has already completed on this
                    // publication frame; the following call owns the gameplay fade.
                    endingFadeBrightness = 15;
                    endingFadeCounter = 0;
                    runtime.GameplayTimeFrozen = true;
                    GameState = SuperMetroidGameState.SamusEscapesFromZebes;
                }
                else if (CanEnterPause())
                {
                    // Samus_PauseCheck at `$90:EA45` executes during the already-completed
                    // state-eight frame. It initializes both fade counters and publishes
                    // state $0C; the following dispatcher call performs the first darken.
                    pauseBrightness = 15;
                    pauseMenu = null;
                    GameState = SuperMetroidGameState.PausingDarkening;
                }
                else if (runtime.LastCeresElevatorShaftRoomMain.DepartureRequestedThisFrame)
                {
                    // `$89:ACC3` has already selected standing pose, locked Samus, and
                    // published game state $20. The following dispatcher call owns the
                    // 60-frame hold; do not decrement it on this trigger frame.
                    ceresDeparture.Begin();
                    GameState = SuperMetroidGameState.MadeItToCeresElevator;
                }
                else if (runtime.HasPendingDoorTransition)
                {
                    // `$94:938B/$93CE` changes WRAM game_state during the gameplay call.
                    // The already-produced gameplay image remains this frame's image; the
                    // following dispatcher call begins state `$09` from that publication.
                    GameState = SuperMetroidGameState.HitDoorBlock;
                }
                break;

            case SuperMetroidGameState.ReserveTanksAuto:
                SamusReserveAutoRecoveryStep reserveStep = default;
                runtime!.StepFrame(
                    controllerInput,
                    afterAcceptedNmi: () =>
                    {
                        SamusState samus = runtime.Samus
                            ?? throw new InvalidOperationException(
                                "Reserve recovery requires a live Samus state.");
                        reserveStep = reserveRecovery.StepAfterNmi(
                            samus,
                            runtime.NmiFrameCounter);
                    });
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                if (reserveStep.RefillSoundRequested)
                    audio.QueueSound(library: 3, soundId: 0x2d, maximumQueued: 3);
                if (reserveStep.Completed)
                {
                    runtime.GameplayTimeFrozen = false;
                    GameState = SuperMetroidGameState.MainGameplay;
                }
                break;

            case SuperMetroidGameState.DeathSequenceStart:
                // State $13 deliberately completes one final state-eight pass under the
                // global freeze word, then snapshots the visible palette and makes every
                // target row black except Samus's sixteen-color suit row.
                runtime!.StepFrame(controllerInput, advanceGameTime: false);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                PrepareDeathPaletteFade();
                SamusState dyingSamus = runtime.Samus
                    ?? throw new InvalidOperationException("Death sequence lost its Samus owner.");
                dyingSamus.SelectedHudItem = 0;
                dyingSamus.AutoCancelHudItemIndex = 0;
                dyingSamus.InvincibilityTimer = 0;
                dyingSamus.KnockbackTimer = 0;
                dyingSamus.KnockbackActive = false;
                GameState = SuperMetroidGameState.DeathBlackOutSurroundings;
                break;

            case SuperMetroidGameState.DeathBlackOutSurroundings:
                runtime!.StepFrame(controllerInput, advanceGameTime: false);
                bool paletteBlackoutComplete = StepDeathPaletteFade();
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                if (paletteBlackoutComplete)
                {
                    // `$82:DCE0` cancels all three SFX libraries, then installs the death
                    // music data and track through the ordinary delayed music queue.
                    audio.QueueSound(library: 1, soundId: 0x02, maximumQueued: 15);
                    audio.QueueSound(library: 2, soundId: 0x71, maximumQueued: 15);
                    audio.QueueSound(library: 3, soundId: 0x01, maximumQueued: 15);
                    audio.QueueMusicDelayed8(0);
                    audio.QueueMusicDelayed8(0xff39);
                    audio.QueueMusicDelayed(5, 0x000e);
                    GameState = SuperMetroidGameState.DeathWaitForMusic;
                }
                break;

            case SuperMetroidGameState.DeathWaitForMusic:
                runtime!.DrawFatalSamusFrame(controllerInput);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                if (!audio.HasQueuedMusic)
                {
                    runtime.BeginDeathSequenceAfterMusicWait();
                    GameState = SuperMetroidGameState.DeathPreFlashing;
                }
                break;

            case SuperMetroidGameState.DeathPreFlashing:
            case SuperMetroidGameState.DeathFlashing:
            case SuperMetroidGameState.DeathExplosionWhiteOut:
                SamusDeathSequenceStepResult deathStep =
                    runtime!.StepDeathSequenceFrame(controllerInput);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
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
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
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
                MasterBrightnessFilter.Apply(lastPixels, deathFadeBrightness);
                if (deathFadeBrightness == 0)
                {
                    runtime.GameplayTimeFrozen = false;
                    GameState = SuperMetroidGameState.GameOverMenu;
                }
                break;

            case SuperMetroidGameState.GameOverMenu:
                gameOver ??= new GameOverMenuState(bus, audio);
                gameOver.Step(controllerInput);
                lastPixels = gameOver.Render();
                if (gameOver.ContinueRequested)
                {
                    // Native menu index six publishes state $05 after the fully black
                    // frame. Keep that dispatcher boundary separate from SRAM reload.
                    gameOver = null;
                    GameState = SuperMetroidGameState.FileSelectMap;
                    lastPixels = CreateBlackFrame();
                }
                else if (gameOver.TitleRequested)
                {
                    // GameOverMenu_7 invokes SoftReset only after its fade reaches black.
                    gameOver = null;
                    GameState = SuperMetroidGameState.Reset;
                    lastPixels = CreateBlackFrame();
                }
                break;

            case SuperMetroidGameState.FileSelectMap:
                // The game-over route has already selected and loaded the active slot in
                // native WRAM. Recreate that load from the same checksummed SRAM image, then
                // enter the ordinary gameplay fade rather than retaining the dead runtime.
                loadingExistingSave = true;
                bool continueUsesCeresArrival = SetupSelectedGame();
                if (continueUsesCeresArrival)
                {
                    GameState = SuperMetroidGameState.MadeItToCeresElevator;
                    lastPixels = CreateBlackFrame();
                }
                else
                {
                    postCeresFadeBrightness = 0;
                    postCeresFadeCounter = 1;
                    GameState = SuperMetroidGameState.MainGameplayFadeIn;
                    lastPixels = CreateBlackFrame();
                }
                break;

            case SuperMetroidGameState.PausingDarkening:
                // State $0C continues running ordinary gameplay while INIDISP darkens.
                // This matters for moving enemies/projectiles and is why pause cannot be
                // represented as a desktop-only frozen bitmap.
                runtime!.StepFrame(controllerInput);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                pauseBrightness = (byte)Math.Max(0, pauseBrightness - 1);
                MasterBrightnessFilter.Apply(lastPixels, pauseBrightness);
                if (pauseBrightness == 0)
                    GameState = SuperMetroidGameState.Pausing;
                break;

            case SuperMetroidGameState.Pausing:
                // `$82:8CEF` owns the force-blank setup frame: pause tiles, base maps,
                // palette, inventory labels, and PPU bases are installed before state $0E.
                // Keep accepting NMI so the controller's previous sample remains truthful.
                runtime!.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                CartridgeRoomHeader pauseRoom = runtime.ActiveRoom ??
                    throw new InvalidOperationException(
                        "Pause setup requires an active cartridge room header.");
                pauseMenu = new PauseMenuState(
                    bus,
                    runtime.Samus ?? throw new InvalidOperationException(
                        "Pause setup requires a live Samus state."),
                    runtime.System,
                    pauseRoom.AreaIndex,
                    pauseRoom.MapX,
                    pauseRoom.MapY,
                    audio,
                    runtime.Vram);
                pauseBrightness = 0;
                lastPixels = pauseMenu.Render();
                MasterBrightnessFilter.Apply(lastPixels, pauseBrightness);
                GameState = SuperMetroidGameState.PausedA;
                break;

            case SuperMetroidGameState.PausedA:
                runtime!.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                pauseBrightness = (byte)Math.Min(15, pauseBrightness + 1);
                pauseMenu!.AdvanceAnimations();
                lastPixels = pauseMenu!.Render();
                MasterBrightnessFilter.Apply(lastPixels, pauseBrightness);
                if (pauseBrightness == 15)
                    GameState = SuperMetroidGameState.PausedB;
                break;

            case SuperMetroidGameState.PausedB:
                runtime!.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                runtime.UpdatePauseHeldInput();
                bool unpauseRequested = pauseMenu!.Step(
                    runtime.System.TimedHeldInput,
                    runtime.Controller1.NewlyPressed);
                lastPixels = pauseMenu.Render();
                if (unpauseRequested)
                {
                    pauseBrightness = 15;
                    GameState = SuperMetroidGameState.UnpausingA;
                }
                break;

            case SuperMetroidGameState.UnpausingA:
                runtime!.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                pauseMenu!.AdvanceAnimations();
                lastPixels = pauseMenu!.Render();
                pauseBrightness = (byte)Math.Max(0, pauseBrightness - 1);
                MasterBrightnessFilter.Apply(lastPixels, pauseBrightness);
                if (pauseBrightness == 0)
                    GameState = SuperMetroidGameState.UnpausingB;
                break;

            case SuperMetroidGameState.UnpausingB:
                // Native state $11 restores gameplay PPU state, BG2, beam tiles, palette,
                // hooks, HDMA, and animtiles under forced blank. Pause graphics live in a
                // separate PPU image here, so discarding it restores the untouched runtime
                // image without a host-authored reconstruction.
                runtime!.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                pauseMenu = null;
                pauseBrightness = 0;
                lastPixels = CreateBlackFrame();
                GameState = SuperMetroidGameState.Unpausing;
                break;

            case SuperMetroidGameState.Unpausing:
                // State $12 resumes the full state-eight loop behind an INIDISP fade.
                runtime!.StepFrame(controllerInput);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                pauseBrightness = (byte)Math.Min(15, pauseBrightness + 1);
                MasterBrightnessFilter.Apply(lastPixels, pauseBrightness);
                if (pauseBrightness == 15)
                    GameState = SuperMetroidGameState.MainGameplay;
                break;

            case SuperMetroidGameState.BlackoutFromCeres:
                runtime!.StepFrame(
                    controllerInput,
                    allowCeresElevatorDeparture: false);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                bool ceresReachedForcedBlank = ceresDeparture.StepFadeAfterGameplay();
                ceresDeparture.ApplyBrightness(lastPixels);
                if (ceresReachedForcedBlank)
                {
                    // GameState_33 saves the live area-six/station-zero slot only after
                    // INIDISP reaches forced blank. This is the durable post-Ridley Ceres
                    // checkpoint the retail loader later replaces with Zebes arrival.
                    SamusState samus = runtime.Samus
                        ?? throw new InvalidOperationException(
                            "Ceres blackout completed without a live Samus state.");
                    saveRam.SaveSlot(
                        selectedSaveSlot,
                        SuperMetroidSaveSnapshot.Capture(
                            samus,
                            runtime.System,
                            area: 6,
                            saveStation: 0,
                            gameTime: runtime.GameTime,
                            controllerBindings: runtime.ControllerBindings,
                            moonwalkEnabled: runtime.MoonwalkEnabled,
                            iconCancelEnabled: runtime.IconCancelEnabled));
                    SaveRamChanged?.Invoke();
                    runtime.Enemies.CeresStatus = 0;
                    runtime.EscapeTimer.Clear();
                    ceresDestruction = new CeresDestructionCinematicState(bus, audio);
                    GameState = SuperMetroidGameState.CeresGoesBoom;
                    lastPixels = CreateBlackFrame();
                }
                break;

            case SuperMetroidGameState.CeresGoesBoom:
                ceresDestruction ??= new CeresDestructionCinematicState(bus, audio);
                ceresDestruction.Step();
                lastPixels = ceresDestruction.Render();
                if (ceresDestruction.Finished)
                {
                    // CADF forces blank and publishes state six. The loader's special
                    // `$22` branch—not cinematic code—selects area zero/station eighteen.
                    GameState = SuperMetroidGameState.LoadingGameData;
                    postCeresLoadFramesRemaining = -1;
                    lastPixels = CreateBlackFrame();
                }
                break;

            case SuperMetroidGameState.LoadingGameData:
                if (postCeresLoadFramesRemaining < 0)
                {
                    runtime!.InitializePostCeresZebesRoom();
                    // `$82:80FB` performs fifteen enemy-tile transfer/NMI waits for this
                    // branch, versus six for an ordinary saved-game load.
                    postCeresLoadFramesRemaining = 15;
                }
                else if (--postCeresLoadFramesRemaining <= 0)
                {
                    postCeresFadeBrightness = 0;
                    postCeresFadeCounter = 1;
                    GameState = SuperMetroidGameState.MainGameplayFadeIn;
                }
                lastPixels = CreateBlackFrame();
                break;

            case SuperMetroidGameState.MainGameplayFadeIn:
                runtime!.StepFrame(controllerInput);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                MasterBrightnessFilter.Apply(lastPixels, postCeresFadeBrightness);
                HandleGunshipLandingSave();
                if (postCeresFadeCounter-- <= 0)
                {
                    postCeresFadeCounter = 1;
                    postCeresFadeBrightness = (byte)Math.Min(
                        15,
                        postCeresFadeBrightness + 1);
                    if (postCeresFadeBrightness == 15)
                        GameState = SuperMetroidGameState.MainGameplay;
                }
                break;

            case SuperMetroidGameState.HitDoorBlock:
                // A non-elevator type-$9 door enters `$82:E17D`, which immediately advances
                // through state $0A into the state-$0B transition coroutine. Before that
                // transition, $84:8250 calls Samus code $1D and queues library-two $71 so
                // movement/charge loops terminate rather than leaking into the next room.
                SamusState doorSamus = runtime!.Samus
                    ?? throw new InvalidOperationException("Door transition requires live Samus state.");
                byte doorMovementType = doorSamus.ReadMovementType(bus);
                if (doorMovementType is 3 or 20)
                {
                    audio.QueueSound(library: 1, soundId: 0x32, maximumQueued: 15);
                }
                else if ((controllerInput & (ushort)SnesButton.X) == 0 &&
                    runtime.Projectiles.FlareCounter < 16)
                {
                    audio.QueueSound(library: 1, soundId: 0x02, maximumQueued: 15);
                }
                audio.QueueSound(library: 2, soundId: 0x71, maximumQueued: 15);
                doorTransition.Begin(runtime);
                // State $09 calls state $0A synchronously for ordinary doors; state $0A
                // publishes state $0B before returning. Consequently neither intermediate
                // numeric value owns a separately displayed frame.
                GameState = SuperMetroidGameState.LoadingNextRoomB;
                break;

            case SuperMetroidGameState.LoadingNextRoomB:
                doorTransition.Step(runtime!, audio, controllerInput);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime!);
                if (doorTransition.Phase == DoorTransitionPhase.Complete)
                    GameState = SuperMetroidGameState.MainGameplay;
                break;

            case SuperMetroidGameState.SamusEscapesFromZebes:
                // State $26 calls the complete state-eight gameplay coroutine before
                // HandleFadeOut. The fleeing gunship, room animations, and APU publishers
                // therefore continue behind every darkening frame.
                runtime!.StepFrame(controllerInput, advanceGameTime: false);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                if (endingFadeCounter-- <= 0)
                {
                    endingFadeCounter = 1;
                    endingFadeBrightness = (byte)Math.Max(0, endingFadeBrightness - 1);
                }
                MasterBrightnessFilter.Apply(lastPixels, endingFadeBrightness);
                if (endingFadeBrightness == 0)
                {
                    // $82:84D3-$8527 resets PPU ownership, stops the escape timer/music,
                    // cancels all three SFX libraries, and publishes state $27. The new
                    // state performs its own cartridge-backed setup on the following call.
                    audio.QueueMusicDelayed8(0);
                    audio.QueueSound(library: 1, soundId: 0x02, maximumQueued: 15);
                    audio.QueueSound(library: 2, soundId: 0x71, maximumQueued: 15);
                    audio.QueueSound(library: 3, soundId: 0x01, maximumQueued: 15);
                    SamusState endingSamus = runtime.Samus ??
                        throw new InvalidOperationException(
                            "Ending inventory calculation requires the live Samus state.");
                    endingCredits = new EndingCreditsState(
                        bus,
                        audio,
                        runtime.GameTime.Hours,
                        runtime.GameTime.Minutes,
                        new EndingInventorySnapshot(
                            endingSamus.MaxHealth,
                            endingSamus.MaxReserveEnergy,
                            endingSamus.MaxMissiles,
                            endingSamus.MaxSuperMissiles,
                            endingSamus.MaxPowerBombs,
                            endingSamus.CollectedItems,
                            endingSamus.CollectedBeams),
                        runtime.JapaneseText);
                    GameState = SuperMetroidGameState.EndingAndCredits;
                    lastPixels = CreateBlackFrame();
                }
                break;

            case SuperMetroidGameState.EndingAndCredits:
                endingCredits!.Step();
                lastPixels = endingCredits.Render();
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

        CollectTranslatedAudioRequests();
        lastAudioCommands = audio.AdvanceFrame(bus, audioAcknowledgements);
        return CurrentFrame;
    }

    /// <summary>Last completed frame, useful for repainting without advancing emulation.</summary>
    public FrontendFrame CurrentFrame =>
        new(GameState, PhaseName, FrameNumber, lastPixels, lastAudioCommands);

    /// <summary>
    /// Moves already-translated per-frame publishers into the single cartridge queue.
    /// </summary>
    /// <remarks>
    /// This is intentionally a collection seam, not sound policy. Samus, projectiles, and
    /// PLMs retain ownership of the exact library/ID/cap chosen by their ROM routines; the
    /// frontend merely performs the global QueueSfx call those producers requested.
    /// </remarks>
    private void CollectTranslatedAudioRequests()
    {
        if (runtime?.ActiveRoom?.State is { } roomState &&
            roomState.Pointer != lastAudioRoomStatePointer)
        {
            audio.QueueRoomMusic(roomState.MusicDataIndex, roomState.MusicTrackIndex);
            lastAudioRoomStatePointer = roomState.Pointer;
        }

        // Frontend states sometimes render several frames without calling runtime.StepFrame.
        // Consume publishers only when the runtime's NMI count changes, or their most recent
        // one-frame request list would be incorrectly enqueued again by a transition frame.
        if (runtime is null || runtime.NmiFrameCounter == lastAudioRuntimeNmiFrame)
            return;
        lastAudioRuntimeNmiFrame = runtime.NmiFrameCounter;

        if (runtime.Samus is { } samus)
        {
            if (runtime.MessageBoxSelectionSoundRequestedThisFrame)
                audio.QueueSound(library: 1, soundId: 0x37, maximumQueued: 6);

            if (runtime.Hud.SelectionSoundRequestedThisFrame)
                audio.QueueSound(library: 1, soundId: 0x39, maximumQueued: 6);

            foreach (SamusSoundRequest request in samus.LiquidPhysics.SoundRequests)
                audio.QueueSound(request.Library, request.SoundId, request.MaximumQueued);

            // These state machines predate the shared SamusSoundRequest list. Consume their
            // one-shot publications here, retaining the exact native library and MaxN entry.
            if (samus.HorizontalSpeed.ConsumeEchoSoundRequest())
                audio.QueueSound(library: 3, soundId: 0x03, maximumQueued: 6);
            if (samus.Shinespark.ConsumeStoredShineWarningSoundRequest())
                audio.QueueSound(library: 3, soundId: 0x0c, maximumQueued: 9);
            if (samus.Shinespark.ConsumeLaunchSoundRequest())
                audio.QueueSound(library: 3, soundId: 0x0f, maximumQueued: 9);
            if (samus.Shinespark.ConsumeCrashSoundRequest())
            {
                audio.QueueSound(library: 1, soundId: 0x35, maximumQueued: 6);
                audio.QueueSound(library: 3, soundId: 0x10, maximumQueued: 6);
            }
            if (samus.CrystalFlash.ConsumeActivationSoundRequest())
                audio.QueueSound(library: 3, soundId: 0x01, maximumQueued: 15);
            if (samus.Xray.ConsumeActivationSoundRequest())
                audio.QueueSound(library: 1, soundId: 0x09, maximumQueued: 6);
            if (samus.Xray.ConsumeDeactivationSoundRequest())
                audio.QueueSound(library: 1, soundId: 0x0a, maximumQueued: 6);
            if (samus.DeathSequence.ConsumeSpinJumpSoundRequest())
                audio.QueueSound(library: 1, soundId: 0x32, maximumQueued: 6);
        }

        ushort projectileSound = runtime.Projectiles.LastFrameResult.QueuedSoundEffect;
        if (projectileSound != 0)
        {
            audio.QueueSound(
                library: 1,
                unchecked((byte)projectileSound),
                runtime.Projectiles.LastFrameResult.QueuedSoundMaximum);
        }

        foreach (PlmSoundRequest request in runtime.Plms.SoundRequests)
            audio.QueueSound(request.Library, request.SoundId, request.MaximumQueued);
        foreach (PlmMusicRequest request in runtime.Plms.MusicRequests)
            audio.QueueMusicDelayed(request.Track, request.DelayFrames);
        if (runtime.Plms.ConsumeCollectibleFanfareRequest())
            audio.QueuePermanentItemFanfare();

        foreach (EnemySoundRequest request in runtime.Enemies.SoundRequests)
            audio.QueueSound(request.Library, request.SoundId, request.MaximumQueued);
        foreach (EnemyMusicRequest request in runtime.Enemies.MusicRequests)
            audio.QueueMusicDelayed(request.Entry, request.DelayFrames);
    }

    private string PhaseName => GameState switch
    {
        SuperMetroidGameState.Reset => "Reset",
        SuperMetroidGameState.OpeningCinematic => title!.Phase.ToString(),
        SuperMetroidGameState.FileSelectMenus => fileSelect!.Phase.ToString(),
        SuperMetroidGameState.GameOptionsMenu => options!.Phase.ToString(),
        SuperMetroidGameState.IntroCinematic => intro!.Phase.ToString(),
        SuperMetroidGameState.SetUpNewGame =>
            loadingExistingSave ? "Loading saved game" : "Loading fresh Ceres game",
        SuperMetroidGameState.MadeItToCeresElevator =>
            ceresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator
                ? $"Ceres elevator departure ({ceresDeparture.HoldFramesRemaining})"
                : runtime?.CeresElevatorArrival is { IsComplete: false }
                ? "Ceres elevator arrival"
                : "Ceres controls unlocked",
        SuperMetroidGameState.BlackoutFromCeres =>
            $"Ceres blackout (brightness {ceresDeparture.Brightness})",
        SuperMetroidGameState.CeresGoesBoom =>
            $"Ceres destruction: {ceresDestruction?.Phase}",
        SuperMetroidGameState.LoadingGameData =>
            $"Loading Landing Site ({Math.Max(0, postCeresLoadFramesRemaining)} transfers)",
        SuperMetroidGameState.MainGameplayFadeIn =>
            $"Landing Site fade-in (brightness {postCeresFadeBrightness})",
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
        _ => GameState.ToString(),
    };

    private bool CanEnterPause()
    {
        if (runtime?.Samus is not SamusState samus || runtime.ActiveRoom is null)
            return false;

        // This is the complete retail predicate at `$90:EA45` for the translated owners.
        // HasPendingDoorTransition represents the enemies/door transition flag, X-ray owns
        // time freeze, and the power-bomb system owns $0CE2. Area six (Ceres) is excluded.
        return runtime.PowerBombExplosionStatus == 0 &&
               !samus.Xray.TimeIsFrozen &&
               !runtime.HasPendingDoorTransition &&
               runtime.ActiveRoom.AreaIndex != 6 &&
               (runtime.Controller1.NewlyPressed & (ushort)SnesButton.Start) != 0;
    }

    /// <summary>
    /// Ports the state-publication half of <c>HandleSamusOutOfHealthAndGameTile</c>. Runtime
    /// state eight has already completed its room-main and clock work when this is called.
    /// </summary>
    private bool RouteOutOfHealth()
    {
        if (runtime?.Samus is not SamusState samus || unchecked((short)samus.Health) > 0)
            return false;

        runtime.GameplayTimeFrozen = true;
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

    /// <summary>Builds state $13's target-palettes image and resets <c>$7E:C400</c>.</summary>
    private void PrepareDeathPaletteFade()
    {
        if (runtime is null)
            throw new InvalidOperationException("Death palette setup requires a runtime.");

        var target = new ushort[SnesCgram.ColorCount];
        ReadOnlySpan<ushort> current = runtime.Cgram.Colors;
        // `$82:DCA3-$DCB3` restores only palette-buffer colors $C0-$CF into the otherwise
        // black target. Suitless row $F0 and all room/OBJ rows therefore fade away.
        current.Slice(0xc0, 0x10).CopyTo(target.AsSpan(0xc0, 0x10));
        deathPaletteFade = new CartridgePaletteTransition(target, denominator: 6);
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

    private bool SetupSelectedGame()
    {
        runtime = new SuperMetroidRuntime(bus);
        runtime.JapaneseText = options?.JapaneseText ?? false;
        runtime.ControllerBindings = options?.ControllerBindings ?? ControllerBindings.Default;
        runtime.MoonwalkEnabled = options?.MoonwalkEnabled ?? false;
        runtime.IconCancelEnabled = options?.IconCancelEnabled ?? false;

        if (loadingExistingSave)
        {
            SuperMetroidSaveSlot slot = saveRam.ReadSlot(selectedSaveSlot)
                ?? throw new InvalidDataException(
                    $"Selected save slot {selectedSaveSlot} became invalid during startup.");
            runtime.InitializeHud(new HudSnapshot(
                slot.Health,
                slot.MaxHealth,
                slot.Missiles,
                slot.MaxMissiles,
                slot.SuperMissiles,
                slot.MaxSuperMissiles,
                slot.PowerBombs,
                slot.MaxPowerBombs,
                slot.EquippedItems,
                slot.HudItem,
                slot.ReserveEnergy,
                slot.ReserveMode));
            runtime.GameTime.Load(
                slot.GameTimeFrames,
                slot.GameTimeSeconds,
                slot.GameTimeMinutes,
                slot.GameTimeHours);
            runtime.ControllerBindings = slot.ControllerBindings;
            runtime.MoonwalkEnabled = slot.MoonwalkEnabled;
            runtime.IconCancelEnabled = slot.IconCancelEnabled;
            runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);

            // Preserve the already-translated Ceres elevator entrance for its checkpoint.
            // Every other station uses the general cartridge-backed loader below.
            if (slot.Area == 6 && slot.SaveStation == 0)
            {
                runtime.InitializeStartingCeresRoom();
                runtime.InitializeCeresStartSamus();
                slot.ApplyTo(
                    runtime.Samus ?? throw new InvalidOperationException(
                        "Ceres initialization did not create Samus."),
                    runtime.System);
                return true;
            }

            runtime.InitializeSavedGame(slot);
            CartridgeRoomState loadedState = runtime.ActiveRoom?.State
                ?? throw new InvalidOperationException(
                    "Saved-game appearance started without an active room state.");
            // `$92:ED24` starts appearance track one after 14 frames and, on its fifth
            // call, schedules the room track 360 frames later. Mark this room state as
            // already handled so the generic room-change collector cannot overwrite the
            // fanfare with an immediate track request.
            audio.QueueMusicDelayed(1, 0x000e);
            audio.QueueMusicDelayed(loadedState.MusicTrackIndex, 0x0168 + 5);
            lastAudioRoomStatePointer = loadedState.Pointer;
            return false;
        }

        // Native new-game loading first publishes the standard HUD/OBJ transfer, then
        // loads the area-six station-zero room and finally runs SamusCode_08. This is the
        // same cartridge-backed sequence used by the room diagnostic; there is no host
        // terrain image, hand-placed Samus, or alternate playable-only initialization.
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        SamusState samus = runtime.Samus
            ?? throw new InvalidOperationException("Ceres initialization did not create Samus.");

        // CinematicFunction_Intro_Func73 at `$8B:C100` publishes area six/station zero
        // and calls the ordinary `$81:8000` saver immediately before state $1F. Both the
        // full cinematic and host-configured skip converge here, so neither path can omit
        // the automatic checkpoint.
        saveRam.SaveSlot(
            selectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(
                samus,
                runtime.System,
                area: 6,
                saveStation: 0,
                gameTime: runtime.GameTime,
                controllerBindings: runtime.ControllerBindings,
                moonwalkEnabled: runtime.MoonwalkEnabled,
                iconCancelEnabled: runtime.IconCancelEnabled));
        SaveRamChanged?.Invoke();
        return true;
    }

    private void HandleGunshipLandingSave()
    {
        if (runtime is null ||
            !AutomaticCheckpointSaver.TrySaveGunshipLanding(bus, runtime, selectedSaveSlot))
            return;
        SaveRamChanged?.Invoke();
    }

    private void HandleSaveStationPersistence()
    {
        if (runtime?.ConsumeSaveStationPersistenceRequest() is not { } request)
            return;
        SamusState samus = runtime.Samus
            ?? throw new InvalidOperationException(
                "A confirmed save station has no live Samus state.");
        saveRam.SaveSlot(
            selectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(
                samus,
                runtime.System,
                area: request.AreaIndex,
                saveStation: request.StationIndex,
                gameTime: runtime.GameTime,
                controllerBindings: runtime.ControllerBindings,
                moonwalkEnabled: runtime.MoonwalkEnabled,
                iconCancelEnabled: runtime.IconCancelEnabled));
        SaveRamChanged?.Invoke();
    }

    private static Rgba32[] CreateBlackFrame()
    {
        var pixels = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        Array.Fill(pixels, new Rgba32(0, 0, 0, 255));
        return pixels;
    }
}
