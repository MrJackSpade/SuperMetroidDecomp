using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Incremental, frame-steppable C# runtime shell for translated Super Metroid systems.
/// </summary>
/// <remarks>
/// Each update begins at the frame boundary, which shows the previous update's frame, latches
/// input and advances the frame counters; the update's gameplay then runs. Tools and viewers
/// step one update at a time through the same boundary.
/// </remarks>
internal sealed class SuperMetroidRuntime
{
    /// <summary>The per-line effect phase each update runs before its random number is drawn.</summary>
    internal PerLineEffectPhase PerLineEffects { get; }

    /// <summary>
    /// Where the per-line effect work falls among the random draws; created with the runtime, whose door transitions stop
    /// and resume it.
    /// </summary>
    internal EffectPhaseRandomOrder EffectPhaseOrder { get; } = new();

    /// <summary>
    /// How a door transition's scroll counts frames on the short frame counter; created with the runtime, whose room
    /// loader starts and ends it.
    /// </summary>
    internal DoorTransitionFrameCounting DoorFrameCounting { get; } = new();

    /// <summary>The <see cref="GameplayTimeFreeze"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayTimeFreeze Freeze { get; }

    /// <summary>The <see cref="PlmFramePhase"/> owner of this SuperMetroidRuntime.</summary>
    internal PlmFramePhase PlmPhase { get; }

    /// <summary>The <see cref="RoomMainRoutines"/> owner of this SuperMetroidRuntime.</summary>
    internal RoomMainRoutines RoomMains { get; }

    /// <summary>The <see cref="GameplayHudSetup"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayHudSetup HudSetup { get; }

    /// <summary>The <see cref="BackgroundScrollPhase"/> owner of this SuperMetroidRuntime.</summary>
    internal BackgroundScrollPhase BackgroundPhase { get; }

    /// <summary>The <see cref="FrameBoundary"/> owner of this SuperMetroidRuntime.</summary>
    internal AcceptedFrameBoundary FrameBoundary { get; }

    /// <summary>The <see cref="MessageBoxFlow"/> owner of this SuperMetroidRuntime.</summary>
    internal MessageBoxFlow MessageBoxes { get; }

    /// <summary>The <see cref="DeathSequenceRunner"/> owner of this SuperMetroidRuntime.</summary>
    internal DeathSequenceRunner DeathSequence { get; }

    /// <summary>The <see cref="SamusFramePhases"/> owner of this SuperMetroidRuntime.</summary>
    internal SamusFramePhases SamusPhases { get; }

    /// <summary>The <see cref="RoomLoader"/> owner of this SuperMetroidRuntime.</summary>
    internal RoomLoader RoomLoader { get; }
    /// <summary>Applies room-scoped enemy effects before frame owners advance.</summary>
    internal EnemyRoomEffects EnemyRoomEffects { get; }
    /// <summary>Retains scripted demo input and progression across runtime frames.</summary>
    internal AttractDemoSession AttractSession { get; } = new();

    /// <summary>The <see cref="GameplayDisplayState"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayDisplayState Display { get; }

    /// <summary>The <see cref="SamusFrameState"/> owner of this SuperMetroidRuntime.</summary>
    internal SamusFrameState Player { get; }

    /// <summary>The <see cref="ActiveRoomState"/> owner of this SuperMetroidRuntime.</summary>
    internal ActiveRoomState Room { get; }

    /// <summary>The <see cref="FrameClock"/> owner of this SuperMetroidRuntime.</summary>
    internal FrameClock Clock { get; }

    /// <summary>The <see cref="GameplayOptions"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayOptions Options { get; }

    /// <summary>The assists this runtime plays under, and the owner of its granted loadout and saved-encounter reset.</summary>
    internal ActiveAssists Assists { get; }

    /// <summary>The <see cref="PlmShotBlockVisuals"/> owner of this SuperMetroidRuntime.</summary>
    internal PlmShotBlockVisuals ShotBlockVisuals { get; }

    /// <summary>The <see cref="GameplayActorDrawing"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayActorDrawing ActorDrawing { get; }

    /// <summary>
    /// What Mother Brain's rainbow-beam push hides from the camera this update, bound into the enemies and consumed by
    /// the update's scrolling.
    /// </summary>
    internal RainbowBeamCameraCheckpoint RainbowBeamCameraCheckpoint { get; } = new();

    /// <summary>
    /// The compatibility services reproducing what Samus's glitch paths read past their data, from the installed content;
    /// the runtime hands each to the owner that consults it as it builds that owner and every Samus and room.
    /// </summary>
    internal SamusOverreadServices SamusOverreads { get; }

    /// <summary>The <see cref="XrayReveal"/> owner of this SuperMetroidRuntime.</summary>
    internal XrayReveal Xray { get; }

    /// <summary>The main-screen layers gameplay chooses and the accepted frame shows.</summary>
    internal GameplayScreenLayers ScreenLayers { get; }

    /// <summary>The <see cref="EscapeRoomEffects"/> owner of this SuperMetroidRuntime.</summary>
    internal EscapeRoomEffects EscapeEffects { get; }

    /// <summary>The <see cref="EnemyFramePhase"/> owner of this SuperMetroidRuntime.</summary>
    internal EnemyFramePhase EnemyPhase { get; }

    /// <summary>The <see cref="AttractDemoDriver"/> owner of this SuperMetroidRuntime.</summary>
    internal AttractDemoDriver AttractDemo { get; }

    /// <summary>The <see cref="DoorTransitionRunner"/> owner of this SuperMetroidRuntime.</summary>
    internal DoorTransitionRunner DoorTransition { get; }

    /// <summary>The <see cref="RuntimeArtwork"/> owner of this SuperMetroidRuntime.</summary>
    internal RuntimeArtwork Artwork { get; }

