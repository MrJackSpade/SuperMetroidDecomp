using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using System.Collections.Immutable;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Incremental, frame-steppable C# runtime shell for translated Super Metroid systems.
/// </summary>
/// <remarks>
/// This is not yet the complete game loop. It establishes the real synchronization seam:
/// an accepted NMI drains video work and latches input, advances NMI counters, and then the
/// currently translated main-thread logic runs. Future bank ports can be added to the logic
/// phase without changing every tool or viewer that wants to step one frame.
/// </remarks>
internal sealed class SuperMetroidRuntime
{
    /// <summary>The <see cref="HdmaObjectPhase"/> owner of this SuperMetroidRuntime.</summary>
    internal HdmaObjectPhase HdmaPhase { get; }

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

    /// <summary>The <see cref="TesterTools"/> owner of this SuperMetroidRuntime.</summary>
    internal TesterTools Tester { get; }

    /// <summary>The <see cref="PlmShotBlockVisuals"/> owner of this SuperMetroidRuntime.</summary>
    internal PlmShotBlockVisuals ShotBlockVisuals { get; }

    /// <summary>The <see cref="GameplayActorDrawing"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayActorDrawing ActorDrawing { get; }

    /// <summary>The session's BG tilemap staging (<c>$7E:4000</c>), shared by library backgrounds and X-ray.</summary>
    internal Bg2TilemapStaging Bg2TilemapStaging { get; }

    /// <summary>The session's Mode 7 transfer queue residue, bound into the enemies and every Samus this runtime creates.</summary>
    internal SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue Mode7QueueResidue { get; }

    /// <summary>The <see cref="XrayReveal"/> owner of this SuperMetroidRuntime.</summary>
    internal XrayReveal Xray { get; }

    /// <summary>The <see cref="GameplayWindowRegisters"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayWindowRegisters GameplayWindows { get; }

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

    /// <summary>Executes the queued video transfers from the installed artwork.</summary>
    private void DrainQueuedVramWrites() => VramWrites.DrainTo(Vram, Artwork, EnemySession.ObjCharacters);

    /// <summary>Creates the frame-steppable runtime, installs host gameplay options, and loads the required initial palette.</summary>
    /// <param name="overrides">The host's tester conveniences around the cartridge-authentic runtime: invincibility, infinite ammo, map reveal, escape-timeout prevention, and new-session equipment and Tourian unlocks (#633).</param>
    /// <param name="artwork">The installed catalogs; their initial palette is loaded into CGRAM during construction, and <see cref="BindArtwork"/> binds the rest.</param>
    /// <param name="clock">The session's frame clock, shared with the frontend that counts while no runtime exists.</param>
    /// <param name="mode7QueueResidue">The session's Mode 7 transfer queue residue, which outlives each runtime as WRAM does.</param>
    internal SuperMetroidRuntime(HostGameplayOverrides overrides, IGameplayArtwork artwork,
        FrameClock clock, SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue mode7QueueResidue)
    {
        Ensure.NotNull(artwork);
        Display = new GameplayDisplayState();
        Player = new SamusFrameState();
        Room = new ActiveRoomState();
        Clock = Ensure.NotNull(clock);
        Options = new GameplayOptions();
        Mode7QueueResidue = Ensure.NotNull(mode7QueueResidue);
        EnemySession.Mode7QueueResidue = Mode7QueueResidue;

        BeamCharge = new BeamChargeState();
        Freeze = new GameplayTimeFreeze(Player);
        Tester = new TesterTools(Cgram, Player, System);
        TourianStatueDependencies = new TourianStatueSequenceDependencies { Cgram = Cgram, Player = Player, Room = Room, System = System, Tester = Tester, Freeze = Freeze, VramWrites = VramWrites };
        Artwork = new RuntimeArtwork(new RuntimeArtworkDependencies { EnemySession = EnemySession, Room = Room, RoomFx = RoomFx, Cgram = Cgram, Display = Display, Hud = Hud, MessageBox = MessageBox, Player = Player, Vram = Vram, VramWrites = VramWrites });
        HudSetup = new GameplayHudSetup(new GameplayHudSetupDependencies { Artwork = Artwork, Display = Display, Room = Room, Hud = Hud, Player = Player, Vram = Vram, VramWrites = VramWrites});
        GameplayWindows = new GameplayWindowRegisters(Room);
        Bg2TilemapStaging = new Bg2TilemapStaging();
        Xray = new XrayReveal(BackgroundScroll, Player, Room, System, Vram, Bg2TilemapStaging);
        FrameBoundary = new AcceptedFrameBoundary(new AcceptedFrameBoundaryDependencies { EnemySession = EnemySession, RoomFx = RoomFx, Artwork = Artwork, BackgroundScroll = BackgroundScroll, Controller1 = Controller1, PhysicalController1 = PhysicalController1, Display = Display, DisplayedSprites = DisplayedSprites, GameplayWindows = GameplayWindows, Clock = Clock, Sprites = Sprites, Options = Options, Player = Player, Room = Room, SuitPickup = SuitPickup, Vram = Vram, Xray = Xray, DrainQueuedVramWrites = DrainQueuedVramWrites });
        BackgroundPhase = new BackgroundScrollPhase(BackgroundScroll, Room, Vram);
        ActorDrawing = new GameplayActorDrawing(new GameplayActorDrawingDependencies { EnemySession = EnemySession, Artwork = Artwork, AttractSession = AttractSession, BackgroundScroll = BackgroundScroll, Cgram = Cgram, Controller1 = Controller1, Display = Display, Freeze = Freeze, Clock = Clock, Sprites = Sprites, Player = Player, Room = Room, VramWrites = VramWrites});
        EnemyRoomEffects = new EnemyRoomEffects(new EnemyRoomEffectsDependencies { Cgram = Cgram, Display = Display, EscapeTimer = EscapeTimer, Player = Player, Room = Room, System = System});
        EnemyPhase = new EnemyFramePhase(new EnemyFramePhaseDependencies { BackgroundScroll = BackgroundScroll, Cgram = Cgram, Controller1 = Controller1, Display = Display, EnemyRoomEffects = EnemyRoomEffects, EscapeTimer = EscapeTimer, Freeze = Freeze, Clock = Clock, Oam = Oam, Player = Player, Room = Room, VramWrites = VramWrites});
        HdmaPhase = new HdmaObjectPhase(Freeze, System);
        DoorTransition = new DoorTransitionRunner(new DoorTransitionRunnerDependencies { ActorDrawing = ActorDrawing, BackgroundScroll = BackgroundScroll, Display = Display, EnemyPhase = EnemyPhase, HdmaPhase = HdmaPhase, Clock = Clock, FrameBoundary = FrameBoundary, Sprites = Sprites, Player = Player, Room = Room, System = System});
        EscapeEffects = new EscapeRoomEffects(Freeze, Clock, Room, System);
        ShotBlockVisuals = new PlmShotBlockVisuals();
        RoomLoader = new RoomLoader(new RoomLoaderDependencies { EnemySession = EnemySession, RoomFx = RoomFx, PaletteFxSession = PaletteFxSession, TourianStatueDependencies = TourianStatueDependencies, GameplayWindows = GameplayWindows, ShotBlockVisuals = ShotBlockVisuals, BeamCharge = BeamCharge, Bg2TilemapStaging = Bg2TilemapStaging, Artwork = Artwork, BackgroundPhase = BackgroundPhase, BackgroundScroll = BackgroundScroll, Cgram = Cgram, Controller1 = Controller1, Display = Display, DoorTransition = DoorTransition, EnemyRoomEffects = EnemyRoomEffects, EscapeEffects = EscapeEffects, GameTime = GameTime, Hud = Hud, HudSetup = HudSetup, Clock = Clock, Options = Options, Player = Player, Room = Room, System = System, Tester = Tester, Vram = Vram, VramWrites = VramWrites});
        AttractDemo = new AttractDemoDriver(new AttractDemoDriverDependencies { EnemySession = EnemySession, Artwork = Artwork, AttractSession = AttractSession, Cgram = Cgram, Controller1 = Controller1, HudSetup = HudSetup, FrameBoundary = FrameBoundary, Player = Player, Room = Room, RoomLoader = RoomLoader, System = System});
        DeathSequence = new DeathSequenceRunner(new DeathSequenceRunnerDependencies { ActorDrawing = ActorDrawing, Artwork = Artwork, Cgram = Cgram, Display = Display, FrameBoundary = FrameBoundary, Sprites = Sprites, Player = Player, Room = Room, VramWrites = VramWrites});
        MessageBoxes = new MessageBoxFlow(new MessageBoxFlowDependencies { BackgroundScroll = BackgroundScroll, Freeze = Freeze, Hud = Hud, MessageBox = MessageBox, Player = Player, Room = Room, SuitPickup = SuitPickup, System = System, Vram = Vram, VramWrites = VramWrites});
        PlmPhase = new PlmFramePhase(new PlmFramePhaseDependencies { Artwork = Artwork, BackgroundScroll = BackgroundScroll, Cgram = Cgram, Controller1 = Controller1, EnemyRoomEffects = EnemyRoomEffects, Freeze = Freeze, MessageBox = MessageBox, MessageBoxes = MessageBoxes, Player = Player, Room = Room, SuitPickup = SuitPickup, Vram = Vram, VramWrites = VramWrites});
        RoomMains = new RoomMainRoutines(new RoomMainRoutinesDependencies { BackgroundScroll = BackgroundScroll, Player = Player, Room = Room, System = System, Vram = Vram, VramWrites = VramWrites});
        SamusPhases = new SamusFramePhases(new SamusFramePhasesDependencies { Artwork = Artwork, AttractDemo = AttractDemo, SuitPickup = SuitPickup, Xray = Xray, Cgram = Cgram, Controller1 = Controller1, EnemyPhase = EnemyPhase, Freeze = Freeze, GameTime = GameTime, Clock = Clock, Options = Options, Player = Player, Room = Room, System = System });

        Artwork.InitialPaletteArt = artwork.GameplayBasePalettes;
        Tester.GrantAllEquipmentEnabled = overrides.GrantAllEquipment;
        Tester.UnlockTourianEnabled = overrides.UnlockTourian;
        Options.PlayerInvincibilityEnabled = overrides.PlayerInvincibilityEnabled;
        Options.InfiniteAmmoEnabled = overrides.InfiniteAmmoEnabled;
        Options.PreventEscapeTimeout = overrides.PreventEscapeTimeout;
        Options.MapRevealMode = Enum.IsDefined(overrides.MapRevealMode)
            ? overrides.MapRevealMode
            : throw new ArgumentOutOfRangeException(nameof(overrides.MapRevealMode));


        // $82:82C5 copies all 512 bytes of kInitialPalette from ROM $9A:8000. The retail
        // game stages this through WRAM before NMI uploads CGRAM; initializing the modeled
        // PPU here produces the same starting colors while that fade pipeline is ported.
        (Artwork.InitialPaletteArt ?? throw new InvalidOperationException(
            "Gameplay requires the installed initial palette catalog.")).LoadInitial(Cgram);
    }

