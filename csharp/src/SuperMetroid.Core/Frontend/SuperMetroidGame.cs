using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
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
    private TitleSequenceState? title;
    private FileSelectMenuState? fileSelect;
    private GameOptionsMenuState? options;
    private IntroCinematicState? intro;
    private SuperMetroidRuntime? runtime;
    private Rgba32[] lastPixels = CreateBlackFrame();

    public SuperMetroidGame(
        ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.gameOptions = gameOptions ?? new SuperMetroidGameOptions();
    }

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

    /// <summary>Current bank-$83 entry door pointer, exposed for door-transition watches.</summary>
    public ushort? GameplayActiveDoorPointer => runtime?.ActiveDoor?.Pointer;

    /// <summary>Runs one dispatcher frame and returns the PPU-visible result.</summary>
    public FrontendFrame Step(ushort controllerInput)
    {
        FrameNumber++;
        switch (GameState)
        {
            case SuperMetroidGameState.Reset:
                // `Vector_RESET_Async` ultimately stores state one and initializes
                // `cinematic_function` to `CinematicFunctionOpening` at $8B:9B68.
                title = new TitleSequenceState(bus);
                fileSelect = null;
                GameState = SuperMetroidGameState.OpeningCinematic;
                lastPixels = title.Render();
                break;

            case SuperMetroidGameState.OpeningCinematic:
                title!.Step(controllerInput);
                lastPixels = title.Render();
                if (title.FileSelectRequested)
                {
                    // `$8B:9F52` sets game_state=4 only after the slow fade reaches black.
                    fileSelect = new FileSelectMenuState(bus);
                    GameState = SuperMetroidGameState.FileSelectMenus;
                    lastPixels = fileSelect.Render();
                }
                break;

            case SuperMetroidGameState.FileSelectMenus:
                fileSelect!.Step(controllerInput);
                lastPixels = fileSelect.Render();
                if (fileSelect.TitleRequested)
                {
                    // The assembly calls SoftReset from menu index 33. Re-entering state
                    // zero makes all frontend-owned state disposable and deterministic.
                    GameState = SuperMetroidGameState.Reset;
                }
                else if (fileSelect.NewGameRequested)
                {
                    options = new GameOptionsMenuState(bus);
                    GameState = SuperMetroidGameState.GameOptionsMenu;
                    lastPixels = options.Render();
                }
                break;

            case SuperMetroidGameState.GameOptionsMenu:
                options!.Step(controllerInput);
                lastPixels = options.Render();
                if (options.FileSelectRequested)
                {
                    fileSelect = new FileSelectMenuState(bus);
                    GameState = SuperMetroidGameState.FileSelectMenus;
                    lastPixels = fileSelect.Render();
                }
                else if (options.IntroRequested)
                {
                    if (gameOptions.SkipOpeningCinematic)
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
                        intro = new IntroCinematicState(bus);
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
                SetupNewCeresGame();
                GameState = SuperMetroidGameState.MadeItToCeresElevator;
                lastPixels = CreateBlackFrame();
                break;

            case SuperMetroidGameState.MadeItToCeresElevator:
                runtime!.StepFrame(controllerInput);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                if (runtime.GroundedSamusMovementEnabled)
                {
                    // The bank-$86 elevator objects restore Samus's ordinary frame
                    // handler only after the native 60-frame wait and 72-pixel descent.
                    GameState = SuperMetroidGameState.MainGameplay;
                }
                break;

            case SuperMetroidGameState.MainGameplay:
                runtime!.StepFrame(controllerInput);
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                if (runtime.HasPendingDoorTransition)
                {
                    // `$94:938B/$93CE` changes WRAM game_state during the gameplay call.
                    // The already-produced gameplay image remains this frame's image; the
                    // following dispatcher call begins state `$09` from that publication.
                    GameState = SuperMetroidGameState.HitDoorBlock;
                }
                break;

            case SuperMetroidGameState.HitDoorBlock:
                // A non-elevator type-$9 door enters `$82:E17D`, which immediately advances
                // through state $0A into the state-$0B transition coroutine. Audio draining,
                // palette fade, and scrolling are not translated yet, so keep those native
                // state boundaries explicit while loading only the cartridge destination.
                GameState = SuperMetroidGameState.LoadingNextRoomA;
                runtime!.LoadPendingDoorDestination();
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                GameState = SuperMetroidGameState.LoadingNextRoomB;
                break;

            case SuperMetroidGameState.LoadingNextRoomB:
                // This is the current endpoint of the incremental state-$0B port. The room
                // header, level, scrolls, graphics, enemies, beam tiles, and native final
                // Samus placement are live; the omitted presentation phase is intentionally
                // not simulated with invented fade/scroll timings.
                GameState = SuperMetroidGameState.MainGameplay;
                lastPixels = SuperMetroidRuntimeFrameRenderer.Render(runtime!);
                break;

            default:
                throw new NotSupportedException(
                    $"Frontend dispatcher state ${((ushort)GameState):X2} ({GameState}) is not translated yet.");
        }

        return CurrentFrame;
    }

    /// <summary>Last completed frame, useful for repainting without advancing emulation.</summary>
    public FrontendFrame CurrentFrame => new(GameState, PhaseName, FrameNumber, lastPixels);

    private string PhaseName => GameState switch
    {
        SuperMetroidGameState.Reset => "Reset",
        SuperMetroidGameState.OpeningCinematic => title!.Phase.ToString(),
        SuperMetroidGameState.FileSelectMenus => fileSelect!.Phase.ToString(),
        SuperMetroidGameState.GameOptionsMenu => options!.Phase.ToString(),
        SuperMetroidGameState.IntroCinematic => intro!.Phase.ToString(),
        SuperMetroidGameState.SetUpNewGame => "Loading fresh Ceres game",
        SuperMetroidGameState.MadeItToCeresElevator =>
            runtime?.CeresElevatorArrival is { IsComplete: false }
                ? "Ceres elevator arrival"
                : "Ceres controls unlocked",
        SuperMetroidGameState.MainGameplay => "Ceres gameplay",
        SuperMetroidGameState.HitDoorBlock => "Door collision",
        SuperMetroidGameState.LoadingNextRoomA => "Loading destination room",
        SuperMetroidGameState.LoadingNextRoomB => "Destination room ready",
        _ => GameState.ToString(),
    };

    private void SetupNewCeresGame()
    {
        runtime = new SuperMetroidRuntime(bus);

        // Native new-game loading first publishes the standard HUD/OBJ transfer, then
        // loads the area-six station-zero room and finally runs SamusCode_08. This is the
        // same cartridge-backed sequence used by the room diagnostic; there is no host
        // terrain image, hand-placed Samus, or alternate playable-only initialization.
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
    }

    private static Rgba32[] CreateBlackFrame()
    {
        var pixels = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        Array.Fill(pixels, new Rgba32(0, 0, 0, 255));
        return pixels;
    }
}