    /// <summary>Creates the frame-steppable runtime under its assists, and loads the required initial palette.</summary>
    /// <param name="assists">The assists the runtime plays under; <see cref="GameplayAssists.None"/> for the unassisted game.</param>
    /// <param name="artwork">
    /// The installed catalogs; their initial palette is loaded into the colour table during construction, their Samus overread services
    /// are handed to the owners that consult them, their boss overread services are bound to the enemy session, and
    /// <see cref="BindArtwork"/> binds the rest.
    /// </param>
    /// <param name="session">
    /// What the session keeps across its runtimes: the frame clock it shares with the frontend, the random-number word,
    /// the unauthored Speed Booster cadence its enemies and every Samus share, the power-on earthquake delay, the save
    /// confirmation's input cadence and the live and stored explored maps.
    /// </param>
    internal SuperMetroidRuntime(GameplayAssists assists, IGameplayArtwork artwork, GameplaySessionCarryover session)
    {
        Ensure.NotNull(artwork);
        Ensure.NotNull(session);
        FrameClock clock = session.Clock;
        UnauthoredBoostCadence unauthoredBoostCadence = session.UnauthoredBoostCadence;
        MessageBox = new GameplayMessageBoxState(session.SaveChoiceInput);
        SamusOverreads = artwork.SamusOverreads ?? throw new InvalidOperationException(
            "Gameplay requires the installed Samus overread services.");
        System = new SharedGameState(session.Random, session.EarthquakeSoundDelay, session.StoredExploredMaps);
        Display = new GameplayDisplayState();
        Player = new SamusFrameState();
        Room = new ActiveRoomState();
        Clock = Ensure.NotNull(clock);
        Options = new GameplayOptions();
        EnemySession = new EnemySession(unauthoredBoostCadence);
        EnemySession.RainbowBeamCameraCheckpoint = RainbowBeamCameraCheckpoint;
        EnemySession.BossOverreads = artwork.BossAndIntroOverreads ?? throw new InvalidOperationException(
            "Gameplay requires the installed boss and intro overread services.");

        BeamCharge = new BeamChargeState();
        Freeze = new GameplayTimeFreeze(Player);
        Assists = new ActiveAssists(Player, System, assists);
        TourianStatueDependencies = new TourianStatueSequenceDependencies { Player = Player, Room = Room, System = System, Assists = Assists, Freeze = Freeze };
        Artwork = new RuntimeArtwork(new RuntimeArtworkDependencies { BossArtwork = BossArtwork, Room = Room, RoomFx = RoomFx, Hud = Hud, Player = Player });
        HudSetup = new GameplayHudSetup(new GameplayHudSetupDependencies { Artwork = Artwork, Hud = Hud, Player = Player, EnemySession = EnemySession, Room = Room });
        ScreenLayers = new GameplayScreenLayers(Room);
        Xray = new XrayReveal(BackgroundScroll, Room, System);
        FrameBoundary = new AcceptedFrameBoundary(new AcceptedFrameBoundaryDependencies { Input = new AcceptedFrameInput(Controller1, PhysicalController1, Options, Clock), EnemySession = EnemySession, RoomFx = RoomFx, Artwork = Artwork, Hud = Hud, BackgroundScroll = BackgroundScroll, Display = Display, DisplayedSprites = DisplayedSprites, ScreenLayers = ScreenLayers, Sprites = Sprites, Player = Player, Room = Room, SuitPickup = SuitPickup, Xray = Xray });
        BackgroundPhase = new BackgroundScrollPhase(BackgroundScroll, Room);
        ActorDrawing = new GameplayActorDrawing(new GameplayActorDrawingDependencies { Artwork = Artwork, AttractSession = AttractSession, Controller1 = Controller1, Display = Display, Freeze = Freeze, Clock = Clock, Sprites = Sprites, Player = Player, Room = Room });
        EnemyRoomEffects = new EnemyRoomEffects(new EnemyRoomEffectsDependencies { Display = Display, EscapeTimer = EscapeTimer, Player = Player, Room = Room, System = System});
        EnemyPhase = new EnemyFramePhase(new EnemyFramePhaseDependencies { BackgroundScroll = BackgroundScroll, Controller1 = Controller1, Display = Display, EnemyRoomEffects = EnemyRoomEffects, Freeze = Freeze, Clock = Clock, Sprites = Sprites, Player = Player, Room = Room});
        PerLineEffects = new PerLineEffectPhase(Freeze, System, EffectPhaseOrder);
        DoorTransition = new DoorTransitionRunner(new DoorTransitionRunnerDependencies { ActorDrawing = ActorDrawing, BackgroundScroll = BackgroundScroll, Display = Display, EnemyPhase = EnemyPhase, PerLineEffects = PerLineEffects, Clock = Clock, FrameBoundary = FrameBoundary, Sprites = Sprites, Player = Player, Room = Room, System = System});
        EscapeEffects = new EscapeRoomEffects(Freeze, Clock, Room, System);
        ShotBlockVisuals = new PlmShotBlockVisuals();
        RoomLoader = new RoomLoader(new RoomLoaderDependencies { EnemySession = EnemySession, BossArtwork = BossArtwork, AreaArtwork = AreaArtwork, RoomFx = RoomFx, PaletteFxSession = PaletteFxSession, TourianStatueDependencies = TourianStatueDependencies, ShotBlockVisuals = ShotBlockVisuals, BeamCharge = BeamCharge, Artwork = Artwork, BackgroundScroll = BackgroundScroll, Controller1 = Controller1, Display = Display, DoorTransition = DoorTransition, EnemyRoomEffects = EnemyRoomEffects, EscapeEffects = EscapeEffects, GameTime = GameTime, Hud = Hud, Clock = Clock, DoorFrameCounting = DoorFrameCounting, Options = Options, Player = Player, Room = Room, System = System, Assists = Assists, SamusOverreads = SamusOverreads });
        AttractDemo = new AttractDemoDriver(new AttractDemoDriverDependencies { EnemySession = EnemySession, Artwork = Artwork, AttractSession = AttractSession, Controller1 = Controller1, HudSetup = HudSetup, FrameBoundary = FrameBoundary, Player = Player, Room = Room, RoomLoader = RoomLoader, System = System, AnimationContinuation = SamusOverreads.AnimationContinuation });
        DeathSequence = new DeathSequenceRunner(new DeathSequenceRunnerDependencies { Artwork = Artwork, Display = Display, FrameBoundary = FrameBoundary, Sprites = Sprites, Player = Player, Room = Room});
        MessageBoxes = new MessageBoxFlow(new MessageBoxFlowDependencies { Freeze = Freeze, Hud = Hud, MessageBox = MessageBox, Player = Player, Room = Room, SuitPickup = SuitPickup, System = System });
        PlmPhase = new PlmFramePhase(new PlmFramePhaseDependencies { Artwork = Artwork, Controller1 = Controller1, EnemyRoomEffects = EnemyRoomEffects, Freeze = Freeze, MessageBox = MessageBox, MessageBoxes = MessageBoxes, Player = Player, Room = Room, SuitPickup = SuitPickup});
        RoomMains = new RoomMainRoutines(new RoomMainRoutinesDependencies { BackgroundScroll = BackgroundScroll, Player = Player, Room = Room, System = System});
        SamusPhases = new SamusFramePhases(new SamusFramePhasesDependencies { Artwork = Artwork, AttractDemo = AttractDemo, Xray = Xray, Controller1 = Controller1, Freeze = Freeze, GameTime = GameTime, Clock = Clock, Options = Options, Assists = Assists, Player = Player, Room = Room, System = System });

        Artwork.InitialPaletteArt = artwork.GameplayBasePalettes;

        // Gameplay starts with Samus in her starting shades; every other picture shows its standard colours (#1512).
        Player.Colours.ShowStarting();
    }

    /// <summary>
    /// Binds the installed catalogs to the runtime's owners: every catalog when <paramref name="previous"/> is null (a new
    /// runtime), otherwise only the catalogs whose reference changed.
    /// </summary>
    /// <remarks>
    /// The map presentation binds first: its setter pushes the HUD, message-box, layer-3 and Samus palette bindings and
    /// rebinds queued transfers that later catalogs' setters may read.
    /// </remarks>
    /// <param name="artwork">The catalogs to bind.</param>
    /// <param name="previous">The catalogs bound before, or <see langword="null"/> to bind every catalog.</param>
    internal void BindArtwork(IGameplayArtwork artwork, IGameplayArtwork? previous)
    {
        Ensure.NotNull(artwork);
        void Bind<T>(Func<IGameplayArtwork, T?> catalog, Action<T?> bind) where T : class
        {
            T? value = catalog(artwork);
            if (previous is null || !ReferenceEquals(catalog(previous), value))
                bind(value);
        }

        Bind(a => a.MapPresentation, value => Artwork.MapPresentation = value);
        Bind(a => a.GameplayBasePalettes, value => Artwork.InitialPaletteArt = value);
        Bind(a => a.SamusBodyArt, value => Artwork.SamusBodyArt = value);
        Bind(a => a.RoomBlockSheets, value => Artwork.RoomBlockSheets = value);
        Bind(a => a.RoomVisualLayouts, value => Artwork.RoomVisualLayouts = value);
        Bind(a => a.RoomLevelStreams, value => Artwork.RoomLevelStreams = value);
        Bind(a => a.RoomPlmShotBlockVisuals, value => ShotBlockVisuals.RoomPlmShotBlockVisuals = value);
        Bind(a => a.RoomPlmGrappleBlockVisuals, value => ShotBlockVisuals.RoomPlmGrappleBlockVisuals = value);
        Bind(a => a.RoomPlmStationVisuals, value => ShotBlockVisuals.RoomPlmStationVisuals = value);
        Bind(a => a.RoomPlmBlueDoorVisuals, value => ShotBlockVisuals.RoomPlmBlueDoorVisuals = value);
        Bind(a => a.RoomPlmColoredDoorVisuals, value => ShotBlockVisuals.RoomPlmColoredDoorVisuals = value);
        Bind(a => a.RoomPlmGreyDoorVisuals, value => ShotBlockVisuals.RoomPlmGreyDoorVisuals = value);
        Bind(a => a.RoomPlmEyeDoorVisuals, value => ShotBlockVisuals.RoomPlmEyeDoorVisuals = value);
        Bind(a => a.RoomPlmMotherBrainGlassVisuals, value => ShotBlockVisuals.RoomPlmMotherBrainGlassVisuals = value);
        Bind(a => a.RoomPlmNoobTubeVisuals, value => ShotBlockVisuals.RoomPlmNoobTubeVisuals = value);
        Bind(a => a.RoomPlmDownwardGateVisuals, value => ShotBlockVisuals.RoomPlmDownwardGateVisuals = value);
        Bind(a => a.RoomPlmElevatorPlatformVisuals, value => ShotBlockVisuals.RoomPlmElevatorPlatformVisuals = value);
        Bind(a => a.RoomPlmEscapeGateVisuals, value => ShotBlockVisuals.RoomPlmEscapeGateVisuals = value);
        Bind(a => a.RoomPlmBombTorizoHandVisuals, value => ShotBlockVisuals.RoomPlmBombTorizoHandVisuals = value);
        Bind(a => a.RoomPlmDraygonCannonVisuals, value => ShotBlockVisuals.RoomPlmDraygonCannonVisuals = value);
        Bind(a => a.RoomPlmChozoStatueVisuals, value => ShotBlockVisuals.RoomPlmChozoStatueVisuals = value);
        Bind(a => a.RoomPlmLinkedRestoreVisuals, value => ShotBlockVisuals.RoomPlmLinkedRestoreVisuals = value);
        Bind(a => a.RoomPlmTourianAccessVisuals, value => ShotBlockVisuals.RoomPlmTourianAccessVisuals = value);
        Bind(a => a.RoomPlmSpeedBoosterVisuals, value => ShotBlockVisuals.RoomPlmSpeedBoosterVisuals = value);
        Bind(a => a.RoomPlmMaridiaElevatubeVisuals, value => ShotBlockVisuals.RoomPlmMaridiaElevatubeVisuals = value);
        Bind(a => a.RoomPlmSporeSpawnCeilingVisuals, value => ShotBlockVisuals.RoomPlmSporeSpawnCeilingVisuals = value);
        Bind(a => a.RoomPlmSamusEaterVisuals, value => ShotBlockVisuals.RoomPlmSamusEaterVisuals = value);
        Bind(a => a.RoomPlmBotwoonWallVisuals, value => ShotBlockVisuals.RoomPlmBotwoonWallVisuals = value);
        Bind(a => a.RoomPlmKraidVisuals, value => ShotBlockVisuals.RoomPlmKraidVisuals = value);
        Bind(a => a.RoomPlmCrocomireVisuals, value => ShotBlockVisuals.RoomPlmCrocomireVisuals = value);
        Bind(a => a.RoomPlmMotherBrainFakeDeathVisuals, value => ShotBlockVisuals.RoomPlmMotherBrainFakeDeathVisuals = value);
        Bind(a => a.RoomPlmCollectibleVisuals, value => ShotBlockVisuals.RoomPlmCollectibleVisuals = value);
        Bind(a => a.XrayRevealVisuals, value => Xray.XrayRevealVisuals = value);

        Bind(a => a.ProjectileFrameBindings, value => Artwork.ProjectileFrameBindings = value);
        Bind(a => a.SpriteSheets, value => Artwork.SpriteSheets = value);
        Bind(a => a.EnemySheets, value => EnemySession.Sheets = value);
        Bind(a => a.EnemyOverreads, value => EnemySession.Overreads = value);
        Bind(a => a.ChargeFlarePlacement, value => Artwork.ChargeFlarePlacement = value);
        Bind(a => a.GrappleFlarePlacement, value => Artwork.GrappleFlarePlacement = value);
        Bind(a => a.GrappleSwingFrames, value => Artwork.GrappleSwingFrames = value);

        Bind(a => a.BossArtwork, value => BossArtwork.Artwork = value);
        Bind(a => a.BossBodySheets, value => BossArtwork.BodySheets = value);
        Bind(a => a.AreaArtwork, value => AreaArtwork.Artwork = value);
        Bind(a => a.EnemyTileArtwork, value => EnemySession.TileArtwork = value);
    }

