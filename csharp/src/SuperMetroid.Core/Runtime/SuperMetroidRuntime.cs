using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
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
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The owner that advances the game one gameplay update at a time.
/// </summary>
/// <remarks>
/// Each update begins at the frame boundary, which shows the previous update's frame, latches
/// input and advances the frame counters; the update's gameplay then runs. Tools and viewers
/// step one update at a time through the same boundary.
/// </remarks>
internal sealed partial class SuperMetroidRuntime : IFrameColourLatch
{
    /// <summary>The liquid wave and statue descent each update advances before its random number is drawn.</summary>
    internal LiquidWaveAndStatueDescent LiquidWaveAndStatueDescent { get; }

    /// <summary>
    /// Where the liquid wave and statue descent fall among the random draws; created with the runtime, whose door transitions stop
    /// and resume it.
    /// </summary>
    internal LiquidWaveAndStatueDescentOrder LiquidWaveAndStatueDescentOrder { get; } = new();

    /// <summary>
    /// How a door transition's scroll counts the enemies' short update count; created with the runtime, whose room
    /// loader starts and ends it.
    /// </summary>
    internal DoorTransitionFrameCounting DoorFrameCounting { get; } = new();

    /// <summary>Whether gameplay time is frozen, and whether the X-ray scope freezes it too.</summary>
    internal GameplayTimeFreeze Freeze { get; }

    /// <summary>The room objects' part of each update: the room's objects run, and an item's or station's message box opens.</summary>
    internal RoomObjectFramePhase RoomObjectPhase { get; }

    /// <summary>Sets up the HUD when gameplay starts.</summary>
    internal GameplayHudSetup HudSetup { get; }

    /// <summary>Places the room's foreground and background layers for the camera each update.</summary>
    internal BackgroundScrollPhase BackgroundPhase { get; }

    /// <summary>The start of each update: the previous update's frame is shown, the controllers are read and the frame counters advance.</summary>
    internal AcceptedFrameBoundary FrameBoundary { get; }

    /// <summary>What follows a message box closing: its selection sound, a save, a suit's transformation or the gunship's exit.</summary>
    internal MessageBoxFlow MessageBoxes { get; }

    /// <summary>Samus's death: the update she dies on and the sequence that follows.</summary>
    internal DeathSequenceRunner DeathSequence { get; }

    /// <summary>Samus's part of each update: her input, movement and animation, the grapple beam and the X-ray scope.</summary>
    internal SamusFramePhases SamusPhases { get; }

    /// <summary>Loads games, rooms and doors: each room's level, camera, effects, room objects and enemies.</summary>
    internal RoomLoader RoomLoader { get; }
    /// <summary>Applies room-scoped enemy effects before frame owners advance.</summary>
    internal EnemyRoomEffects EnemyRoomEffects { get; }
    /// <summary>Retains scripted demo input and progression across runtime frames.</summary>
    internal AttractDemoSession AttractSession { get; } = new();

