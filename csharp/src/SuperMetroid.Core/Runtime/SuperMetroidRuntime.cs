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
using SuperMetroid.Core.Areas.Ceres;
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
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
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
public sealed class SuperMetroidRuntime
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

    /// <summary>The <see cref="NmiHandler"/> owner of this SuperMetroidRuntime.</summary>
    internal NmiHandler NmiHandler { get; }

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

    /// <summary>The <see cref="NmiClock"/> owner of this SuperMetroidRuntime.</summary>
    internal NmiClock Nmi { get; }

    /// <summary>The <see cref="GameplayOptions"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayOptions Options { get; }

    /// <summary>The <see cref="TesterTools"/> owner of this SuperMetroidRuntime.</summary>
    internal TesterTools Tester { get; }

    /// <summary>The <see cref="PlmShotBlockVisuals"/> owner of this SuperMetroidRuntime.</summary>
    internal PlmShotBlockVisuals ShotBlockVisuals { get; }

    /// <summary>The <see cref="GameplayActorDrawing"/> owner of this SuperMetroidRuntime.</summary>
    internal GameplayActorDrawing ActorDrawing { get; }

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

    /// <summary>Mutable emulated address space shared by translated gameplay owners.</summary>
    internal readonly ISnesAddressSpace _addressSpace;

    /// <summary>
    /// Mutable-state write boundary. Immutable gameplay definitions and installed
    /// presentation data are separate from the emulated memory owner.
    /// </summary>
    internal ISnesAddressSpace AddressSpace => _addressSpace;

    /// <summary>Creates the frame-steppable runtime, installs host gameplay options, and loads the required initial palette.</summary>
    /// <param name="addressSpace">Cartridge definition source and mutable WRAM/SRAM owner used by translated systems.</param>
    /// <param name="overrides">The host's tester conveniences around the cartridge-authentic runtime: invincibility, infinite ammo, map reveal, escape-timeout prevention, and new-session equipment and Tourian unlocks (#633).</param>
    /// <param name="initialPaletteArt">Required installed palette image loaded into CGRAM during construction.</param>
    public SuperMetroidRuntime(ISnesAddressSpace addressSpace, HostGameplayOverrides overrides, GameplayBasePaletteCatalog? initialPaletteArt = null)
    {
        Display = new GameplayDisplayState();
        Player = new SamusFrameState();
        Room = new ActiveRoomState();
        Nmi = new NmiClock();
        Options = new GameplayOptions();
        _addressSpace = Ensure.NotNull(addressSpace);

        // These two host objects represent the lower and upper halves of the cartridge's
        // parallel ten-slot projectile arrays. BombProjectiles remains the owner of shared
        // cooldown $0CCC until the remaining weapon producers are consolidated.
        BombProjectiles = new SamusBombProjectileSystem();
        Projectiles = new SamusProjectileSystem();
        Freeze = new GameplayTimeFreeze(Player);
        Tester = new TesterTools(Cgram, Player, System, _addressSpace);
        TourianStatues = new TourianStatueSequence(new TourianStatueSequenceDependencies { AddressSpace = AddressSpace, Cgram = Cgram, Enemies = Enemies, Player = Player, Plms = Plms, Room = Room, RoomLayer3Fx = RoomLayer3Fx, RoomPaletteFx = RoomPaletteFx, System = System, Tester = Tester, Freeze = Freeze, VramWrites = VramWrites });
        Artwork = new RuntimeArtwork(new RuntimeArtworkDependencies { BombProjectiles = BombProjectiles, Cgram = Cgram, Display = Display, Enemies = Enemies, Hud = Hud, MessageBox = MessageBox, Player = Player, Projectiles = Projectiles, RoomLayer3Fx = RoomLayer3Fx, Vram = Vram, VramWrites = VramWrites });
        HudSetup = new GameplayHudSetup(new GameplayHudSetupDependencies { Artwork = Artwork, Display = Display, Enemies = Enemies, Hud = Hud, Player = Player, Vram = Vram, VramWrites = VramWrites, AddressSpace = _addressSpace });
        GameplayWindows = new GameplayWindowRegisters(Enemies, Projectiles, Room, RoomLayer3Fx);
        Xray = new XrayReveal(BackgroundScroll, Player, Plms, Room, System, Vram, _addressSpace);
        NmiHandler = new NmiHandler(new NmiHandlerDependencies { Artwork = Artwork, BackgroundScroll = BackgroundScroll, BombProjectiles = BombProjectiles, Controller1 = Controller1, PhysicalController1 = PhysicalController1, Display = Display, DisplayedSprites = DisplayedSprites, Enemies = Enemies, GameplayWindows = GameplayWindows, Nmi = Nmi, Sprites = Sprites, Options = Options, Player = Player, Room = Room, RoomLayer3Fx = RoomLayer3Fx, SuitPickup = SuitPickup, TourianStatues = TourianStatues, Vram = Vram, VramWrites = VramWrites, Xray = Xray, AddressSpace = _addressSpace });
        BackgroundPhase = new BackgroundScrollPhase(BackgroundScroll, Room, Vram);
        ActorDrawing = new GameplayActorDrawing(new GameplayActorDrawingDependencies { Artwork = Artwork, AttractSession = AttractSession, BackgroundScroll = BackgroundScroll, BombProjectiles = BombProjectiles, Cgram = Cgram, Controller1 = Controller1, Display = Display, Enemies = Enemies, Freeze = Freeze, Nmi = Nmi, Sprites = Sprites, Player = Player, Projectiles = Projectiles, Room = Room, VramWrites = VramWrites, AddressSpace = _addressSpace });
        EnemyRoomEffects = new EnemyRoomEffects(new EnemyRoomEffectsDependencies { Cgram = Cgram, Display = Display, Enemies = Enemies, EscapeTimer = EscapeTimer, Player = Player, Plms = Plms, Room = Room, RoomLayer3Fx = RoomLayer3Fx, RoomPaletteFx = RoomPaletteFx, System = System, AddressSpace = _addressSpace });
        EnemyPhase = new EnemyFramePhase(new EnemyFramePhaseDependencies { BackgroundScroll = BackgroundScroll, BombProjectiles = BombProjectiles, Cgram = Cgram, Controller1 = Controller1, Display = Display, Enemies = Enemies, EnemyRoomEffects = EnemyRoomEffects, EscapeTimer = EscapeTimer, Freeze = Freeze, Nmi = Nmi, Oam = Oam, Player = Player, Plms = Plms, Projectiles = Projectiles, Room = Room, RoomLayer3Fx = RoomLayer3Fx, VramWrites = VramWrites, AddressSpace = _addressSpace });
        HdmaPhase = new HdmaObjectPhase(Freeze, RoomLayer3Fx, System, TourianStatues);
        DoorTransition = new DoorTransitionRunner(new DoorTransitionRunnerDependencies { ActorDrawing = ActorDrawing, BackgroundScroll = BackgroundScroll, BombProjectiles = BombProjectiles, CeresElevatorShaft = CeresElevatorShaft, Display = Display, Enemies = Enemies, EnemyPhase = EnemyPhase, HdmaPhase = HdmaPhase, MaridiaElevatube = MaridiaElevatube, Nmi = Nmi, NmiHandler = NmiHandler, Sprites = Sprites, Player = Player, Plms = Plms, Room = Room, RoomLayer3Fx = RoomLayer3Fx, System = System, WreckedShipTreadmill = WreckedShipTreadmill, AddressSpace = _addressSpace });
        EscapeEffects = new EscapeRoomEffects(Enemies, Freeze, Nmi, Room, System);
        RoomLoader = new RoomLoader(new RoomLoaderDependencies { Artwork = Artwork, BackgroundPhase = BackgroundPhase, BackgroundScroll = BackgroundScroll, BombProjectiles = BombProjectiles, CeresElevatorShaft = CeresElevatorShaft, CeresHaze = CeresHaze, Cgram = Cgram, Controller1 = Controller1, Display = Display, DoorTransition = DoorTransition, Enemies = Enemies, EnemyRoomEffects = EnemyRoomEffects, EscapeEffects = EscapeEffects, GameTime = GameTime, Hud = Hud, HudSetup = HudSetup, MaridiaElevatube = MaridiaElevatube, Nmi = Nmi, Options = Options, Player = Player, Plms = Plms, Projectiles = Projectiles, Room = Room, RoomLayer3Fx = RoomLayer3Fx, RoomPaletteFx = RoomPaletteFx, RoomSpikes = RoomSpikes, RoomTreadmills = RoomTreadmills, SandAnimatedTiles = SandAnimatedTiles, System = System, Tester = Tester, TourianStatues = TourianStatues, Vram = Vram, VramWrites = VramWrites, WreckedShipTreadmill = WreckedShipTreadmill, AddressSpace = _addressSpace });
        AttractDemo = new AttractDemoDriver(new AttractDemoDriverDependencies { Artwork = Artwork, AttractSession = AttractSession, Cgram = Cgram, Controller1 = Controller1, Enemies = Enemies, HudSetup = HudSetup, NmiHandler = NmiHandler, Player = Player, Room = Room, RoomLoader = RoomLoader, System = System, AddressSpace = _addressSpace });
        DeathSequence = new DeathSequenceRunner(new DeathSequenceRunnerDependencies { ActorDrawing = ActorDrawing, Artwork = Artwork, Cgram = Cgram, Display = Display, NmiHandler = NmiHandler, Sprites = Sprites, Player = Player, Room = Room, VramWrites = VramWrites, AddressSpace = _addressSpace });
        MessageBoxes = new MessageBoxFlow(new MessageBoxFlowDependencies { BackgroundScroll = BackgroundScroll, BombProjectiles = BombProjectiles, Enemies = Enemies, Freeze = Freeze, Hud = Hud, MessageBox = MessageBox, Player = Player, Plms = Plms, Room = Room, SuitPickup = SuitPickup, System = System, Vram = Vram, VramWrites = VramWrites, AddressSpace = _addressSpace });
        PlmPhase = new PlmFramePhase(new PlmFramePhaseDependencies { Artwork = Artwork, BackgroundScroll = BackgroundScroll, BombProjectiles = BombProjectiles, Cgram = Cgram, Controller1 = Controller1, Enemies = Enemies, EnemyRoomEffects = EnemyRoomEffects, Freeze = Freeze, MessageBox = MessageBox, MessageBoxes = MessageBoxes, Player = Player, Plms = Plms, Room = Room, SuitPickup = SuitPickup, Vram = Vram, VramWrites = VramWrites, AddressSpace = _addressSpace });
        ShotBlockVisuals = new PlmShotBlockVisuals(Plms);
        RoomMains = new RoomMainRoutines(new RoomMainRoutinesDependencies { BackgroundScroll = BackgroundScroll, Enemies = Enemies, Player = Player, Room = Room, RoomSpikes = RoomSpikes, RoomTreadmills = RoomTreadmills, SandAnimatedTiles = SandAnimatedTiles, System = System, TourianStatues = TourianStatues, Vram = Vram, VramWrites = VramWrites, WreckedShipTreadmill = WreckedShipTreadmill, AddressSpace = _addressSpace });
        SamusPhases = new SamusFramePhases(new SamusFramePhasesDependencies { Artwork = Artwork, BombProjectiles = BombProjectiles, Cgram = Cgram, Controller1 = Controller1, Enemies = Enemies, EnemyPhase = EnemyPhase, Freeze = Freeze, GameTime = GameTime, Nmi = Nmi, Options = Options, Player = Player, Plms = Plms, Projectiles = Projectiles, Room = Room, RoomPaletteFx = RoomPaletteFx, System = System, VramWrites = VramWrites, AddressSpace = _addressSpace });

        Artwork.InitialPaletteArt = initialPaletteArt;
        Tester.GrantAllEquipmentEnabled = overrides.GrantAllEquipment;
        Tester.UnlockTourianEnabled = overrides.UnlockTourian;
        Options.PlayerInvincibilityEnabled = overrides.PlayerInvincibilityEnabled;
        Options.InfiniteAmmoEnabled = overrides.InfiniteAmmoEnabled;
        Options.PreventEscapeTimeout = overrides.PreventEscapeTimeout;
        Options.MapRevealMode = Enum.IsDefined(overrides.MapRevealMode)
            ? overrides.MapRevealMode
            : throw new ArgumentOutOfRangeException(nameof(overrides.MapRevealMode));

        GameplayWindows.BindGameplayWindowRegisters();
        Projectiles.Earthquakes.BindRoomEarthquakeOwner(Enemies);

        // $82:82C5 copies all 512 bytes of kInitialPalette from ROM $9A:8000. The retail
        // game stages this through WRAM before NMI uploads CGRAM; initializing the modeled
        // PPU here produces the same starting colors while that fade pipeline is ported.
        (Artwork.InitialPaletteArt ?? throw new InvalidOperationException(
            "Gameplay requires the installed initial palette catalog.")).LoadInitial(Cgram);
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
        get => Enemies.RoomStatus.JapaneseText;
        set => Enemies.RoomStatus.JapaneseText = value;
    }

    /// <summary>Ordinary seven-byte-record VRAM write table.</summary>
    public VramWriteQueue VramWrites { get; } = new();

    /// <summary>PPU video RAM receiving accepted-NMI transfers.</summary>
    public SnesVram Vram { get; } = new();

    /// <summary>PPU color RAM initialized from <c>kInitialPalette</c> at ROM <c>$9A:8000</c>.</summary>
    public SnesCgram Cgram { get; } = new();

    /// <summary>Bank-$8D palette objects selected by the active room's bank-$83 FX record.</summary>
    public RoomPaletteFxSystem RoomPaletteFx { get; } = new();

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
    public RoomEnemySystem Enemies { get; } = new();

    /// <summary>
    /// Native elevator status word at WRAM <c>$0E18</c>. The room's elevator actor owns this
    /// producer. Every nonzero status runs forward-facing `$00/$9B` through `$90:A392`'s
    /// exact one-pixel downward terrain scan. Destination arrival depends on that scan to
    /// wake cartridge scroll PLMs along the elevator shaft. The host-only collapsed door
    /// transition suppresses only repeated pseudo-door publication during status two/three;
    /// every other terrain reaction remains native.
    /// </summary>
    public ushort ElevatorStatus
    {
        get => (ushort)Enemies.ElevatorBehavior.ElevatorStatus;
        set => Enemies.ElevatorBehavior.SetElevatorStatusForDebugging(value);
    }

    /// <summary>
    /// WRAM GameState ($0998) of the frontend dispatch now running. Samus commands such as
    /// $90:F4A2 test it from inside the outer frame; runtime-only hosts run state eight.
    /// </summary>
    [field: NonSerialized]
    public ushort DispatchGameState { get; set; } = (ushort)SuperMetroid.Core.Gameplay.SuperMetroidGameState.MainGameplay;

    /// <summary>
    /// Native power-bomb explosion status at WRAM <c>$0CE2</c>. X-ray setup rejects every
    /// nonzero phase. The getter reads the translated bank-$88 owner; the setter is a
    /// debugger seam for forcing preconditions without constructing an explosion.
    /// </summary>
    public ushort PowerBombExplosionStatus
    {
        get => BombProjectiles.PowerBombExplosion.Status;
        set => BombProjectiles.PowerBombExplosion.SetStatusForDebugging(value);
    }

    /// <summary>
    /// The power-bomb gate in all three sound-queue routines (<c>$80:9072</c>,
    /// <c>$80:90F4</c>, <c>$80:9176</c>): while the explosion status is negative every
    /// request is dropped after the occupancy check, whoever makes it.
    /// </summary>
    public bool PowerBombExplosionSuppressesSounds =>
        unchecked((short)PowerBombExplosionStatus) < 0;

    /// <summary>Door-selected bank-$83/$88 gameplay BG3 effect.</summary>
    public RoomLayer3FxState RoomLayer3Fx { get; } = new();
    /// <summary>Persistent bank-$88 Ceres room-haze HDMA owner, activated and reset by room setup.</summary>
    public CeresHazeState CeresHaze { get; } = new();

    /// <summary>
    /// Exact room-main owner for Ceres elevator shaft <c>$DF45</c>. The object persists so
    /// debugger watches can inspect its signed rotation phase, but every room load resets
    /// and explicitly activates or deactivates it from the cartridge room-state pointer.
    /// </summary>
    public CeresElevatorShaftRoomMainState CeresElevatorShaft { get; } = new();

    /// <summary>
    /// Door-spawned bank-$87 animated-tile object for the Wrecked Ship entrance treadmill.
    /// </summary>
    public WreckedShipTreadmillAnimatedTilesState WreckedShipTreadmill { get; } = new();

    /// <summary>Bank-$87 treadmill population selected by the room FX record, rather than door ASM.</summary>
    public RoomTreadmillAnimatedTilesState RoomTreadmills { get; } = new();

    /// <summary>Ceiling and falling-sand character animation selected by the current room FX record.</summary>
    public RoomSandAnimatedTilesState SandAnimatedTiles { get; } = new();

    /// <summary>FX-selected horizontal spikes.</summary>
    public RoomSpikeAnimatedTilesState RoomSpikes { get; internal set; } = new();

    /// <summary>Room-main owner for Maridia elevatube routine $8F:E2B6.</summary>
    public MaridiaElevatubeRoomMainState MaridiaElevatube { get; } = new();

    /// <summary>
    /// Five-slot normal-bomb lifecycle spanning the translated bank-$90/$93/$94/$A0 seams.
    /// </summary>
    public SamusBombProjectileSystem BombProjectiles { get; }

    /// <summary>
    /// Five-slot ordinary beam/missile lifecycle spanning banks $90, $93, and $94.
    /// </summary>
    public SamusProjectileSystem Projectiles { get; }

    /// <summary>
    /// Forty-slot bank-$84 room-object owner. Breakable grapple terrain lives here rather
    /// than in Samus state so its delayed disappearance/respawn survives rope release.
    /// </summary>
    public RoomPlmSystem Plms { get; } = new();

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
        NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
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

        // Routing references are deliberately not serialized as duplicate native state.
        // Reattach before any producer executes, including after loading an older snapshot.
        Artwork.BindGrapplePresentation();
        GameplayWindows.BindGameplayWindowRegisters();
        Projectiles.Earthquakes.BindRoomEarthquakeOwner(Enemies);
        Plms.Sounds.BindPowerBombAudio(BombProjectiles.PowerBombExplosion);
        Player.Samus?.Shinespark.BindProjectileOwners(Projectiles, BombProjectiles.PowerBombExplosion);
        EnemyRoomEffects.ApplyPendingChozoStatuePlms();
        // Bank $85's message routine runs on lag frames inside the suspended dispatch: the
        // NMI accepts no input and the main loop neither calls the RNG nor any owner.
        if (MessageBox.IsActive)
            return StepMessageBoxFrame(controller1Input, frameRequest.Policy, infiniteAmmoGuard);
        Projectiles.ImpactAudio.BeginImpactAudioFrame(cinematicActive: false);
        NmiHandler.RunNmi(controller1Input, mainLoopRequestedNmi: true);
        frameRequest.AfterAcceptedNmi();
        GameplayWindows.PrepareGameplayWindowRegisters();

        // HDMA owns shared RNG mutations before the main loop advances it. A
        // message-box NMI wait does not execute that outer-loop HDMA pass.
        if (Room.Camera is not null && !MessageBox.IsActive)
            HdmaPhase.AdvanceHdmaObjectPreInstructions();

        // Blast HDMA belongs to the outer frame, not Samus alpha. A statue carry
        // replaces her handlers while the already-spawned blast continues normally.
        // Its cleanup runs Samus command $1E, which can queue sounds before the Samus
        // handler; the frame's Samus sound window therefore opens here.
        if (Player.Samus is { } hdmaSamus && !MessageBox.IsActive)
        {
            hdmaSamus.LiquidPhysics.BeginFrameSoundRequests(BombProjectiles.PowerBombExplosion);
            BombProjectiles.AdvancePowerBombHdma(_addressSpace, hdmaSamus, Controller1.Current, DispatchGameState);
        }

        // The bank-$82 main loop calls GenerateRandomNumber at $82:894F on every accepted
        // main-loop pass, immediately after the bank-$88 HDMA-object handler and before it
        // dispatches the current game state. Several room/enemy routines deliberately only
        // sample $05E5 instead of advancing it themselves. Falling Tile room main $8F:E525
        // is the first visibly obvious example: without this global call, every ceiling
        // fragment selects the same X table entry forever. Keep this unconditional and in
        // the frame prologue; advancing only at a consumer would change the shared sequence
        // and would be a bespoke behavioral patch rather than the cartridge call order.
        System.NextRandom();

        CeresHaze.Step();

        MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = false;

        // HDMA object pre-instructions run once near the start of each ordinary gameplay
        // pass. A transformation spawned when the preceding message returned therefore
        // receives its first stage call on this following accepted frame, while every
        // normal owner below continues to run around the input-locked, centered Samus.
        if (SuitPickup.IsActive)
        {
            if (Player.Samus is null)
                throw new InvalidOperationException("A suit transformation has no Samus owner.");
            SuitPickup.Step(_addressSpace, Player.Samus, Cgram);
        }

        // DisplayMessageBox waits on NMI while retaining the already-published room OAM,
        // music, and sound engines. Audio mixing lives in the frontend, but this translated
        // runtime must still latch controller input and block every gameplay owner during
        // those waits. This early seam is shared by all permanent-item identities.
        // Room FX objects run in the ordinary gameplay owner list. In particular, they
        // must not scroll or animate during DisplayMessageBox's NMI-only wait loop above.
        // Keeping this after that early-return seam reproduces that native suspension and
        // prevents rain/fog from advancing behind a station or item message.
        if (Enemies.PhantoonBehavior.Phantoon is { } waveBoss)
        {
            waveBoss.Wave.Step(waveBoss);
            waveBoss.Blending.Step(waveBoss, Display.LayerBlendingDefaultConfig);
        }
        if (Room.Camera is not null)
        {
            RoomLayer3Fx.Step(_addressSpace, Vram, new RoomLayer3FxFrame { Camera = Room.Camera.PixelPosition, TimeIsFrozen = Freeze.TimeIsFrozen, MainGameLoopCarry = System.MainGameLoopCarry, RandomNumber = System.RandomNumber, FirefleaDarknessLevel = Enemies.RoomStatus.FirefleaDarknessLevel, PowerBomb = BombProjectiles.PowerBombExplosion, LiquidMotionAlreadyAdvanced = RoomLayer3Fx.MovesLiquidInHdmaPass });
            if (RoomLayer3Fx.EarthquakeRequest is { } roomFxEarthquake)
            {
                // Lava/acid rise pre-instructions use TSB on the shared timer after writing
                // type $15. Apply that global side effect before the ordinary shake handler
                // later in this frame so its first displacement is visible immediately.
                Enemies.Earthquake.EarthquakeType = roomFxEarthquake.Type;
                Enemies.Earthquake.AddTimerBits(roomFxEarthquake.TimerBits);
            }
            if (Player.Samus is not null && RoomLayer3Fx.Type is
                    RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid)
                RoomLayer3Fx.ApplyToSamusLiquidPhysics(Player.Samus.LiquidPhysics);
        }

        // MainScrollingRoutine saves its previous-position words after scrolling, not at
        // frame start. This fallback is only for a newly loaded/restored camera; otherwise
        // retain the preceding scrolling checkpoint so later room-main movement (such as
        // the elevatube) contributes to the next frame's distance just like enemy movement.
        FixedPosition<RoomPixels>? samusCameraPointAtFrameStart = Player.Samus is null
            ? null
            : Player.Samus.Position;

        // `GameState_8_MainGameplay` begins with Determine_Which_Enemies_to_Process
        // ($82:8B47), before palette FX and Samus. The grapple scan uses this list.
        if (Room.Camera is not null && Enemies.IsLoaded)
            Enemies.ProcessingWindow.PrepareEnemyProcessingList(Room.Camera.PixelPosition);

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
        if (RoomPaletteFx.HandlerEnabled && !Enemies.ElevatorBehavior.ElevatorDoorTransitionActive &&
            Player.Samus?.Xray.ArePaletteFxSuspended != true)
        {
            RoomPaletteFx.Step(_addressSpace, Cgram, Artwork.MapPresentation?.RoomPaletteFx ?? throw new InvalidOperationException(
                    "Gameplay palette FX requires installed map presentation colors."), new PaletteFxConditions { SamusY = Player.Samus?.Position.Y.Whole ?? 0, EquippedItems = Player.Samus?.EquippedItems ?? 0, EnemyZeroIsDead = Enemies.SlotTable.Slots.Count == 0 || Enemies.SlotTable.Slots[0].Health == 0, AreaMiniBossDefeated = Room.ActiveRoom is { } paletteRoom &&
                    System.HasAnyBossBits(paletteRoom.AreaIndex, BossBits.AreaMiniBoss), Samus = Player.Samus, NmiFrameCounter = Nmi.NmiFrameCounter }, BombProjectiles.PowerBombExplosion);
        }
        // Hyper Beam's palette object lives in the same native pool and handler.
        Player.LastHyperBeamPaletteFxStep = RoomPaletteFx.HandlerEnabled
            ? Player.Samus?.Drained.HyperBeamPaletteFx.Step(
                _addressSpace,
                Cgram,
                Artwork.BeamArtwork?.HyperBeamFxColors)
            : null;

        // The bank-$82 main loop clears high OAM and resets its stack before dispatching
        // game state, then finalizes unused entries afterward. Samus is emitted before the
        // escape timer so lower OAM indices retain their normal overlap precedence.
        Sprites.BeginFrame();
        ActorDrawing.LastSamusBodyDrawn = false;
        ActorDrawing.LastShinesparkCrashDrawingHandlerActive = false;
        ActorDrawing.LastGrappleDrawingHandlerActive = false;
        ActorDrawing.LastGrappleBeamSpecificDrawingPath = false;
        ActorDrawing.LastGrappleFlareDrawn = false;

        EnemyPhase.PrepareEnemyFrame();
        // Host-disabled Samus movement has no alpha phase to wait for. Otherwise
        // defer actors until projectile production/update, immediately before beta.
        bool enemyMainAlreadyRan = Player.Samus is null || Room.Camera is null || !Player.GroundedSamusMovementEnabled;
        // Native EnemyMain follows SamusCurrentStateHandler. An actor that restores the
        // normal handler pair there (gunship $A2:A987) affects this frame's beta only:
        // the locked alpha has already run, so it still sees the lock below.
        bool enemyMainReleasedAlphaLock = false;
        if (enemyMainAlreadyRan)
        {
            bool lockedBeforeEnemyMain = Player.Samus?.InputLocked == true;
            EnemyPhase.RunEnemyMainPhase(processingListPrepared: true,
                Room.Camera?.PreviousSamusPoint ?? samusCameraPointAtFrameStart);
            enemyMainReleasedAlphaLock = lockedBeforeEnemyMain && Player.Samus?.InputLocked == false;
        }
        SuspendedGameplayFrameTail? frameTail = null;
        if (Player.Samus is not null && Room.Camera is not null)
        {
            // X-ray's HDMA object is not part of the Samus handler. Advance its explicit
            // setup/beam/deactivation state before alpha so state five can restore the
            // ordinary pose and handler pair before this frame samples pose input. The
            // revealed-block BG2 copies/window table remain a separate renderer boundary.
            Player.LastXrayBeamStep = null;
            Player.LastXrayPoseInput = null;
            Player.LastDeathSequenceStep = null;
            if (Player.Samus.Xray.IsActive && !Player.Samus.DeathSequence.IsActive)
            {
                if (Player.Samus.Xray.SetupStage == XraySetupStage.BuildRevealReadBg2FirstScreen)
                    Xray.BuildXrayRevealTilemap();
                Player.LastXrayBeamStep = Player.Samus.Xray.StepBeam(
                    _addressSpace,
                    Player.Samus,
                    Controller1.Current);
            }

            // Preserve movement on either side of the frame boundary. A new camera uses
            // the fallback sampled before enemy-owned motion and the bank-$90 handler.
            var frame = new SamusHandlerFrame();
            frame.PreviousCameraPoint = Room.Camera.PreviousSamusPoint ?? samusCameraPointAtFrameStart ??
                throw new InvalidOperationException("A gameplay frame with Samus samples her camera point at its start.");

            // Whether this frame's alpha handler is the locked one installed at its start.
            bool AlphaInputLocked() => Player.Samus.InputLocked || enemyMainReleasedAlphaLock;

            // Retain the dispatch pose because command $F8 can replace Samus.Pose during
            // animation later in this same frame. Native alpha/beta/transition phases all
            // agree on that order; using the mutable value afterward would apply an input
            // match selected for the old pose to the newly installed one.
            AttractDemo.BeginAttractSamusInput();
            frame.PoseAtFrameStart = Player.Samus.Pose;
            // Beta dispatches on the pose installed when it runs, which EnemyMain can have
            // replaced since alpha: Draygon's death releases a sparking Samus to pose $01,
            // and standing movement runs. `$91:E8B6` then reads that pose's type.
            frame.PoseAtBetaMovement = frame.PoseAtFrameStart;
            // Ordinary alpha publishes the current pose's live radius before input
            // and collision. Prospective transitions later in beta can retain the
            // previous radius until this point in the following frame. The locked
            // alpha ($90:E713) has no SetSamusRadius, so e.g. an elevator ride keeps
            // the boarding pose's radius.
            if (Player.GroundedSamusMovementEnabled && !Freeze.TimeIsFrozen && !AlphaInputLocked() &&
                !Player.Samus.DeathSequence.IsActive && !Enemies.ElevatorBehavior.ElevatorDoorTransitionActive)
                Player.Samus.PoseMetadata.RefreshCollisionRadii(_addressSpace);
            // Native Samus beta precedes PLMs. A lock/unlock issued by a PLM
            // affects the next beta, not the animation already owned this frame.
            frame.StationaryScriptControlLocked = Player.Samus.ScriptLocks.StationaryScriptControlLocked;
            bool refillStationLocked = Player.Samus.ScriptLocks.RefillStationLocked;
            // Suit command $15 installs an empty beta, not just locked pose input.
            // Preserve the suspended movement pointer and all of its timers so command
            // $0B can resume it after the HDMA transformation (including Blue Suit).
            // The empty beta also skips periodic damage and the energy checks.
            frame.SuitOwnsSamus = SuitPickup.IsActive;
            frame.MovementTypeAtFrameStart = Player.Samus.PoseMetadata.ReadMovementType(_addressSpace);

            // Direction bits already use the transition table's canonical layout. Input
            // matching belongs to frame-handler alpha, before beta moves the CURRENT pose;
            // the winning pose is not installed until after animation below.
            // `$91:804D-$8065` gives front-view `$00/$9B` one special input rule: a
            // nonzero elevator status returns immediately, before the transition-table
            // pointer is even read. Pose zero's pointer bytes otherwise overlap legitimate
            // data and can appear to select nonsense transitions after several frames, so
            // suppressing only their application would be too late and observably wrong.
            frame.DeathOwnsSamus = Player.Samus.DeathSequence.IsActive;
            // Sound queues are global persistent engines on hardware, but this typed host
            // publication is scoped to one frame's Samus handler. It opened before the HDMA
            // pass, ahead of `$91:F046`'s landing sounds during collision.
            bool xrayOwnsPoseInput = Player.Samus.Xray.OwnsSamusControl && !frame.DeathOwnsSamus;
            frame.XrayActivatedThisFrame = false;
            frame.MovementBeforeXrayAdmission = Player.Samus.PoseMetadata.ReadMovementType(_addressSpace);
            if (xrayOwnsPoseInput)
            {
                Player.LastXrayPoseInput = Player.Samus.Xray.HandlePoseInput(
                    _addressSpace,
                    Player.Samus,
                    Controller1.Current);
            }

            frame.BombJumpLocksPoseInput = Player.Samus.BombJumpPoseInputLocked;
            bool actorLocksPoseInput = AlphaInputLocked() || frame.BombJumpLocksPoseInput ||
                ((Player.Samus.InputSnapshot.ShinesparkPoseInputLocked || Player.Samus.InputSnapshot.CrystalFlashPoseInputLocked) && !Player.Samus.InputSnapshot.AutoJumpInputPending) ||
                (SamusPoseMetadata.IsForwardFacingPose(Player.Samus.Pose) && ElevatorStatus != 0);
            // The native type-$0F input dispatcher is an RTS ($91:8146). Transition
            // poses can still have table entries, but reading them here prematurely
            // interrupts crouch/morph animations before their animation command wins.
            bool postureTransitionSkipsPoseInput = frame.MovementTypeAtFrameStart == SamusMovementType.PostureTransition;
            SamusPoseTransitionLookup poseLookup =
                frame.DeathOwnsSamus || xrayOwnsPoseInput || actorLocksPoseInput || postureTransitionSkipsPoseInput
                ? default
                : SamusPoseTransitionTable.Lookup(
                    _addressSpace,
                    Player.Samus.Pose,
                    Controller1.Current,
                    AttractDemo.IsAttractDemo ? Controller1.NewlyPressed : Player.Samus.InputSnapshot.ConsumeAutoJumpInput(Controller1.NewlyPressed));
            Player.ProspectiveSamusPose = poseLookup.Transition;
            // The retail table matcher clears a pending/active bomb direction as
            // soon as it selects a DIFFERENT pose, before collision and movement.
            if (Player.ProspectiveSamusPose is { } matchedBombCancel &&
                matchedBombCancel.ProspectivePose != Player.Samus.Pose)
                Player.Samus.BombJumpDirection = 0;
            frame.UsePoseDefinitionFallback = poseLookup.UsesPoseDefinitionFallback;
            frame.DeceleratingFallbackHasMomentum = Player.Samus.HorizontalSpeed.BaseFixed != 0;
            Player.ProspectiveSamusFallbackPose = null;
            Player.ProspectiveSamusWallCollisionPose = null;
            Player.LastRanIntoWallProbe = null;

            // Zero input and an unmatched nonzero table chord both reach `$91:82D9`; a matched
            // same-pose record does not. Native writes the prospective pose slot in alpha and
            // commits it after beta movement and animation, so the selection is captured here.
            // A retained pose still selects a transition slot whose command runs later.
            if (Player.GroundedSamusMovementEnabled && frame.UsePoseDefinitionFallback)
            {
                Player.ProspectiveSamusFallbackPose = SamusLookupFailurePose.Resolve(
                    Player.Samus.PoseMetadata.ReadMovementType(_addressSpace),
                    Player.Samus.Pose,
                    Player.Samus.PoseMetadata.ReadNoInputFallbackPose(_addressSpace),
                    frame.DeceleratingFallbackHasMomentum).ProspectivePose;
            }

            SamusPhases.StepGroundedSamusMovement(frame, DispatchGameState, enemyMainAlreadyRan, AlphaInputLocked, Player.Samus, Room.Camera);

            // Samus_MovementHandler_Normal calls `$90:EEE7` after the selected movement
            // function and before animation/pose transitions. Every fourth gameplay frame
            // at boost stage four, it alternates between two exact world-position snapshots.
            // Keeping this producer here means the draw handler below consumes post-motion
            // coordinates with the same frame ordering as the cartridge.
            if (!frame.SuitOwnsSamus && Player.LastShinesparkMovement is null)
            {
                Player.Samus.HorizontalSpeed.CaptureSpeedEchoPosition(GameTime.Frames, Player.Samus.Position.Whole);
            }

            // Every ordinary downward collision calls `$91:F046` inside movement, before
            // the timer hack, liquid animation, prospective-pose selection, and velocity
            // cleanup. Knockback clears its live vertical speed in DF6E before this
            // consumer; do not restore its diagnostic pre-impact magnitude for effects.
            // Drained movement retains its separately modeled impact publication.
            bool ordinaryOrBallLanded =
                Player.LastAerialSamusMovement is { Landed: true } ||
                Player.LastMorphBallMovement is { Landed: true };
            bool knockbackLanded = Player.LastKnockbackMovement is { Landed: true };
            DrainedSamusMovementResult? drainedLanding = Player.LastDrainedSamusMovement is { Landed: true } landing ? landing : null;
            bool drainedLanded = drainedLanding is not null;
            if (!frame.DeathOwnsSamus && (ordinaryOrBallLanded || knockbackLanded || drainedLanded))
            {
                ushort impactYSpeed = drainedLanding is { } drainedSpeed
                    ? drainedSpeed.ImpactYSpeed
                    : Player.Samus.Kinematics.YSpeed;
                ushort impactYSubspeed = drainedLanding is { } drainedSubspeed
                    ? drainedSubspeed.ImpactYSubspeed
                    : Player.Samus.Kinematics.YSubspeed;
                Player.Samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
                    _addressSpace,
                    Player.Samus,
                    frame.MovementTypeAtFrameStart,
                    frame.PoseAtFrameStart,
                    impactYSpeed,
                    impactYSubspeed);
            }

            // `$90:E738` runs the timer/hack handler after movement and before animation.
            // Commands five/$18 install the Mother Brain-specific Up-edge branches; the
            // method is a cheap no-op for every ordinary gameplay state.
            if (!frame.SuitOwnsSamus)
                Player.Samus.Drained.StepGetUpHandler(Player.Samus, Controller1.NewlyPressed);

            if (!frame.SuitOwnsSamus && Player.Samus.DraygonGrabbed.IsActive)
            {
                // `$90:E738` calls this installed hack handler after movement and before
                // animation. A locked grapple cancels the prospective pose chosen in alpha;
                // an escape release additionally clears every pending route while installing
                // ordinary `$01/$02` and zeroing the documented velocity words.
                Player.LastDraygonEscape = Player.Samus.DraygonGrabbed.StepEscapeHandler(
                    _addressSpace,
                    Player.Samus,
                    Controller1.NewlyPressed,
                    Player.Samus.Grapple.Phase == GrapplePhase.ConnectedLocked);
                if (Player.LastDraygonEscape.Value.SuppressProspectivePose ||
                    Player.LastDraygonEscape.Value.Released)
                {
                    Player.ProspectiveSamusPose = null;
                    Player.ProspectiveSamusFallbackPose = null;
                }
            }

            // Normal gameplay advances animation during frame-handler beta, before the
            // pose-transition handler, draw handler, and next-NMI tile selection.
            // Animation sees the same held-input word as movement. Ordinary Dash uses B
            // both to accumulate `$0B42.$0B44` and to intercept the running delay-list
            // command at `$90:852C`; passing a reconstructed input later would skew art.
            if (frame.DeathOwnsSamus)
            {
                Player.LastDeathSequenceStep = Player.Samus.DeathSequence.Step(
                    _addressSpace,
                    Player.Samus,
                    Cgram,
                    Artwork.SamusBodyArt?.DeathPalettes);
            }
            // Command six's bare RTL beta ($90:E8D6) skips animation like the locked beta.
            else if (!frame.StationaryScriptControlLocked && !refillStationLocked && !frame.SuitOwnsSamus)
            {
                Player.Samus.Presentation.AnimateNoFx(_addressSpace, new SamusAnimationStep { ControllerInput = Controller1.Current, NmiFrameCounter = Nmi.NmiFrameCounter, System = System, BeginLiquidSoundRequestFrame = false, ProspectiveInputPose = Player.ProspectiveSamusPose?.ProspectivePose ?? Player.ProspectiveSamusFallbackPose, DemoPoseInput = AttractDemo.IsAttractDemo, QueueEchoSound = frameRequest.Audio.QueueEchoSound });
                SamusPhases.StepSamusLoadAppearance();
            }

            SamusPhases.ApplySamusAnimationTransitions(frame, Player.Samus);

            if (!frame.DeathOwnsSamus && !AttractDemo.IsAttractDemo && !frame.SuitOwnsSamus)
            {
                // Normal beta ends with HandlePeriodicDamageToSamus ($90:E9CE), PauseCheck and
                // LowEnergyCheck, before the enemy-projectile and PLM handlers. Damage those
                // handlers accumulate this frame (e.g. $84:AC9D) therefore lands next frame.
                // Demo beta omits these calls, and so does suit command $15's empty beta
                // (RTL_90E8CD): damage pending during the transformation stays accumulated
                // until the first ordinary beta after it, which divides it by the new suit.
                // X-ray freezes time and therefore clears
                // rather than applies accumulated damage; fatal zero-energy game-state
                // acquisition remains the outer seam.
                Player.Samus.LiquidPhysics.ApplyPeriodicDamage(
                    Player.Samus,
                    timeIsFrozen: Freeze.TimeIsFrozen);
                // Locked/elevator/appearance handlers omit LowEnergyCheck; gunship command
                // $1A installs a dedicated checker. Automatic reserves invoke their external
                // check in the frontend.
                if ((!Player.Samus.InputLocked || Enemies.GunshipHealthBehavior.HasGunshipHealthHandler) &&
                    !Player.Samus.Xray.OwnsSamusControl)
                    frameRequest.Audio.CheckLowHealth?.Invoke();
            }

            if (!frame.DeathOwnsSamus)
                DoorTransition.RunEnemyProjectileHandler();

            PlmPhase.RunPlmPhase(frame.DeathOwnsSamus, Player.Samus);

            // $A2:AB1F calls the bank-$85 routine from inside EnemyMain, after this frame's
            // ship bob and earlier owners ran. The prompt exists only while Samus is locked
            // inside the ship, so the owners between EnemyMain and here have nothing to
            // act on; the box therefore opens at this shared suspension seam, and the
            // remainder resumes after the YES/NO/completion chain returns.
            if (Enemies.GunshipBehavior.GunshipSavePromptPending)
            {
                if (MessageBox.IsActive)
                {
                    throw new InvalidDataException(
                        "A PLM message box and the gunship save prompt opened in one gameplay frame.");
                }
                MessageBox.Begin(_addressSpace, GameplayMessageIds.GunshipSaveConfirmation, Controller1.Current);
            }

            frameTail = new SuspendedGameplayFrameTail(
                frame.DeathOwnsSamus,
                frame.PreviousCameraPoint,
                frame.StationaryScriptControlLocked,
                frame.SuitOwnsSamus);
        }

        // Bank $85's message routine runs inside PLM_Handler: the rest of this gameplay
        // frame (enemy-projectile collision, scrolling, drawing, room main and the shared
        // tail) resumes only after the box closes, after the item routine finishes.
        if (frameTail is { } suspendedTail && MessageBox.IsActive)
        {
            _suspendedFrameTail = suspendedTail;
            // Producers up to PLM_Handler queued their sounds before DisplayMessageBox's
            // entry cancel; the frontend publishes this frame's requests at that boundary.
            NmiHandler.CompletedGameplayAudioPublication++;
            return Snapshot(infiniteAmmoGuard);
        }
        return FinishGameplayFrame(frameTail, frameRequest.Policy, infiniteAmmoGuard, resumedAfterMessageBox: false);
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
            bool deathOwnsSamus = tail.DeathOwnsSamus;
            FixedPosition<RoomPixels> previousCameraPoint = tail.PreviousCameraPoint;
            bool stationaryScriptControlLocked = tail.StationaryScriptControlLocked;
            bool suitOwnsSamus = tail.SuitOwnsSamus;
            if (!deathOwnsSamus && !Freeze.TimeIsFrozen)
            {
                Enemies.Projectiles.ResolveEnemyProjectileSamusHits(Player.Samus);
                Enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(
                    _addressSpace, Projectiles, BombProjectiles);
                // GameState_8 calls `$A0:A306` every gameplay frame after the enemy-
                // projectile passes. The native routine reads the high byte of live WRAM
                // radius `$0CEE`; a zero byte is its own inactive/pre-explosion guard.
                // Invincibility timers inside the enemy pass prevent the persistent
                // expansion/afterglow radius from damaging one actor every frame.
                SamusPowerBombExplosionState powerBomb =
                    BombProjectiles.PowerBombExplosion;
                Enemies.Collision.ResolveOrdinaryPowerBombHits(
                    _addressSpace,
                    powerBomb.Position,
                    SnesUnsignedEightEight.Whole(powerBomb.ExplosionRadius),
                    Player.Samus);
            }

            // Native GameState_8 runs bank-$86 enemy projectiles immediately after Samus's
            // movement handler and before MainScrollingRoutine. The two fresh-Ceres actors
            // use that slot to lower Samus one pixel at a time; moving this call below the
            // camera would make the viewport trail one frame behind the cartridge.
            if (!deathOwnsSamus && Room.CeresElevatorArrival is { IsComplete: false } arrival)
            {
                if (arrival.Step(Player.Samus))
                {
                    // Command $0E installed a locked Samus handler before the projectiles
                    // were spawned. Re-enable the translated ordinary movement path only
                    // after the pad reaches the native Y=$0048 deletion condition.
                    Player.Samus.InputLocked = false;
                    Player.GroundedSamusMovementEnabled = true;
                }
            }

            // $82:E737's destination fade runs enemy/draw owners but not $90:94EC.
            // Its shared $0795 gate is set for ordinary doors as well as elevators.
            // Running the normal camera here consumes the upward IRQ's +32 offset
            // before arrival resumes, exposing wrapped Samus art above the room.
            if (!deathOwnsSamus && !Enemies.ElevatorBehavior.ElevatorDoorTransitionActive)
            {
                if (Room.LevelData is null || Room.BackgroundStreamer is null)
                    throw new InvalidOperationException("Gameplay camera tracking requires active room stream data.");

                ActiveRoomGeometry roomGeometry = RoomLoader.GetActiveRoomGeometry();

                previousCameraPoint = Player.Samus.ApplyPreviousPositionWrites(previousCameraPoint);
                var currentCameraPoint = Player.Samus.Position;

                // GameState_8 calls MainScrollingRoutine after movement/PLMs and before
                // DrawSamusEnemiesAndProjectiles regardless of which Samus input handler is
                // installed. InputLocked suppresses controller transitions; it does not
                // suppress scrolling. That is why the station-eighteen gunship can carry
                // Samus down several screens while her ordinary movement handler is absent.
                if (Player.Samus.Grapple.SlowScrolling)
                {
                    // $90:94F2: a fast grapple swing takes the slow branch, which leaves
                    // the camera speed words from the last normal frame in place.
                    Room.Camera.TrackGrappleSlowScrolling(currentCameraPoint.Whole);
                }
                else
                {
                    Room.Camera.TrackMovedSamusHorizontally(
                        previousCameraPoint,
                        currentCameraPoint,
                        new HorizontalCameraContext(
                            KnockbackDirection: Player.Samus.KnockbackDirection,
                            MovementType: Player.Samus.PoseMetadata.ReadMovementType(_addressSpace),
                            XAccelerationMode: Player.Samus.HorizontalSpeed.AccelerationMode,
                            PoseXDirection: Player.Samus.PoseMetadata.ReadPoseXDirection(_addressSpace),
                            CameraDistanceIndex: Enemies.ZebetiteBehavior.CameraDistanceIndex));
                    Room.Camera.TrackMovedSamusVertically(
                        previousCameraPoint,
                        currentCameraPoint,
                        new VerticalCameraContext(
                            YDirection: Player.Samus.Kinematics.YDirection,
                            UpScroller: roomGeometry.UpScroller,
                            DownScroller: roomGeometry.DownScroller));
                }
                // $90:96C0 writes the same distance-plus-one words consumed by Yard
                // kick setup. Publish the camera calculation itself, including its
                // integer-only sign test and previous scrolling checkpoint. A second
                // frame-end absolute delta loses both the bias and checkpoint timing.
                Player.Samus.AbsoluteMovedLastFrameXFixed = unchecked((uint)Room.Camera.Speed.X.RawValue);
                SamusProjectileInheritance.PublishCameraYSubspeed(_addressSpace, Room.Camera.Speed.Y.Fraction);

                // MainScrollingRoutine `$90:9563` invokes the mutable global
                // scrolling-finished hook after both axes have tracked Samus. Live Spore
                // Spawn installs `$90:9589`, which owns the arena's vertical camera floor;
                // it must run before BG streaming and OAM consume layer-one position.
                Enemies.SporeSpawnBehavior.RunScrollingFinishedHook(Room.Camera);
                Room.Camera.FinishSamusScrolling(currentCameraPoint);

                // The camera can cross a 16-pixel boundary in the same main-loop pass.
                // Build and execute the exact row/column staging transfers now so both the
                // live PPU diagnostic and the following frame see the newly exposed edge.
                // Kraid's dead-room function wrote PreviousLayer1XBlock during EnemyMain.
                if (Enemies.KraidBehavior.Kraid is { Layer1XBlockResetRequested: true } kraid)
                {
                    BackgroundScroll.ResetPreviousLayer1XBlock();
                    kraid.Layer1XBlockResetRequested = false;
                }
                IReadOnlyList<BackgroundUpdateRequest> backgroundRequests =
                    BackgroundPhase.UpdateBackgroundScrollingFromCamera();
                BackgroundPhase.ExecuteBackgroundStreamRequests(backgroundRequests, "active room camera");

                // UpdateMinimap belongs to Samus's normal frame-handler beta; it uses world
                // position, not camera position. Fresh debug sessions begin without an area
                // map, so only tiles actually visited by this runtime are revealed.
                Hud.UpdateMinimap(_addressSpace, System, new MinimapFrame { AreaIndex = roomGeometry.AreaIndex, RoomMap = roomGeometry.MapPosition, RoomSize = Room.LevelData.Size, Samus = Player.Samus.Position.Whole, NmiFrameCounter = Nmi.NmiFrameCounter8, MapRevealMode = Options.MapRevealMode, PresentationMap = Artwork.MapPresentation?.Get(roomGeometry.AreaIndex) });
            }

            AttractDemo.RestoreAttractPlayerInput();

            ActorDrawing.DrawGameplayActors(deathOwnsSamus,
                // Command zero's `$90:E8DC` beta handler does not dispatch bank-$91
                // palette effects. In particular, automatic Reserve recovery freezes a
                // stored shine instead of consuming it while Samus is stationary.
                advanceSamusPalette: !suitOwnsSamus && !stationaryScriptControlLocked);
        }
        // EnemyMain can install the gunship's $90:E902 beta handler on this frame.
        // It omits the normal $90:E738 timer-hack call: neither countdown nor OAM
        // survives boarding, even though timer status stays active. Decide after the
        // actor transition, before liftoff can repurpose the timer's OBJ tiles as dust.
        bool runEscapeTimerHandler = !Enemies.GunshipHealthBehavior.HasGunshipHealthHandler;
        if (runEscapeTimerHandler && EscapeTimer.Process(Nmi.NmiFrameCounter, Options.PreventEscapeTimeout))
        {
            // `$90:E0E6`: the expired countdown publishes game state $23 for the
            // dispatcher, whites the target palettes and disables palette FX.
            TimeUpRequested = true;
            RoomPaletteFx.DisableHandler();
        }
        if (runEscapeTimerHandler && EscapeTimer.IsActive)
        {
            var presentation = Artwork.MapPresentation ?? throw new InvalidOperationException(
                "Escape timer requires installed presentation assets.");
            EscapeTimerRenderer.Draw(EscapeTimer, Sprites, presentation.EscapeTimer, presentation.EscapeTimerTiles.Artwork);
        }
        Sprites.FinalizeFrame();

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
            if (Player.Samus is not null && !Plms.CollectiblePlms.HasPendingCollectibleMessage)
                Hud.UpdateGameplayCounters(_addressSpace, Player.Samus, Freeze.TimeIsFrozen,
                    soundSuppressed: BombProjectiles.PowerBombExplosion.IsActive);
            Hud.QueueUpload(_addressSpace, VramWrites);
        }

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

        RoomMains.RunAnimatedTilesObjectHandler();
        EscapeEffects.StepEscapeRoomEffects();

        // Room main $8F:E2B6 is selected by the room state rather than by coordinates.
        // It runs at the common room-main seam after gameplay drawing and before shaking.
        if (MaridiaElevatube.IsActive)
        {
            if (Room.LevelData is null)
                throw new InvalidOperationException("Maridia elevatube has no active room level.");
            _ = MaridiaElevatube.Step(
                _addressSpace,
                Room.LevelData,
                Player.Samus,
                Nmi.NmiFrameCounter,
                Plms,
                Room.RoomMainScratch);
        }

        // Execute the active room's bank-$8F wrapper at the same seam as Landing Site's
        // room main above: after gameplay drawing/HUD work and before global shaking. The
        // Ceres routine publishes a matrix for the following frame's presentation and may
        // change the outer dispatcher to state $20 only when its caller is state eight.
        RoomMains.LastCeresElevatorShaftRoomMain = CeresElevatorShaft.Step(
            _addressSpace,
            Player.Samus,
            Enemies.RidleyEncounter.CeresStatus,
            allowDeparture: policy.AllowCeresElevatorDeparture,
            Room.RoomMainScratch);
        if (RoomMains.LastCeresElevatorShaftRoomMain.MatrixChanged)
            Display.ActiveSamusMode7Transform = RoomMains.LastCeresElevatorShaftRoomMain.Transform;

        RoomMains.RunCeresFallingDebrisRoomMain();
        RoomMains.RunCrocomireComebackRoomMain();
        if (Room.ActiveRoom?.State.MainCallback == RoomMainCallback.HandleCeresRidleyGetawayCutscene)
            Enemies.CeresRidleyBehavior.RunCeresRidleyGetawayRoomMain(Player.Samus, Nmi.NmiFrameCounter);

        // HandleSamusOutOfHealthAndGameTile advances the four-word gameplay clock after
        // room main and before shaking. Message-box frames returned above, exactly as the
        // suspended native coroutine does, so item fanfare time is not counted here.
        if (policy.AdvanceGameTime)
            GameTime.Step();

        // `$82:8BAF` executes room shaking after room main ASM and game-time handling, but
        // before the active-enemy lists are cleared. Enemy attacks above may have installed
        // a new global quake this frame; consuming it here gives that request its first
        // displacement/decrement immediately and preserves the list used for actor shake.
        if (Enemies.IsLoaded)
            Enemies.HandleRoomShaking(Freeze.TimeIsFrozen);

        // Gameplay state eight calls `$A0:9169` after Samus, enemies, drawing, HUD/BG
        // bookkeeping, room main ASM, the energy-zero check, and room shaking. Keep this
        // shared tail out of individual movement handlers: in particular, grapple spike
        // reactions set `$18A8/$18AA` without installing the bank-$90 knockback handler.
        // Death game states do not execute the gameplay-state tail at all.
        if (Player.Samus is not null && !Player.Samus.DeathSequence.IsActive)
        {
            Player.Samus.DecrementHurtTimers();
            Projectiles.Firing.DecrementInteractionTimer();
        }

        // A resumed tail's frame already published its audio when it suspended.
        if (!resumedAfterMessageBox)
            NmiHandler.CompletedGameplayAudioPublication++;
        return Snapshot(infiniteAmmoGuard);
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
        CrocomireDeathState? crocomireDeath = Enemies.CrocomireBehavior.Crocomire is not null
            ? Enemies.CrocomireBehavior.CrocomireDeath : null;
        MotherBrainEnemyState? motherBrain = Enemies.MotherBrainEncounter.MotherBrain;
        ScrollingSkyState? sky = Room.ScrollingSky;
        var frameEnd = new GameplayFrameEndDisplayInputs(
            Room.DoorTransitionMainScreenLayers,
            sky is { HorizontalHdmaConfigured: true }
                ? ImmutableArray.Create(sky.BuildGameplayHorizontalScrolls(Room.Camera?.PixelPosition.Y
                    ?? throw new InvalidOperationException("Scrolling sky has no gameplay camera.")))
                : null,
            crocomireDeath is { MeltingHdmaActive: true } melting
                ? ImmutableArray.CreateRange(melting.Bg2ScrollByScanline) : null,
            motherBrain is not null,
            motherBrain is { RisingHdmaActive: true },
            motherBrain?.RainbowBeamHdma.RenderSnapshot,
            CeresHazeRenderSnapshot.Capture(CeresHaze),
            Artwork.MapPresentation?.RoomFxPaletteBlends,
            PowerBombRenderSnapshot.Capture(BombProjectiles.PowerBombExplosion),
            GameplayMessageBoxRenderer.Capture(MessageBox),
            SuitPickup.IsActive ? SuitPickup.Beam : null,
            SamusXrayDisplaySnapshot.Capture(Player.Samus, Player.LastXrayBeamStep, _addressSpace),
            TourianStatues.Enabled);
        return new(Display.Latched, frameEnd, PpuMemorySnapshot.Capture(Vram, Cgram, DisplayedSprites));
    }

    /// <summary>
    /// Reused final gameplay frame buffer. Callers keeping an image beyond the next render
    /// must copy it because subsequent software rendering overwrites the same array.
    /// </summary>
    [field: NonSerialized]
    internal Rgba32[] GameplayFrameBuffer =>
        field ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];

    /// <summary>
    /// Gameplay-frame state the remainder of state eight reads after <c>PLM_Handler</c>.
    /// The bank-$85 message routine runs inside that handler, so a PLM that opens a box
    /// suspends the native frame; the port parks this state until the box closes.
    /// </summary>
    internal readonly record struct SuspendedGameplayFrameTail(
        bool DeathOwnsSamus,
        FixedPosition<RoomPixels> PreviousCameraPoint,
        bool StationaryScriptControlLocked,
        bool SuitOwnsSamus);

    /// <summary>State-eight tail parked when a PLM opens a message box during a gameplay frame.</summary>
    internal SuspendedGameplayFrameTail? _suspendedFrameTail;

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
        if (openingTourian && TourianStatues.Enabled) TourianStatues.Load();
        Options.PreventEscapeTimeout = options.PreventEscapeTimeout;
        Options.MapRevealMode = options.MapRevealMode;
    }

    /// <summary>Room-owned native statue tile programs and HDMA descent.</summary>
    public TourianStatueSequence TourianStatues { get; }

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
                HdmaPhase.AdvanceHdmaObjectPreInstructions();
            if (Player.Samus is { } hdmaSamus)
                BombProjectiles.AdvancePowerBombHdma(_addressSpace, hdmaSamus, Controller1.Current, DispatchGameState);
        }
        if (MessageBox.ConfirmationSelectionChangedThisFrame)
            MessageBoxes.MessageBoxSelectionSoundRequestedThisFrame = true;
        if (MessageBox.IsActive)
            return Snapshot(infiniteAmmoGuard);

        MessageBoxes.ResolveClosedMessageBox(frameSuspended: _suspendedFrameTail is not null);
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