    /// <summary>
    /// Set when the escape countdown expired during this runtime's frame: the countdown
    /// writes the time-up game state from inside gameplay. The frontend dispatcher consumes it.
    /// </summary>
    public bool TimeUpRequested { get; private set; }

    /// <summary>Takes the pending time-up publication; the dispatcher may still override it.</summary>
    public bool ConsumeTimeUpRequest()
    {
        bool requested = TimeUpRequested;
        TimeUpRequested = false;
        return requested;
    }

    /// <summary>Shared random/event/input-filter state, over the session's random-number word.</summary>
    public SharedGameState System { get; }

    /// <summary>The game time, which each save slot also records.</summary>
    public GameTimeState GameTime { get; } = new();

    /// <summary>
    /// Controller 1 as the frame boundary latched it, in the default action layout: the configured bindings are
    /// applied once at the latch, so Samus's action checks read the buttons they test through the bindings.
    /// </summary>
    public ControllerInputState Controller1 { get; } = new();

    /// <summary>
    /// The same sample as physical buttons. Code that tests fixed buttons rather than the bindings (the pause menu)
    /// reads this latch.
    /// </summary>
    public ControllerInputState PhysicalController1 { get; } = new();

    /// <summary>Escape timer state machine.</summary>
    public EscapeTimer EscapeTimer { get; } = new();

    /// <summary>
    /// Frontend language bit consumed by room actors whose script differs
    /// between English and Japanese. The enemy owner retains the actual language branch.
    /// </summary>
    public bool JapaneseText
    {
        get => EnemySession.JapaneseText;
        set => EnemySession.JapaneseText = value;
    }

    /// <summary>The HUD's images the last frame boundary latched (#1351, #1466): the HUD as displayed.</summary>
    /// <exception cref="InvalidOperationException">No frame boundary has latched the HUD yet.</exception>
    public ShownHud AcceptedHud =>
        Display.Latched.Hud ?? throw new InvalidOperationException("No frame boundary has latched the HUD.");

    /// <summary>The palette-FX handler enable bit and heat phases, which outlive each room's palette FX.</summary>
    public PaletteFxSessionState PaletteFxSession { get; } = new();

    /// <summary>The colours a door's fades show, which the door transition steps (#1512).</summary>
    internal DoorFade DoorFade { get; } = new();

    /// <summary>The death fade and the time-up whiteout of the whole display, which their game states step (#1512).</summary>
    internal GameplayFadeOut FadeOut { get; } = new();

    /// <summary>The sprite frame the update is building for the next frame boundary: sprites plus whole-frame draws.</summary>
    public SpriteFrameBuffer Sprites { get; } = new();

    /// <summary>
    /// The sprite frame the most recent frame boundary made visible. Renderers draw it while the current update
    /// prepares <see cref="Sprites"/>.
    /// </summary>
    public SpriteFrameBuffer DisplayedSprites { get; } = new();

    /// <summary>Active room's fixed 32-slot enemy system.</summary>
    public EnemySession EnemySession { get; }

    /// <summary>The installed boss artwork every room's bosses read.</summary>
    public BossArtworkBinding BossArtwork { get; } = new();

    /// <summary>The installed area artwork every room's area actors read.</summary>
    public AreaArtworkBinding AreaArtwork { get; } = new();

    /// <summary>The last enemy pass's results, for readers after the frame.</summary>
    internal IEnemyFrameResults EnemyFrameResults => Room.LoadedScope.Enemies;

    /// <summary>
    /// The game state of the frontend dispatch now running. Samus commands test it from
    /// inside the update; runtime-only hosts run main gameplay.
    /// </summary>
    public SuperMetroid.Core.Progression.SuperMetroidGameState DispatchGameState { get; set; } = SuperMetroid.Core.Progression.SuperMetroidGameState.MainGameplay;

    /// <summary>Whether a power-bomb explosion is under way; X-ray setup and the pause check reject one.</summary>
    /// <remarks>Before the first room exists there is no explosion.</remarks>
    public bool PowerBombExplosionActive => Room.BombProjectiles?.PowerBombExplosion.IsActive == true;

    /// <summary>
    /// The power-bomb gate in all three sound queues:
    /// while the explosion status is negative every
    /// request is dropped after the occupancy check, whoever makes it.
    /// </summary>
    public bool PowerBombExplosionSuppressesSounds => Room.BombProjectiles?.PowerBombExplosion.IsActive == true;

    /// <summary>The Fireflea state, the room's fixed colour and installed FX artwork, which outlive each room's layer-3 FX.</summary>
    public RoomFxSession RoomFx { get; } = new();

    /// <summary>The held beam charge, flare and charge palette, which survive room loads; each room's projectiles share it.</summary>
    internal BeamChargeState BeamCharge { get; }

    /// <summary>
    /// The gameplay message box. A permanent pickup opens it, and subsequent
    /// <see cref="StepFrame"/> calls advance the box's updates without running gameplay
    /// until the box has closed.
    /// </summary>
    public GameplayMessageBoxState MessageBox { get; }

    /// <summary>
    /// Shared post-message Varia/Gravity light-beam transformation.
    /// </summary>
    public SamusSuitPickupState SuitPickup { get; } = new();

    /// <summary>The gameplay HUD.</summary>
    public HudState Hud { get; } = new();

    /// <summary>
    /// Parallax/layer/block dispatcher fed by the active layer-1 camera.
    /// Its requests become row/column staging transfers when a room loader supplies level data.
    /// </summary>
    public BackgroundScrollState BackgroundScroll { get; } = new();

    /// <summary>Runs one frame boundary and publishes an empty sprite frame without advancing gameplay owners.</summary>
    public void RunBlankGameplayFrame(ControllerButton controllerInput)
    {
        FrameBoundary.BeginAcceptedFrame(controllerInput);
        Sprites.BeginFrame();
        Sprites.FinalizeFrame();
        RoomLoader.RestartShortCounterDuringDoorScroll();
    }