    /// <summary>
    /// The sound and music requests the current update makes, in the order its systems make them (#1620): every room
    /// system and Samus take it as they are built and queue theirs here as they run, and they go straight on into the
    /// session's queues (#2057).
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; }

    /// <summary>What the gameplay display shows: the turning floor's and the elevator shaft's transforms and the latched scene.</summary>
    internal GameplayDisplayState Display { get; }

    /// <summary>Samus and what her update keeps: her poses, movement mode, colours and death sequence.</summary>
    internal SamusFrameState Player { get; }

    /// <summary>The loaded room: its definition, door, level, camera and the parts each room builds, and the room being loaded.</summary>
    internal ActiveRoomState Room { get; }

    /// <summary>The game's frame counters, which time the enemies' cadences, the block animations and blinking.</summary>
    internal FrameClock Clock { get; }

    /// <summary>The in-game options: Moonwalk, Icon Cancel and the controller bindings.</summary>
    internal GameplayOptions Options { get; }

    /// <summary>The assists this runtime plays under, and the owner of its granted loadout and saved-encounter reset.</summary>
    internal ActiveAssists Assists { get; }

    /// <summary>The installed pictures of the room objects' blocks, one set for each kind of room object.</summary>
    internal RoomObjectVisuals RoomObjectVisuals { get; }

    /// <summary>Draws Samus, her shots and grapple beam and the enemies each update.</summary>
    internal GameplayActorDrawing ActorDrawing { get; }

    /// <summary>
    /// How far the camera last measured Samus moving sideways, plus one pixel, which a kicked Yard flies off with; created
    /// with the runtime, recorded by the update's scrolling and handed to every room's enemies.
    /// </summary>
    internal YardKickTrackingDistance YardKickDistance { get; } = new();

    /// <summary>Initialises the runtime's first Samus and each new one its setups start.</summary>
    private SamusInitialization SamusInitialization { get; }

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

    /// <summary>What the X-ray scope reveals of the room's hidden blocks and items.</summary>
    internal XrayReveal Xray { get; }

    /// <summary>The layers gameplay chooses to show and the accepted frame shows.</summary>
    internal GameplayScreenLayers ScreenLayers { get; }

    /// <summary>The escapes' room effects: their setup as Samus enters a room and their step each update.</summary>
    internal EscapeRoomEffects EscapeEffects { get; }

    /// <summary>The enemies' part of each update: the enemies and their projectiles run, and Samus's shots hit them.</summary>
    internal EnemyFramePhase EnemyPhase { get; }

    /// <summary>The title screen's attract demo: its room and Samus, and the recorded input that plays her.</summary>
    internal AttractDemoDriver AttractDemo { get; }

    /// <summary>Door transitions: the door's setup, its scroll and fades, and the actors drawn while it runs.</summary>
    internal DoorTransitionRunner DoorTransition { get; }

    /// <summary>The installed artwork the runtime draws and colours from: rooms, skies, Samus, beams, the grapple beam and the map.</summary>
    internal RuntimeArtwork Artwork { get; }

    /// <summary>Creates the runtime under its assists, Samus in her starting shades.</summary>
    /// <param name="assists">The assists the runtime plays under; <see cref="GameplayAssists.None"/> for the unassisted game.</param>
    /// <param name="artwork">
    /// The installed catalogs, every one required (#2049); their Samus overread services
    /// are handed to the owners that consult them, their enemy and boss overread services, enemy sheets and enemy frame
    /// displays and the rainbow-beam camera checkpoint are injected into the enemy session it builds, the Samus, projectile
    /// and map artwork are its <see cref="Artwork"/>'s from the start, and the rest are bound before the constructor returns.
    /// </param>
    /// <param name="session">
    /// What the session keeps across its runtimes: the frame clock it shares with the frontend; what the shared game state
    /// continues from (the random number generator, the power-on earthquake delay and the live and stored explored maps);
    /// and the compatibility services kept across games (the unauthored Speed Booster cadence its enemies and every Samus
    /// share, the save confirmation's input cadence and the camera fractions).
    /// </param>
    internal SuperMetroidRuntime(GameplayAssists assists, GameplayArtwork artwork, GameplaySessionCarryover session)
    {
        Ensure.NotNull(artwork);
        Ensure.NotNull(session);
        FrameClock clock = session.Clock;
        UnauthoredBoostCadence = Ensure.NotNull(session.Compatibility.UnauthoredBoostCadence);
        CameraFractions = Ensure.NotNull(session.Compatibility.CameraFractions);
        FadeOut = new GameplayFadeOut(FadeStepCount);
        MessageBox = new GameplayMessageBoxState(session.Compatibility.SaveChoiceInput, session.Compatibility.MessageBoxPauses);
        SamusOverreads = artwork.SamusOverreads;
        System = new SharedGameState(session.GameState.Random, session.GameState.EarthquakeSoundDelay, session.GameState.StoredExploredMaps);
        Display = new GameplayDisplayState();
        Room = new ActiveRoomState();
        Clock = Ensure.NotNull(clock);
        Options = new GameplayOptions();
        var sessionAudio = new SessionGameplayAudioQueues(session.Audio, AttractSession);
        AudioRequests = new GameplayAudioRequests(sessionAudio, sessionAudio);
        EnemySession = new EnemySession(
            AudioRequests,artwork.EnemyOverreads, artwork.BossAndIntroOverreads,
            artwork.EnemySheets, artwork.EnemyFrameDisplays);

        BeamCharge = new BeamChargeState();
        BossArtwork = new BossArtworkBinding(artwork.ScreenContent.CeresRidleyGetawayBoostDelays, artwork.ScreenContent.EscapeTypewriter,
            artwork.ScreenContent.MotherBrainRainbowBeamColours, artwork.KraidEyes, artwork.BossBodySheets);
        Hud = new HudState(artwork.ScreenContent.GameplayHud);
        Artwork = new RuntimeArtwork(new RuntimeArtworkDependencies
        {
            ScreenContent = artwork.ScreenContent, SpriteSheets = artwork.SpriteSheets, SamusBodyArt = artwork.SamusBodyArt, SamusShownFrames = artwork.SamusShownFrames,
            GrappleFlarePlacement = artwork.GrappleFlarePlacement, GrappleSwingFrames = artwork.GrappleSwingFrames,
            ProjectileFrameBindings = artwork.ProjectileFrameBindings, ChargeFlarePlacement = artwork.ChargeFlarePlacement,
            RoomBlockSheets = artwork.RoomBlockSheets, RoomVisualLayouts = artwork.RoomVisualLayouts, RoomLevelStreams = artwork.RoomLevelStreams,
            BossArtwork = BossArtwork, Room = Room, Hud = Hud,
        });
        SamusInitialization = new SamusInitialization(AudioRequests, SamusOverreads, UnauthoredBoostCadence, BeamCharge, Artwork.SamusArtwork,
            YardKickDistance);
        Player = new SamusFrameState(SamusInitialization.Initialize());
        DoorFade = new DoorFade(Player.Colours);
        Freeze = new GameplayTimeFreeze(Player);
        Assists = new ActiveAssists(Player, System, assists);
        TourianStatueDependencies = new TourianStatueSequenceDependencies { Player = Player, System = System, Assists = Assists, Freeze = Freeze };
        HudSetup = new GameplayHudSetup(new GameplayHudSetupDependencies { Artwork = Artwork, Hud = Hud });
        ScreenLayers = new GameplayScreenLayers();
        Xray = new XrayReveal(BackgroundScroll, System, artwork.XrayRevealVisuals);
        FrameBoundary = new AcceptedFrameBoundary(new AcceptedFrameBoundaryDependencies { Colours = this, Input = new AcceptedFrameInput(PlayerActions, PlayerButtons, Options, Clock), Hud = Hud, BackgroundScroll = BackgroundScroll, Display = Display, DisplayedSprites = DisplayedSprites, ScreenLayers = ScreenLayers, Sprites = Sprites, Player = Player, Room = Room, SuitPickup = SuitPickup, Xray = Xray });
        BackgroundPhase = new BackgroundScrollPhase(BackgroundScroll);
        ActorDrawing = new GameplayActorDrawing(new GameplayActorDrawingDependencies { Artwork = Artwork, AttractSession = AttractSession, PlayerActions = PlayerActions, Display = Display, Freeze = Freeze, Clock = Clock, Sprites = Sprites, Player = Player });
        EnemyRoomEffects = new EnemyRoomEffects(new EnemyRoomEffectsDependencies { EscapeTimer = EscapeTimer, System = System});
        EnemyPhase = new EnemyFramePhase(new EnemyFramePhaseDependencies { BackgroundScroll = BackgroundScroll, PlayerActions = PlayerActions, EnemyRoomEffects = EnemyRoomEffects, Freeze = Freeze, Clock = Clock, Sprites = Sprites, Player = Player});
        LiquidWaveAndStatueDescent = new LiquidWaveAndStatueDescent(Freeze, System, LiquidWaveAndStatueDescentOrder);
        DoorTransition = new DoorTransitionRunner(new DoorTransitionRunnerDependencies { ActorDrawing = ActorDrawing,  Display = Display, EnemyPhase = EnemyPhase, LiquidWaveAndStatueDescent = LiquidWaveAndStatueDescent, Clock = Clock, FrameBoundary = FrameBoundary, Sprites = Sprites, System = System, Freeze = Freeze});
        EscapeEffects = new EscapeRoomEffects(Freeze, Clock, System);
        RoomObjectVisuals = new RoomObjectVisuals(artwork.RoomObjects);
        RoomLoader = new RoomLoader(new RoomLoaderDependencies { AudioRequests = AudioRequests, EnemySession = EnemySession, BossArtwork = BossArtwork, ColourEffectSession = ColourEffectSession, TourianStatueDependencies = TourianStatueDependencies, RoomObjectVisuals = RoomObjectVisuals, BeamCharge = BeamCharge, Artwork = Artwork, BackgroundScroll = BackgroundScroll, PlayerActions = PlayerActions, Display = Display, DoorTransition = DoorTransition, EnemyRoomEffects = EnemyRoomEffects, GameTime = GameTime, Hud = Hud, Clock = Clock, DoorFrameCounting = DoorFrameCounting, Options = Options, Player = Player, Room = Room, System = System, Assists = Assists, SamusOverreads = SamusOverreads, ScrollCellsPastRoomScreens = artwork.ScrollCellsPastRoomScreens, UnauthoredBoostCadence = UnauthoredBoostCadence, CameraFractions = CameraFractions, FadeStepCount = FadeStepCount, YardKickDistance = YardKickDistance, SamusInitialization = SamusInitialization });
        AttractDemo = new AttractDemoDriver(new AttractDemoDriverDependencies { AttractSession = AttractSession, PlayerActions = PlayerActions, HudSetup = HudSetup, FrameBoundary = FrameBoundary, Player = Player, Room = Room, RoomLoader = RoomLoader, System = System });
        DeathSequence = new DeathSequenceRunner(new DeathSequenceRunnerDependencies { Display = Display, FrameBoundary = FrameBoundary, Sprites = Sprites, Player = Player});
        MessageBoxes = new MessageBoxFlow(new MessageBoxFlowDependencies { AudioRequests = AudioRequests, Freeze = Freeze, Hud = Hud, SuitPickup = SuitPickup, System = System });
        RoomObjectPhase = new RoomObjectFramePhase(new RoomObjectFramePhaseDependencies { PlayerActions = PlayerActions, PlayerButtons = PlayerButtons, Options = Options, Freeze = Freeze, MessageBox = MessageBox, MessageBoxes = MessageBoxes, Player = Player });
        SamusPhases = new SamusFramePhases(new SamusFramePhasesDependencies { AttractDemo = AttractDemo, PlayerActions = PlayerActions, Freeze = Freeze, GameTime = GameTime, Clock = Clock, Options = Options, Assists = Assists, Player = Player, System = System, DoorPassage = DoorTransition.Passage });

        // Gameplay starts with Samus in her starting shades; every other picture shows its standard colours (#1512).
        Player.Colours.ShowStarting();
        BindArtwork(artwork, previous: null);
    }

    /// <summary>The catalogs the runtime last bound: those it was built over until the host binds others.</summary>
    private GameplayArtwork _boundArtwork;

    /// <summary>
    /// Binds the catalogs the host installs in place of those the runtime holds: only the catalogs whose reference changed
    /// reach their owners.
    /// </summary>
    /// <param name="artwork">The catalogs to bind.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="artwork"/> names enemy sheets or enemy frame displays other than those the runtime was built with.
    /// </exception>
    internal void BindArtwork(GameplayArtwork artwork) => BindArtwork(artwork, _boundArtwork);

    /// <summary>
    /// Binds the installed catalogs to the runtime's owners: every catalog when <paramref name="previous"/> is null (the
    /// runtime being built), otherwise only the catalogs whose reference changed.
    /// </summary>
    /// <remarks>
    /// The screen content binds first: its setter pushes the HUD, message-box, effect overlay and Samus colour bindings and
    /// rebinds the queued artwork that later catalogs' setters may read.
    /// </remarks>
    /// <param name="artwork">The catalogs to bind.</param>
    /// <param name="previous">The catalogs bound before, or <see langword="null"/> to bind every catalog.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="artwork"/> names enemy sheets or enemy frame displays other than those the runtime was built with.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_boundArtwork))]
    internal void BindArtwork(GameplayArtwork artwork, GameplayArtwork? previous)
    {
        Ensure.NotNull(artwork);
        // The enemy session holds the sheets and frame displays it was built with for the runtime's lifetime (#1616).
        if (!ReferenceEquals(artwork.EnemySheets, EnemySession.Sheets) || !ReferenceEquals(artwork.EnemyFrameDisplays, EnemySession.FrameDisplays))
            throw new InvalidOperationException("A runtime draws its enemies from the sheets and frame displays it was built with.");
        _boundArtwork = artwork;
        void Bind<T>(Func<GameplayArtwork, T> catalog, Action<T> bind) where T : class
        {
            T value = catalog(artwork);
            if (previous is null || !ReferenceEquals(catalog(previous), value))
                bind(value);
        }

        Bind(a => a.ScreenContent, value => Artwork.ScreenContent = value);
        Bind(a => a.SamusBodyArt, value => Artwork.SamusBodyArt = value);
        Bind(a => a.SamusShownFrames, value => Artwork.SamusShownFrames = value);
        Bind(a => a.RoomBlockSheets, value => Artwork.RoomBlockSheets = value);
        Bind(a => a.RoomVisualLayouts, value => Artwork.RoomVisualLayouts = value);
        Bind(a => a.RoomLevelStreams, value => Artwork.RoomLevelStreams = value);
        Bind(a => a.RoomObjects, RoomObjectVisuals.Rebind);
        Bind(a => a.XrayRevealVisuals, Xray.Rebind);

        Bind(a => a.ProjectileFrameBindings, value => Artwork.ProjectileFrameBindings = value);
        Bind(a => a.SpriteSheets, value => Artwork.SpriteSheets = value);
        Bind(a => a.ChargeFlarePlacement, value => Artwork.ChargeFlarePlacement = value);
        Bind(a => a.GrappleFlarePlacement, value => Artwork.GrappleFlarePlacement = value);
        Bind(a => a.GrappleSwingFrames, value => Artwork.GrappleSwingFrames = value);

        Bind(a => a.KraidEyes, value => BossArtwork.KraidEyes = value);
        Bind(a => a.BossBodySheets, value => BossArtwork.BodySheets = value);
    }

    /// <summary>Shared random/event/input-filter state, over the session's random number generator.</summary>
    public SharedGameState System { get; }

    /// <summary>The game time, which each save slot also records.</summary>
    public GameTimeState GameTime { get; } = new();

    /// <summary>
    /// The save made at <paramref name="station"/> now (#2500): saving first stores the live explored map as the station's
    /// area's, then keeps Samus, the progress, the clock's reading, the controls and the options.
    /// </summary>
    /// <exception cref="InvalidOperationException">No Samus is live to save.</exception>
    internal SuperMetroidSaveSnapshot SaveSnapshotAt(LoadStation station)
    {
        Ensure.NotNull(station);
        SamusState samus = Player.Samus;
        System.StoredExploredMaps.StoreLiveMap(station.Area);
        return SuperMetroidSaveSnapshot.Capture(samus, System, new SaveSnapshotContext
        {
            Checkpoint = station,
            GameTime = GameTime.Reading,
            ControllerBindings = Options.ControllerBindings,
            MoonwalkEnabled = Options.MoonwalkEnabled,
            IconCancelEnabled = Options.IconCancelEnabled,
            Language = Language,
        });
    }

    /// <summary>
    /// The player's gameplay actions as the frame boundary latched them: the configured bindings read the buttons as
    /// actions once at the latch, so gameplay tests actions and never a button.
    /// </summary>
    public GameplayActionInput PlayerActions { get; } = new();

    /// <summary>
    /// The same sample as physical buttons. Code that tests fixed buttons rather than the bindings (the pause menu, the
    /// message box's save choice) reads this latch.
    /// </summary>
    public ControllerInputState PlayerButtons { get; } = new();

    /// <summary>The pause screen's held-button repeat, which keeps its count between pauses for the runtime's lifetime.</summary>
    public HeldButtonRepeat PauseHeldButtonRepeat { get; } = new();

    /// <summary>Escape timer state machine.</summary>
    public EscapeTimer EscapeTimer { get; } = new();

    /// <summary>
    /// Frontend language choice read by room actors whose script differs
    /// between English and Japanese. Each enemy chooses its own script by it.
    /// </summary>
    public TextLanguage Language
    {
        get => EnemySession.Language;
        set => EnemySession.Language = value;
    }

    /// <summary>
    /// The HUD's images the last frame boundary latched (#1351, #1466): the HUD as displayed, which shows no image before any
    /// frame boundary.
    /// </summary>
    public ShownHud AcceptedHud => Display.Latched.Hud;

    /// <summary>Whether the colour effects run, and the heat phases, which outlive each room's colour effects.</summary>
    internal ColourEffectSessionState ColourEffectSession { get; } = new();

    /// <summary>The colours a door's fades show, which the door transition steps (#1512).</summary>
    internal DoorFade DoorFade { get; }

    /// <summary>
    /// The death fade and the time-up whiteout of the whole display, which their game states step (#1512), on the runtime's
    /// <see cref="FadeStepCount"/>.
    /// </summary>
    internal GameplayFadeOut FadeOut { get; }

    /// <summary>The sprite frame the update is building for the next frame boundary: sprites plus whole-frame draws.</summary>
    public SpriteFrameBuffer Sprites { get; } = new();

    /// <summary>
    /// The sprite frame the most recent frame boundary made visible. The display draws it while the current update
    /// prepares <see cref="Sprites"/>.
    /// </summary>
    internal SpriteFrameBuffer DisplayedSprites { get; } = new();

    /// <summary>
    /// The enemy side of the game session: the enemy state that outlives each room, and what every room's enemies are
    /// built over.
    /// </summary>
    public EnemySession EnemySession { get; }

    /// <summary>The installed boss artwork every room's bosses read.</summary>
    internal BossArtworkBinding BossArtwork { get; }


    /// <summary>
    /// The game state whose phase the frontend is now running. Samus's control setups test it from
    /// inside the update; runtime-only hosts run main gameplay.
    /// </summary>
    public SuperMetroid.Core.Progression.SuperMetroidGameState RunningGameState { internal get; set; } = SuperMetroid.Core.Progression.SuperMetroidGameState.MainGameplay;

    /// <summary>Whether a power-bomb explosion is under way; X-ray setup and the pause check reject one, and the sound queues turn every sound away during one.</summary>
    /// <remarks>Before the first room exists there is no explosion.</remarks>
    public bool PowerBombExplosionActive => Room.BombProjectiles?.PowerBombExplosion.IsActive == true;

    /// <summary>The held beam charge, flare and charge colours, which survive room loads; each room's projectiles share it.</summary>
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
    internal SamusSuitPickupState SuitPickup { get; } = new();

    /// <summary>The gameplay HUD.</summary>
    public HudState Hud { get; }

    /// <summary>The layers' positions and views, fed by the active camera; the frame boundary latches the views the screen shows.</summary>
    internal BackgroundScrollState BackgroundScroll { get; } = new();

    /// <summary>Runs one frame boundary and publishes an empty sprite frame without advancing gameplay owners.</summary>
    public void RunBlankGameplayFrame(ControllerButton controllerInput)
    {
        FrameBoundary.BeginAcceptedFrame(controllerInput);
        Sprites.BeginFrame();
        Sprites.FinalizeFrame();
        RoomLoader.RestartShortCounterDuringDoorScroll();
    }

    /// <summary>Runs one frame boundary followed by the update's gameplay, as <paramref name="request"/> asks.</summary>
    /// <param name="playerInput">The host's controller buttons, read as gameplay actions through the bindings when the frame boundary latches player input; attract playback publishes its independent scripted actions later.</param>
    /// <param name="request">The game state that owns the update and the host's work after the frame boundary.</param>
    /// <returns>What the update started for the game-state loop to act on after it: the time-up and where its out-of-health check sends Samus.</returns>
    /// <param name="loaded">The runtime's loaded room, which every gameplay update runs in.</param>
    public RuntimeFrameOutcome StepFrame(ControllerButton playerInput, RuntimeFrameRequest request, RoomScope loaded)
    {
        Ensure.NotNull(request);
        Ensure.NotNull(loaded);
        // Every gameplay update runs with Samus: each game's start, a saved game's load and the attract demo create her
        // before their first room loads, and only those setups, between updates, replace her.
        SamusState samus = Player.Samus;
        Assists.GrantLoadoutToNewSamus(samus);
        AmmoLoanGuard infiniteAmmoGuard =
            AmmoLoanGuard.Begin(Assists.Current.AmmoNeverRunsOut, samus);
        try
        {
            return StepFrameGuarded(playerInput, request, infiniteAmmoGuard, loaded, samus);
        }
        finally
        {
            // A failed update may be followed by another update, so the borrowed round is returned here whatever
            // happens: otherwise an update that throws after borrowing the last round leaves live ammo at two.
            infiniteAmmoGuard.Complete();
            // A failed demo frame must not leak scripted input into the player latch.
            AttractDemo.RestoreAttractPlayerInput();
        }
    }

    /// <summary>
    /// Advances one frame boundary and the corresponding gameplay update while the
    /// frame-exit ammo guard remains active through callbacks.
    /// </summary>
    private RuntimeFrameOutcome StepFrameGuarded(
        ControllerButton playerInput,
        RuntimeFrameRequest frameRequest,
        AmmoLoanGuard infiniteAmmoGuard,
        RoomScope room,
        SamusState samus)
    {
        // An open message box runs its own updates inside the suspended gameplay update:
        // the frame boundary latches no input and no random number is drawn nor owner run.
        if (MessageBox.IsActive)
        {
            var boxStart = new GameplayUpdateStart(MessageBox: MessageBox.MessageId, SamusInputLocked: samus.Control.InputLocked);
            return StepMessageBoxFrame(playerInput, boxStart, frameRequest.Owner, infiniteAmmoGuard, room, samus);
        }
        GameplayUpdateOwner owner = BeginAcceptedGameplayFrame(playerInput, frameRequest, room);
        // What the update's game-state transitions read of how its gameplay began, before any of its steps run: the host's
        // work after the frame boundary has run, and the reserve refill's completion there gives Samus her control back
        // before her update's pause check (#2526).
        var start = new GameplayUpdateStart(MessageBox: null, SamusInputLocked: samus.Control.InputLocked);
        RunLiquidWaveAndStatueDescent(room, samus);
        DrawUpdateRandomNumber();
        RunEffectsBeforePhase(room, samus);
        var context = new GameplayFrameContext(samus.Position);
        LatestGameplayFrame = context.Samus;
        PrepareEnemyProcessingList(room);
        RunColourEffects(room, samus);
        BeginSpriteFrame();
        EnemyFrameResult? earlyEnemyPass = PrepareEnemiesAndEarlyEnemyUpdate(context, room, samus);
        EnemyFrameResult? lateEnemyPass = RunSamusAndEnemyUpdate(context, samus, room, owner.RunsLowHealthCheck);
        bool deathOwnsSamus = context.Samus.Mode.Movement == SamusMovementMode.Fatal;
        if (!deathOwnsSamus)
            DoorTransition.RunEnemyProjectiles(room, samus);
        RoomObjectPhase.RunObjectPhase(deathOwnsSamus, samus, room);
        OpenGunshipPrompt(earlyEnemyPass ?? lateEnemyPass);
        var tail = new SuspendedGameplayFrameTail(context.Samus.Mode, context.Samus.PreviousCameraPoint, context.Samus.DeathSequenceStep);
        if (SuspendTailForMessageBox(tail))
            return EndUpdateWithoutGameplayTail(start, owner, infiniteAmmoGuard, room, samus);
        return FinishGameplayFrame(tail, start, owner, infiniteAmmoGuard, room, samus);
    }

    /// <summary>Marks the frame's impacts as gameplay's, runs the accepted-frame boundary and the host's post-boundary work, and chooses the screen layers.</summary>
    /// <returns>The game state the host's post-boundary work leaves owning the rest of the update (#2526).</returns>
    private GameplayUpdateOwner BeginAcceptedGameplayFrame(ControllerButton playerInput, RuntimeFrameRequest frameRequest, RoomScope room)
    {
        room.Projectiles.ImpactAudio.BeginImpactAudioFrame(context: ImpactAudioContext.Gameplay);
        FrameBoundary.BeginAcceptedFrame(playerInput);
        GameplayUpdateOwner owner = frameRequest.AfterFrameBoundary(frameRequest.Owner);
        ScreenLayers.Choose(room);
        return owner;
    }

    /// <summary>Advances the liquid wave and statue descent and the power bomb's shape step before the update draws its random number.</summary>
    private void RunLiquidWaveAndStatueDescent(RoomScope room, SamusState samus)
    {
        // The liquid wave and the statues read the random number before the update draws the next one. A
        // message box's wait does not run them: its updates return before this.
        LiquidWaveAndStatueDescent.Advance(room.RoomObjects, room);

        // The power bomb blast belongs to the outer frame, not Samus's input phase. A statue carry
        // takes over her movement while the already-spawned blast continues normally.
        // Its cleanup resumes Samus's sounds, which can queue them before Samus's frame.
        room.BombProjectiles.AdvancePowerBombExplosion(samus, PlayerActions.Current, RunningGameState);
    }

    /// <summary>Draws the update's random number.</summary>
    private void DrawUpdateRandomNumber()
    {
        // Every update draws a random number right after the liquid wave and statue descent
        // (LiquidWaveAndStatueDescentOrder) and before the game state's own work, whatever else runs. Several room
        // and enemy behaviours only read the random number without drawing one themselves: the falling ceiling
        // fragments land at new places only because of this draw.
        System.NextRandom();
    }

    /// <summary>Runs the owners before the game state's phase: Ceres haze, the suit transformation, Phantoon's wave and blending, and room effect overlay FX.</summary>
    private void RunEffectsBeforePhase(RoomScope room, SamusState samus)
    {
        room.CeresHaze?.Step();

        // The suit transformation's light beam advances once near the start of each
        // ordinary gameplay update. A transformation spawned when the preceding message
        // returned therefore receives its first stage update on this following update, while
        // every normal owner below continues to run around the input-locked, centered Samus.
        if (SuitPickup.IsActive)
            SuitPickup.Step(samus, Player.Colours);

        // An open message box keeps the room's sprites, music and sounds while it waits,
        // and blocks every gameplay owner. Room FX objects run in the ordinary gameplay
        // owner list; in particular, they must not scroll or animate while a message box
        // waits. Running this only once no message box waits keeps rain and fog
        // from advancing behind a station or item message.
        foreach (IStepWithRoomEffects effect in room.Enemies.DisplaySources.RoomEffectSteps)
            effect.StepWithRoomEffects();
        room.FxOverlay.Step(room.ArtworkFrames, new RoomFxOverlayFrame { Camera = room.Camera.PixelPosition, TimeIsFrozen = Freeze.TimeIsFrozen, FirefleaDarkness = room.Enemies.RoomStatus.FirefleaDarkness });
        // A lava or acid rise sustains the room's earthquake. Apply it before the ordinary earthquake step later in
        // this frame so its first displacement is visible immediately.
        if (room.FxOverlay.SustainedEarthquake is { } risingLiquidShake)
            room.Enemies.Earthquake.Sustain(risingLiquidShake);
        if (RoomFxTypes.ShowsLiquid(room.FxOverlay.Type))
            samus.LiquidPhysics.ConfigureFor(room.FxOverlay);
    }

    /// <summary>Builds the enemy processing list that the grapple scan and the enemy phase use.</summary>
    private static void PrepareEnemyProcessingList(RoomScope room)
    {
        // Main gameplay begins by determining which enemies to process,
        // before colour effects and Samus. The grapple scan uses this list.
        room.Enemies.PrepareEnemyProcessingList(room.Camera.PixelPosition);
    }

    /// <summary>Runs the room's colour effects and Hyper Beam's colour effect.</summary>
    private void RunColourEffects(RoomScope room, SamusState samus)
    {
        // Main gameplay runs the colour effects before Samus's update and
        // before enemies are processed. The Baby equips the Hyper Beam during that later enemy phase, so a newly
        // started Hyper Beam colour effect begins on the following frame.
        // The room's colour effects take their turns first, in the order they started: room FX starts first,
        // and Hyper Beam's effect, which starts later, takes its turn after them.
        // Door destination drawing borrows this frame path, but the door's destination
        // fade does not run the colour effects. Its colors must remain
        // owned by the gradual room fade until the door transition releases them.
        if (room.ColourEffects.ColourEffectsRun && !DoorTransition.Passage.IsUnderWay &&
            !samus.Xray.AreColourEffectsSuspended)
        {
            room.ColourEffects.Step(new ColourEffectInputs { SamusY = samus.Position.Y.Whole, EquippedItems = samus.EquippedItems, AreaMiniBossDefeated =
                    System.HasDefeatedAnyOf(room.Room.Area, DefeatedBosses.AreaMiniBoss), Samus = Player, FrameCounter = Clock.Counter });
            // Her heat glow and her suit's loading show over her suit while the room's colour effects run (#1512).
            foreach ((ColourEffectColours colours, ColourEffectFrame frame) in room.ColourEffects.SuitShown)
                Player.Colours.LayOverSuit(ColourEffectShades.Of(colours, frame));
            // The escape's shutter flash shows on the room's destroyable shutters (#1512).
            foreach (ColourEffectFrame frame in room.ColourEffects.ShutterFlashShown)
                room.Enemies.ShowEscapeShutterFlash(frame);
            // An effect an enemy shows shows on the enemy its start handed it, for as long as the room's colour effects run
            // it (#1783, #2288).
            foreach ((IGlowingEnemy enemy, ColourEffectFrame frame) in room.ColourEffects.GlowsShown)
                enemy.ShowGlow(frame);
        }
        // Hyper Beam's colour effect, once she has received the Hyper Beam, runs whenever the colour effects run.
        if (room.ColourEffects.ColourEffectsRun && samus.Drained.HyperBeamColourEffect is { } hyperBeamColours)
            hyperBeamColours.Step(Player.Colours);
    }

    /// <summary>Starts the frame's sprite list for its draw pass.</summary>
    private void BeginSpriteFrame()
    {
        // The update starts the sprite list before running the game state and closes it
        // afterward. Samus is drawn before the escape timer so she keeps her normal overlap
        // precedence.
        Sprites.BeginFrame();
    }

    /// <summary>Prepares the enemy frame and runs the enemy phase before Samus when the frame has no movement to wait for.</summary>
    /// <returns>What the enemy pass did when it ran before Samus, or null when it waits for her.</returns>
    private EnemyFrameResult? PrepareEnemiesAndEarlyEnemyUpdate(GameplayFrameContext context, RoomScope room, SamusState samus)
    {
        EnemyFramePhase.PrepareEnemyFrame(room, samus);
        // Host-disabled Samus movement has no input phase to wait for. Otherwise
        // defer actors until projectile production/update, immediately before the movement phase.
        SamusControlFrame frame = context.Samus;
        frame.RecordEnemyUpdateRanFirst(!Player.GroundedSamusMovementEnabled);
        // The enemy phase follows Samus's input mode. An actor that restores the
        // normal modes there (the gunship) affects this frame's movement phase only:
        // the locked input mode has already run, so it still sees the lock below.
        if (!frame.EnemyUpdateRanFirst)
            return null;
        bool lockedBeforeEnemyUpdate = samus.Control.InputLocked;
        EnemyFrameResult pass = EnemyPhase.RunEnemyUpdatePhase(processingList: EnemyProcessingList.Prepared,
            // A camera placed since its last scroll takes the point Samus stood at as the frame began.
            SamusScrollCheckpoint.At(room.Camera.PreviousSamusPoint is SamusScrolledCheckpoint { Point: var point }
                ? point
                : context.SamusCameraPointAtFrameStart), room, samus);
        frame.RecordEnemyUpdateReleasedInputLock(lockedBeforeEnemyUpdate && !samus.Control.InputLocked);
        return pass;
    }

    /// <summary>Runs the input phase, the enemy phase, the resample after it and the movement phase, in the game's order.</summary>
    /// <returns>What the enemy pass did when it ran after Samus's input, or null when it ran before her or not at all.</returns>
    private EnemyFrameResult? RunSamusAndEnemyUpdate(GameplayFrameContext context, SamusState samus, RoomScope room, bool runsLowHealthCheck)
    {
        SamusControlFrame frame = context.Samus;
        SamusPhases.RunInputPhase(frame, RunningGameState, samus, context.SamusCameraPointAtFrameStart, room);
        EnemyFrameResult? pass = frame.MovementEnabled && !frame.EnemyUpdateRanFirst
            ? EnemyPhase.RunEnemyUpdatePhase(processingList: EnemyProcessingList.Prepared, SamusScrollCheckpoint.At(frame.PreviousCameraPoint), room, samus)
            : null;
        SamusPhases.RunPostEnemyUpdateResample(frame, samus, room);
        SamusPhases.RunMovementPhase(frame, samus, runsLowHealthCheck, room);
        return pass;
    }

    /// <summary>
    /// Opens the gunship's save prompt at the shared suspension seam after the room objects, when the update's enemy pass,
    /// <paramref name="enemyPass"/>, has the gunship asking for it (#2562).
    /// </summary>
    private void OpenGunshipPrompt(EnemyFrameResult? enemyPass)
    {
        // The gunship opens the save prompt from inside the enemy phase, after this frame's
        // ship bob and earlier owners ran. The prompt exists only while Samus is locked
        // inside the ship, so the owners between the enemy phase and here have nothing to
        // act on; the box therefore opens at this shared suspension seam, and the
        // remainder resumes after the YES/NO/completion chain returns.
        if (enemyPass is { } pass && pass.All<GunshipSavePromptRequested>().FirstOrDefault() is { } request)
        {
            if (MessageBox.IsActive)
            {
                throw new InvalidOperationException(
                    "A room object's message box and the gunship save prompt opened in one gameplay frame.");
            }
            MessageBox.Begin(GameplayMessageId.GunshipSaveConfirmation, PlayerButtons.Current, Options.ControllerBindings);
            MessageBoxes.Await(new MessageBoxOutcome.GunshipSaveAnswer(request.Prompt));
        }
    }

    /// <summary>Parks the frame's tail when a room object opened a message box; returns whether it did.</summary>
    private bool SuspendTailForMessageBox(SuspendedGameplayFrameTail tail)
    {
        // The message runs inside the room objects' step: the rest of this gameplay
        // frame (enemy-projectile collision, scrolling, drawing, room main and the shared
        // tail) resumes only after the box closes, after the item's step finishes.
        if (!MessageBox.IsActive)
            return false;
        _suspendedFrameTail = tail;
        return true;
    }

    /// <summary>
    /// The remainder of a main-gameplay update after the room objects' step. A frame whose room-object
    /// pass opened a message box parks this state and resumes it when the box closes.
    /// </summary>
    /// <returns>The game state the update moves the game to, and where its out-of-health check sends Samus.</returns>
    private RuntimeFrameOutcome FinishGameplayFrame(
        SuspendedGameplayFrameTail tail,
        GameplayUpdateStart start,
        GameplayUpdateOwner owner,
        AmmoLoanGuard infiniteAmmoGuard,
        RoomScope room,
        SamusState samus)
    {
        bool deathOwnsSamus = tail.Mode.Movement == SamusMovementMode.Fatal;
        ResolveEnemyProjectileAndPowerBombHits(room, samus, deathOwnsSamus);
        StepCeresElevatorArrival(room, samus, deathOwnsSamus);
        RunMainScrolling(room, samus, tail.PreviousCameraPoint, deathOwnsSamus);
        DrawGameplayActors(room, samus, tail);
        bool startsTimeUp = RunEscapeTimer(tail, owner) || ReturnsToTimeUp(owner);
        Sprites.FinalizeFrame();

        UpdateHud(infiniteAmmoGuard, room, samus);
        // Every step before room main that moves the game to another state has run by now; the game is still in main
        // gameplay at room main only when main gameplay owns the update and none of them did, and only then can the Ceres
        // elevator take Samus (#2534).
        bool gameInMainGameplay = owner.IsMainGameplay &&
            DecideTransition(start, owner, startsTimeUp, RoomMainOutcome.None, room, samus) is GameplayUpdateTransition.Stays;
        RoomMainOutcome roomMain = RunRoomMains(gameInMainGameplay, room, samus);
        // The out-of-health check follows room main, and the game-time step follows it: a death another game state
        // withholds returns before the step (#2509, #2517). Every step that moves the game to another state has run by
        // now, room main's own among them: the Ceres elevator, and the Maridia elevatube's ride, which carries Samus
        // through her collision into the tube's door. The step advances the gameplay clock before shaking. Message-box
        // frames returned above, exactly as the suspended frame does, so item fanfare time is not counted here.
        GameplayUpdateTransition transition = DecideTransition(start, owner, startsTimeUp,
            roomMain, room, samus);
        SamusOutOfHealthRoute outOfHealth = CheckSamusOutOfHealth(samus, owner, transition, room);
        if (owner.CountsPlayTime && outOfHealth != SamusOutOfHealthRoute.DeathWithheld)
            GameTime.Step();

        // Room shaking runs after room main and game-time handling, but before the active-enemy lists are cleared.
        // An enemy attack above may have started an earthquake this frame; it shakes the screen and counts down once
        // here at once, and shakes the enemies still listed this frame. The refill or death the out-of-health check
        // began has already frozen time, so neither shakes on the update it begins on (#2519).
        room.Enemies.HandleRoomShaking(Freeze.TimeIsFrozen);

        DecrementHurtTimers(room, samus);
        infiniteAmmoGuard.Complete();
        return new RuntimeFrameOutcome(transition, outOfHealth);
    }

    /// <summary>
    /// Ends an update that runs no gameplay remainder, because a message box opened in it, waits through it, or closed
    /// with no suspended remainder to resume, then returns the ammunition the update borrowed. A box still open suspends
    /// the update before its out-of-health check (#2517): the game's box runs inside the update's room objects, and
    /// the check after room main reads Samus's energy only once the box has closed, which the update the box closes on
    /// does. A box that closed with nothing to resume makes the check now.
    /// </summary>
    private RuntimeFrameOutcome EndUpdateWithoutGameplayTail(
        GameplayUpdateStart start,
        GameplayUpdateOwner owner,
        AmmoLoanGuard infiniteAmmoGuard,
        RoomScope room,
        SamusState samus)
    {
        // Room main does not run on such an update, so the Ceres elevator takes no one on it.
        GameplayUpdateTransition transition = DecideTransition(start, owner, startsTimeUp: false,
            RoomMainOutcome.None, room, samus);
        SamusOutOfHealthRoute outOfHealth = MessageBox.IsActive
            ? SamusOutOfHealthRoute.NotChecked
            : CheckSamusOutOfHealth(samus, owner, transition, room);
        infiniteAmmoGuard.Complete();
        return new RuntimeFrameOutcome(transition, outOfHealth);
    }

    /// <summary>
    /// The update's out-of-health check (#2509, #2517, #2519): the AUTO reserves' refill begins whatever else the update
    /// started, and Samus's death only while the game is still in main gameplay, which it is when main gameplay owns the
    /// update and none of its earlier steps moved the game to another state. Any other game state owns the update instead:
    /// the death is withheld, and the check returns before the update counts the play time. A game state that enters the
    /// refill or the death (<see cref="GameplayUpdateOwner.EntersOutOfHealthRoute"/>) has the check freeze gameplay time as
    /// it begins either, before the rest of the update. The refill it begins replaces the game state the update's earlier
    /// steps wrote, so a door Samus touched earlier in the update is forgotten, and she enters a door only by touching one
    /// again (#2534).
    /// </summary>
    /// <param name="samus">The update's Samus.</param>
    /// <param name="owner">
    /// The game state that owns the update, whose permissions say whether the game is in main gameplay and whether it
    /// enters the refill or the death.
    /// </param>
    /// <param name="transition">The game state the update's earlier steps moved the game to.</param>
    /// <param name="room">The update's room, whose entered door the refill's beginning forgets.</param>
    private SamusOutOfHealthRoute CheckSamusOutOfHealth(SamusState samus, GameplayUpdateOwner owner, GameplayUpdateTransition transition, RoomScope room)
    {
        SamusOutOfHealthRoute route = DecideSamusOutOfHealthRoute(samus, owner, transition);
        if (!owner.EntersOutOfHealthRoute)
            return route;
        if (route is SamusOutOfHealthRoute.ReserveRefill or SamusOutOfHealthRoute.Death)
            Freeze.GameplayTimeFrozen = true;
        if (route == SamusOutOfHealthRoute.ReserveRefill)
            room.RoomObjects.DoorContacts.ForgetEnteredDoor();
        return route;
    }

    /// <summary>Where the update's out-of-health check sends Samus (#2509, #2517).</summary>
    /// <param name="samus">The update's Samus.</param>
    /// <param name="owner">The game state that owns the update, whose permissions say whether the game is in main gameplay.</param>
    /// <param name="transition">The game state the update's earlier steps moved the game to.</param>
    private SamusOutOfHealthRoute DecideSamusOutOfHealthRoute(SamusState samus, GameplayUpdateOwner owner, GameplayUpdateTransition transition)
    {
        // Every damage source applies its normal amount first. The cannot-die assist acts here, where zero energy would
        // otherwise send Samus to the refill or her death.
        if (Assists.Current.SamusCannotDie && samus.Health == 0)
            samus.Health = 1;
        if (samus.Health > 0)
            return SamusOutOfHealthRoute.HasHealth;
        if (samus.ReserveTankMode == ReserveTankMode.Auto && samus.ReserveEnergy != 0)
            return SamusOutOfHealthRoute.ReserveRefill;
        return owner.IsMainGameplay && transition is GameplayUpdateTransition.Stays
            ? SamusOutOfHealthRoute.Death
            : SamusOutOfHealthRoute.DeathWithheld;
    }

    /// <summary>
    /// The game state the update's steps moved the game to (#2517), after every step that can write one: the escape
    /// countdown's time-up in Samus's update, the gunship's takeoff in the enemies' update, the map station's message
    /// closing, Samus's pause check, the Ceres elevator in room main, and a door Samus touched. Where more than one applies
    /// the earliest listed wins. Read before room main, it says whether the game is still in main gameplay there, which the
    /// Ceres elevator requires (#2534).
    /// </summary>
    /// <param name="start">How the update began.</param>
    /// <param name="owner">The game state that owns the update, whose permissions say whether the game is in main gameplay.</param>
    /// <param name="startsTimeUp">
    /// Whether this update started the time-up earlier: its countdown ran out, or it returned to the time-up the countdown
    /// started (#2518).
    /// </param>
    /// <param name="roomMain">What the update's room main did: took Samus onto the Ceres elevator, or nothing the update follows.</param>
    /// <param name="samus">The update's Samus.</param>
    /// <param name="room">The room the update runs in.</param>
    private GameplayUpdateTransition DecideTransition(
        GameplayUpdateStart start,
        GameplayUpdateOwner owner,
        bool startsTimeUp,
        RoomMainOutcome roomMain,
        RoomScope room,
        SamusState samus)
    {
        if (startsTimeUp)
            return new GameplayUpdateTransition.TimeUp();
        if (room.Enemies.LastEnemyFrame.Reports(new GunshipEvent(GunshipFrameEvent.EscapeTakeoffCompleted)))
            return new GameplayUpdateTransition.LeavesZebes();
        // The map station's completion message pauses on the map as it closes, independently of Start or the station's
        // lock on Samus's input.
        if (start.MessageBox == GameplayMessageId.MapDataAccessCompleted && !MessageBox.IsActive)
            return new GameplayUpdateTransition.MapStationPause();
        if (PauseCheckAdmits(start, owner, room, samus))
            return new GameplayUpdateTransition.Pause();
        if (roomMain == RoomMainOutcome.CeresElevatorTookSamus)
            return new GameplayUpdateTransition.CeresDeparture();
        // A message box opened later in the same update (a station's completion notice in the room objects' step)
        // suspends that update until it closes, so the door waits for the box's final update.
        if (room.RoomObjects.DoorContacts.EnteredDoor is { } door && !MessageBox.IsActive)
            return new GameplayUpdateTransition.Door(door);
        return new GameplayUpdateTransition.Stays();
    }

    /// <summary>Whether Samus's collision has entered a door the door transition has yet to go through (#2545).</summary>
    private static bool DoorEntered(RoomScope room) => room.RoomObjects.DoorContacts.EnteredDoor is not null;

    /// <summary>Samus's pause check: whether a new Start press pauses the game this update.</summary>
    /// <param name="start">How the update began.</param>
    /// <param name="owner">The game state that owns the update, whose permissions say whether the game is in main gameplay.</param>
    /// <param name="samus">The update's Samus.</param>
    /// <param name="room">The room the update runs in.</param>
    private bool PauseCheckAdmits(GameplayUpdateStart start, GameplayUpdateOwner owner, RoomScope room, SamusState samus)
    {
        if (Room.ActiveRoom is null)
            return false;

        // A message box shown inside the update waits for it to close, and its final acknowledgement returns after the
        // point where Samus's update checks for pause, so Start may neither pause while the box is visible nor on the
        // update it closes on. The pause check runs only while Samus takes her normal input: a station's lock holds her
        // input through its whole insertion, message or data load and retraction, so none of it can be paused, and a
        // retraction that restores her input later in the update comes after her pause check.
        return owner.IsMainGameplay &&
               start.MessageBox is null &&
               !MessageBox.IsActive &&
               !start.SamusInputLocked &&
               !samus.Control.InputLocked &&
               !PowerBombExplosionActive &&
               !samus.Xray.TimeIsFrozen &&
               !DoorEntered(room) &&
               AreaIds.Of(Room.ActiveRoom.Area).AllowsPausing &&
               PlayerActions.NewlyPressed.HasAny(GameplayAction.Start);
    }

    /// <summary>Resolves enemy-projectile hits on Samus and her projectiles, then ordinary power-bomb hits.</summary>
    private void ResolveEnemyProjectileAndPowerBombHits(RoomScope room, SamusState samus, bool deathOwnsSamus)
    {
        if (!deathOwnsSamus && !Freeze.TimeIsFrozen)
        {
            room.Enemies.Projectiles.ResolveEnemyProjectileSamusHits(samus);
            room.Enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(
                room.Projectiles, room.BombProjectiles);
            // Every gameplay update resolves the power bomb's hits after the enemy-
            // projectile passes. It reads the explosion's whole-pixel radius, a real size: an explosion whose radius has not
            // grown from zero, before the blast or between blasts, reaches nothing, as the game's zero test skips it.
            // Invincibility timers inside the enemy pass prevent the persistent
            // expansion/afterglow radius from damaging one actor every frame.
            SamusPowerBombExplosionState powerBomb =
                room.BombProjectiles.PowerBombExplosion;
            // The radius's whole pixels, which the radius's own type holds to its range (#1627). A reaction that runs an
            // enemy's main behaviour reads the room's terrain and the camera as they stand now, before the main scrolling.
            room.Enemies.Collision.ResolveOrdinaryPowerBombHits(
                powerBomb.Position,
                powerBomb.Blast.ExplosionRadius.WholePixels,
                room.LevelData,
                room.Camera.PixelPosition,
                samus);
        }
    }

    /// <summary>Steps the fresh-Ceres elevator arrival, which lowers Samus where the enemy projectiles run, before scrolling.</summary>
    private void StepCeresElevatorArrival(RoomScope room, SamusState samus, bool deathOwnsSamus)
    {
        // Main gameplay runs enemy projectiles immediately after Samus's
        // movement phase and before the main scrolling. The two fresh-Ceres actors
        // use that step to lower Samus one pixel at a time; the camera follows each
        // pixel in the same update.
        if (!deathOwnsSamus && room.CeresElevatorArrival is { IsComplete: false } arrival)
        {
            if (arrival.Step(samus))
            {
                // The arrival's Samus lock locked Samus's control before the projectiles
                // were spawned. Re-enable the ordinary movement path only
                // after the pad reaches its deletion height.
                samus.Control.Unlock();
                Player.GroundedSamusMovementEnabled = true;
            }
        }
    }

    /// <summary>Runs the main scrolling, the room's camera floor and the minimap.</summary>
    private void RunMainScrolling(RoomScope room, SamusState samus, FixedPosition<RoomPixels> previousCameraPoint, bool deathOwnsSamus)
    {
        // The door's destination fade runs the enemies and the drawing but not the camera, for ordinary doors as
        // well as elevators: the upward scroll's 32-pixel offset stays until the arrival resumes.
        if (!deathOwnsSamus && !DoorTransition.Passage.IsUnderWay)
        {
            ScrollBoundaryCamera camera = room.Camera;
            ActiveRoomGeometry roomGeometry = RoomLoader.GetActiveRoomGeometry(room.Room);

            previousCameraPoint = samus.ApplyPreviousPositionWrites(previousCameraPoint);
            var currentCameraPoint = samus.Position;

            // The main gameplay state runs the main scrolling after movement and room objects and before
            // drawing Samus, enemies and projectiles, whichever input mode Samus's control has
            // selected. A locked input mode suppresses controller transitions; it does not
            // suppress scrolling. That is why the gunship arriving on Zebes can carry
            // Samus down several screens while her ordinary movement owner is absent.
            if (samus.Grapple.CameraScroll == GrappleCameraScroll.Slow)
            {
                // A fast grapple swing scrolls the camera slowly, leaving the camera speeds from the last normal frame
                // in place.
                camera.TrackGrappleSlowScrolling(currentCameraPoint.Whole, Freeze.TimeIsFrozen);
            }
            else
            {
                camera.TrackMovedSamusHorizontally(
                    previousCameraPoint,
                    currentCameraPoint,
                    new HorizontalCameraContext(
                        KnockedBack: samus.Knockback is not Samus.Movement.SamusKnockback.NoKnockback,
                        MovementType: SamusPoseMetadata.ReadMovementType(samus),
                        XAccelerationMode: samus.HorizontalSpeed.AccelerationMode,
                        Facing: SamusPoseMetadata.ReadFacingDirection(samus),
                        CameraDistance: room.Enemies.RoomMainHooks.CameraDistance),
                    Freeze.TimeIsFrozen);
                camera.TrackMovedSamusVertically(
                    previousCameraPoint,
                    currentCameraPoint,
                    new VerticalCameraContext(
                        YDirection: samus.Kinematics.VerticalVelocity.Direction,
                        FallingCameraOffset: roomGeometry.FallingCameraOffset,
                        RisingCameraOffset: roomGeometry.RisingCameraOffset),
                    Freeze.TimeIsFrozen);
            }
            // A kicked Yard flies off with the camera's own measure of Samus's movement (YardKickTrackingDistance),
            // including its whole-pixel direction check, its added pixel and its previous tracking point. A second
            // frame-end absolute delta loses both the added pixel and the tracking point's timing.
            YardKickDistance.Record(camera.Speed.X);
            samus.ProjectileInheritance.CameraVerticalSpeed = camera.Speed.Y;

            // The camera keeps to the floor the room's bosses hold once both axes have tracked Samus, before the layer
            // scrolls and the sprites read where it stands: live Spore Spawn keeps it from looking above his arena.
            camera.ApplyFloor(room.Enemies.RoomMainHooks.CameraFloor);
            camera.FinishSamusScrolling(currentCameraPoint);

            // The layer scrolls follow the camera in the same update, so the
            // following frame shows the room from level data at the new position.
            BackgroundPhase.UpdateBackgroundScrollingFromCamera(camera, Freeze.TimeIsFrozen);

            // The minimap updates in Samus's normal movement phase from her room position, not the camera's.
            Hud.UpdateMinimap(System, new MinimapFrame { Area = roomGeometry.Area, RoomMap = roomGeometry.MapPosition, RoomSize = room.LevelData.Size, Samus = samus.Position.Whole, FrameCounter = Clock.ShortCounter, MapRevealMode = Assists.Current.MapReveal, PresentationMap = Artwork.ScreenContent.Get(roomGeometry.Area) });
        }
    }

    /// <summary>Restores the attract demo's player input and draws Samus, enemies and projectiles.</summary>
    private void DrawGameplayActors(RoomScope room, SamusState samus, SuspendedGameplayFrameTail tail)
    {
        AttractDemo.RestoreAttractPlayerInput();

        ActorDrawing.DrawGameplayActors(room, samus, tail.Mode.Movement == SamusMovementMode.Fatal ? SamusDrawOwner.DeathSequence : SamusDrawOwner.Gameplay,
            // The stationary lock's movement mode does not run Samus's
            // colour effects. In particular, automatic Reserve recovery freezes a
            // stored shine instead of consuming it while Samus is stationary.
            tail.Mode.Movement is SamusMovementMode.SuitPickup or SamusMovementMode.ScriptLocked ? SamusColourEffects.Hold : SamusColourEffects.Advance,
            tail.DeathStep);
    }

    /// <summary>
    /// Counts the escape timer down and draws it when Samus's normal movement mode runs the update, and only draws it when
    /// the update's state shows the timer Samus is held under (<see cref="GameplayUpdateOwner.ShowsHeldEscapeTimer"/>).
    /// </summary>
    /// <returns>Whether the countdown ran out on this update, starting the time-up.</returns>
    private bool RunEscapeTimer(SuspendedGameplayFrameTail tail, GameplayUpdateOwner owner)
    {
        // Only Samus's normal movement mode runs the escape timer (#2504). Every other mode, held for the reserve refill,
        // the death, the Ceres elevator, an elevator ride, a station, the rainbow beam, a suit pickup or the gunship,
        // neither counts it down nor shows it, so a countdown cannot run out while she is held. The escape stays timed,
        // and the update that restores her control counts on. The enemy phase can start the gunship's boarding this
        // update, which hides the timer as Samus boards; the boarding is decided after the actor transition, before
        // liftoff.
        bool escapeTimerRuns = tail.Mode.Movement == SamusMovementMode.Normal;
        bool startsTimeUp = false;
        if (escapeTimerRuns && EscapeTimer.Process(Clock.Counter, Assists.Current.EscapeTimerHoldsAtOneSecond))
        {
            // The expired countdown starts the time-up from inside gameplay, which the
            // game-state loop begins after the update, whites the target colours and disables colour effects.
            startsTimeUp = true;
            ColourEffectSession.StopColourEffects();
        }
        // The Ceres elevator's hold and blackout show the timer themselves while the elevator holds Samus, as it stands.
        if ((escapeTimerRuns || owner.ShowsHeldEscapeTimer) && EscapeTimer.IsActive)
        {
            EscapeTimerDrawing.Draw(EscapeTimer, Sprites, Artwork.ScreenContent.EscapeTimer, Artwork.SpriteSheets.Get(SpriteSheetId.EscapeTimer));
        }
        return startsTimeUp;
    }

    /// <summary>
    /// Whether the update returns the game to the time-up its run-out escape countdown started (#2518). Once the Zebes time
    /// bomb is set, every gameplay update's Samus draw, which follows her update, writes the time-up again while the
    /// countdown that ran out is shown and the time-up does not own the update. So the AUTO reserves' refill, which takes an
    /// update from the time-up, gives it back on its own next update, with the refill unfinished, and that update's
    /// out-of-health check finds the game out of main gameplay. Before the time bomb is set, in the Ceres escape, only
    /// Samus's Ceres handling writes it again, and the refill's end gives her the normal handling instead, so that game stays
    /// in gameplay with the run-out countdown shown at zero.
    /// </summary>
    /// <param name="owner">The game state that owns the update, whose permissions say whether it returns to the time-up.</param>
    private bool ReturnsToTimeUp(GameplayUpdateOwner owner) =>
        owner.MayReturnToTimeUp && EscapeTimer.Phase == EscapeTimerPhase.Expired && System.HasEvent(StoryEvent.ZebesTimebombSet);

    /// <summary>Rebuilds the HUD counters; the next accepted frame shows the rows as it latches them.</summary>
    private void UpdateHud(AmmoLoanGuard infiniteAmmoGuard, RoomScope room, SamusState samus)
    {
        // The HUD's three rows are rewritten during the update; they become visible at the next frame boundary,
        // which latches the HUD's images (AcceptedFrameBoundary, HudState.Shown).
        // Every gameplay update redraws the HUD after its draw pass, which sets what a Puyo's ceiling strike in the
        // next update reads.
        room.Enemies.PuyoCeilingTurn.HudRedrawn();
        if (Hud.IsInitialized)
        {
            // The loan exists only so firing does not see zero ammunition and cancel the selected weapon. It is
            // returned before the HUD update shows the live counters, so the HUD shows the ammunition Samus holds.
            infiniteAmmoGuard.Complete();
            // The HUD update rebuilds live energy/ammo cells every gameplay frame.
            // Initialization alone is insufficient: Ridley contact and fireballs mutate
            // Samus during this frame, and those values must enter the next frame boundary.
            // The room objects' step has not returned while a pickup message is open.
            // Its newly granted inventory reaches the HUD only after that return.
            if (!room.RoomObjects.CollectibleObjects.HasPendingCollectibleMessage)
                Hud.UpdateGameplayCounters(samus, AudioRequests, Freeze.TimeIsFrozen);
        }
    }

    /// <summary>Runs the block animations and the room's main behaviour.</summary>
    /// <param name="gameInMainGameplay">
    /// Whether the game is still in main gameplay where room main runs: main gameplay owns the update and none of its
    /// earlier steps moved the game to another state (#2534).
    /// </param>
    /// <param name="room">The update's room.</param>
    /// <param name="samus">The update's Samus.</param>
    /// <returns>What the room's main did that the update acts on; a room with no main does nothing (#2545).</returns>
    private RoomMainOutcome RunRoomMains(bool gameInMainGameplay, RoomScope room, SamusState samus)
    {
        room.StepBlockAnimations(samus, System);

        // The room's main runs after gameplay drawing and the HUD, and before the room shakes (#2287). The Ceres elevator
        // shaft's main publishes a tilt for the following frame's presentation and may take Samus onto the Ceres elevator
        // only while the game is still in main gameplay.
        if (room.Main is not { } main)
            return RoomMainOutcome.None;
        return main.Run(new RoomMainUpdate
        {
            Room = room,
            BackgroundScroll = BackgroundScroll,
            RandomNumber = System.RandomNumber,
            Escape = EscapeEffects,
            Player = Player,
            FrameCounter = Clock.Counter,
            TimeIsFrozen = Freeze.TimeIsFrozen,
            Display = Display,
            AllowCeresElevatorDeparture = gameInMainGameplay,
        });
    }

    /// <summary>Decrements Samus's hurt timers and the projectile interaction timer.</summary>
    private static void DecrementHurtTimers(RoomScope room, SamusState samus)
    {
        // Main gameplay decrements these timers after Samus, enemies, drawing, the HUD and background
        // scrolling, the room's own update, the energy-zero check, and room shaking. Keep these
        // countdowns out of individual movement owners: in particular, grapple spike
        // reactions set the knockback timers without handing her movement to the knockback.
        // Death game states do not count them down at all.
        if (!samus.DeathSequence.IsActive)
        {
            samus.DecrementHurtTimers();
            room.Projectiles.Firing.CountInteractionImmunityDown();
        }
    }

    /// <summary>
    /// Updates the pause screen's held-button repeat from the buttons the most recent frame boundary latched.
    /// </summary>
    /// <param name="repeatDelay">The unchanged updates a held button waits through before it repeats.</param>
    public void UpdatePauseHeldButtonRepeat(int repeatDelay) =>
        PauseHeldButtonRepeat.Update(repeatDelay, PlayerButtons.Current, PlayerButtons.NewlyPressed);

    /// <summary>
    /// Publishes the gameplay display at the end of an update: the most recent frame-boundary
    /// latch and the colours latched with it (#3036), and the display state read while the frame is shown
    /// (the door transition's layers and the screen effects). Publishing never advances
    /// simulation, and the result shares no storage with it.
    /// </summary>
    /// <param name="loaded">The runtime's loaded room, from which every gameplay frame is shown.</param>
    public GameplayDisplayFrame PublishDisplayFrame(RoomScope loaded)
    {
        Ensure.NotNull(loaded);
        RoomScope room = loaded;
        SamusState samus = Player.Samus;
        IEnemyDisplaySources enemyDisplays = room.Enemies.DisplaySources;
        var frameEnd = new GameplayFrameEndDisplayInputs(
            Room.DoorTransitionShownLayers,
            room.Sky?.State.Publish(room.Camera.PixelPosition),
            enemyDisplays.BackgroundDistortion,
            enemyDisplays.ClipsRisingFloor,
            enemyDisplays.RainbowBeam,
            CeresHazeRenderSnapshot.Capture(room.CeresHaze),
            Artwork.ScreenContent.CeresHaze,
            PowerBombRenderSnapshot.Capture(room.BombProjectiles.PowerBombExplosion),
            MessageBoxLayer.Capture(MessageBox, room.MessageBoxes),
            SuitPickup.IsActive ? SuitPickup.Beam : null,
            SamusXrayDisplaySnapshot.Capture(samus, LatestGameplayFrame?.XrayBeamStep));
        return new(Display.Latched, frameEnd);
    }

    /// <summary>
    /// The frame's colours as the frame boundary latches them with its scene (#1512, #3036): the room's shading, with the
    /// door's fade and the death sequence's or the fade-out's over all of it, and Samus's own colours on the sprite draws
    /// the boundary makes visible.
    /// </summary>
    FramePaint IFrameColourLatch.LatchColours(RoomScope room, SpriteFrameBuffer shownSprites) =>
        new(Ensure.NotNull(room).Shading(DisplayFades(Player.Samus)), Player.Colours.ShadeOwnDraws(Ensure.NotNull(shownSprites).Draws));

    /// <summary>The fades of the whole display the frame shows over the room's shading, in the order they are laid.</summary>
    private IEnumerable<IGameplayShadingSource> DisplayFades(SamusState samus)
    {
        yield return DoorFade;
        // The death explosion's whiteout writes over the colours the death fade left black, so it shows instead of it.
        if (samus.DeathSequence.Whiteout is not null)
            yield return samus.DeathSequence;
        else
            yield return FadeOut;
    }

    /// <summary>
    /// Gameplay state the rest of a main-gameplay update reads after the room objects' step. A message box opens inside
    /// that step, so a room object that opens one holds the update's rest until the box closes.
    /// </summary>
    /// <param name="Mode">The frame's installed input and movement modes.</param>
    /// <param name="PreviousCameraPoint">Samus's camera point carried to the rest of the update.</param>
    /// <param name="DeathStep">The frame's death-sequence step, which the tail draws, or null when death does not own Samus.</param>
    internal readonly partial record struct SuspendedGameplayFrameTail(
        SamusControlMode Mode,
        FixedPosition<RoomPixels> PreviousCameraPoint,
        SamusDeathSequenceStepResult? DeathStep);

    /// <summary>
    /// Samus's control in the most recent gameplay frame, which the display shows the X-ray beam's last step from: each
    /// gameplay frame replaces it whole as it begins, and a frame that runs no gameplay (a message box's) leaves it.
    /// </summary>
    internal SamusControlFrame? LatestGameplayFrame { get; set; }

    /// <summary>The rest of a gameplay frame, held until a message box a room object opened during it closes.</summary>
    private SuspendedGameplayFrameTail? _suspendedFrameTail;

    /// <summary>
    /// How a gameplay update began, which its game-state transitions read after its steps have run (#2517): as an open
    /// message box's update began, or as a gameplay update's steps begin, after the host's work at the frame boundary (#2526).
    /// </summary>
    /// <param name="MessageBox">The message the box open as the update began shows, or null when none was open.</param>
    /// <param name="SamusInputLocked">Whether Samus's input was locked as the update's steps began.</param>
    internal readonly partial record struct GameplayUpdateStart(GameplayMessageId? MessageBox, bool SamusInputLocked);

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
        // A runtime that has loaded no room yet has no statues to restart; its statue room loads with them open.
        if (statuesOpening && Room.Scope is { TourianStatues: { } statues } scope) statues.RestartWithStatuesOpen(scope.RoomObjects);
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
    /// <returns>
    /// The resumed update's outcome when the box closed into it, otherwise the box's update's, which leaves the
    /// out-of-health check to the update the box closes on; either carries the save the box's close completed.
    /// </returns>
    private RuntimeFrameOutcome StepMessageBoxFrame(
        ControllerButton playerInput,
        GameplayUpdateStart start,
        GameplayUpdateOwner owner,
        AmmoLoanGuard infiniteAmmoGuard,
        RoomScope room,
        SamusState samus)
    {
        // The box reads the controller directly; the frame boundary latches none.
        MessageBoxStep step = MessageBox.Step(playerInput);
        if (MessageBox.LastFrameAudio.AdvancesLiquidWaveAndStatueDescent)
        {
            // One update of the box's pause after closing advances the liquid wave and the statues too.
            LiquidWaveAndStatueDescent.Advance(room.RoomObjects, room);
            room.BombProjectiles.AdvancePowerBombExplosion(samus, PlayerActions.Current, RunningGameState);
        }
        if (step is MessageBoxStep.ChoiceMoved)
            MessageBoxes.SaveChoiceMoved();
        if (step is MessageBoxStep.SavingSoundStarted)
            MessageBoxes.GunshipSavingSoundStarted();
        if (step is not MessageBoxStep.Closed closing)
            return EndUpdateWithoutGameplayTail(start, owner, infiniteAmmoGuard, room, samus);

        SaveStationPersistenceRequest? completedSave =
            MessageBoxes.ResolveClosedMessageBox(closing, frameSuspended: _suspendedFrameTail is not null, room, samus);
        // The next room object's box opens as the one before it returns (#2553), and the suspended update waits for it too.
        RoomObjectPhase.OpenNextMessage();
        // The room objects' step returns once the box and its item's step finish; the
        // parked remainder of that gameplay frame runs now, in this same update.
        if (!MessageBox.IsActive && _suspendedFrameTail is { } tail)
        {
            _suspendedFrameTail = null;
            return FinishGameplayFrame(tail, start, owner, infiniteAmmoGuard, room, samus) with { CompletedSave = completedSave };
        }
        return EndUpdateWithoutGameplayTail(start, owner, infiniteAmmoGuard, room, samus) with { CompletedSave = completedSave };
    }
}
