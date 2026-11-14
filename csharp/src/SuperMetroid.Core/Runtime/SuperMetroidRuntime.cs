using SuperMetroid.Core.Bosses.Artwork;
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
using SuperMetroid.Core.Fx.ColourEffects;
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
/// Incremental, frame-steppable C# runtime shell for the Super Metroid systems.
/// </summary>
/// <remarks>
/// Each update begins at the frame boundary, which shows the previous update's frame, latches
/// input and advances the frame counters; the update's gameplay then runs. Tools and viewers
/// step one update at a time through the same boundary.
/// </remarks>
internal sealed class SuperMetroidRuntime
{
    /// <summary>The liquid wave and statue descent each update advances before its random number is drawn.</summary>
    internal LiquidWaveAndStatueDescent LiquidWaveAndStatueDescent { get; }

    /// <summary>
    /// Where the liquid wave and statue descent fall among the random draws; created with the runtime, whose door transitions stop
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

    /// <summary>The <see cref="RoomObjectFramePhase"/> owner of this SuperMetroidRuntime.</summary>
    internal RoomObjectFramePhase RoomObjectPhase { get; }

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

    /// <summary>
    /// The sound and music requests the current update makes, in the order its systems make them (#1620): every room
    /// system and Samus queue theirs here as they run, and a session's go straight on into its queues.
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; }

    /// <summary>Sends the runtime's later audio requests straight on into <paramref name="audio"/>, the queues of the session running it.</summary>
    /// <exception cref="InvalidOperationException">The runtime's requests already reach a session's queues.</exception>
    internal void AttachSessionAudio(AudioQueues audio) =>
        AudioRequests.AttachTo(new SessionGameplayAudioQueues(audio, AttractSession));

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

    /// <summary>The <see cref="RoomObjectVisuals"/> owner of this SuperMetroidRuntime.</summary>
    internal RoomObjectVisuals ShotBlockVisuals { get; }

    /// <summary>The <see cref="GameplayActorDrawing"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayActorDrawing ActorDrawing { get; }

    /// <summary>
    /// What Mother Brain's rainbow-beam push hides from the camera this update, injected into the enemy session and consumed by
    /// the update's scrolling.
    /// </summary>
    internal RainbowBeamCameraCheckpoint RainbowBeamCameraCheckpoint { get; } = new();

    /// <summary>
    /// The step count every gradual fade shares, created with the runtime and handed to the owners whose fades count on it:
    /// the door transition's fades, the death and time-up fades of <see cref="FadeOut"/>, and every room's enemies, whose
    /// Kraid backdrop and Torizo body fades count on it too. It outlives every room.
    /// </summary>
    internal SharedFadeStepCount FadeStepCount { get; } = new();

    /// <summary>
    /// The session's unauthored Speed Booster cadence, which the runtime hands its room loader and attract demo for every
    /// Samus they build and every room's enemies, whose Ceres Ridley getaway leaves its delays behind.
    /// </summary>
    internal UnauthoredBoostCadence UnauthoredBoostCadence { get; }

    /// <summary>
    /// The session's camera fractions kept across games: the runtime's first room's camera starts from them, and
    /// <see cref="LeaveCameraFractions"/> keeps this runtime's when the session releases it.
    /// </summary>
    internal CameraFractionsKeptAcrossGames CameraFractions { get; }

    /// <summary>
    /// The compatibility services reproducing what Samus's glitch paths read past their data, from the installed content;
    /// the runtime hands each to the owner that consults it as it builds that owner and every Samus and room.
    /// </summary>
    internal SamusOverreadServices SamusOverreads { get; }

    /// <summary>The <see cref="XrayReveal"/> owner of this SuperMetroidRuntime.</summary>
    internal XrayReveal Xray { get; }

    /// <summary>The layers gameplay chooses to show and the accepted frame shows.</summary>
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

    /// <summary>Creates the frame-steppable runtime under its assists, Samus in her starting shades.</summary>
    /// <param name="assists">The assists the runtime plays under; <see cref="GameplayAssists.None"/> for the unassisted game.</param>
    /// <param name="artwork">
    /// The installed catalogs; their Samus overread services
    /// are handed to the owners that consult them, their enemy and boss overread services, enemy sheets and enemy frame
    /// displays and the rainbow-beam camera checkpoint are injected into the enemy session it builds, and
    /// <see cref="BindArtwork"/> binds the rest.
    /// </param>
    /// <param name="session">
    /// What the session keeps across its runtimes: the frame clock it shares with the frontend; what the shared game state
    /// continues from (the random number generator, the power-on earthquake delay and the live and stored explored maps);
    /// and the compatibility services kept across games (the unauthored Speed Booster cadence its enemies and every Samus
    /// share, the save confirmation's input cadence and the camera fractions).
    /// </param>
    internal SuperMetroidRuntime(GameplayAssists assists, IGameplayArtwork artwork, GameplaySessionCarryover session)
    {
        Ensure.NotNull(artwork);
        Ensure.NotNull(session);
        FrameClock clock = session.Clock;
        UnauthoredBoostCadence = Ensure.NotNull(session.Compatibility.UnauthoredBoostCadence);
        CameraFractions = Ensure.NotNull(session.Compatibility.CameraFractions);
        FadeOut = new GameplayFadeOut(FadeStepCount);
        MessageBox = new GameplayMessageBoxState(session.Compatibility.SaveChoiceInput);
        SamusOverreads = artwork.SamusOverreads ?? throw new InvalidOperationException(
            "Gameplay requires the installed Samus overread services.");
        System = new SharedGameState(session.GameState.Random, session.GameState.EarthquakeSoundDelay, session.GameState.StoredExploredMaps);
        Display = new GameplayDisplayState();
        Player = new SamusFrameState();
        Room = new ActiveRoomState();
        Clock = Ensure.NotNull(clock);
        Options = new GameplayOptions();
        AudioRequests = new GameplayAudioRequests();
        if (session.Audio is { } audio)
            AttachSessionAudio(audio);
        EnemySession = new EnemySession(
            artwork.EnemyOverreads ?? throw new InvalidOperationException("Gameplay requires the installed enemy overread services."),
            artwork.BossAndIntroOverreads ?? throw new InvalidOperationException("Gameplay requires the installed boss and intro overread services."),
            RainbowBeamCameraCheckpoint,
            artwork.EnemySheets ?? throw new InvalidOperationException("Gameplay requires the installed enemy sheets."),
            artwork.EnemyFrameDisplays ?? throw new InvalidOperationException("Gameplay requires the installed enemy frame displays."));

        BeamCharge = new BeamChargeState();
        Freeze = new GameplayTimeFreeze(Player);
        Assists = new ActiveAssists(Player, System, assists);
        TourianStatueDependencies = new TourianStatueSequenceDependencies { Player = Player, Room = Room, System = System, Assists = Assists, Freeze = Freeze };
        Artwork = new RuntimeArtwork(new RuntimeArtworkDependencies { BossArtwork = BossArtwork, Room = Room, Hud = Hud, Player = Player });
        HudSetup = new GameplayHudSetup(new GameplayHudSetupDependencies { Artwork = Artwork, Hud = Hud });
        ScreenLayers = new GameplayScreenLayers(Room);
        Xray = new XrayReveal(BackgroundScroll, Room, System);
        FrameBoundary = new AcceptedFrameBoundary(new AcceptedFrameBoundaryDependencies { Input = new AcceptedFrameInput(Controller1, PhysicalController1, Options, Clock), EnemySession = EnemySession, Hud = Hud, BackgroundScroll = BackgroundScroll, Display = Display, DisplayedSprites = DisplayedSprites, ScreenLayers = ScreenLayers, Sprites = Sprites, Player = Player, Room = Room, SuitPickup = SuitPickup, Xray = Xray });
        BackgroundPhase = new BackgroundScrollPhase(BackgroundScroll, Room);
        ActorDrawing = new GameplayActorDrawing(new GameplayActorDrawingDependencies { Artwork = Artwork, AttractSession = AttractSession, Controller1 = Controller1, Display = Display, Freeze = Freeze, Clock = Clock, Sprites = Sprites, Player = Player, Room = Room });
        EnemyRoomEffects = new EnemyRoomEffects(new EnemyRoomEffectsDependencies { EscapeTimer = EscapeTimer, Player = Player, Room = Room, System = System});
        EnemyPhase = new EnemyFramePhase(new EnemyFramePhaseDependencies { BackgroundScroll = BackgroundScroll, Controller1 = Controller1, Display = Display, EnemyRoomEffects = EnemyRoomEffects, Freeze = Freeze, Clock = Clock, Sprites = Sprites, Player = Player, Room = Room});
        LiquidWaveAndStatueDescent = new LiquidWaveAndStatueDescent(Freeze, System, EffectPhaseOrder);
        DoorTransition = new DoorTransitionRunner(new DoorTransitionRunnerDependencies { ActorDrawing = ActorDrawing,  Display = Display, EnemyPhase = EnemyPhase, LiquidWaveAndStatueDescent = LiquidWaveAndStatueDescent, Clock = Clock, FrameBoundary = FrameBoundary, Sprites = Sprites, Player = Player, Room = Room, System = System});
        EscapeEffects = new EscapeRoomEffects(Freeze, Clock, Room, System);
        ShotBlockVisuals = new RoomObjectVisuals();
        RoomLoader = new RoomLoader(new RoomLoaderDependencies { AudioRequests = AudioRequests, EnemySession = EnemySession, BossArtwork = BossArtwork, RoomFx = RoomFx, ColourEffectSession = ColourEffectSession, TourianStatueDependencies = TourianStatueDependencies, ShotBlockVisuals = ShotBlockVisuals, BeamCharge = BeamCharge, Artwork = Artwork, BackgroundScroll = BackgroundScroll, Controller1 = Controller1, Display = Display, DoorTransition = DoorTransition, EnemyRoomEffects = EnemyRoomEffects, EscapeEffects = EscapeEffects, GameTime = GameTime, Hud = Hud, Clock = Clock, DoorFrameCounting = DoorFrameCounting, Options = Options, Player = Player, Room = Room, System = System, Assists = Assists, SamusOverreads = SamusOverreads, UnauthoredBoostCadence = UnauthoredBoostCadence, CameraFractions = CameraFractions, FadeStepCount = FadeStepCount });
        AttractDemo = new AttractDemoDriver(new AttractDemoDriverDependencies { UnauthoredBoostCadence = UnauthoredBoostCadence, Artwork = Artwork, AttractSession = AttractSession, Controller1 = Controller1, HudSetup = HudSetup, FrameBoundary = FrameBoundary, Player = Player, Room = Room, RoomLoader = RoomLoader, System = System, FramesBeyondAnimation = SamusOverreads.FramesBeyondAnimation, BeamCharge = BeamCharge });
        DeathSequence = new DeathSequenceRunner(new DeathSequenceRunnerDependencies { Artwork = Artwork, Display = Display, FrameBoundary = FrameBoundary, Sprites = Sprites, Player = Player, Room = Room});
        MessageBoxes = new MessageBoxFlow(new MessageBoxFlowDependencies { AudioRequests = AudioRequests, Freeze = Freeze, Hud = Hud, MessageBox = MessageBox, Player = Player, Room = Room, SuitPickup = SuitPickup, System = System });
        RoomObjectPhase = new RoomObjectFramePhase(new RoomObjectFramePhaseDependencies { Controller1 = Controller1, EnemyRoomEffects = EnemyRoomEffects, Freeze = Freeze, MessageBox = MessageBox, MessageBoxes = MessageBoxes, Player = Player, Room = Room, SuitPickup = SuitPickup});
        RoomMains = new RoomMainRoutines(new RoomMainRoutinesDependencies { BackgroundScroll = BackgroundScroll, Player = Player, Room = Room, System = System});
        SamusPhases = new SamusFramePhases(new SamusFramePhasesDependencies { Artwork = Artwork, AttractDemo = AttractDemo, Controller1 = Controller1, Freeze = Freeze, GameTime = GameTime, Clock = Clock, Options = Options, Assists = Assists, Player = Player, Room = Room, System = System });

        // Gameplay starts with Samus in her starting shades; every other picture shows its standard colours (#1512).
        Player.Colours.ShowStarting();
    }

    /// <summary>
    /// Binds the installed catalogs to the runtime's owners: every catalog when <paramref name="previous"/> is null (a new
    /// runtime), otherwise only the catalogs whose reference changed.
    /// </summary>
    /// <remarks>
    /// The map presentation binds first: its setter pushes the HUD, message-box, effect overlay and Samus palette bindings and
    /// rebinds the queued artwork that later catalogs' setters may read.
    /// </remarks>
    /// <param name="artwork">The catalogs to bind.</param>
    /// <param name="previous">The catalogs bound before, or <see langword="null"/> to bind every catalog.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="artwork"/> names enemy sheets or enemy frame displays other than those the runtime was built with.
    /// </exception>
    internal void BindArtwork(IGameplayArtwork artwork, IGameplayArtwork? previous)
    {
        Ensure.NotNull(artwork);
        // The enemy session holds the sheets and frame displays it was built with for the runtime's lifetime (#1616).
        if (!ReferenceEquals(artwork.EnemySheets, EnemySession.Sheets) || !ReferenceEquals(artwork.EnemyFrameDisplays, EnemySession.FrameDisplays))
            throw new InvalidOperationException("A runtime draws its enemies from the sheets and frame displays it was built with.");
        void Bind<T>(Func<IGameplayArtwork, T?> catalog, Action<T?> bind) where T : class
        {
            T? value = catalog(artwork);
            if (previous is null || !ReferenceEquals(catalog(previous), value))
                bind(value);
        }

        Bind(a => a.MapPresentation, value => Artwork.MapPresentation = value);
        Bind(a => a.SamusBodyArt, value => Artwork.SamusBodyArt = value);
        Bind(a => a.RoomBlockSheets, value => Artwork.RoomBlockSheets = value);
        Bind(a => a.RoomVisualLayouts, value => Artwork.RoomVisualLayouts = value);
        Bind(a => a.RoomLevelStreams, value => Artwork.RoomLevelStreams = value);
        Bind(a => a.RoomObjectShotBlockVisuals, value => ShotBlockVisuals.RoomObjectShotBlockVisuals = value);
        Bind(a => a.RoomObjectGrappleBlockVisuals, value => ShotBlockVisuals.RoomObjectGrappleBlockVisuals = value);
        Bind(a => a.RoomObjectStationVisuals, value => ShotBlockVisuals.RoomObjectStationVisuals = value);
        Bind(a => a.RoomObjectBlueDoorVisuals, value => ShotBlockVisuals.RoomObjectBlueDoorVisuals = value);
        Bind(a => a.RoomObjectColoredDoorVisuals, value => ShotBlockVisuals.RoomObjectColoredDoorVisuals = value);
        Bind(a => a.RoomObjectGreyDoorVisuals, value => ShotBlockVisuals.RoomObjectGreyDoorVisuals = value);
        Bind(a => a.RoomObjectEyeDoorVisuals, value => ShotBlockVisuals.RoomObjectEyeDoorVisuals = value);
        Bind(a => a.RoomObjectMotherBrainGlassVisuals, value => ShotBlockVisuals.RoomObjectMotherBrainGlassVisuals = value);
        Bind(a => a.RoomObjectNoobTubeVisuals, value => ShotBlockVisuals.RoomObjectNoobTubeVisuals = value);
        Bind(a => a.RoomObjectDownwardGateVisuals, value => ShotBlockVisuals.RoomObjectDownwardGateVisuals = value);
        Bind(a => a.RoomObjectElevatorPlatformVisuals, value => ShotBlockVisuals.RoomObjectElevatorPlatformVisuals = value);
        Bind(a => a.RoomObjectEscapeGateVisuals, value => ShotBlockVisuals.RoomObjectEscapeGateVisuals = value);
        Bind(a => a.RoomObjectBombTorizoHandVisuals, value => ShotBlockVisuals.RoomObjectBombTorizoHandVisuals = value);
        Bind(a => a.RoomObjectDraygonCannonVisuals, value => ShotBlockVisuals.RoomObjectDraygonCannonVisuals = value);
        Bind(a => a.RoomObjectPhantoonDoorVisuals, value => ShotBlockVisuals.RoomObjectPhantoonDoorVisuals = value);
        Bind(a => a.RoomObjectChozoStatueVisuals, value => ShotBlockVisuals.RoomObjectChozoStatueVisuals = value);
        Bind(a => a.RoomObjectLinkedRestoreVisuals, value => ShotBlockVisuals.RoomObjectLinkedRestoreVisuals = value);
        Bind(a => a.RoomObjectTourianAccessVisuals, value => ShotBlockVisuals.RoomObjectTourianAccessVisuals = value);
        Bind(a => a.RoomObjectSpeedBoosterVisuals, value => ShotBlockVisuals.RoomObjectSpeedBoosterVisuals = value);
        Bind(a => a.RoomObjectMaridiaElevatubeVisuals, value => ShotBlockVisuals.RoomObjectMaridiaElevatubeVisuals = value);
        Bind(a => a.RoomObjectSporeSpawnCeilingVisuals, value => ShotBlockVisuals.RoomObjectSporeSpawnCeilingVisuals = value);
        Bind(a => a.RoomObjectSamusEaterVisuals, value => ShotBlockVisuals.RoomObjectSamusEaterVisuals = value);
        Bind(a => a.RoomObjectBotwoonWallVisuals, value => ShotBlockVisuals.RoomObjectBotwoonWallVisuals = value);
        Bind(a => a.RoomObjectKraidVisuals, value => ShotBlockVisuals.RoomObjectKraidVisuals = value);
        Bind(a => a.RoomObjectCrocomireVisuals, value => ShotBlockVisuals.RoomObjectCrocomireVisuals = value);
        Bind(a => a.RoomObjectMotherBrainFakeDeathVisuals, value => ShotBlockVisuals.RoomObjectMotherBrainFakeDeathVisuals = value);
        Bind(a => a.RoomObjectCollectibleVisuals, value => ShotBlockVisuals.RoomObjectCollectibleVisuals = value);
        Bind(a => a.RoomObjectRevealAndWallVisuals, value => ShotBlockVisuals.RoomObjectRevealAndWallVisuals = value);
        Bind(a => a.RoomObjectSetupBlocks, value => ShotBlockVisuals.RoomObjectSetupBlocks = value);
        Bind(a => a.XrayRevealVisuals, value => Xray.XrayRevealVisuals = value);

        Bind(a => a.ProjectileFrameBindings, value => Artwork.ProjectileFrameBindings = value);
        Bind(a => a.SpriteSheets, value => Artwork.SpriteSheets = value);
        Bind(a => a.ChargeFlarePlacement, value => Artwork.ChargeFlarePlacement = value);
        Bind(a => a.GrappleFlarePlacement, value => Artwork.GrappleFlarePlacement = value);
        Bind(a => a.GrappleSwingFrames, value => Artwork.GrappleSwingFrames = value);

        Bind(a => a.KraidEyes, value => BossArtwork.KraidEyes = value);
        Bind(a => a.BossBodySheets, value => BossArtwork.BodySheets = value);
    }

    /// <summary>
    /// Set when the escape countdown expired during this runtime's frame: the countdown
    /// writes the time-up game state from inside gameplay. The frontend game-state loop consumes it.
    /// </summary>
    public bool TimeUpRequested { get; private set; }

    /// <summary>Takes the pending time-up publication; the game-state loop may still override it.</summary>
    public bool ConsumeTimeUpRequest()
    {
        bool requested = TimeUpRequested;
        TimeUpRequested = false;
        return requested;
    }

    /// <summary>Shared random/event/input-filter state, over the session's random number generator.</summary>
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
    /// Frontend language choice read by room actors whose script differs
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

    /// <summary>Whether the colour effects run, and the heat phases, which outlive each room's colour effects.</summary>
    public ColourEffectSessionState ColourEffectSession { get; } = new();

    /// <summary>The colours a door's fades show, which the door transition steps (#1512).</summary>
    internal DoorFade DoorFade { get; } = new();

    /// <summary>
    /// The death fade and the time-up whiteout of the whole display, which their game states step (#1512), on the runtime's
    /// <see cref="FadeStepCount"/>.
    /// </summary>
    internal GameplayFadeOut FadeOut { get; }

    /// <summary>The sprite frame the update is building for the next frame boundary: sprites plus whole-frame draws.</summary>
    public SpriteFrameBuffer Sprites { get; } = new();

    /// <summary>
    /// The sprite frame the most recent frame boundary made visible. Renderers draw it while the current update
    /// prepares <see cref="Sprites"/>.
    /// </summary>
    public SpriteFrameBuffer DisplayedSprites { get; } = new();

    /// <summary>Active room's enemy system.</summary>
    public EnemySession EnemySession { get; }

    /// <summary>The installed boss artwork every room's bosses read.</summary>
    public BossArtworkBinding BossArtwork { get; } = new();

    /// <summary>The last enemy pass's results, for readers after the frame.</summary>
    internal IEnemyFrameResults EnemyFrameResults => Room.LoadedScope.Enemies;

    /// <summary>
    /// The game state whose phase the frontend is now running. Samus's control setups test it from
    /// inside the update; runtime-only hosts run main gameplay.
    /// </summary>
    public SuperMetroid.Core.Progression.SuperMetroidGameState RunningGameState { get; set; } = SuperMetroid.Core.Progression.SuperMetroidGameState.MainGameplay;

    /// <summary>Whether a power-bomb explosion is under way; X-ray setup and the pause check reject one, and the sound queues turn every sound away during one.</summary>
    /// <remarks>Before the first room exists there is no explosion.</remarks>
    public bool PowerBombExplosionActive => Room.BombProjectiles?.PowerBombExplosion.IsActive == true;

    /// <summary>The Fireflea state, the room's fixed colour and installed FX artwork, which outlive each room's effect overlay FX.</summary>
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

    /// <summary>The layers' positions and views, fed by the active camera; the frame boundary latches the views the screen shows.</summary>
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
            // The desktop error reporter deliberately catches modelled exceptions and
            // attempts another frame. A finally seam is therefore mandatory: otherwise a
            // frame that throws after borrowing the last round leaves live ammo at two.
            infiniteAmmoGuard.Complete(Player.Samus);
            // A failed demo frame must not leak scripted input into the player latch.
            AttractDemo.RestoreAttractPlayerInput();
        }
    }

    /// <summary>
    /// Advances one frame boundary and the corresponding gameplay update while the
    /// frame-exit ammo guard remains active through callbacks.
    /// </summary>
    private RuntimeFrameResult StepFrameGuarded(
        ControllerButton controller1Input,
        RuntimeFrameRequest frameRequest,
        AmmoLoanGuard infiniteAmmoGuard)
    {
        // The update's audio record starts empty, so an update that skips a system records nothing of an earlier one.
        AudioRequests.BeginUpdate();
        RebindRoutingReferences();
        // An open message box runs its own updates inside the suspended gameplay update:
        // the frame boundary latches no input and no random number is drawn nor owner run.
        if (MessageBox.IsActive)
            return StepMessageBoxFrame(controller1Input, frameRequest.Policy, infiniteAmmoGuard);
        BeginAcceptedGameplayFrame(controller1Input, frameRequest);
        RunLiquidWaveAndStatueDescent();
        AdvanceMainLoopRandom();
        RunEffectsBeforePhase();
        var context = new GameplayFrameContext(SampleSamusCameraPoint());
        PrepareEnemyProcessingList();
        RunColourEffects();
        BeginSpriteFrame();
        PrepareEnemiesAndHoistedEnemyMain(context);
        if (Player.Samus is not null && Room.Camera is not null)
        {
            RunSamusAndEnemyMain(context, Player.Samus, Room.Camera, frameRequest.Audio);
            bool deathOwnsSamus = context.Samus.Mode.Movement == SamusMovementMode.Fatal;
            if (!deathOwnsSamus)
                DoorTransition.RunEnemyProjectileHandler(Room.LoadedScope.BombProjectiles, Room.LoadedScope);
            RoomObjectPhase.RunObjectPhase(deathOwnsSamus, Player.Samus, Room.LoadedScope.RoomObjects, Room.LoadedScope.BombProjectiles, Room.LoadedScope);
            OpenGunshipPrompt();
            context.Tail = new SuspendedGameplayFrameTail(context.Samus.Mode, context.Samus.PreviousCameraPoint);
        }
        if (SuspendTailForMessageBox(context))
            return Snapshot(infiniteAmmoGuard);
        return FinishGameplayFrame(context.Tail, frameRequest.Policy, infiniteAmmoGuard);
    }

    /// <summary>Points the cross-owner routing at this frame's room owners and Samus, then applies pending Chozo statue room objects.</summary>
    private void RebindRoutingReferences()
    {
        // Samus and the room's owners are made on separate paths, so the frame's entry, before any producer executes,
        // is where both exist to be connected.
        Artwork.BindGrapplePresentation();
        RoomScope scope = Room.LoadedScope;
        Artwork.BindProjectiles(scope.Projectiles, scope.BombProjectiles);
        scope.Projectiles.Earthquakes.BindRoomEarthquake(scope.Enemies.Earthquake);
        scope.BindAudioRequests(AudioRequests);
        Player.Samus?.BindAudioRequests(AudioRequests);
        EnemyRoomEffects.ApplyPendingChozoStatueObjects(scope.RoomObjects, scope.FxOverlay, Room.LoadedScope.Enemies);
    }

    /// <summary>Marks the frame's impacts as gameplay's, runs the accepted-frame boundary and the host's post-boundary work, and chooses the screen layers.</summary>
    private void BeginAcceptedGameplayFrame(ControllerButton controller1Input, RuntimeFrameRequest frameRequest)
    {
        Room.LoadedScope.Projectiles.ImpactAudio.BeginImpactAudioFrame(context: ImpactAudioContext.Gameplay);
        FrameBoundary.BeginAcceptedFrame(controller1Input);
        frameRequest.AfterFrameBoundary();
        ScreenLayers.Choose(Room.LoadedScope);
    }

    /// <summary>Advances the liquid wave and statue descent and the power bomb's shape step before the main loop draws its random number.</summary>
    private void RunLiquidWaveAndStatueDescent()
    {
        // The liquid wave and the statues read the random number before the main loop draws the next one. A
        // message box's wait does not run them.
        if (Room.Camera is not null && !MessageBox.IsActive)
            LiquidWaveAndStatueDescent.Advance(Room.LoadedScope.RoomObjects, Room.LoadedScope);

        // The power bomb blast belongs to the outer frame, not Samus alpha. A statue carry
        // takes over her movement while the already-spawned blast continues normally.
        // Its cleanup resumes Samus's sounds, which can queue them before the Samus handler.
        if (Player.Samus is { } samus && !MessageBox.IsActive)
            Room.LoadedScope.BombProjectiles.AdvancePowerBombExplosion(samus, Controller1.Current, RunningGameState);
    }

    /// <summary>Draws the update's random number.</summary>
    private void AdvanceMainLoopRandom()
    {
        // Every update draws a random number right after the liquid wave and statue descent
        // (EffectPhaseRandomOrder) and before the game state's own work. Several room and
        // enemy behaviours only sample the random number instead of advancing it themselves;
        // the falling-tile room is the first visibly obvious example: without this draw,
        // every ceiling fragment selects the same X table entry forever. Keep this
        // unconditional and at the start of the update; advancing only at a consumer would
        // change the shared sequence.
        System.NextRandom();
    }

    /// <summary>Runs the owners before the game state's phase: Ceres haze, the suit transformation, Phantoon's wave and blending, and room effect overlay FX.</summary>
    private void RunEffectsBeforePhase()
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
            waveBoss.BodyWave.Step(waveBoss);
            waveBoss.BodyVisibility.Step(waveBoss);
        }
        if (Room.Camera is not null)
        {
            Room.LoadedScope.FxOverlay.Step(Room.LoadedScope.ArtworkFrames, new RoomFxOverlayFrame { Camera = Room.Camera.PixelPosition, TimeIsFrozen = Freeze.TimeIsFrozen, EarthquakeSoundDelay = System.EarthquakeSoundDelay, RandomNumber = System.RandomNumber, FirefleaDarknessStep = Room.LoadedScope.Enemies.RoomStatus.FirefleaDarknessStep, LiquidMotionAlreadyAdvanced = Room.LoadedScope.FxOverlay.MovesLiquidBeforeMainLoop });
            // A lava or acid rise sustains the room's earthquake. Apply it before the ordinary shake handler later in
            // this frame so its first displacement is visible immediately.
            if (Room.LoadedScope.FxOverlay.SustainedEarthquake is { } risingLiquidShake)
                Room.LoadedScope.Enemies.Earthquake.Sustain(risingLiquidShake);
            if (Player.Samus is not null && Room.LoadedScope.FxOverlay.Type is
                    RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid)
                Player.Samus.LiquidPhysics.ConfigureFor(Room.LoadedScope.FxOverlay);
        }
    }

    /// <summary>
    /// Samus's position before this frame's movement: the camera fallback for a newly loaded or restored camera.
    /// </summary>
    /// <remarks>
    /// The main scrolling saves Samus's previous position after scrolling, not at
    /// frame start. This fallback is only for a newly loaded/restored camera; otherwise
    /// retain the preceding scrolling checkpoint so later room-main movement (such as
    /// the elevatube) contributes to the next frame's distance just like enemy movement.
    /// </remarks>
    private FixedPosition<RoomPixels>? SampleSamusCameraPoint() => Player.Samus is null
        ? null
        : Player.Samus.Position;

    /// <summary>Builds the enemy processing list that the grapple scan and the enemy phase use.</summary>
    private void PrepareEnemyProcessingList()
    {
        // Main gameplay begins by determining which enemies to process,
        // before colour effects and Samus. The grapple scan uses this list.
        if (Room.Camera is not null && Room.LoadedScope.Enemies.IsLoaded)
            Room.LoadedScope.Enemies.ProcessingWindow.PrepareEnemyProcessingList(Room.Camera.PixelPosition);
    }

    /// <summary>Runs the room's colour effects and Hyper Beam's colour effect.</summary>
    private void RunColourEffects()
    {
        // Main gameplay runs the colour effects before Samus's update and
        // before enemies are processed. The Hyper Beam is equipped by the
        // Baby during that later enemy phase, so a newly started Hyper Beam colour effect naturally
        // begins on the following frame. Debug scripts that grant Hyper Beam before their
        // first StepFrame get the same effects-first behavior immediately.
        // The room's colour effects take their turns first (ColourEffectTurnOrder): room FX starts first,
        // and Hyper Beam's effect, which starts later, takes its turn after them.
        // Running the room's effects before the separately owned Hyper Beam effect keeps that
        // order.
        // Door destination drawing borrows this frame path, but the door's destination
        // fade does not run the colour effects. Its colors must remain
        // owned by the gradual room fade until the door transition releases them.
        if (Room.LoadedScope.ColourEffects.ColourEffectsRun && !Room.LoadedScope.Enemies.Elevator.ElevatorDoorTransitionActive &&
            Player.Samus?.Xray.AreColourEffectsSuspended != true)
        {
            Room.LoadedScope.ColourEffects.Step(new ColourEffectInputs { SamusY = Player.Samus?.Position.Y.Whole ?? 0, EquippedItems = Player.Samus?.EquippedItems ?? 0, AreaMiniBossDefeated = Room.ActiveRoom is { } activeRoom &&
                    System.HasDefeatedAnyOf(activeRoom.Area, DefeatedBosses.AreaMiniBoss), Samus = Player.Samus?.RoomView, FrameCounter = Clock.Counter });
            // Her heat glow and her suit's loading show over her suit as the frames that wrote it (#1512).
            foreach ((ColourEffectColours colours, ColourEffectFrame frame) in Room.LoadedScope.ColourEffects.SuitShown)
                Player.Colours.LayOverSuit(ColourEffectShades.Of(colours, frame));
            // The enemies' groups show over their owners' own colours as the frames that wrote them (#1512).
            foreach ((ColourEffectColours colours, ColourEffectFrame frame) in Room.LoadedScope.ColourEffects.EnemyShown)
                Room.LoadedScope.Enemies.ColourEffectShows.Show(colours, frame);
            // An enemy's own glow shows on the enemy that started it, for as long as the room's colour effects run it (#1783).
            foreach ((IGlowingEnemy enemy, ColourEffectFrame frame) in Room.LoadedScope.ColourEffects.GlowsShown)
                enemy.ShowGlow(frame);
        }
        // Hyper Beam's colour effect runs whenever the colour effects run.
        if (Room.LoadedScope.ColourEffects.ColourEffectsRun)
        {
            Player.Samus?.Drained.HyperBeamColourEffect.Step(Player.Colours);
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

    /// <summary>Prepares the enemy frame and runs the enemy phase before Samus when the frame has no movement to wait for.</summary>
    private void PrepareEnemiesAndHoistedEnemyMain(GameplayFrameContext context)
    {
        EnemyPhase.PrepareEnemyFrame(Room.LoadedScope);
        // Host-disabled Samus movement has no input phase to wait for. Otherwise
        // defer actors until projectile production/update, immediately before the movement phase.
        SamusHandlerFrame frame = context.Samus;
        frame.RecordHoistedEnemyMain(Player.Samus is null || Room.Camera is null || !Player.GroundedSamusMovementEnabled);
        // The enemy phase follows Samus's input mode. An actor that restores the
        // normal modes there (the gunship) affects this frame's movement phase only:
        // the locked input mode has already run, so it still sees the lock below.
        if (frame.EnemyMainAlreadyRan)
        {
            bool lockedBeforeEnemyMain = Player.Samus?.Control.InputLocked == true;
            EnemyPhase.RunEnemyMainPhase(processingList: EnemyProcessingList.Prepared,
                Room.Camera?.PreviousSamusPoint ?? context.SamusCameraPointAtFrameStart, Room.LoadedScope);
            frame.RecordEnemyMainReleasedInputLock(lockedBeforeEnemyMain && Player.Samus?.Control.InputLocked == false);
        }
    }

    /// <summary>Runs the input phase, the enemy phase, the resample after it and the movement phase, in the game's order.</summary>
    private void RunSamusAndEnemyMain(GameplayFrameContext context, SamusState samus, ScrollBoundaryCamera camera, RuntimeFrameAudio audio)
    {
        SamusHandlerFrame frame = context.Samus;
        SamusPhases.RunInputPhase(frame, RunningGameState, samus, camera, context.SamusCameraPointAtFrameStart, Room.LoadedScope);
        if (frame.MovementEnabled && !frame.EnemyMainAlreadyRan)
            EnemyPhase.RunEnemyMainPhase(processingList: EnemyProcessingList.Prepared, frame.PreviousCameraPoint, Room.LoadedScope);
        SamusPhases.RunPostEnemyMainResample(frame, samus, Room.LoadedScope.BombProjectiles, Room.LoadedScope.Projectiles, Room.LoadedScope);
        SamusPhases.RunMovementPhase(frame, samus, camera, audio, Room.LoadedScope);
    }

    /// <summary>Opens the gunship's save prompt at the shared suspension seam after the room objects.</summary>
    private void OpenGunshipPrompt()
    {
        // The gunship opens the save prompt from inside the enemy phase, after this frame's
        // ship bob and earlier owners ran. The prompt exists only while Samus is locked
        // inside the ship, so the owners between the enemy phase and here have nothing to
        // act on; the box therefore opens at this shared suspension seam, and the
        // remainder resumes after the YES/NO/completion chain returns.
        if (Room.LoadedScope.Enemies.Gunship.SavePromptPending)
        {
            if (MessageBox.IsActive)
            {
                throw new InvalidDataException(
                    "A room object's message box and the gunship save prompt opened in one gameplay frame.");
            }
            MessageBox.Begin(GameplayMessageId.GunshipSaveConfirmation, Controller1.Current);
        }
    }

    /// <summary>Parks the frame's tail when a room object opened a message box; returns whether it did.</summary>
    private bool SuspendTailForMessageBox(GameplayFrameContext context)
    {
        // The message runs inside the room-object handler: the rest of this gameplay
        // frame (enemy-projectile collision, scrolling, drawing, room main and the shared
        // tail) resumes only after the box closes, after the item's step finishes.
        if (context.Tail is not { } suspendedTail || !MessageBox.IsActive)
            return false;
        _suspendedFrameTail = suspendedTail;
        return true;
    }

    /// <summary>
    /// The remainder of a main-gameplay update after the room-object handler. A frame whose room-object
    /// pass opened a message box parks this state and resumes it when the box closes.
    /// </summary>
    internal RuntimeFrameResult FinishGameplayFrame(
        SuspendedGameplayFrameTail? frameTail,
        RuntimeFramePolicy policy,
        AmmoLoanGuard infiniteAmmoGuard)
    {
        if (frameTail is { } tail && Player.Samus is not null && Room.Camera is not null)
        {
            bool deathOwnsSamus = tail.Mode.Movement == SamusMovementMode.Fatal;
            ResolveEnemyProjectileAndPowerBombHits(Player.Samus, deathOwnsSamus);
            StepCeresElevatorArrival(Player.Samus, deathOwnsSamus);
            RunMainScrolling(Player.Samus, Room.Camera, tail.PreviousCameraPoint, deathOwnsSamus);
            DrawGameplayActors(tail, deathOwnsSamus);
        }
        RunEscapeTimer(frameTail);
        Sprites.FinalizeFrame();

        UpdateHud(infiniteAmmoGuard);
        RunRoomMains(policy);
        // The game-time step advances the gameplay clock after
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
            // The radius's whole pixels, which the radius's own type holds to its range (#1627). A reaction that runs an
            // enemy's main behaviour reads the room's terrain and the camera as they stand now, before the main scrolling.
            Room.LoadedScope.Enemies.Collision.ResolveOrdinaryPowerBombHits(
                powerBomb.Position,
                powerBomb.ExplosionRadius.WholePixels,
                Room.LoadedLevelData,
                Room.LoadedCamera.PixelPosition,
                samus);
        }
    }

    /// <summary>Steps the fresh-Ceres elevator arrival, which lowers Samus in the enemy-projectile slot before scrolling.</summary>
    private void StepCeresElevatorArrival(SamusState samus, bool deathOwnsSamus)
    {
        // Main gameplay runs enemy projectiles immediately after Samus's
        // movement handler and before the main scrolling. The two fresh-Ceres actors
        // use that slot to lower Samus one pixel at a time; moving this call below the
        // camera would make the viewport trail one frame behind the game.
        if (!deathOwnsSamus && Room.CeresElevatorArrival is { IsComplete: false } arrival)
        {
            if (arrival.Step(samus))
            {
                // The arrival's Samus lock installed a locked Samus handler before the projectiles
                // were spawned. Re-enable the ordinary movement path only
                // after the pad reaches its deletion height.
                samus.Control.Unlock();
                Player.GroundedSamusMovementEnabled = true;
            }
        }
    }

    /// <summary>Runs the main scrolling, its scrolling-finished hook, background streaming and the minimap.</summary>
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

            // The main gameplay state runs the main scrolling after movement and room objects and before
            // drawing Samus, enemies and projectiles, regardless of which Samus input handler is
            // installed. A locked input mode suppresses controller transitions; it does not
            // suppress scrolling. That is why the gunship arriving on Zebes can carry
            // Samus down several screens while her ordinary movement handler is absent.
            if (samus.Grapple.SlowScrolling)
            {
                // A fast grapple swing takes the slow camera branch, which leaves
                // the camera speeds from the last normal frame in place.
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
                        Facing: SamusPoseMetadata.ReadFacingDirection(samus),
                        CameraDistance: Room.LoadedScope.Enemies.RoomMainHooks.ZebetiteCameraDistance),
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
            // The camera writes the same distance-plus-one values consumed by Yard
            // kick setup. Publish the camera calculation itself, including its
            // integer-only direction check and previous scrolling checkpoint. A second
            // frame-end absolute delta loses both the bias and checkpoint timing.
            samus.HorizontalDistanceMovedPlusOne = camera.Speed.X;
            samus.ProjectileInheritance.CameraYSubspeed = camera.Speed.Y.Fraction;

            // The main scrolling invokes the mutable global
            // scrolling-finished hook after both axes have tracked Samus. Live Spore
            // Spawn installs its own hook, which owns the arena's vertical camera floor;
            // it must run before the layer scrolls and the sprite list consume camera position.
            Room.LoadedScope.Enemies.RoomMainHooks.RunSporeSpawnScrollingFinishedHook(camera);
            camera.FinishSamusScrolling(currentCameraPoint);

            // The layer scrolls follow the camera in the same main-loop pass, so the
            // following frame shows the room from level data at the new position.
            BackgroundPhase.UpdateBackgroundScrollingFromCamera(Freeze.TimeIsFrozen);

            // UpdateMinimap belongs to Samus's normal movement phase; it uses world
            // position, not camera position. Fresh debug sessions begin without an area
            // map, so only tiles actually visited by this runtime are revealed.
            Hud.UpdateMinimap(System, new MinimapFrame { Area = roomGeometry.Area, RoomMap = roomGeometry.MapPosition, RoomSize = Room.LevelData.Size, Samus = samus.Position.Whole, FrameCounter = Clock.ShortCounter, MapRevealMode = Assists.Current.MapReveal, PresentationMap = Artwork.MapPresentation?.Get(roomGeometry.Area) });
        }
    }

    /// <summary>Restores the attract demo's player input and draws Samus, enemies and projectiles.</summary>
    private void DrawGameplayActors(SuspendedGameplayFrameTail tail, bool deathOwnsSamus)
    {
        AttractDemo.RestoreAttractPlayerInput();

        RoomScope scope = Room.LoadedScope;
        ActorDrawing.DrawGameplayActors(scope, deathOwnsSamus ? SamusDrawOwner.DeathSequence : SamusDrawOwner.Gameplay,
            // The stationary lock's movement mode does not dispatch Samus's
            // palette effects. In particular, automatic Reserve recovery freezes a
            // stored shine instead of consuming it while Samus is stationary.
            tail.Mode.Movement is SamusMovementMode.SuitPickup or SamusMovementMode.ScriptLocked ? SamusPaletteEffects.Hold : SamusPaletteEffects.Advance);
    }

    /// <summary>Counts the escape timer down and draws it, unless the gunship's movement mode owns the frame.</summary>
    private void RunEscapeTimer(SuspendedGameplayFrameTail? frameTail)
    {
        // The enemy phase can install the gunship's movement mode on this frame.
        // It omits the normal timer-drawing call: neither countdown nor its sprites
        // survives boarding, even though timer status stays active. Decide after the
        // actor transition, before liftoff can repurpose the timer's sprite pictures as dust.
        bool escapeTimerRuns = frameTail?.Mode.Movement != SamusMovementMode.Gunship;
        if (escapeTimerRuns && EscapeTimer.Process(Clock.Counter, Assists.Current.EscapeTimerHoldsAtOneSecond))
        {
            // The expired countdown publishes the time-up game state for the
            // game-state loop, whites the target palettes and disables colour effects.
            TimeUpRequested = true;
            ColourEffectSession.StopColourEffects();
        }
        if (escapeTimerRuns && EscapeTimer.IsActive)
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
        // Every gameplay update redraws the HUD after its draw pass, which sets what a Puyo's ceiling strike in the
        // next update reads.
        Room.LoadedScope.Enemies.PuyoCeilingTurn.HudRedrawn();
        if (Hud.IsInitialized)
        {
            // The loan exists only so firing logic does not observe zero and
            // auto-cancel the selected weapon. Remove it before the HUD update converts the
            // live counters into HUD cells; publishing first made the on-screen
            // minimum read two even though Snapshot later restored the actor to one.
            infiniteAmmoGuard.Complete(Player.Samus);
            // The HUD update rebuilds live energy/ammo cells every gameplay frame.
            // Initialization alone is insufficient: Ridley contact and fireballs mutate
            // Samus during this frame, and those values must enter the next frame boundary.
            // The room-object handler has not returned while a pickup message is open.
            // Its newly granted inventory reaches the HUD only after that return.
            if (Player.Samus is not null && !Room.LoadedScope.RoomObjects.CollectibleObjects.HasPendingCollectibleMessage)
                Hud.UpdateGameplayCounters(Player.Samus, AudioRequests, Freeze.TimeIsFrozen);
        }
    }

    /// <summary>Runs the scrolling sky, block animations, escape effects, the elevatube and the room main behaviours.</summary>
    private void RunRoomMains(RuntimeFramePolicy policy)
    {
        // Landing Site's room main advances the sky bands after ordinary gameplay logic.
        if (Room.ScrollingSky is not null && Room.Camera is not null)
            Room.ScrollingSky.ProcessFrame(Room.Camera.PixelPosition.Y, timeIsFrozen: Freeze.TimeIsFrozen);

        RoomMains.RunBlockAnimations(Room.LoadedScope);
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
                Room.LoadedScope.RoomObjects,
                Room.RoomMainCarryover);
        }

        // Execute the active room's room main at the same seam as Landing Site's
        // room main above: after gameplay drawing/HUD work and before global shaking. The
        // Ceres room main publishes a tilt for the following frame's presentation and may
        // change the outer game-state loop to the Ceres elevator state only when its caller is main gameplay.
        RoomMains.LastCeresElevatorShaftRoomMain = Room.LoadedScope.CeresElevatorShaft.Step(
            Player.Samus,
            Room.LoadedScope.Enemies.Ceres.CeresStatus == CeresEscapeStatus.ElevatorDoorDestroyed,
            allowDeparture: policy.AllowCeresElevatorDeparture,
            Room.RoomMainCarryover);
        if (RoomMains.LastCeresElevatorShaftRoomMain.TiltChanged)
            Display.ActiveTurningFloor = RoomMains.LastCeresElevatorShaftRoomMain.Transform;

        RoomMains.RunCeresFallingDebrisRoomMain(Room.LoadedScope);
        RoomMains.RunCrocomireComebackRoomMain(Room.LoadedScope);
        if (Room.ActiveRoom?.State.MainBehavior == RoomMainBehavior.HandleCeresRidleyGetawayCutscene)
            Room.LoadedScope.Enemies.Ceres.RunCeresRidleyGetawayRoomMain(Player.Samus, Clock.Counter);
    }

    /// <summary>Decrements Samus's hurt timers and the projectile interaction timer.</summary>
    private void DecrementHurtTimers()
    {
        // Main gameplay decrements these timers after Samus, enemies, drawing, HUD/background
        // bookkeeping, the room's own update, the energy-zero check, and room shaking. Keep this
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
            Room.DoorTransitionShownLayers,
            sky?.Publish(Room.Camera?.PixelPosition.Y ?? throw new InvalidOperationException("Scrolling sky has no gameplay camera.")),
            crocomireDeath is { MeltingScrollActive: true } melting ? melting.MeltDistortion : null,
            motherBrain is { RisingFloorClipActive: true },
            motherBrain?.RainbowBeam.RenderSnapshot,
            CeresHazeRenderSnapshot.Capture(Room.LoadedScope.CeresHaze),
            Artwork.MapPresentation?.CeresHaze,
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
    /// state shows, in the order later colour writes won over earlier ones: the room's palette effects and the effect overlay colours,
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
        yield return Room.LoadedScope.ColourEffects;
        yield return Room.LoadedScope.FxOverlay;
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
    /// Gameplay-frame state the remainder of a main-gameplay update reads after the room-object handler.
    /// The message runs inside that handler, so a room object that opens a box
    /// suspends the frame; the model parks this state until the box closes.
    /// </summary>
    /// <param name="Mode">The frame's installed input and movement modes.</param>
    /// <param name="PreviousCameraPoint">Samus's camera point carried to the frame tail.</param>
    internal readonly record struct SuspendedGameplayFrameTail(
        SamusControlMode Mode,
        FixedPosition<RoomPixels> PreviousCameraPoint);

    /// <summary>Main-gameplay tail parked when a room object opens a message box during a gameplay frame.</summary>
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
        if (statuesOpening && Room.LoadedScope.TourianStatues.Enabled) Room.LoadedScope.TourianStatues.RestartWithStatuesOpen(Room.LoadedScope.RoomObjects);
    }

    /// <summary>
    /// Keeps this runtime's camera fractions for the next game as the session releases it
    /// (<see cref="CameraFractionsKeptAcrossGames"/>). A runtime that never placed a camera leaves the kept fractions as
    /// it found them.
    /// </summary>
    internal void LeaveCameraFractions()
    {
        if (Room.Camera is { } camera)
            CameraFractions.Keep(camera.Position);
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
        if (MessageBox.LastFrameAudio.AdvancesLiquidWaveAndStatueDescent)
        {
            // The message box's restoring update advances the liquid wave and the statues too.
            if (Room.Camera is not null)
                LiquidWaveAndStatueDescent.Advance(Room.LoadedScope.RoomObjects, Room.LoadedScope);
            if (Player.Samus is { } samus)
                Room.LoadedScope.BombProjectiles.AdvancePowerBombExplosion(samus, Controller1.Current, RunningGameState);
        }
        if (MessageBox.ConfirmationSelectionChangedThisFrame)
            MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = true;
        if (MessageBox.IsActive)
            return Snapshot(infiniteAmmoGuard);

        MessageBoxes.ResolveClosedMessageBox(frameSuspended: _suspendedFrameTail is not null, Room.LoadedScope.RoomObjects, Room.LoadedScope);
        // The room-object handler returns once the box and its item's step finish; the
        // parked remainder of that gameplay frame runs now, in this same dispatch.
        if (!MessageBox.IsActive && _suspendedFrameTail is { } tail)
        {
            _suspendedFrameTail = null;
            return FinishGameplayFrame(tail, policy, infiniteAmmoGuard);
        }
        return Snapshot(infiniteAmmoGuard);
    }
}