    /// <summary>Runs one frame boundary followed by the update's gameplay, as <paramref name="request"/> asks.</summary>
    /// <param name="controller1Input">The host's controller buttons, normalized through the action bindings when the frame boundary latches player input; attract playback publishes its independent scripted buttons later.</param>
    /// <param name="request">The state policy, audio calls and the host's work after the frame boundary.</param>
    public RuntimeFrameResult StepFrame(ControllerButton controller1Input, RuntimeFrameRequest request)
    {
        Ensure.NotNull(request);
        Assists.GrantLoadoutToNewSamus();
        AmmoLoanGuard infiniteAmmoGuard =
            AmmoLoanGuard.Begin(Assists.Current.AmmoNeverRunsOut, Player.Samus);
        try
        {
            return StepFrameGuarded(controller1Input, request, infiniteAmmoGuard);
        }
        finally
        {
            // The desktop error reporter deliberately catches translated exceptions and
            // attempts another frame. A finally seam is therefore mandatory: otherwise a
            // frame that throws after borrowing the last round leaves live ammo at two.
            infiniteAmmoGuard.Complete(Player.Samus);
            // A failed demo frame must not leak scripted input into the player latch.
            AttractDemo.RestoreAttractPlayerInput();
        }
    }

    /// <summary>
    /// Advances one frame boundary and the corresponding gameplay update while the
    /// frame-exit ammo guard remains active through translated callbacks.
    /// </summary>
    private RuntimeFrameResult StepFrameGuarded(
        ControllerButton controller1Input,
        RuntimeFrameRequest frameRequest,
        AmmoLoanGuard infiniteAmmoGuard)
    {
        RebindRoutingReferences();
        // An open message box runs its own updates inside the suspended gameplay update:
        // the frame boundary latches no input and no random number is drawn nor owner run.
        if (MessageBox.IsActive)
            return StepMessageBoxFrame(controller1Input, frameRequest.Policy, infiniteAmmoGuard);
        BeginAcceptedGameplayFrame(controller1Input, frameRequest);
        RunPerLineEffects();
        AdvanceMainLoopRandom();
        RunPreDispatchEffects();
        var context = new GameplayFrameContext(SampleSamusCameraPoint());
        PrepareEnemyProcessingList();
        RunPaletteFx();
        BeginSpriteFrame();
        PrepareEnemiesAndHoistedEnemyMain(context);
        if (Player.Samus is not null && Room.Camera is not null)
        {
            RunSamusAndEnemyMain(context, Player.Samus, Room.Camera, frameRequest.Audio);
            bool deathOwnsSamus = context.Samus.Mode.Beta == SamusBetaHandler.Fatal;
            if (!deathOwnsSamus)
                DoorTransition.RunEnemyProjectileHandler(Room.LoadedScope.BombProjectiles, Room.LoadedScope);
            PlmPhase.RunPlmPhase(deathOwnsSamus, Player.Samus, Room.LoadedScope.Plms, Room.LoadedScope.BombProjectiles, Room.LoadedScope);
            OpenGunshipPrompt();
            context.Tail = new SuspendedGameplayFrameTail(context.Samus.Mode, context.Samus.PreviousCameraPoint);
        }
        if (SuspendTailForMessageBox(context))
            return Snapshot(infiniteAmmoGuard);
        return FinishGameplayFrame(context.Tail, frameRequest.Policy, infiniteAmmoGuard, entry: GameplayFrameEntry.Fresh);
    }

    /// <summary>Points the cross-owner routing at this frame's room owners and Samus, then applies pending Chozo statue PLMs.</summary>
    private void RebindRoutingReferences()
    {
        // Samus and the room's owners are made on separate paths, so the frame's entry, before any producer executes,
        // is where both exist to be connected.
        Artwork.BindGrapplePresentation();
        RoomScope scope = Room.LoadedScope;
        Artwork.BindProjectiles(scope.Projectiles, scope.BombProjectiles);
        scope.Projectiles.Earthquakes.BindRoomEarthquake(scope.Enemies.Earthquake);
        scope.Plms.Sounds.BindPowerBombAudio(scope.BombProjectiles.PowerBombExplosion.Activity);
        Player.Samus?.Shinespark.BindProjectileOwners(BeamCharge, scope.BombProjectiles.PowerBombExplosion);
        EnemyRoomEffects.ApplyPendingChozoStatuePlms(scope.Plms, scope.Layer3Fx, Room.LoadedScope.Enemies);
    }

    /// <summary>Opens the frame's impact-audio window, runs the accepted-frame boundary and the host's post-boundary work, and chooses the screen layers.</summary>
    private void BeginAcceptedGameplayFrame(ControllerButton controller1Input, RuntimeFrameRequest frameRequest)
    {
        Room.LoadedScope.Projectiles.ImpactAudio.BeginImpactAudioFrame(context: ImpactAudioContext.Gameplay);
        FrameBoundary.BeginAcceptedFrame(controller1Input);
        frameRequest.AfterFrameBoundary();
        ScreenLayers.Choose(Room.LoadedScope);
    }

    /// <summary>Runs the per-line effect phase and the power bomb's shape step before the main loop draws its random number.</summary>
    private void RunPerLineEffects()
    {
        // The per-line effects read the random number before the main loop draws the next one. A
        // message box's wait does not run them.
        if (Room.Camera is not null && !MessageBox.IsActive)
            PerLineEffects.Advance(Room.LoadedScope.Plms, Room.LoadedScope);

        // The power bomb blast belongs to the outer frame, not Samus alpha. A statue carry
        // replaces her handlers while the already-spawned blast continues normally.
        // Its cleanup runs Samus command $1E, which can queue sounds before the Samus
        // handler; the frame's Samus sound window therefore opens here.
        if (Player.Samus is { } samus && !MessageBox.IsActive)
        {
            samus.LiquidPhysics.BeginFrameSoundRequests(Room.LoadedScope.BombProjectiles.PowerBombExplosion);
            Room.LoadedScope.BombProjectiles.AdvancePowerBombExplosion(samus, Controller1.Current, DispatchGameState);
        }
    }

    /// <summary>Draws the update's random number.</summary>
    private void AdvanceMainLoopRandom()
    {
        // Every update draws a random number right after the per-line effects
        // (EffectPhaseRandomOrder) and before the game state's own work. Several room and
        // enemy routines only sample the random number instead of advancing it themselves;
        // the falling-tile room is the first visibly obvious example: without this draw,
        // every ceiling fragment selects the same X table entry forever. Keep this
        // unconditional and at the start of the update; advancing only at a consumer would
        // change the shared sequence.
        System.NextRandom();
    }

    /// <summary>Runs the owners before the game-state dispatch: Ceres haze, the suit transformation, Phantoon's wave and blending, and room layer-3 FX.</summary>
    private void RunPreDispatchEffects()
    {
        Room.LoadedScope.CeresHaze.Step();

        MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = false;

        // The suit transformation's light beam advances once near the start of each
        // ordinary gameplay update. A transformation spawned when the preceding message
        // returned therefore receives its first stage call on this following update, while
        // every normal owner below continues to run around the input-locked, centered Samus.
        if (SuitPickup.IsActive)
        {
            if (Player.Samus is null)
                throw new InvalidOperationException("A suit transformation has no Samus owner.");
            SuitPickup.Step(Player.Samus, Player.Colours);
        }

        // An open message box keeps the room's sprites, music and sounds while it waits,
        // and blocks every gameplay owner. Room FX objects run in the ordinary gameplay
        // owner list; in particular, they must not scroll or animate while a message box
        // waits. Keeping this after the message box's early return prevents rain and fog
        // from advancing behind a station or item message.
        if (Room.LoadedScope.Enemies.DisplaySources.Phantoon is { } waveBoss)
        {
            waveBoss.Wave.Step(waveBoss);
            waveBoss.Blending.Step(waveBoss, Display.LayerBlendingDefaultConfig);
        }
        if (Room.Camera is not null)
        {
            Room.LoadedScope.Layer3Fx.Step(Room.LoadedScope.ArtworkFrames, new RoomLayer3FxFrame { Camera = Room.Camera.PixelPosition, TimeIsFrozen = Freeze.TimeIsFrozen, EarthquakeSoundDelay = System.EarthquakeSoundDelay, RandomNumber = System.RandomNumber, FirefleaDarknessLevel = Room.LoadedScope.Enemies.RoomStatus.FirefleaDarknessLevel, PowerBomb = Room.LoadedScope.BombProjectiles.PowerBombExplosion.Activity, LiquidMotionAlreadyAdvanced = Room.LoadedScope.Layer3Fx.MovesLiquidBeforeMainLoop });
            if (Room.LoadedScope.Layer3Fx.EarthquakeRequest is { } roomFxEarthquake)
            {
                // Lava/acid rise pre-instructions use TSB on the shared timer after writing
                // type $15. Apply that global side effect before the ordinary shake handler
                // later in this frame so its first displacement is visible immediately.
                Room.LoadedScope.Enemies.Earthquake.EarthquakeType = roomFxEarthquake.Type;
                Room.LoadedScope.Enemies.Earthquake.AddTimerBits(roomFxEarthquake.TimerBits);
            }
            if (Player.Samus is not null && Room.LoadedScope.Layer3Fx.Type is
                    RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid)
                Player.Samus.LiquidPhysics.ConfigureFor(Room.LoadedScope.Layer3Fx);
        }
    }