    /// <summary>
    /// Binds the installed catalogs to the runtime's owners: every catalog when <paramref name="previous"/> is null (a new
    /// or restored runtime, whose catalogs are never serialized), otherwise only the catalogs whose reference changed.
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
        Bind(a => a.StandardObjectArt, value => Artwork.StandardObjectArt = value);
        Bind(a => a.SamusBodyArt, value => Artwork.SamusBodyArt = value);
        Bind(a => a.RoomCharacterArt, value => Artwork.RoomCharacterArt = value);
        Bind(a => a.RoomPaletteArt, value => Artwork.RoomPaletteArt = value);
        Bind(a => a.RoomMetatileArt, value => Artwork.RoomMetatileArt = value);
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
        Bind(a => a.RoomPlmDynamicCollectibleArt, value => ShotBlockVisuals.RoomPlmDynamicCollectibleArt = value);
        Bind(a => a.XrayRevealVisuals, value => Xray.XrayRevealVisuals = value);
        Bind(a => a.RoomBackgroundTilemapArt, value => Artwork.RoomBackgroundTilemapArt = value);
        Bind(a => a.RoomSkyTilemapArt, value => Artwork.RoomSkyTilemapArt = value);
        Bind(a => a.ProjectileCompositions, value => Artwork.ProjectileCompositions = value);
        Bind(a => a.ProjectileFrameBindings, value => Artwork.ProjectileFrameBindings = value);
        Bind(a => a.TrailArtwork, value => Artwork.TrailArtwork = value);
        Bind(a => a.SamusOverreads, value => Artwork.SamusOverreads = value);
        Bind(a => a.ChargeFlarePlacement, value => Artwork.ChargeFlarePlacement = value);
        Bind(a => a.ChargeFlareCompositions, value => Artwork.ChargeFlareCompositions = value);
        Bind(a => a.GrappleArtwork, value => Artwork.GrappleArtwork = value);
        Bind(a => a.BeamArtwork, value => Artwork.BeamArtwork = value);
        Bind(a => a.EnemyTileArtwork, value =>
        {
            EnemySession.TileArtwork = value;
            Room.CeresElevatorArrival?.BindProjectileSpritemaps(value?.ProjectileSpritemaps);
        });
    }

    /// <summary>
    /// Set when the escape countdown expired during this runtime's frame: <c>$90:E0EC</c>
    /// writes game state $23 from inside gameplay. The frontend dispatcher consumes it.
    /// </summary>
    public bool TimeUpRequested { get; private set; }

    /// <summary>Takes the pending time-up publication; the dispatcher may still override it.</summary>
    public bool ConsumeTimeUpRequest()
    {
        bool requested = TimeUpRequested;
        TimeUpRequested = false;
        return requested;
    }

    /// <summary>Bank-$80 shared random/event/input-filter state.</summary>
    public Bank80SystemState System { get; } = new();

    /// <summary>Gameplay clock mirrored at WRAM $09DA-$09E1 and in each SRAM slot.</summary>
    public GameTimeState GameTime { get; } = new();

    /// <summary>
    /// Controller-1 NMI latch from <c>$80:9459</c>, in the default action layout: the configured bindings are
    /// applied once at the latch, so bank-$90/$91 action checks read the buttons they test natively through the
    /// binding words at <c>$09B2-$09BE</c>.
    /// </summary>
    public ControllerInputState Controller1 { get; } = new();

    /// <summary>
    /// The same NMI sample as physical buttons, as WRAM <c>$8B/$8F</c> hold it. Code that tests fixed button bits
    /// rather than the binding words (the bank-$82 pause menu) reads this latch.
    /// </summary>
    public ControllerInputState PhysicalController1 { get; } = new();

    /// <summary>Escape timer state machine from <c>$80:9DE7</c>.</summary>
    public EscapeTimer EscapeTimer { get; } = new();

    /// <summary>
    /// Frontend language bit consumed by room actors whose cartridge script differs
    /// between English and Japanese. The enemy owner retains the actual native branch.
    /// </summary>
    public bool JapaneseText
    {
        get => EnemySession.JapaneseText;
        set => EnemySession.JapaneseText = value;
    }

    /// <summary>Ordinary seven-byte-record VRAM write table.</summary>
    public VramWriteQueue VramWrites { get; } = new();

    /// <summary>PPU video RAM receiving accepted-NMI transfers.</summary>
    public SnesVram Vram { get; } = new();

    /// <summary>PPU color RAM initialized from <c>kInitialPalette</c> at ROM <c>$9A:8000</c>.</summary>
    public SnesCgram Cgram { get; } = new();

    /// <summary>The palette-FX handler enable bit and heat phases, which outlive each room's palette FX.</summary>
    public PaletteFxSessionState PaletteFxSession { get; } = new();

    /// <summary>The sprite frame the main loop is building for the next NMI: OAM sprites plus whole-frame draws.</summary>
    public SpriteFrameBuffer Sprites { get; } = new();

    /// <summary>Current 544-byte sprite table prepared for the next PPU OAM upload.</summary>
    public OamBuffer Oam => Sprites.Oam;

    /// <summary>
    /// Sprite frame made visible by the most recent accepted NMI. Renderers use it; debugger watches
    /// can compare it with <see cref="Sprites"/>, which the current main-loop pass is preparing.
    /// </summary>
    public SpriteFrameBuffer DisplayedSprites { get; } = new();

    /// <summary>Active room's fixed 32-slot bank-$A0 enemy system.</summary>
    public EnemySession EnemySession { get; } = new();

    /// <summary>The last enemy pass's results, for readers after the frame.</summary>
    internal IEnemyFrameResults EnemyFrameResults => Room.LoadedScope.Enemies;


    /// <summary>
    /// WRAM GameState ($0998) of the frontend dispatch now running. Samus commands such as
    /// $90:F4A2 test it from inside the outer frame; runtime-only hosts run state eight.
    /// </summary>
    [field: NonSerialized]
    public SuperMetroid.Core.Gameplay.SuperMetroidGameState DispatchGameState { get; set; } = SuperMetroid.Core.Gameplay.SuperMetroidGameState.MainGameplay;

    /// <summary>
    /// Native power-bomb explosion status at WRAM <c>$0CE2</c>, 0 through 65535. X-ray setup rejects every
    /// nonzero phase. The getter reads the translated bank-$88 owner; the setter is a
    /// debugger seam for forcing preconditions without constructing an explosion.
    /// </summary>
    public int PowerBombExplosionStatus
    {
        // Before the first room exists there is no explosion, so its status is the cleared one.
        get => Room.BombProjectiles?.PowerBombExplosion.Status ?? 0;
        set => Room.LoadedScope.BombProjectiles.PowerBombExplosion.SetStatusForDebugging(value);
    }

    /// <summary>
    /// The power-bomb gate in all three sound-queue routines (<c>$80:9072</c>,
    /// <c>$80:90F4</c>, <c>$80:9176</c>): while the explosion status is negative every
    /// request is dropped after the occupancy check, whoever makes it.
    /// </summary>
    public bool PowerBombExplosionSuppressesSounds => Room.BombProjectiles?.PowerBombExplosion.IsActive == true;

    /// <summary>The Fireflea state, COLDATA shadows and installed FX artwork, which outlive each room's layer-3 FX.</summary>
    public RoomFxSession RoomFx { get; } = new();








    /// <summary>The held beam charge, flare and charge palette, which survive room loads; each room's projectiles share it.</summary>
    internal BeamChargeState BeamCharge { get; }


    /// <summary>
    /// Shared bank-$85 gameplay message coroutine. A permanent pickup activates this
    /// owner and subsequent <see cref="StepFrame"/> calls advance accepted NMIs without
    /// running gameplay until the ROM-authored box has closed.
    /// </summary>
    public GameplayMessageBoxState MessageBox { get; } = new();

    /// <summary>
    /// Shared post-message Varia/Gravity light-beam transformation from banks $88/$91.
    /// </summary>
    public SamusSuitPickupState SuitPickup { get; } = new();

    /// <summary>Mutable gameplay HUD tilemap at WRAM <c>$7E:C608</c>.</summary>
    public HudState Hud { get; } = new();

    /// <summary>
    /// Bank-$80 parallax/register/block dispatcher fed by the active layer-1 camera.
    /// Its requests become row/column staging DMAs when a room loader supplies level data.
    /// </summary>
    public BackgroundScrollState BackgroundScroll { get; } = new();

    /// <summary>Runs one accepted NMI and publishes an empty finalized OAM frame without advancing gameplay owners.</summary>
    public void RunBlankGameplayFrame(SnesButton controllerInput)
    {
        FrameBoundary.BeginAcceptedFrame(controllerInput);
        Sprites.BeginFrame();
        Sprites.FinalizeFrame();
        RoomLoader.PublishDoorScrollingIrqNmiRequest();
    }

    /// <summary>Runs one accepted NMI followed by the translated per-frame timer logic, as <paramref name="request"/> asks.</summary>
    /// <param name="controller1Input">Host-provided raw SNES controller word, normalized through action bindings when the accepted NMI latches player input; attract playback publishes its independent scripted words later.</param>
    /// <param name="request">The state policy, audio calls and post-NMI host work for this frame.</param>
    public RuntimeFrameResult StepFrame(SnesButton controller1Input, RuntimeFrameRequest request)
    {
        Ensure.NotNull(request);
        Tester.ApplyTesterInventory();
        HostInfiniteAmmoFrameGuard infiniteAmmoGuard =
            HostInfiniteAmmoFrameGuard.Begin(Options.InfiniteAmmoEnabled, Player.Samus);
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
    /// Advances one accepted NMI and the corresponding gameplay dispatch while the
    /// frame-exit ammo guard remains active through translated callbacks.
    /// </summary>
    private RuntimeFrameResult StepFrameGuarded(
        SnesButton controller1Input,
        RuntimeFrameRequest frameRequest,
        HostInfiniteAmmoFrameGuard infiniteAmmoGuard)
    {
        RebindRoutingReferences();
        // Bank $85's message routine runs on lag frames inside the suspended dispatch: the
        // NMI accepts no input and the main loop neither calls the RNG nor any owner.
        if (MessageBox.IsActive)
            return StepMessageBoxFrame(controller1Input, frameRequest.Policy, infiniteAmmoGuard);
        BeginAcceptedGameplayFrame(controller1Input, frameRequest);
        RunHdmaObjects();
        AdvanceMainLoopRandom();
        RunPreDispatchEffects();
        var context = new GameplayFrameContext(SampleSamusCameraPoint());
        PrepareEnemyProcessingList();
        RunPaletteFx();
        BeginOamFrame();
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
        return FinishGameplayFrame(context.Tail, frameRequest.Policy, infiniteAmmoGuard, resumedAfterMessageBox: false);
    }

    /// <summary>Reattaches the routing references that are deliberately not serialized, then applies pending Chozo statue PLMs.</summary>
    private void RebindRoutingReferences()
    {
        // Routing references are deliberately not serialized as duplicate native state.
        // Reattach before any producer executes, including after loading an older snapshot.
        Artwork.BindGrapplePresentation();
        GameplayWindows.BindGameplayWindowRegisters(Room.LoadedScope.Projectiles);
        RoomScope scope = Room.LoadedScope;
        Artwork.BindProjectiles(scope.Projectiles, scope.BombProjectiles);
        scope.Projectiles.Earthquakes.BindRoomEarthquakeOwner(scope.Enemies);
        scope.Plms.Sounds.BindPowerBombAudio(scope.BombProjectiles.PowerBombExplosion);
        Player.Samus?.Shinespark.BindProjectileOwners(scope.Projectiles, scope.BombProjectiles.PowerBombExplosion);
        EnemyRoomEffects.ApplyPendingChozoStatuePlms(scope.Plms, scope.Layer3Fx, Room.LoadedScope.Enemies);
    }

    /// <summary>Opens the frame's impact-audio window, runs the accepted-frame boundary and the host's post-boundary work, and prepares the window registers.</summary>
    private void BeginAcceptedGameplayFrame(SnesButton controller1Input, RuntimeFrameRequest frameRequest)
    {
        Room.LoadedScope.Projectiles.ImpactAudio.BeginImpactAudioFrame(cinematicActive: false);
        FrameBoundary.BeginAcceptedFrame(controller1Input);
        frameRequest.AfterAcceptedNmi();
        GameplayWindows.PrepareGameplayWindowRegisters(Room.LoadedScope);
    }

    /// <summary>Runs the bank-$88 HDMA-object pre-instructions and the power-bomb HDMA before the main loop's RNG call.</summary>
    private void RunHdmaObjects()
    {
        // HDMA owns shared RNG mutations before the main loop advances it. A
        // message-box NMI wait does not execute that outer-loop HDMA pass.
        if (Room.Camera is not null && !MessageBox.IsActive)
            HdmaPhase.AdvanceHdmaObjectPreInstructions(Room.LoadedScope.Plms, Room.LoadedScope);

        // Blast HDMA belongs to the outer frame, not Samus alpha. A statue carry
        // replaces her handlers while the already-spawned blast continues normally.
        // Its cleanup runs Samus command $1E, which can queue sounds before the Samus
        // handler; the frame's Samus sound window therefore opens here.
        if (Player.Samus is { } hdmaSamus && !MessageBox.IsActive)
        {
            hdmaSamus.LiquidPhysics.BeginFrameSoundRequests(Room.LoadedScope.BombProjectiles.PowerBombExplosion);
            Room.LoadedScope.BombProjectiles.AdvancePowerBombHdma(hdmaSamus, Controller1.Current, DispatchGameState);
        }
    }

    /// <summary>Makes the main loop's <c>GenerateRandomNumber</c> call ($82:894F).</summary>
    private void AdvanceMainLoopRandom()
    {
        // The bank-$82 main loop calls GenerateRandomNumber at $82:894F on every accepted
        // main-loop pass, immediately after the bank-$88 HDMA-object handler and before it
        // dispatches the current game state. Several room/enemy routines deliberately only
        // sample $05E5 instead of advancing it themselves. Falling Tile room main $8F:E525
        // is the first visibly obvious example: without this global call, every ceiling
        // fragment selects the same X table entry forever. Keep this unconditional and in
        // the frame prologue; advancing only at a consumer would change the shared sequence
        // and would be a bespoke behavioral patch rather than the cartridge call order.
        System.NextRandom();
    }

    /// <summary>Runs the owners before the game-state dispatch: Ceres haze, the suit transformation, Phantoon's wave and blending, and room layer-3 FX.</summary>
    private void RunPreDispatchEffects()
    {
        Room.LoadedScope.CeresHaze.Step();

        MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = false;

        // HDMA object pre-instructions run once near the start of each ordinary gameplay
        // pass. A transformation spawned when the preceding message returned therefore
        // receives its first stage call on this following accepted frame, while every
        // normal owner below continues to run around the input-locked, centered Samus.
        if (SuitPickup.IsActive)
        {
            if (Player.Samus is null)
                throw new InvalidOperationException("A suit transformation has no Samus owner.");
            SuitPickup.Step(Player.Samus, Cgram);
        }

        // DisplayMessageBox waits on NMI while retaining the already-published room OAM,
        // music, and sound engines. Audio mixing lives in the frontend, but this translated
        // runtime must still latch controller input and block every gameplay owner during
        // those waits. This early seam is shared by all permanent-item identities.
        // Room FX objects run in the ordinary gameplay owner list. In particular, they
        // must not scroll or animate during DisplayMessageBox's NMI-only wait loop above.
        // Keeping this after that early-return seam reproduces that native suspension and
        // prevents rain/fog from advancing behind a station or item message.
        if (Room.LoadedScope.Enemies.DisplaySources.Phantoon is { } waveBoss)
        {
            waveBoss.Wave.Step(waveBoss);
            waveBoss.Blending.Step(waveBoss, Display.LayerBlendingDefaultConfig);
        }
        if (Room.Camera is not null)
        {
            Room.LoadedScope.Layer3Fx.Step(Vram, new RoomLayer3FxFrame { Camera = Room.Camera.PixelPosition, TimeIsFrozen = Freeze.TimeIsFrozen, MainGameLoopCarry = System.MainGameLoopCarry, RandomNumber = System.RandomNumber, FirefleaDarknessLevel = Room.LoadedScope.Enemies.RoomStatus.FirefleaDarknessLevel, PowerBomb = Room.LoadedScope.BombProjectiles.PowerBombExplosion, LiquidMotionAlreadyAdvanced = Room.LoadedScope.Layer3Fx.MovesLiquidInHdmaPass });
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
                Room.LoadedScope.Layer3Fx.ApplyToSamusLiquidPhysics(Player.Samus.LiquidPhysics);
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

    /// <summary>Builds the enemy processing list ($82:8B47) that the grapple scan and EnemyMain use.</summary>
    private void PrepareEnemyProcessingList()
    {
        // `GameState_8_MainGameplay` begins with Determine_Which_Enemies_to_Process
        // ($82:8B47), before palette FX and Samus. The grapple scan uses this list.
        if (Room.Camera is not null && Room.LoadedScope.Enemies.IsLoaded)
            Room.LoadedScope.Enemies.ProcessingWindow.PrepareEnemyProcessingList(Room.Camera.PixelPosition);
    }

    /// <summary>Runs the bank-$8D palette-FX handler ($8D:C527) and Hyper Beam's palette object.</summary>
    private void RunPaletteFx()
    {
        // Gameplay state eight calls `$8D:C527` before `$91:8000` dispatches Samus and
        // before `$A0:868F` processes enemies. Controller function three is invoked by the
        // Baby during that later enemy phase, so a newly spawned `$E1F0` object naturally
        // begins on the following frame. Debug scripts that grant Hyper Beam before their
        // first StepFrame get the same handler-first behavior immediately.
        // Native has one descending bank-$8D object pool. Room FX is spawned first and
        // therefore occupies its highest slots; Hyper Beam later takes the next free slot.
        // Running the room owner before the specialized Hyper Beam owner preserves that
        // ordering until both are consolidated behind one allocator.
        // Door destination drawing borrows this frame path, but native state $0B
        // ($82:E737) does not call the palette-FX handler. Its colors must remain
        // owned by the gradual room fade until the door transition releases them.
        if (Room.LoadedScope.PaletteFx.HandlerEnabled && !Room.LoadedScope.Enemies.Elevator.ElevatorDoorTransitionActive &&
            Player.Samus?.Xray.ArePaletteFxSuspended != true)
        {
            Room.LoadedScope.PaletteFx.Step(Cgram, Artwork.MapPresentation?.RoomPaletteFx ?? throw new InvalidOperationException(
                    "Gameplay palette FX requires installed map presentation colors."), new PaletteFxConditions { SamusY = Player.Samus?.Position.Y.Whole ?? 0, EquippedItems = Player.Samus?.EquippedItems ?? 0, EnemyZeroIsDead = Room.LoadedScope.Enemies.SlotTable.Slots.Count == 0 || Room.LoadedScope.Enemies.SlotTable.Slots[0].Health == 0, AreaMiniBossDefeated = Room.ActiveRoom is { } paletteRoom &&
                    System.HasAnyBossBits(paletteRoom.AreaIndex, BossBits.AreaMiniBoss), Samus = Player.Samus, NmiFrameCounter = Clock.Counter }, Room.LoadedScope.BombProjectiles.PowerBombExplosion);
        }
        // Hyper Beam's palette object lives in the same native pool and handler.
        if (Room.LoadedScope.PaletteFx.HandlerEnabled)
        {
            Player.Samus?.Drained.HyperBeamPaletteFx.Step(
                Cgram,
                Artwork.BeamArtwork?.HyperBeamFxColors);
        }
    }

    /// <summary>Clears high OAM for the frame's draw pass.</summary>
    private void BeginOamFrame()
    {
        // The bank-$82 main loop clears high OAM and resets its stack before dispatching
        // game state, then finalizes unused entries afterward. Samus is emitted before the
        // escape timer so lower OAM indices retain their normal overlap precedence.
        Sprites.BeginFrame();
    }

    /// <summary>Prepares the enemy frame and runs EnemyMain before Samus when the frame has no movement to wait for.</summary>
    private void PrepareEnemiesAndHoistedEnemyMain(GameplayFrameContext context)
    {
        EnemyPhase.PrepareEnemyFrame(Room.LoadedScope);
        // Host-disabled Samus movement has no alpha phase to wait for. Otherwise
        // defer actors until projectile production/update, immediately before beta.
        SamusHandlerFrame frame = context.Samus;
        frame.EnemyMainAlreadyRan = Player.Samus is null || Room.Camera is null || !Player.GroundedSamusMovementEnabled;
        // Native EnemyMain follows SamusCurrentStateHandler. An actor that restores the
        // normal handler pair there (gunship $A2:A987) affects this frame's beta only:
        // the locked alpha has already run, so it still sees the lock below.
        if (frame.EnemyMainAlreadyRan)
        {
            bool lockedBeforeEnemyMain = Player.Samus?.InputLocked == true;
            EnemyPhase.RunEnemyMainPhase(processingListPrepared: true,
                Room.Camera?.PreviousSamusPoint ?? context.SamusCameraPointAtFrameStart, Room.LoadedScope);
            frame.EnemyMainReleasedAlphaLock = lockedBeforeEnemyMain && Player.Samus?.InputLocked == false;
        }
    }

    /// <summary>Runs the frame handler's alpha, EnemyMain, the post-EnemyMain resample and beta, in native order.</summary>
    private void RunSamusAndEnemyMain(GameplayFrameContext context, SamusState samus, ScrollBoundaryCamera camera, RuntimeFrameAudio audio)
    {
        SamusHandlerFrame frame = context.Samus;
        SamusPhases.RunAlpha(frame, DispatchGameState, samus, camera, context.SamusCameraPointAtFrameStart, Room.LoadedScope);
        if (frame.MovementEnabled && !frame.EnemyMainAlreadyRan)
            EnemyPhase.RunEnemyMainPhase(processingListPrepared: true, frame.PreviousCameraPoint, Room.LoadedScope);
        SamusPhases.RunPostEnemyMainResample(frame, samus, Room.LoadedScope.BombProjectiles, Room.LoadedScope.Projectiles, Room.LoadedScope);
        SamusPhases.RunBeta(frame, samus, camera, audio, Room.LoadedScope);
    }

    /// <summary>Opens the gunship's save prompt at the shared suspension seam after PLMs.</summary>
    private void OpenGunshipPrompt()
    {
        // $A2:AB1F calls the bank-$85 routine from inside EnemyMain, after this frame's
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
        // Bank $85's message routine runs inside PLM_Handler: the rest of this gameplay
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
        HostInfiniteAmmoFrameGuard infiniteAmmoGuard,
        bool resumedAfterMessageBox)
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

        QueueHudUpload(infiniteAmmoGuard);
        RunRoomMains(policy);
        // HandleSamusOutOfHealthAndGameTile advances the four-word gameplay clock after
        // room main and before shaking. Message-box frames returned above, exactly as the
        // suspended native coroutine does, so item fanfare time is not counted here.
        if (policy.AdvanceGameTime)
            GameTime.Step();

        // `$82:8BAF` executes room shaking after room main ASM and game-time handling, but
        // before the active-enemy lists are cleared. Enemy attacks above may have installed
        // a new global quake this frame; consuming it here gives that request its first
        // displacement/decrement immediately and preserves the list used for actor shake.
        if (Room.LoadedScope.Enemies.IsLoaded)
            Room.LoadedScope.Enemies.HandleRoomShaking(Freeze.TimeIsFrozen);

        DecrementHurtTimers();

        // A resumed tail's frame already published its audio when it suspended.
        if (!resumedAfterMessageBox)
            FrameBoundary.CompletedGameplayAudioPublication++;
        return Snapshot(infiniteAmmoGuard);
    }

    /// <summary>Resolves enemy-projectile hits on Samus and her projectiles, then ordinary power-bomb hits ($A0:A306).</summary>
    private void ResolveEnemyProjectileAndPowerBombHits(SamusState samus, bool deathOwnsSamus)
    {
        if (!deathOwnsSamus && !Freeze.TimeIsFrozen)
        {
            Room.LoadedScope.Enemies.Projectiles.ResolveEnemyProjectileSamusHits(samus);
            Room.LoadedScope.Enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(
                Room.LoadedScope.Projectiles, Room.LoadedScope.BombProjectiles);
            // GameState_8 calls `$A0:A306` every gameplay frame after the enemy-
            // projectile passes. The native routine reads the high byte of live WRAM
            // radius `$0CEE`; a zero byte is its own inactive/pre-explosion guard.
            // Invincibility timers inside the enemy pass prevent the persistent
            // expansion/afterglow radius from damaging one actor every frame.
            SamusPowerBombExplosionState powerBomb =
                Room.LoadedScope.BombProjectiles.PowerBombExplosion;
            Room.LoadedScope.Enemies.Collision.ResolveOrdinaryPowerBombHits(
                powerBomb.Position,
                checked((byte)powerBomb.ExplosionRadius.WholePixels),
                samus);
        }
    }

    /// <summary>Steps the fresh-Ceres elevator arrival, which lowers Samus in the enemy-projectile slot before scrolling.</summary>
    private void StepCeresElevatorArrival(SamusState samus, bool deathOwnsSamus)
    {
        // Native GameState_8 runs bank-$86 enemy projectiles immediately after Samus's
        // movement handler and before MainScrollingRoutine. The two fresh-Ceres actors
        // use that slot to lower Samus one pixel at a time; moving this call below the
        // camera would make the viewport trail one frame behind the cartridge.
        if (!deathOwnsSamus && Room.CeresElevatorArrival is { IsComplete: false } arrival)
        {
            if (arrival.Step(samus))
            {
                // Command $0E installed a locked Samus handler before the projectiles
                // were spawned. Re-enable the translated ordinary movement path only
                // after the pad reaches the native Y=$0048 deletion condition.
                samus.InputLocked = false;
                Player.GroundedSamusMovementEnabled = true;
            }
        }
    }

    /// <summary>Runs MainScrollingRoutine, its scrolling-finished hook, background streaming and the minimap.</summary>
    private void RunMainScrolling(SamusState samus, ScrollBoundaryCamera camera, FixedPosition<RoomPixels> previousCameraPoint, bool deathOwnsSamus)
    {
        // $82:E737's destination fade runs enemy/draw owners but not $90:94EC.
        // Its shared $0795 gate is set for ordinary doors as well as elevators.
        // Running the normal camera here consumes the upward IRQ's +32 offset
        // before arrival resumes, exposing wrapped Samus art above the room.
        if (!deathOwnsSamus && !Room.LoadedScope.Enemies.Elevator.ElevatorDoorTransitionActive)
        {
            if (Room.LevelData is null || Room.BackgroundStreamer is null)
                throw new InvalidOperationException("Gameplay camera tracking requires active room stream data.");

            ActiveRoomGeometry roomGeometry = RoomLoader.GetActiveRoomGeometry();

            previousCameraPoint = samus.ApplyPreviousPositionWrites(previousCameraPoint);
            var currentCameraPoint = samus.Position;

            // GameState_8 calls MainScrollingRoutine after movement/PLMs and before
            // DrawSamusEnemiesAndProjectiles regardless of which Samus input handler is
            // installed. InputLocked suppresses controller transitions; it does not
            // suppress scrolling. That is why the station-eighteen gunship can carry
            // Samus down several screens while her ordinary movement handler is absent.
            if (samus.Grapple.SlowScrolling)
            {
                // $90:94F2: a fast grapple swing takes the slow branch, which leaves
                // the camera speed words from the last normal frame in place.
                camera.TrackGrappleSlowScrolling(currentCameraPoint.Whole);
            }
            else
            {
                camera.TrackMovedSamusHorizontally(
                    previousCameraPoint,
                    currentCameraPoint,
                    new HorizontalCameraContext(
                        KnockedBack: samus.KnockbackDirection != Samus.Movement.SamusKnockbackDirection.None,
                        MovementType: samus.PoseMetadata.ReadMovementType(),
                        XAccelerationMode: samus.HorizontalSpeed.AccelerationMode,
                        PoseXDirection: samus.PoseMetadata.ReadPoseXDirection(),
                        CameraDistanceIndex: Room.LoadedScope.Enemies.RoomMainHooks.ZebetiteCameraDistanceIndex));
                camera.TrackMovedSamusVertically(
                    previousCameraPoint,
                    currentCameraPoint,
                    new VerticalCameraContext(
                        YDirection: samus.Kinematics.YDirection,
                        UpScroller: roomGeometry.UpScroller,
                        DownScroller: roomGeometry.DownScroller));
            }
            // $90:96C0 writes the same distance-plus-one words consumed by Yard
            // kick setup. Publish the camera calculation itself, including its
            // integer-only sign test and previous scrolling checkpoint. A second
            // frame-end absolute delta loses both the bias and checkpoint timing.
            samus.AbsoluteMovedLastFrameXFixed = unchecked((uint)camera.Speed.X.RawValue);
            samus.ProjectileInheritance.CameraYSubspeed = camera.Speed.Y.Fraction;

            // MainScrollingRoutine `$90:9563` invokes the mutable global
            // scrolling-finished hook after both axes have tracked Samus. Live Spore
            // Spawn installs `$90:9589`, which owns the arena's vertical camera floor;
            // it must run before BG streaming and OAM consume layer-one position.
            Room.LoadedScope.Enemies.RoomMainHooks.RunSporeSpawnScrollingFinishedHook(camera);
            camera.FinishSamusScrolling(currentCameraPoint);

            // The camera can cross a 16-pixel boundary in the same main-loop pass.
            // Build and execute the exact row/column staging transfers now so both the
            // live PPU diagnostic and the following frame see the newly exposed edge.
            IReadOnlyList<BackgroundUpdateRequest> backgroundRequests =
                BackgroundPhase.UpdateBackgroundScrollingFromCamera();
            BackgroundPhase.ExecuteBackgroundStreamRequests(backgroundRequests, "active room camera");

            // UpdateMinimap belongs to Samus's normal frame-handler beta; it uses world
            // position, not camera position. Fresh debug sessions begin without an area
            // map, so only tiles actually visited by this runtime are revealed.
            Hud.UpdateMinimap(System, new MinimapFrame { AreaIndex = roomGeometry.AreaIndex, RoomMap = roomGeometry.MapPosition, RoomSize = Room.LevelData.Size, Samus = samus.Position.Whole, NmiFrameCounter = Clock.Counter8, MapRevealMode = Options.MapRevealMode, PresentationMap = Artwork.MapPresentation?.Get(roomGeometry.AreaIndex) });
        }
    }

    /// <summary>Restores the attract demo's player input and draws Samus, enemies and projectiles.</summary>
    private void DrawGameplayActors(SuspendedGameplayFrameTail tail, bool deathOwnsSamus)
    {
        AttractDemo.RestoreAttractPlayerInput();

        RoomScope scope = Room.LoadedScope;
        ActorDrawing.DrawGameplayActors(scope, deathOwnsSamus,
            // Command zero's `$90:E8DC` beta handler does not dispatch bank-$91
            // palette effects. In particular, automatic Reserve recovery freezes a
            // stored shine instead of consuming it while Samus is stationary.
            advanceSamusPalette: tail.Mode.Beta is not (SamusBetaHandler.SuitPickup or SamusBetaHandler.ScriptLocked));
    }

    /// <summary>Runs the escape-timer handler unless the gunship's beta handler owns the frame.</summary>
    private void RunEscapeTimerHandler(SuspendedGameplayFrameTail? frameTail)
    {
        // EnemyMain can install the gunship's $90:E902 beta handler on this frame.
        // It omits the normal $90:E738 timer-hack call: neither countdown nor OAM
        // survives boarding, even though timer status stays active. Decide after the
        // actor transition, before liftoff can repurpose the timer's OBJ tiles as dust.
        bool runEscapeTimerHandler = frameTail?.Mode.Beta != SamusBetaHandler.Gunship;
        if (runEscapeTimerHandler && EscapeTimer.Process(Clock.Counter, Options.PreventEscapeTimeout))
        {
            // `$90:E0E6`: the expired countdown publishes game state $23 for the
            // dispatcher, whites the target palettes and disables palette FX.
            TimeUpRequested = true;
            PaletteFxSession.DisableHandler();
        }
        if (runEscapeTimerHandler && EscapeTimer.IsActive)
        {
            var presentation = Artwork.MapPresentation ?? throw new InvalidOperationException(
                "Escape timer requires installed presentation assets.");
            EscapeTimerRenderer.Draw(EscapeTimer, Sprites, presentation.EscapeTimer, presentation.EscapeTimerTiles.Artwork);
        }
    }

    /// <summary>Rebuilds the HUD counters and appends the HUD upload ($80:9B44).</summary>
    private void QueueHudUpload(HostInfiniteAmmoFrameGuard infiniteAmmoGuard)
    {
        // $80:9B44 appends the mutable three-row HUD upload during main-thread logic; it
        // becomes visible when the next accepted NMI drains this queue. The first frame's
        // initialization upload has already been consumed earlier in this StepFrame call.
        if (Hud.IsInitialized)
        {
            // The loan exists only so cartridge firing logic does not observe zero and
            // auto-cancel the selected weapon. Remove it before `$80:9B44` converts the
            // live counters into HUD tile words; publishing first made the on-screen
            // minimum read two even though Snapshot later restored the actor to one.
            infiniteAmmoGuard.Complete(Player.Samus);
            // `$80:9B44` rebuilds live energy/ammo words before appending the HUD transfer.
            // Initialization alone is insufficient: Ridley contact and fireballs mutate
            // Samus during this frame, and those values must enter the next accepted NMI.
            // Native PLM_Handler has not returned while a pickup message is open.
            // Its newly granted inventory reaches the HUD only after that return.
            if (Player.Samus is not null && !Room.LoadedScope.Plms.CollectiblePlms.HasPendingCollectibleMessage)
                Hud.UpdateGameplayCounters(Player.Samus, Freeze.TimeIsFrozen,
                    soundSuppressed: Room.LoadedScope.BombProjectiles.PowerBombExplosion.IsActive);
            Hud.QueueUpload(VramWrites);
        }
    }

    /// <summary>Runs the scrolling sky, animated tiles, escape effects, the elevatube and the room main routines.</summary>
    private void RunRoomMains(RuntimeFramePolicy policy)
    {
        // Landing Site's room main ASM appends four sky rows after ordinary gameplay logic;
        // they become visible when the following accepted NMI drains the queue.
        if (Room.ScrollingSky is not null && Room.Camera is not null)
        {
            Room.ScrollingSky.ProcessFrame(
                Room.Camera.PixelPosition.Y,
                timeIsFrozen: Freeze.TimeIsFrozen,
                VramWrites,
                Room.ActiveRoom?.State.MainCallback ?? RoomMainCallback.ScrollingSkyLand);
        }

        RoomMains.RunAnimatedTilesObjectHandler(Room.LoadedScope);
        EscapeEffects.StepEscapeRoomEffects(Room.LoadedScope.Enemies);

        // Room main $8F:E2B6 is selected by the room state rather than by coordinates.
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
                Room.RoomMainScratch);
        }

        // Execute the active room's bank-$8F wrapper at the same seam as Landing Site's
        // room main above: after gameplay drawing/HUD work and before global shaking. The
        // Ceres routine publishes a matrix for the following frame's presentation and may
        // change the outer dispatcher to state $20 only when its caller is state eight.
        RoomMains.LastCeresElevatorShaftRoomMain = Room.LoadedScope.CeresElevatorShaft.Step(
            Player.Samus,
            Room.LoadedScope.Enemies.Ceres.CeresStatus == CeresEscapeStatus.ElevatorDoorDestroyed,
            allowDeparture: policy.AllowCeresElevatorDeparture,
            Room.RoomMainScratch);
        if (RoomMains.LastCeresElevatorShaftRoomMain.MatrixChanged)
            Display.ActiveSamusMode7Transform = RoomMains.LastCeresElevatorShaftRoomMain.Transform;

        RoomMains.RunCeresFallingDebrisRoomMain(Room.LoadedScope);
        RoomMains.RunCrocomireComebackRoomMain(Room.LoadedScope);
        if (Room.ActiveRoom?.State.MainCallback == RoomMainCallback.HandleCeresRidleyGetawayCutscene)
            Room.LoadedScope.Enemies.Ceres.RunCeresRidleyGetawayRoomMain(Player.Samus, Clock.Counter);
    }

    /// <summary>Decrements Samus's hurt timers and the projectile interaction timer ($A0:9169).</summary>
    private void DecrementHurtTimers()
    {
        // Gameplay state eight calls `$A0:9169` after Samus, enemies, drawing, HUD/BG
        // bookkeeping, room main ASM, the energy-zero check, and room shaking. Keep this
        // shared tail out of individual movement handlers: in particular, grapple spike
        // reactions set `$18A8/$18AA` without installing the bank-$90 knockback handler.
        // Death game states do not execute the gameplay-state tail at all.
        if (Player.Samus is not null && !Player.Samus.DeathSequence.IsActive)
        {
            Player.Samus.DecrementHurtTimers();
            Room.LoadedScope.Projectiles.Firing.DecrementInteractionTimer();
        }
    }

    /// <summary>
    /// Invokes the pause-menu held-input filter with the controller sample latched by the
    /// most recent accepted NMI. The retail caller passes reset value three.
    /// </summary>
    public void UpdatePauseHeldInput(ushort timerReset = 3)
    {
        System.UpdateHeldInput(timerReset, PhysicalController1.Current, PhysicalController1.NewlyPressed);
    }

    /// <summary>Applies host-only frame-exit guards before publishing the runtime result.</summary>
    internal RuntimeFrameResult Snapshot(
        HostInfiniteAmmoFrameGuard infiniteAmmoGuard)
    {
        // Every translated producer gets to apply its normal amount of damage first. The
        // host intervenes only at the single frame-exit seam where zero would otherwise be
        // observed by SuperMetroidGame and routed into the cartridge death states.
        if (Options.PlayerInvincibilityEnabled && Player.Samus is { Health: 0 } samus)
            samus.Health = 1;

        infiniteAmmoGuard.Complete(Player.Samus);

        return new RuntimeFrameResult();
    }

    /// <summary>
    /// Publishes the gameplay display at the end of a logic frame: the most recent NMI latch,
    /// the inputs the cartridge reads while the frame is displayed (IRQ register writes and
    /// HDMA tables), and the PPU memory image. Publishing never advances simulation, and the
    /// result shares no storage with it.
    /// </summary>
    public GameplayDisplayFrame PublishDisplayFrame()
    {
        if (Room.ActiveDoor is null)
            throw new InvalidOperationException("A cartridge room and door must be loaded before publishing gameplay.");
        CrocomireDeathState? crocomireDeath = Room.LoadedScope.Enemies.RoomMainHooks.Crocomire is not null
            ? Room.LoadedScope.Enemies.RoomMainHooks.CrocomireDeath : null;
        MotherBrainEnemyState? motherBrain = Room.LoadedScope.Enemies.DisplaySources.MotherBrain;
        ScrollingSkyState? sky = Room.ScrollingSky;
        var frameEnd = new GameplayFrameEndDisplayInputs(
            Room.DoorTransitionMainScreenLayers,
            sky is { HorizontalHdmaConfigured: true }
                ? ImmutableArray.Create(sky.BuildGameplayHorizontalScrolls(Room.Camera?.PixelPosition.Y
                    ?? throw new InvalidOperationException("Scrolling sky has no gameplay camera.")))
                : null,
            crocomireDeath is { MeltingHdmaActive: true } melting
                ? ImmutableArray.CreateRange(melting.Bg2ScrollByScanline) : null,
            motherBrain is { RisingHdmaActive: true },
            motherBrain?.RainbowBeamHdma.RenderSnapshot,
            CeresHazeRenderSnapshot.Capture(Room.LoadedScope.CeresHaze),
            Artwork.MapPresentation?.RoomFxPaletteBlends,
            PowerBombRenderSnapshot.Capture(Room.LoadedScope.BombProjectiles.PowerBombExplosion),
            GameplayMessageBoxRenderer.Capture(MessageBox, Artwork.HudCharacters),
            SuitPickup.IsActive ? SuitPickup.Beam : null,
            SamusXrayDisplaySnapshot.Capture(Player.Samus, Player.LastXrayBeamStep),
            Room.LoadedScope.TourianStatues.Enabled);
        return new(Display.Latched, frameEnd, PpuMemorySnapshot.Capture(Vram, Cgram, DisplayedSprites));
    }

    /// <summary>
    /// Gameplay-frame state the remainder of state eight reads after <c>PLM_Handler</c>.
    /// The bank-$85 message routine runs inside that handler, so a PLM that opens a box
    /// suspends the native frame; the port parks this state until the box closes.
    /// </summary>
    /// <param name="Mode">The frame's installed alpha and beta handlers.</param>
    /// <param name="PreviousCameraPoint">Samus's camera point carried to the frame tail.</param>
    internal readonly record struct SuspendedGameplayFrameTail(
        SamusControlMode Mode,
        FixedPosition<RoomPixels> PreviousCameraPoint);

    /// <summary>State-eight tail parked when a PLM opens a message box during a gameplay frame.</summary>
    private SuspendedGameplayFrameTail? _suspendedFrameTail;

    /// <summary>Updates live host guards without reconstructing any cartridge-owned state.</summary>
    internal void ApplyHostOptions(HostGameplayOverrides options)
    {
        if (!Enum.IsDefined(options.MapRevealMode))
            throw new ArgumentOutOfRangeException(nameof(options), "Unknown map reveal mode.");
        Options.PlayerInvincibilityEnabled = options.PlayerInvincibilityEnabled;
        Options.InfiniteAmmoEnabled = options.InfiniteAmmoEnabled;
        if (!Tester.GrantAllEquipmentEnabled && options.GrantAllEquipment) Tester.testerInventoryRecipient = null;
        Tester.GrantAllEquipmentEnabled = options.GrantAllEquipment;
        bool openingTourian = !Tester.UnlockTourianEnabled && options.UnlockTourian;
        Tester.UnlockTourianEnabled = options.UnlockTourian;
        Tester.ApplyTesterInventory();
        if (openingTourian && Room.LoadedScope.TourianStatues.Enabled) Room.LoadedScope.TourianStatues.RestartForTesterUnlock(Room.LoadedScope.Plms);
        Options.PreventEscapeTimeout = options.PreventEscapeTimeout;
        Options.MapRevealMode = options.MapRevealMode;
    }

    /// <summary>The session owners each room's Tourian statue sequence is built over.</summary>
    internal TourianStatueSequenceDependencies TourianStatueDependencies { get; }

    /// <summary>
    /// One lag frame of the bank-$85 message routine. Its completion returns into the
    /// suspended dispatch, whose remaining owners already ran when the box opened.
    /// </summary>
    internal RuntimeFrameResult StepMessageBoxFrame(
        SnesButton controller1Input,
        RuntimeFramePolicy policy,
        HostInfiniteAmmoFrameGuard infiniteAmmoGuard)
    {
        MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = false;
        // The joypad registers are the routine's only input: the NMI reads no controller.
        MessageBox.Step(controller1Input);
        if (MessageBox.LastFrameAudio.RunsHdmaObjects)
        {
            // Restore_PPU's $88:84B9 runs the HDMA objects, including their RNG mutations.
            if (Room.Camera is not null)
                HdmaPhase.AdvanceHdmaObjectPreInstructions(Room.LoadedScope.Plms, Room.LoadedScope);
            if (Player.Samus is { } hdmaSamus)
                Room.LoadedScope.BombProjectiles.AdvancePowerBombHdma(hdmaSamus, Controller1.Current, DispatchGameState);
        }
        if (MessageBox.ConfirmationSelectionChangedThisFrame)
            MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = true;
        if (MessageBox.IsActive)
            return Snapshot(infiniteAmmoGuard);

        MessageBoxes.ResolveClosedMessageBox(frameSuspended: _suspendedFrameTail is not null, Room.LoadedScope.Plms, Room.LoadedScope.BombProjectiles, Room.LoadedScope);
        // The native PLM_Handler returns once the box and its item routine finish; the
        // parked remainder of that gameplay frame runs now, in this same dispatch.
        if (!MessageBox.IsActive && _suspendedFrameTail is { } tail)
        {
            _suspendedFrameTail = null;
            return FinishGameplayFrame(tail, policy, infiniteAmmoGuard, resumedAfterMessageBox: true);
        }
        return Snapshot(infiniteAmmoGuard);
    }
}