    /// <summary>
    /// Samus's position before this frame's movement: the camera fallback for a newly loaded or restored camera.
    /// </summary>
    /// <remarks>
    /// MainScrollingRoutine saves its previous-position words after scrolling, not at
    /// frame start. This fallback is only for a newly loaded/restored camera; otherwise
    /// retain the preceding scrolling checkpoint so later room-main movement (such as
    /// the elevatube) contributes to the next frame's distance just like enemy movement.
    /// </remarks>
    private FixedPosition<RoomPixels>? SampleSamusCameraPoint() => Player.Samus is null
        ? null
        : Player.Samus.Position;

    /// <summary>Builds the enemy processing list that the grapple scan and EnemyMain use.</summary>
    private void PrepareEnemyProcessingList()
    {
        // Main gameplay begins by determining which enemies to process,
        // before palette FX and Samus. The grapple scan uses this list.
        if (Room.Camera is not null && Room.LoadedScope.Enemies.IsLoaded)
            Room.LoadedScope.Enemies.ProcessingWindow.PrepareEnemyProcessingList(Room.Camera.PixelPosition);
    }

    /// <summary>Runs the palette-FX handler and Hyper Beam's palette object.</summary>
    private void RunPaletteFx()
    {
        // Main gameplay runs the palette-FX handler before Samus's update and
        // before enemies are processed. Controller function three is invoked by the
        // Baby during that later enemy phase, so a newly spawned Hyper Beam palette object naturally
        // begins on the following frame. Debug scripts that grant Hyper Beam before their
        // first StepFrame get the same handler-first behavior immediately.
        // The game has one descending palette-FX object pool. Room FX is spawned first and
        // therefore occupies its highest slots; Hyper Beam later takes the next free slot.
        // Running the room owner before the specialized Hyper Beam owner preserves that
        // ordering until both are consolidated behind one allocator.
        // Door destination drawing borrows this frame path, but the door's destination
        // fade does not call the palette-FX handler. Its colors must remain
        // owned by the gradual room fade until the door transition releases them.
        if (Room.LoadedScope.PaletteFx.HandlerEnabled && !Room.LoadedScope.Enemies.Elevator.ElevatorDoorTransitionActive &&
            Player.Samus?.Xray.ArePaletteFxSuspended != true)
        {
            Room.LoadedScope.PaletteFx.Step(new PaletteFxConditions { SamusY = Player.Samus?.Position.Y.Whole ?? 0, EquippedItems = Player.Samus?.EquippedItems ?? 0, EnemyZeroIsDead = Room.LoadedScope.Enemies.SlotTable.Slots.Count == 0 || Room.LoadedScope.Enemies.SlotTable.Slots[0].Health == 0, AreaMiniBossDefeated = Room.ActiveRoom is { } paletteRoom &&
                    System.HasAnyBossBits(paletteRoom.AreaIndex, BossBits.AreaMiniBoss), Samus = Player.Samus?.RoomView, FrameCounter = Clock.Counter }, Room.LoadedScope.BombProjectiles.PowerBombExplosion.Activity);
            // Her heat glow and her suit's loading show over her suit as the records that wrote it (#1512).
            foreach ((PaletteFxColours colours, PaletteFxFrame record) in Room.LoadedScope.PaletteFx.SuitShown)
                Player.Colours.LayOverSuit(PaletteFxShades.Of(colours, record));
            // The enemies' groups show over their owners' own colours as the records that wrote them (#1512).
            foreach ((PaletteFxColours colours, PaletteFxFrame record) in Room.LoadedScope.PaletteFx.EnemyShown)
                ShowEnemyPaletteFx(colours, record);
        }
        // Hyper Beam's palette object lives in the same pool and handler.
        if (Room.LoadedScope.PaletteFx.HandlerEnabled)
        {
            Player.Samus?.Drained.HyperBeamPaletteFx.Step(Player.Colours);
        }
    }

    /// <summary>
    /// Hands one record of an enemy's palette-FX group to the enemy that shows it (#1512): the Torizos lay their belly
    /// glow, Tourian's sprite statues their grey-out, and Mother Brain and the destroyable shutters the escape's shutter
    /// flash. The shutter flash shows on whichever of them the room holds, and on nothing once both shutters are destroyed,
    /// as the record's colours are then shown by no draw.
    /// </summary>
    /// <exception cref="InvalidOperationException">The group names no enemy that shows it, or its owner is not loaded.</exception>
    private void ShowEnemyPaletteFx(PaletteFxColours colours, PaletteFxFrame record)
    {
        RoomEnemySystem enemies = Room.LoadedScope.Enemies;
        switch (colours)
        {
            case PaletteFxColours.TorizoBelly:
                enemies.BombTorizoBehavior.ShowBellyGlow(record);
                return;
            case PaletteFxColours.RidleyStatue:
            case PaletteFxColours.PhantoonStatue:
                enemies.TourianEntranceStatueBehavior.ShowStatueGreyOut(colours, record);
                return;
            case PaletteFxColours.TourianShutters:
                // The flash recolours the first enemy colours' nine to fourteen: Mother Brain's exploded door in her room,
                // the destroyable shutters in the first escape room, whose FX record also starts the flash.
                if (enemies.MotherBrainEncounter.MotherBrain is not null)
                    enemies.MotherBrainEncounter.ShowEscapeShutterFlash(record);
                enemies.ShuttersBehavior.ShowEscapeShutterFlash(enemies.SlotTable.Slots, record);
                return;
            default:
                throw new InvalidOperationException($"Palette-FX colours {colours} name no enemy that shows them.");
        }
    }

    /// <summary>Starts the frame's sprite list for its draw pass.</summary>
    private void BeginSpriteFrame()
    {
        // The main loop starts the sprite list before dispatching the game state and closes it
        // afterward. Samus is drawn before the escape timer so she keeps her normal overlap
        // precedence.
        Sprites.BeginFrame();
    }

    /// <summary>Prepares the enemy frame and runs EnemyMain before Samus when the frame has no movement to wait for.</summary>
    private void PrepareEnemiesAndHoistedEnemyMain(GameplayFrameContext context)
    {
        EnemyPhase.PrepareEnemyFrame(Room.LoadedScope);
        // Host-disabled Samus movement has no alpha phase to wait for. Otherwise
        // defer actors until projectile production/update, immediately before beta.
        SamusHandlerFrame frame = context.Samus;
        frame.RecordHoistedEnemyMain(Player.Samus is null || Room.Camera is null || !Player.GroundedSamusMovementEnabled);
        // EnemyMain follows SamusCurrentStateHandler. An actor that restores the
        // normal handler pair there (the gunship) affects this frame's beta only:
        // the locked alpha has already run, so it still sees the lock below.
        if (frame.EnemyMainAlreadyRan)
        {
            bool lockedBeforeEnemyMain = Player.Samus?.Control.AlphaLocked == true;
            EnemyPhase.RunEnemyMainPhase(processingList: EnemyProcessingList.Prepared,
                Room.Camera?.PreviousSamusPoint ?? context.SamusCameraPointAtFrameStart, Room.LoadedScope);
            frame.RecordEnemyMainReleasedAlphaLock(lockedBeforeEnemyMain && Player.Samus?.Control.AlphaLocked == false);
        }
    }

    /// <summary>Runs the frame handler's alpha, EnemyMain, the post-EnemyMain resample and beta, in the game's order.</summary>
    private void RunSamusAndEnemyMain(GameplayFrameContext context, SamusState samus, ScrollBoundaryCamera camera, RuntimeFrameAudio audio)
    {
        SamusHandlerFrame frame = context.Samus;
        SamusPhases.RunAlpha(frame, DispatchGameState, samus, camera, context.SamusCameraPointAtFrameStart, Room.LoadedScope);
        if (frame.MovementEnabled && !frame.EnemyMainAlreadyRan)
            EnemyPhase.RunEnemyMainPhase(processingList: EnemyProcessingList.Prepared, frame.PreviousCameraPoint, Room.LoadedScope);
        SamusPhases.RunPostEnemyMainResample(frame, samus, Room.LoadedScope.BombProjectiles, Room.LoadedScope.Projectiles, Room.LoadedScope);
        SamusPhases.RunBeta(frame, samus, camera, audio, Room.LoadedScope);
    }

    /// <summary>Opens the gunship's save prompt at the shared suspension seam after PLMs.</summary>
    private void OpenGunshipPrompt()
    {
        // The gunship opens the save prompt from inside EnemyMain, after this frame's
        // ship bob and earlier owners ran. The prompt exists only while Samus is locked
        // inside the ship, so the owners between EnemyMain and here have nothing to
        // act on; the box therefore opens at this shared suspension seam, and the
        // remainder resumes after the YES/NO/completion chain returns.
        if (Room.LoadedScope.Enemies.Gunship.SavePromptPending)
        {
            if (MessageBox.IsActive)
            {
                throw new InvalidDataException(
                    "A PLM message box and the gunship save prompt opened in one gameplay frame.");
            }
            MessageBox.Begin(GameplayMessageIds.GunshipSaveConfirmation, Controller1.Current);
        }
    }

    /// <summary>Parks the frame's tail when a PLM opened a message box; returns whether it did.</summary>
    private bool SuspendTailForMessageBox(GameplayFrameContext context)
    {
        // The message routine runs inside PLM_Handler: the rest of this gameplay
        // frame (enemy-projectile collision, scrolling, drawing, room main and the shared
        // tail) resumes only after the box closes, after the item routine finishes.
        if (context.Tail is not { } suspendedTail || !MessageBox.IsActive)
            return false;
        _suspendedFrameTail = suspendedTail;
        // Producers up to PLM_Handler queued their sounds before DisplayMessageBox's
        // entry cancel; the frontend publishes this frame's requests at that boundary.
        FrameBoundary.CompletedGameplayAudioPublication++;
        return true;
    }

    /// <summary>
    /// The remainder of gameplay state eight after <c>PLM_Handler</c>. A frame whose PLM
    /// pass opened a message box parks this state and resumes it when the box closes.
    /// </summary>
    internal RuntimeFrameResult FinishGameplayFrame(
        SuspendedGameplayFrameTail? frameTail,
        RuntimeFramePolicy policy,
        AmmoLoanGuard infiniteAmmoGuard,
        GameplayFrameEntry entry)
    {
        if (frameTail is { } tail && Player.Samus is not null && Room.Camera is not null)
        {
            bool deathOwnsSamus = tail.Mode.Beta == SamusBetaHandler.Fatal;
            ResolveEnemyProjectileAndPowerBombHits(Player.Samus, deathOwnsSamus);
            StepCeresElevatorArrival(Player.Samus, deathOwnsSamus);
            RunMainScrolling(Player.Samus, Room.Camera, tail.PreviousCameraPoint, deathOwnsSamus);
            DrawGameplayActors(tail, deathOwnsSamus);
        }
        RunEscapeTimerHandler(frameTail);
        Sprites.FinalizeFrame();

        UpdateHud(infiniteAmmoGuard);
        RunRoomMains(policy);
        // HandleSamusOutOfHealthAndGameTile advances the four-word gameplay clock after
        // room main and before shaking. Message-box frames returned above, exactly as the
        // suspended frame does, so item fanfare time is not counted here.
        if (policy.AdvanceGameTime)
            GameTime.Step();

        // Room shaking runs after room main and game-time handling, but
        // before the active-enemy lists are cleared. Enemy attacks above may have installed
        // a new global quake this frame; consuming it here gives that request its first
        // displacement/decrement immediately and preserves the list used for actor shake.
        if (Room.LoadedScope.Enemies.IsLoaded)
            Room.LoadedScope.Enemies.HandleRoomShaking(Freeze.TimeIsFrozen);

        DecrementHurtTimers();

        // A resumed tail's frame already published its audio when it suspended.
        if (entry != GameplayFrameEntry.ResumedAfterMessageBox)
            FrameBoundary.CompletedGameplayAudioPublication++;
        return Snapshot(infiniteAmmoGuard);
    }

    /// <summary>Resolves enemy-projectile hits on Samus and her projectiles, then ordinary power-bomb hits.</summary>
    private void ResolveEnemyProjectileAndPowerBombHits(SamusState samus, bool deathOwnsSamus)
    {
        if (!deathOwnsSamus && !Freeze.TimeIsFrozen)
        {
            Room.LoadedScope.Enemies.Projectiles.ResolveEnemyProjectileSamusHits(samus);
            Room.LoadedScope.Enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(
                Room.LoadedScope.Projectiles, Room.LoadedScope.BombProjectiles);
            // Every gameplay update resolves the power bomb's hits after the enemy-
            // projectile passes. It reads the explosion's whole-pixel radius; a zero
            // radius is its own inactive/pre-explosion guard.
            // Invincibility timers inside the enemy pass prevent the persistent
            // expansion/afterglow radius from damaging one actor every frame.
            SamusPowerBombExplosionState powerBomb =
                Room.LoadedScope.BombProjectiles.PowerBombExplosion;
            Room.LoadedScope.Enemies.Collision.ResolveOrdinaryPowerBombHits(
                powerBomb.Position,
                Ensure.BetweenInclusive(0, byte.MaxValue, powerBomb.ExplosionRadius.WholePixels),
                samus);
        }
    }

    /// <summary>Steps the fresh-Ceres elevator arrival, which lowers Samus in the enemy-projectile slot before scrolling.</summary>
    private void StepCeresElevatorArrival(SamusState samus, bool deathOwnsSamus)
    {
        // Main gameplay runs enemy projectiles immediately after Samus's
        // movement handler and before MainScrollingRoutine. The two fresh-Ceres actors
        // use that slot to lower Samus one pixel at a time; moving this call below the
        // camera would make the viewport trail one frame behind the game.
        if (!deathOwnsSamus && Room.CeresElevatorArrival is { IsComplete: false } arrival)
        {
            if (arrival.Step(samus))
            {
                // Command $0E installed a locked Samus handler before the projectiles
                // were spawned. Re-enable the translated ordinary movement path only
                // after the pad reaches its deletion height.
                samus.Control.Unlock();
                Player.GroundedSamusMovementEnabled = true;
            }
        }
    }

    /// <summary>Runs MainScrollingRoutine, its scrolling-finished hook, background streaming and the minimap.</summary>
    private void RunMainScrolling(SamusState samus, ScrollBoundaryCamera camera, FixedPosition<RoomPixels> previousCameraPoint, bool deathOwnsSamus)
    {
        // The door's destination fade runs enemy/draw owners but not the camera.
        // Its shared gate is set for ordinary doors as well as elevators.
        // Running the normal camera here consumes the upward scroll's +32 offset
        // before arrival resumes, exposing wrapped Samus art above the room.
        if (!deathOwnsSamus && !Room.LoadedScope.Enemies.Elevator.ElevatorDoorTransitionActive)
        {
            if (Room.LevelData is null)
                throw new InvalidOperationException("Gameplay camera tracking requires active room level data.");

            ActiveRoomGeometry roomGeometry = RoomLoader.GetActiveRoomGeometry();

            previousCameraPoint = RainbowBeamCameraCheckpoint.Consume(samus.ApplyPreviousPositionWrites(previousCameraPoint));
            var currentCameraPoint = samus.Position;

            // GameState_8 calls MainScrollingRoutine after movement/PLMs and before
            // DrawSamusEnemiesAndProjectiles regardless of which Samus input handler is
            // installed. A locked alpha suppresses controller transitions; it does not
            // suppress scrolling. That is why the station-eighteen gunship can carry
            // Samus down several screens while her ordinary movement handler is absent.
            if (samus.Grapple.SlowScrolling)
            {
                // A fast grapple swing takes the slow camera branch, which leaves
                // the camera speed words from the last normal frame in place.
                camera.TrackGrappleSlowScrolling(currentCameraPoint.Whole, Freeze.TimeIsFrozen);
            }
            else
            {
                camera.TrackMovedSamusHorizontally(
                    previousCameraPoint,
                    currentCameraPoint,
                    new HorizontalCameraContext(
                        KnockedBack: samus.KnockbackDirection != Samus.Movement.SamusKnockbackDirection.None,
                        MovementType: SamusPoseMetadata.ReadMovementType(samus),
                        XAccelerationMode: samus.HorizontalSpeed.AccelerationMode,
                        PoseXDirection: SamusPoseMetadata.ReadPoseXDirection(samus),
                        CameraDistanceIndex: Room.LoadedScope.Enemies.RoomMainHooks.ZebetiteCameraDistanceIndex),
                    Freeze.TimeIsFrozen);
                camera.TrackMovedSamusVertically(
                    previousCameraPoint,
                    currentCameraPoint,
                    new VerticalCameraContext(
                        YDirection: samus.Kinematics.YDirection,
                        UpScroller: roomGeometry.UpScroller,
                        DownScroller: roomGeometry.DownScroller),
                    Freeze.TimeIsFrozen);
            }
            // The camera writes the same distance-plus-one words consumed by Yard
            // kick setup. Publish the camera calculation itself, including its
            // integer-only sign test and previous scrolling checkpoint. A second
            // frame-end absolute delta loses both the bias and checkpoint timing.
            samus.HorizontalDistanceMovedPlusOne = camera.Speed.X;
            samus.ProjectileInheritance.CameraYSubspeed = camera.Speed.Y.Fraction;

            // MainScrollingRoutine invokes the mutable global
            // scrolling-finished hook after both axes have tracked Samus. Live Spore
            // Spawn installs its own hook, which owns the arena's vertical camera floor;
            // it must run before the layer scrolls and the sprite list consume layer-one position.
            Room.LoadedScope.Enemies.RoomMainHooks.RunSporeSpawnScrollingFinishedHook(camera);
            camera.FinishSamusScrolling(currentCameraPoint);

            // The scroll registers follow the camera in the same main-loop pass, so the
            // following frame shows the room from level data at the new position.
            BackgroundPhase.UpdateBackgroundScrollingFromCamera(Freeze.TimeIsFrozen);

            // UpdateMinimap belongs to Samus's normal frame-handler beta; it uses world
            // position, not camera position. Fresh debug sessions begin without an area
            // map, so only tiles actually visited by this runtime are revealed.
            Hud.UpdateMinimap(System, new MinimapFrame { AreaIndex = roomGeometry.AreaIndex, RoomMap = roomGeometry.MapPosition, RoomSize = Room.LevelData.Size, Samus = samus.Position.Whole, FrameCounter = Clock.ShortCounter, MapRevealMode = Assists.Current.MapReveal, PresentationMap = Artwork.MapPresentation?.Get(roomGeometry.AreaIndex) });
        }
    }

    /// <summary>Restores the attract demo's player input and draws Samus, enemies and projectiles.</summary>
    private void DrawGameplayActors(SuspendedGameplayFrameTail tail, bool deathOwnsSamus)
    {
        AttractDemo.RestoreAttractPlayerInput();

        RoomScope scope = Room.LoadedScope;
        ActorDrawing.DrawGameplayActors(scope, deathOwnsSamus ? SamusDrawOwner.DeathSequence : SamusDrawOwner.Gameplay,
            // Command zero's beta handler does not dispatch Samus's
            // palette effects. In particular, automatic Reserve recovery freezes a
            // stored shine instead of consuming it while Samus is stationary.
            tail.Mode.Beta is SamusBetaHandler.SuitPickup or SamusBetaHandler.ScriptLocked ? SamusPaletteEffects.Hold : SamusPaletteEffects.Advance);
    }

    /// <summary>Runs the escape-timer handler unless the gunship's beta handler owns the frame.</summary>
    private void RunEscapeTimerHandler(SuspendedGameplayFrameTail? frameTail)
    {
        // EnemyMain can install the gunship's beta handler on this frame.
        // It omits the normal timer-drawing call: neither countdown nor its sprites
        // survives boarding, even though timer status stays active. Decide after the
        // actor transition, before liftoff can repurpose the timer's OBJ tiles as dust.
        bool runEscapeTimerHandler = frameTail?.Mode.Beta != SamusBetaHandler.Gunship;
        if (runEscapeTimerHandler && EscapeTimer.Process(Clock.Counter, Assists.Current.EscapeTimerHoldsAtOneSecond))
        {
            // The expired countdown publishes the time-up game state for the
            // dispatcher, whites the target palettes and disables palette FX.
            TimeUpRequested = true;
            PaletteFxSession.DisableHandler();
        }
        if (runEscapeTimerHandler && EscapeTimer.IsActive)
        {
            var presentation = Artwork.MapPresentation ?? throw new InvalidOperationException(
                "Escape timer requires installed presentation assets.");
            EscapeTimerRenderer.Draw(EscapeTimer, Sprites, presentation.EscapeTimer, (Artwork.SpriteSheets ?? throw new InvalidOperationException(
                "The escape timer requires the installed sprite sheets.")).Get(SpriteSheetId.EscapeTimer));
        }
    }

    /// <summary>Rebuilds the HUD counters; the next accepted frame shows the rows as it latches them.</summary>
    private void UpdateHud(AmmoLoanGuard infiniteAmmoGuard)
    {
        // The HUD's three rows are rewritten during the update; they become visible at the next frame boundary,
        // which latches the HUD's images (AcceptedFrameBoundary, HudState.Shown).
        if (Hud.IsInitialized)
        {
            // The loan exists only so firing logic does not observe zero and
            // auto-cancel the selected weapon. Remove it before the HUD update converts the
            // live counters into HUD tile words; publishing first made the on-screen
            // minimum read two even though Snapshot later restored the actor to one.
            infiniteAmmoGuard.Complete(Player.Samus);
            // The HUD update rebuilds live energy/ammo words every gameplay frame.
            // Initialization alone is insufficient: Ridley contact and fireballs mutate
            // Samus during this frame, and those values must enter the next frame boundary.
            // PLM_Handler has not returned while a pickup message is open.
            // Its newly granted inventory reaches the HUD only after that return.
            if (Player.Samus is not null && !Room.LoadedScope.Plms.CollectiblePlms.HasPendingCollectibleMessage)
                Hud.UpdateGameplayCounters(Player.Samus, Freeze.TimeIsFrozen,
                    soundSuppressed: Room.LoadedScope.BombProjectiles.PowerBombExplosion.IsActive);
        }
    }

    /// <summary>Runs the scrolling sky, animated tiles, escape effects, the elevatube and the room main routines.</summary>
    private void RunRoomMains(RuntimeFramePolicy policy)
    {
        // Landing Site's room main advances the sky bands after ordinary gameplay logic.
        if (Room.ScrollingSky is not null && Room.Camera is not null)
            Room.ScrollingSky.ProcessFrame(Room.Camera.PixelPosition.Y, timeIsFrozen: Freeze.TimeIsFrozen);

        RoomMains.RunAnimatedTilesObjectHandler(Room.LoadedScope);
        EscapeEffects.StepEscapeRoomEffects(Room.LoadedScope);

        // Landing Site's room main is selected by the room state rather than by coordinates.
        // It runs at the common room-main seam after gameplay drawing and before shaking.
        if (Room.LoadedScope.MaridiaElevatube.IsActive)
        {
            if (Room.LevelData is null)
                throw new InvalidOperationException("Maridia elevatube has no active room level.");
            _ = Room.LoadedScope.MaridiaElevatube.Step(

                Room.LevelData,
                Player.Samus,
                Clock.Counter,
                Room.LoadedScope.Plms,
                Room.RoomMainCarryover);
        }

        // Execute the active room's room main at the same seam as Landing Site's
        // room main above: after gameplay drawing/HUD work and before global shaking. The
        // Ceres routine publishes a matrix for the following frame's presentation and may
        // change the outer dispatcher to state $20 only when its caller is state eight.
        RoomMains.LastCeresElevatorShaftRoomMain = Room.LoadedScope.CeresElevatorShaft.Step(
            Player.Samus,
            Room.LoadedScope.Enemies.Ceres.CeresStatus == CeresEscapeStatus.ElevatorDoorDestroyed,
            allowDeparture: policy.AllowCeresElevatorDeparture,
            Room.RoomMainCarryover);
        if (RoomMains.LastCeresElevatorShaftRoomMain.MatrixChanged)
            Display.ActiveTurningFloor = RoomMains.LastCeresElevatorShaftRoomMain.Transform;

        RoomMains.RunCeresFallingDebrisRoomMain(Room.LoadedScope);
        RoomMains.RunCrocomireComebackRoomMain(Room.LoadedScope);
        if (Room.ActiveRoom?.State.MainCallback == RoomMainCallback.HandleCeresRidleyGetawayCutscene)
            Room.LoadedScope.Enemies.Ceres.RunCeresRidleyGetawayRoomMain(Player.Samus, Clock.Counter);
    }

    /// <summary>Decrements Samus's hurt timers and the projectile interaction timer.</summary>
    private void DecrementHurtTimers()
    {
        // Main gameplay decrements these timers after Samus, enemies, drawing, HUD/BG
        // bookkeeping, room main ASM, the energy-zero check, and room shaking. Keep this
        // shared tail out of individual movement handlers: in particular, grapple spike
        // reactions set the knockback timers without installing the knockback handler.
        // Death game states do not execute the gameplay-state tail at all.
        if (Player.Samus is not null && !Player.Samus.DeathSequence.IsActive)
        {
            Player.Samus.DecrementHurtTimers();
            Room.LoadedScope.Projectiles.Firing.DecrementInteractionTimer();
        }
    }

    /// <summary>
    /// Invokes the pause-menu held-input filter with the controller sample latched by the
    /// most recent frame boundary. The game's caller passes reset value three.
    /// </summary>
    public void UpdatePauseHeldInput(int timerReset = 3)
    {
        System.UpdateHeldInput(timerReset, PhysicalController1.Current, PhysicalController1.NewlyPressed);
    }

    /// <summary>Applies the assists' end-of-update floors before publishing the runtime result.</summary>
    internal RuntimeFrameResult Snapshot(
        AmmoLoanGuard infiniteAmmoGuard)
    {
        // Every damage source applies its normal amount first. The assist intervenes
        // only at the single end-of-update seam where zero would otherwise be
        // observed by SuperMetroidGame and routed into the death states.
        if (Assists.Current.SamusCannotDie && Player.Samus is { Health: 0 } samus)
            samus.Health = 1;

        infiniteAmmoGuard.Complete(Player.Samus);

        return new RuntimeFrameResult();
    }

    /// <summary>
    /// Publishes the gameplay display at the end of an update: the most recent frame-boundary
    /// latch, the display state read while the frame is shown (the door transition's layers
    /// and the per-line effects), and the frame's colours. Publishing never advances
    /// simulation, and the result shares no storage with it.
    /// </summary>
    public GameplayDisplayFrame PublishDisplayFrame()
    {
        if (Room.ActiveDoor is null)
            throw new InvalidOperationException("A room and door must be loaded before publishing gameplay.");
        CrocomireDeathState? crocomireDeath = Room.LoadedScope.Enemies.RoomMainHooks.Crocomire is not null
            ? Room.LoadedScope.Enemies.RoomMainHooks.CrocomireDeath : null;
        MotherBrainEnemyState? motherBrain = Room.LoadedScope.Enemies.DisplaySources.MotherBrain;
        ScrollingSkyState? sky = Room.ScrollingSky;
        var frameEnd = new GameplayFrameEndDisplayInputs(
            Room.DoorTransitionMainScreenLayers,
            sky?.Publish(Room.Camera?.PixelPosition.Y ?? throw new InvalidOperationException("Scrolling sky has no gameplay camera.")),
            crocomireDeath is { MeltingScrollActive: true } melting ? melting.MeltDistortion : null,
            motherBrain is { RisingFloorClipActive: true },
            motherBrain?.RainbowBeam.RenderSnapshot,
            CeresHazeRenderSnapshot.Capture(Room.LoadedScope.CeresHaze),
            Artwork.MapPresentation?.RoomFxPaletteBlends,
            PowerBombRenderSnapshot.Capture(Room.LoadedScope.BombProjectiles.PowerBombExplosion),
            MessageBoxLayer.Capture(MessageBox, Room.Scope?.MessageBoxes),
            SuitPickup.IsActive ? SuitPickup.Beam : null,
            SamusXrayDisplaySnapshot.Capture(Player.Samus, Player.LastXrayBeamStep),
            Room.LoadedScope.TourianStatues.Enabled);
        return new(Display.Latched, frameEnd, new FramePaint(PublishShading(),
            Player.Colours.ShadeOwnDraws(DisplayedSprites.Draws)));
    }

    /// <summary>
    /// The published frame's gameplay shading (#1512): each owner of a colour effect names the shades and effects its
    /// state shows, in the order later colour writes won over earlier ones: the room's palette effects and layer-3 colours,
    /// then its bosses', then the door's fade over all of them.
    /// </summary>
    private GameplayShading PublishShading()
    {
        var shading = new GameplayShadingBuilder();
        foreach (IGameplayShadingSource source in ShadingSources())
            source.ShadeGameplay(shading);
        return shading.Build(Room.LoadedScope.Assets.BlockSheet.Sheet.Image);
    }

    /// <summary>The owners of the colour effects the frame shows, in the order they are laid over each other.</summary>
    private IEnumerable<IGameplayShadingSource> ShadingSources()
    {
        yield return Room.LoadedScope.PaletteFx;
        yield return Room.LoadedScope.Layer3Fx;
        foreach (IGameplayShadingSource boss in Room.LoadedScope.Enemies.DisplaySources.ShadingSources)
            yield return boss;
        yield return DoorFade;
        // The death explosion's whiteout writes over the colours the death fade left black, so it shows instead of it.
        if (Player.Samus is { DeathSequence.Whiteout: not null } dying)
            yield return dying.DeathSequence;
        else
            yield return FadeOut;
    }

    /// <summary>
    /// Gameplay-frame state the remainder of state eight reads after <c>PLM_Handler</c>.
    /// The message routine runs inside that handler, so a PLM that opens a box
    /// suspends the frame; the port parks this state until the box closes.
    /// </summary>
    /// <param name="Mode">The frame's installed alpha and beta handlers.</param>
    /// <param name="PreviousCameraPoint">Samus's camera point carried to the frame tail.</param>
    internal readonly record struct SuspendedGameplayFrameTail(
        SamusControlMode Mode,
        FixedPosition<RoomPixels> PreviousCameraPoint);

    /// <summary>State-eight tail parked when a PLM opens a message box during a gameplay frame.</summary>
    private SuspendedGameplayFrameTail? _suspendedFrameTail;

    /// <summary>
    /// Changes the assists in the middle of play without rebuilding any game state: a newly granted loadout reaches the
    /// current Samus, and a statue room on screen when its statues newly stand open restarts with them open.
    /// </summary>
    /// <param name="assists">The assists to play under from now on.</param>
    internal void ApplyAssists(GameplayAssists assists)
    {
        Ensure.NotNull(assists);
        bool statuesOpening = !Assists.Current.TourianStatuesOpen && assists.TourianStatuesOpen;
        Assists.Rebind(assists);
        if (statuesOpening && Room.LoadedScope.TourianStatues.Enabled) Room.LoadedScope.TourianStatues.RestartWithStatuesOpen(Room.LoadedScope.Plms);
    }

    /// <summary>The session owners each room's Tourian statue sequence is built over.</summary>
    internal TourianStatueSequenceDependencies TourianStatueDependencies { get; }

    /// <summary>
    /// One update of an open message box. Its completion returns into the suspended
    /// gameplay update, whose remaining owners already ran when the box opened.
    /// </summary>
    internal RuntimeFrameResult StepMessageBoxFrame(
        ControllerButton controller1Input,
        RuntimeFramePolicy policy,
        AmmoLoanGuard infiniteAmmoGuard)
    {
        MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = false;
        // The box reads the controller directly; the frame boundary latches none.
        MessageBox.Step(controller1Input);
        if (MessageBox.LastFrameAudio.AdvancesPerLineEffects)
        {
            // The message box's restoring update runs the per-line effects too.
            if (Room.Camera is not null)
                PerLineEffects.Advance(Room.LoadedScope.Plms, Room.LoadedScope);
            if (Player.Samus is { } samus)
                Room.LoadedScope.BombProjectiles.AdvancePowerBombExplosion(samus, Controller1.Current, DispatchGameState);
        }
        if (MessageBox.ConfirmationSelectionChangedThisFrame)
            MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = true;
        if (MessageBox.IsActive)
            return Snapshot(infiniteAmmoGuard);

        MessageBoxes.ResolveClosedMessageBox(frameSuspended: _suspendedFrameTail is not null, Room.LoadedScope.Plms, Room.LoadedScope.BombProjectiles, Room.LoadedScope);
        // PLM_Handler returns once the box and its item routine finish; the
        // parked remainder of that gameplay frame runs now, in this same dispatch.
        if (!MessageBox.IsActive && _suspendedFrameTail is { } tail)
        {
            _suspendedFrameTail = null;
            return FinishGameplayFrame(tail, policy, infiniteAmmoGuard, entry: GameplayFrameEntry.ResumedAfterMessageBox);
        }
        return Snapshot(infiniteAmmoGuard);
    }
}
