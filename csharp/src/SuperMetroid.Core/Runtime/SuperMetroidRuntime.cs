using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

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
public sealed partial class SuperMetroidRuntime
{
    private readonly ISnesAddressSpace _addressSpace;
    private SamusSuitPickupKind? _pendingSuitPickup;

    public SuperMetroidRuntime(ISnesAddressSpace addressSpace)
    {
        _addressSpace = addressSpace ?? throw new ArgumentNullException(nameof(addressSpace));

        // These two host objects represent the lower and upper halves of the cartridge's
        // parallel ten-slot projectile arrays. BombProjectiles remains the owner of shared
        // cooldown $0CCC until the remaining weapon producers are consolidated.
        BombProjectiles = new SamusBombProjectileSystem();
        Projectiles = new SamusProjectileSystem();

        // $82:82C5 copies all 512 bytes of kInitialPalette from ROM $9A:8000. The retail
        // game stages this through WRAM before NMI uploads CGRAM; initializing the modeled
        // PPU here produces the same starting colors while that fade pipeline is ported.
        Cgram.LoadFromBus(_addressSpace, 0x9a8000);
    }

    /// <summary>Bank-$80 shared random/event/input-filter state.</summary>
    public Bank80SystemState System { get; } = new();

    /// <summary>Gameplay clock mirrored at WRAM $09DA-$09E1 and in each SRAM slot.</summary>
    public GameTimeState GameTime { get; } = new();

    /// <summary>Controller-1 NMI latch from <c>$80:9459</c>.</summary>
    public ControllerInputState Controller1 { get; } = new();

    /// <summary>Escape timer state machine from <c>$80:9DE7</c>.</summary>
    public EscapeTimer EscapeTimer { get; } = new();

    /// <summary>
    /// Frontend language bit consumed by room actors whose cartridge script differs
    /// between English and Japanese. The enemy owner retains the actual native branch.
    /// </summary>
    public bool JapaneseText
    {
        get => Enemies.JapaneseText;
        set => Enemies.JapaneseText = value;
    }

    /// <summary>Ordinary seven-byte-record VRAM write table.</summary>
    public VramWriteQueue VramWrites { get; } = new();

    /// <summary>PPU video RAM receiving accepted-NMI transfers.</summary>
    public SnesVram Vram { get; } = new();

    /// <summary>PPU color RAM initialized from <c>kInitialPalette</c> at ROM <c>$9A:8000</c>.</summary>
    public SnesCgram Cgram { get; } = new();

    /// <summary>Current 544-byte sprite table prepared for the next PPU OAM upload.</summary>
    public OamBuffer Oam { get; } = new();

    /// <summary>
    /// Sprite table made PPU-visible by the most recent accepted NMI. Renderers use this
    /// buffer; debugger watches can compare it with <see cref="Oam"/>, which the current
    /// main-loop pass is preparing for the following NMI.
    /// </summary>
    public OamBuffer DisplayedOam { get; } = new();

    /// <summary>Active room's fixed 32-slot bank-$A0 enemy system.</summary>
    public RoomEnemySystem Enemies { get; } = new();

    /// <summary>
    /// Normal-gameplay Samus rendering state. Null means the current scenario has not
    /// explicitly introduced Samus; the Landing Site cinematic does not do so by itself.
    /// </summary>
    public SamusState? Samus { get; private set; }

    /// <summary>
    /// Winning cartridge pose-transition entry for the most recently latched controller
    /// chord. The grounded debug scenario applies its verified $01/$02/$09/$0A/$25/$26
    /// routes; every other winner remains diagnostic until its native side effects exist.
    /// </summary>
    public SamusPoseTransition? ProspectiveSamusPose { get; private set; }

    /// <summary>
    /// Block-only result of `$91:EADE` after movement. This is separate from the input
    /// table record because a killed running speed can create `$89/$8A/$CF-$D2` even when
    /// no controller record matched, and a one-pixel probe can replace a running target.
    /// </summary>
    public byte? ProspectiveSamusWallCollisionPose { get; private set; }

    /// <summary>The optional one-pixel block probe responsible for the native arm-pump bug.</summary>
    public BlockMoveResult? LastRanIntoWallProbe { get; private set; }

    /// <summary>
    /// No-button fallback selected by <c>Samus_Pose_CancelGrapple</c> at $91:82D9. This is
    /// separate from <see cref="ProspectiveSamusPose"/> because no six-byte table entry wins.
    /// </summary>
    public ushort? ProspectiveSamusFallbackPose { get; private set; }

    /// <summary>
    /// True only for the explicit grounded gameplay-debug scenario. The cinematic render
    /// stimulus remains stationary and continues treating pose matches as diagnostics.
    /// </summary>
    public bool GroundedSamusMovementEnabled { get; private set; }

    /// <summary>
    /// Host-readable equivalent of the native Moonwalk options word consumed by
    /// `$91:F88C`. False preserves the default turn substitution; true admits the six
    /// stable movement-type-$10 poses selected by the unchanged ROM input tables.
    /// </summary>
    public bool MoonwalkEnabled { get; set; }

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
        get => (ushort)Enemies.ElevatorStatus;
        set => Enemies.SetElevatorStatusForDebugging(value);
    }

    /// <summary>
    /// Reconstructs the one side effect still owned by the native door-transition caller.
    /// This host resumes gameplay immediately after loading the destination, so the arriving
    /// actor can cross its pseudo-door while statuses two/three are visible to state eight.
    /// Retain `$90:A392`'s complete terrain/PLM scan, but permit its type-$9 handler to
    /// publish a real transition or elevator flag only for status one; otherwise a
    /// collapsed transition can immediately depart again.
    /// </summary>
    internal static bool ShouldPublishFacingForwardElevatorDoorSideEffects(
        ElevatorActorStatus status) => status == ElevatorActorStatus.Departing;

    /// <summary>
    /// Native power-bomb explosion status at WRAM <c>$0CE2</c>. X-ray setup rejects every
    /// nonzero phase. The getter now reads the translated bank-$88 owner; the setter remains
    /// solely as a debugger seam for precondition tests that force an otherwise impossible
    /// status without constructing an entire explosion.
    /// </summary>
    public ushort PowerBombExplosionStatus
    {
        get => BombProjectiles.PowerBombExplosion.Status;
        set => BombProjectiles.PowerBombExplosion.SetStatusForDebugging(value);
    }

    /// <summary>
    /// Low-byte snapshot at WRAM <c>$0A11</c> used only by X-ray's one-frame stability gate.
    /// The runtime updates it after each completed gameplay frame.
    /// </summary>
    public byte PreviousMovementTypeForXray { get; private set; }

    /// <summary>
    /// Horizontal and vertical collision results from the most recent translated grounded
    /// movement pass. Null before movement, and always null for the cinematic stimulus.
    /// </summary>
    public GroundedMovementResult? LastGroundedSamusMovement { get; private set; }

    /// <summary>Most recent ordinary-air collision result, exposed for debugger watches.</summary>
    public AerialMovementResult? LastAerialSamusMovement { get; private set; }

    /// <summary>Most recent ordinary Morph-Ball collision result, exposed for debugger watches.</summary>
    public MorphBallMovementResult? LastMorphBallMovement { get; private set; }

    /// <summary>Most recent special bomb-jump handler result, exposed for debugger watches.</summary>
    public BombJumpMovementResult? LastBombJumpMovement { get; private set; }

    /// <summary>Most recent special knockback-handler result, exposed for debugger watches.</summary>
    public KnockbackMovementResult? LastKnockbackMovement { get; private set; }

    /// <summary>Most recent Ceres Ridley scripted ejection-handler result.</summary>
    public CeresRidleyEjectionResult? LastCeresRidleyEjection { get; private set; }

    /// <summary>Most recent bank-$9B connected-grapple function result.</summary>
    public GrappleMovementResult? LastGrappleMovement { get; private set; }

    /// <summary>Most recent stored-shine windup or active shinespark handler result.</summary>
    public ShinesparkMovementResult? LastShinesparkMovement { get; private set; }

    /// <summary>Most recent Crystal Flash start, ammo-drain, or finish handler result.</summary>
    public CrystalFlashMovementResult? LastCrystalFlashMovement { get; private set; }

    /// <summary>Most recent call of X-ray's dedicated bank-$91 pose-input handler.</summary>
    public XrayPoseInputResult? LastXrayPoseInput { get; private set; }

    /// <summary>Most recent call of X-ray's bank-$88 setup/beam/deactivation state.</summary>
    public XrayBeamStepResult? LastXrayBeamStep { get; private set; }

    /// <summary>
    /// Angle-selected `$90:E94F` art frame; null while X-ray's type-`$0E` turn handler is RTS.
    /// </summary>
    public ushort? LastXrayAnimationFrame { get; private set; }

    /// <summary>Most recent game-state-owned fatal-damage animation/palette/VRAM call.</summary>
    public SamusDeathSequenceStepResult? LastDeathSequenceStep { get; private set; }

    /// <summary>
    /// Most recent global bank-$8D Hyper Beam palette-object call. Unlike Samus's body
    /// palette handler, this owns OBJ palette six and runs before the Samus state handler.
    /// </summary>
    public HyperBeamPaletteFxStepResult? LastHyperBeamPaletteFxStep { get; private set; }

    /// <summary>
    /// Most recent beam-charge/post-shot body-palette branch from `$91:D743`. This remains
    /// distinct from the global Hyper Beam projectile palette object above: it owns Samus's
    /// OBJ palette four rather than projectile OBJ palette six.
    /// </summary>
    public SamusBeamChargePaletteStepResult LastBeamChargePaletteStep { get; private set; }

    /// <summary>Most recent ordinary `$91:D83F` visor color-cycle step.</summary>
    public SamusVisorPaletteStepResult LastVisorPaletteStep { get; private set; }

    /// <summary>
    /// Native WRAM <c>$1982</c>. Room/HDMA setup owns this word; the current Landing Site
    /// shell starts at bank-$88's cleared-FX default two.
    /// </summary>
    public ushort LayerBlendingDefaultConfig { get; set; } =
        (ushort)LayerBlendingConfiguration.NormalGameplay;

    /// <summary>
    /// Most recent call of the final-priority ordinary hurt palette handler. The result
    /// exposes both visible palette cadence and counter-forty audio recovery to a debugger.
    /// </summary>
    public SamusHurtFlashPaletteStepResult LastHurtFlashPaletteStep { get; private set; }

    /// <summary>Most recent `$90:C5C4` arm-cannon cover state update.</summary>
    public SamusArmCannonUpdateResult LastArmCannonUpdate { get; private set; }

    /// <summary>Most recent optional cannon OBJ and tile-$1F DMA publication.</summary>
    public SamusArmCannonDrawResult LastArmCannonDraw { get; private set; }

    /// <summary>
    /// Whether the current main-loop frame actually appended Samus's body spritemaps.
    /// This differs from merely reaching the drawing phase: invincibility and the elevator
    /// display handler can intentionally suppress the body on odd accepted NMI frames.
    /// </summary>
    public bool LastSamusBodyDrawn { get; private set; }

    /// <summary>
    /// True only when the current frame dispatched `$90:EBF3`, the deliberately minimal
    /// shinespark-crash body-plus-two-echo renderer. Exposed separately from phase state so
    /// a debugger can verify that handler installation and presentation agree on a frame.
    /// </summary>
    public bool LastShinesparkCrashDrawingHandlerActive { get; private set; }

    /// <summary>
    /// True when this frame dispatched grapple's replacement `$90:EB86` Samus handler.
    /// Teardown phases remain true for their one pending frame even though the handler's
    /// signed range test selects ordinary body/cannon/echo rendering inside that routine.
    /// </summary>
    public bool LastGrappleDrawingHandlerActive { get; private set; }

    /// <summary>
    /// True when `$90:EB86` took its beam-specific flare/body/rope half rather than the
    /// teardown fallback. This is intentionally narrower than handler installation.
    /// </summary>
    public bool LastGrappleBeamSpecificDrawingPath { get; private set; }

    /// <summary>Whether grapple's bank-$93 flare passed its unsigned screen-Y gate.</summary>
    public bool LastGrappleFlareDrawn { get; private set; }

    /// <summary>
    /// Optional Ceres-owned Mode 7 matrix used only to calculate Samus's rendered body
    /// origin. Null is ordinary gameplay. A future Ceres cinematic actor should publish
    /// and clear this value with its status high bit; the translated consumer never moves
    /// Samus's physical coordinates.
    /// </summary>
    public SamusMode7Transform? ActiveSamusMode7Transform { get; set; }

    /// <summary>
    /// Exact room-main owner for Ceres elevator shaft <c>$DF45</c>. The object persists so
    /// debugger watches can inspect its signed rotation phase, but every room load resets
    /// and explicitly activates or deactivates it from the cartridge room-state pointer.
    /// </summary>
    public CeresElevatorShaftRoomMainState CeresElevatorShaft { get; } = new();

    /// <summary>Most recent <c>$89:ACC3</c> room-main call.</summary>
    public CeresElevatorShaftRoomMainResult LastCeresElevatorShaftRoomMain { get; private set; }

    /// <summary>Most recent call of drained Samus's installed `$90:94CB` falling handler.</summary>
    public DrainedSamusMovementResult? LastDrainedSamusMovement { get; private set; }

    /// <summary>Most recent normal movement-type-`$1A` dispatcher result.</summary>
    public DraygonGrabbedMovementResult? LastDraygonGrabbedMovement { get; private set; }

    /// <summary>Most recent `$90:E2A1` alternating-D-pad escape-handler result.</summary>
    public DraygonEscapeResult? LastDraygonEscape { get; private set; }

    /// <summary>
    /// Explicit debugger substitute for the untranslated HUD item selector. When enabled,
    /// a new Shoot edge starts grapple firing from the current pose. Normal scenarios leave
    /// this false, so X retains its existing bomb/projectile meaning.
    /// </summary>
    public bool DebugGrappleItemSelected { get; private set; }

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
    /// Active layer-1 scroll-boundary state. It remains null until a room-specific scroll
    /// table is loaded; this prevents the runtime from silently assuming Landing Site.
    /// </summary>
    public ScrollBoundaryCamera? Camera { get; private set; }

    /// <summary>
    /// Bank-$80 parallax/register/block dispatcher fed by the active layer-1 camera.
    /// Its requests become row/column staging DMAs when a room loader supplies level data.
    /// </summary>
    public BackgroundScrollState BackgroundScroll { get; } = new();

    /// <summary>Landing Site's ROM-backed bank-$80 tilemap producer after room initialization.</summary>
    public BackgroundTilemapStreamer? BackgroundStreamer { get; private set; }

    /// <summary>Row/column updates produced by the most recent camera-to-BG pass.</summary>
    public int LastBackgroundUpdateCount { get; private set; }

    /// <summary>
    /// Active room's decompressed BG1/BTS/BG2 allocation. Rendering and collision must
    /// share this instance; null means no room level stream has been loaded yet.
    /// </summary>
    public RoomLevelData? LevelData { get; private set; }

    /// <summary>Landing Site's bank-$88 scrolling-sky updater after room initialization.</summary>
    public ScrollingSkyState? ScrollingSky { get; private set; }

    /// <summary>Door/header/library-background data selected for the current Landing Site entry.</summary>
    public LandingSiteEntryState? LandingSiteEntry { get; private set; }

    /// <summary>
    /// Loads Landing Site's exact 50-byte scroll buffer and creates the door-selected camera
    /// and library-background state.
    /// </summary>
    public void InitializeLandingSiteCamera(
        ushort doorPointer = LandingSiteEntryState.LandingCutsceneDoorPointer)
    {
        // A runtime may be reused by diagnostics that visit Ceres before Crateria. The
        // elevator pair belongs exclusively to the fresh-Ceres setup and must not survive
        // a later room transition merely because both paths share this runtime instance.
        CeresElevatorArrival = null;
        ActiveLoadStation = null;
        ActiveDoor = null;
        ActiveRoom = null;
        ActiveRoomAssets = null;
        LandingSiteEntry = LandingSiteEntryState.Load(_addressSpace, doorPointer);
        RoomScrollGrid scrolls = RoomScrollGrid.LoadLandingSite(_addressSpace);
        Camera = new ScrollBoundaryCamera(scrolls);

        // Applying the selected door's screen bytes here keeps its command-E sky tilemap
        // page and initial camera inseparable. This matters vertically: using cutscene door
        // $88FE's top-of-room page after host-moving to screen row four produces a repeating
        // purple strip instead of the lower cloudy Landing Site sky.
        Camera.SetPosition(LandingSiteEntry.CameraX, LandingSiteEntry.CameraY);

        // Every Landing Site state header stores layer2Scrolls($81, 1). Both axes are odd,
        // so ordinary scrolling does not stream BG2; the scrolling-sky room ASM owns it.
        BackgroundScroll.Layer2ScrollX = 0x81;
        BackgroundScroll.Layer2ScrollY = 0x01;
        BackgroundScroll.PrimePreviousBlocks();
        LevelData = LandingSiteStreamingData.LoadLevel(_addressSpace);
        BackgroundStreamer = LevelData.CreateBackgroundStreamer(sizeOfBg2: 0);
        // Supplying the ROM bus matters at the landing-cutscene's Y=0 edge: bank $88's
        // unsigned cameraY-16 table index intentionally reads adjacent ROM instructions.
        ScrollingSky = new ScrollingSkyState(_addressSpace);
    }

    /// <summary>
    /// Copies the translated layer-1 camera into <c>$80:A3AB</c> scroll bookkeeping and
    /// returns the exact BG update calls selected for this frame.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> UpdateBackgroundScrollingFromCamera(bool timeIsFrozen = false)
    {
        if (Camera is null)
            throw new InvalidOperationException("A room camera must be initialized before BG scrolling.");

        BackgroundScroll.Layer1XPosition = Camera.XPosition;
        BackgroundScroll.Layer1YPosition = Camera.YPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = BackgroundScroll.StepScrolling(timeIsFrozen);

        // Both native block producers begin with the same `$0783` test at
        // `$80:A9DE/$80:AB78`. Ceres's elevator door sets that Mode-7 IRQ word, so crossing
        // a 16-pixel camera boundary still updates all layer/register bookkeeping above but
        // performs no ordinary Mode-1 row or column expansion. Apart from corrupting Mode-7
        // VRAM, retaining these requests made the bottom-right 17th row sample walk beyond
        // Ceres's logical BG2 allocation and turned harmless native overfetch into a host
        // exception.
        if (ActiveDoor?.UsesCeresElevatorMode7 == true)
        {
            LastBackgroundUpdateCount = 0;
            return Array.Empty<BackgroundUpdateRequest>();
        }

        LastBackgroundUpdateCount = requests.Count;
        return requests;
    }

    /// <summary>
    /// Loads Landing Site character graphics and performs the native 17-column initial BG1
    /// viewport fill after an entry camera position has been selected.
    /// </summary>
    public InitialViewportResult InitializeLandingSiteViewport()
    {
        if (Camera is null || BackgroundStreamer is null)
            throw new InvalidOperationException("Landing Site camera and stream data must be initialized first.");

        if (LandingSiteEntry is null)
            throw new InvalidOperationException("Landing Site entry metadata was not initialized.");

        LandingSiteStreamingData.LoadCharacterGraphics(_addressSpace, Vram, LandingSiteEntry);

        // The selected room-state record owns both of these pointers. This call parses the
        // terminated $A1 population and $B4 graphics set, loads palettes/tiles, constructs
        // native $40-byte slots, and dispatches each definition's initialization AI.
        Enemies.Load(
            _addressSpace,
            LandingSiteEntry.EnemyPopulationPointer,
            LandingSiteEntry.EnemyTilesetPointer,
            Vram,
            Cgram,
            System.NextRandom,
            System.SetRandomNumber,
            readRandomNumber: () => System.RandomNumber,
            level: LevelData,
            samus: Samus,
            controllerInput: Controller1.Current,
            isAreaBossDefeated: () => System.HasAnyBossBits(
                LandingSiteEntry.AreaIndex,
                BossBits.AreaBoss),
            setAreaBossDefeated: () => System.SetBossBits(
                LandingSiteEntry.AreaIndex,
                BossBits.AreaBoss));

        // InitializeHud queued the cartridge's $2E00-byte standard OBJ sheet before this
        // room loader existed. That transfer reaches VRAM byte $EDFF and overlaps the main
        // gunship allocation at $E000-$EDFF. Native LoadEnemyTileData follows the standard
        // upload, so append the room enemy transfers now as well as applying their immediate
        // debugger-visible copies above. Omitting this reorder left only the gunship's small
        // $F000 tail intact and decoded HUD/standard tiles through most of its spritemaps.
        Enemies.QueueGraphicsUploads(VramWrites);

        // `$90:AC8D` is normally reached when equipment/room setup settles. Queue it here,
        // after InitializeHud's $2E00-byte standard OBJ transfer, so the smaller $0100 beam
        // region at VRAM $6300 wins in the same NMI order as the cartridge. The gameplay
        // debug spawn has power beam type zero when no save inventory has been supplied.
        Projectiles.QueueBeamTilesAndLoadPalette(
            _addressSpace,
            VramWrites,
            Cgram,
            Samus?.EquippedBeams ?? 0);

        // The queued HUD graphics begin at VRAM word $4000 and span through byte $9FFF,
        // overlapping both scrolling-sky pages at words $4800-$4FFF. The door-selected
        // library background is installed later in the cartridge's room-loading sequence.
        // Requeue its literal ROM slice here so it wins at the first accepted NMI; the four
        // per-frame circular row updates are appended after it and remain the final writers.
        VramWrites.Enqueue(
            LandingSiteEntry.SkyByteCount,
            LandingSiteEntry.SkySourceAddress,
            LandingSiteEntry.SkyVramDestination);
        BackgroundScroll.Layer1XPosition = Camera.XPosition;
        BackgroundScroll.Layer1YPosition = Camera.YPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = BackgroundScroll.BuildInitialViewportRequests();
        LastBackgroundUpdateCount = requests.Count;
        int segmentCount = 0;
        foreach (BackgroundUpdateRequest request in requests)
        {
            TilemapStreamUpdate update = BackgroundStreamer.Build(request)
                ?? throw new InvalidOperationException("Landing Site unexpectedly entered Mode 7 streaming.");
            update.ExecuteTo(Vram);
            segmentCount += update.Segments.Count;
        }

        // $A176 temporarily increments its block words through the fill. Recalculate and
        // seed gameplay comparisons from the actual camera before the first scrolling frame.
        BackgroundScroll.PrimePreviousBlocks();
        ScrollingSky!.ProcessFrame(Camera.YPosition, timeIsFrozen: false, VramWrites);
        return new InitialViewportResult(requests.Count, segmentCount);
    }

    /// <summary>
    /// Loads standard 2-bpp BG3 graphics and initializes the HUD tilemap using the exact
    /// ROM/WRAM/VRAM destinations from <c>$82:82E2</c> and <c>$80:9A79</c>.
    /// </summary>
    public void InitializeHud(HudSnapshot snapshot)
    {
        // The original copies $2000 bytes even though the named standard BG3 graphics are
        // $1000 bytes; the following $1000-byte clear table intentionally fills the rest of
        // VRAM $4000-$4FFF with zeroes.
        VramWrites.Enqueue(sizeInBytes: 0x2000, sourceAddress: 0x9ab200, encodedVramDestination: 0x4000);

        // $82:8318 follows the BG3 transfer with $2E00 bytes of standard sprite tiles at
        // VRAM $6000. The dynamic Samus DMA refreshes its four reserved regions each NMI;
        // fixed projectile tiles such as bomb $14C-$14F remain in the untouched portion.
        VramWrites.Enqueue(sizeInBytes: 0x2e00, sourceAddress: 0x9ad200, encodedVramDestination: 0x6000);

        // LoadEnemyTileData's transfers follow the overlapping standard OBJ upload. The
        // room enemy loader has already populated the modeled VRAM for immediate watches;
        // queueing the source records here makes enemy art win again at the accepted NMI.
        if (Enemies.IsLoaded)
            Enemies.QueueGraphicsUploads(VramWrites);

        // The immutable first row bypasses WRAM and is DMAed straight from $80:988B.
        VramWrites.Enqueue(sizeInBytes: 0x0040, sourceAddress: 0x80988b, encodedVramDestination: 0x5800);

        Hud.Initialize(_addressSpace, snapshot);
        Hud.QueueUpload(_addressSpace, VramWrites);
    }

    /// <summary>
    /// Decompresses graphics-set-zero's Upper Crateria palette at <c>$C2:AD7C</c> into
    /// CGRAM background colors 0-127. Landing Site and Ceres use this tileset palette.
    /// </summary>
    public void LoadUpperCrateriaBackgroundPalette()
    {
        const int sourceAddress = 0xc2ad7c;
        const int compressedByteCount = 0x00e1;

        // The extracted asset boundary is known from the ROM label map. Reading exactly
        // $E1 bytes allows Decompress to insist the $FF terminator ends the whole stream,
        // catching an incorrect address instead of decoding through unrelated ROM data.
        var compressed = new byte[compressedByteCount];
        for (int index = 0; index < compressed.Length; index++)
            compressed[index] = _addressSpace.ReadByte(sourceAddress + index);

        byte[] paletteBytes = SmCompression.Decompress(compressed, maximumOutputBytes: 0x0100);
        if (paletteBytes.Length != 0x0100)
        {
            throw new InvalidDataException(
                $"Upper Crateria palette expanded to ${paletteBytes.Length:X} bytes instead of $100.");
        }

        // $82:E7C9 originally decompresses these 128 colors to target-palette WRAM and a
        // fade later reaches CGRAM. The desktop preview installs the same final words now;
        // target/current fade buffers remain a future timing layer.
        Cgram.LoadBytes(paletteBytes, destinationIndex: 0);
    }

    /// <summary>
    /// Creates a stationary pose-$01 Samus as an explicit debugger stimulus.
    /// </summary>
    /// <remarks>
    /// The landing-cutscene door enters a cinematic state whose ship/actors are not normal
    /// gameplay Samus. Consequently there is no honest cartridge-defined Samus spawn to
    /// read from that door record. This method chooses only a host placement in the current
    /// viewport; pose tables, palette, graphics DMA, screen-position math,
    /// spritemap pointers, and OAM records all remain cartridge-defined translations.
    /// </remarks>
    public void InitializeDebugStandingSamus()
    {
        if (Camera is null)
            throw new InvalidOperationException("A room camera must be initialized before placing debug Samus.");

        GroundedSamusMovementEnabled = false;
        LastGroundedSamusMovement = null;
        LastAerialSamusMovement = null;
        LastMorphBallMovement = null;
        LastBombJumpMovement = null;
        LastKnockbackMovement = null;
        BombProjectiles.Reset();
        Projectiles.Reset();
        InitializeDebugSamus(
            xPosition: unchecked((ushort)(Camera.XPosition + 64)),
            yPosition: unchecked((ushort)(Camera.YPosition + 166)));
    }

    /// <summary>
    /// Creates pose-$01 Samus on a supported Landing Site floor beneath world X=$0440 and
    /// repositions the room camera so both she and the selected terrain are visible.
    /// </summary>
    /// <remarks>
    /// This is an explicitly host-selected debugger scenario, not a claim that the landing
    /// cinematic door owns a gameplay spawn. The selected X and desired screen Y are host
    /// policy. The default minimum row $4D skips a valid but visually transparent collision
    /// slope and selects the first lower supported surface, a visibly rendered solid floor.
    /// Floor type/BTS, height, pose radius, resting world Y, camera clamps, movement,
    /// collision, animation, graphics, and OAM all come from translated cartridge data.
    /// Call it after <see cref="InitializeLandingSiteCamera"/> and before
    /// <see cref="InitializeLandingSiteViewport"/>, because the latter must fill VRAM for
    /// the newly selected camera position.
    /// </remarks>
    public DebugGroundedSamusPlacement InitializeDebugGroundedSamus(
        ushort xPosition = 0x0440,
        ushort desiredScreenY = 166,
        int minimumFloorBlockY = 0x4d)
    {
        if (Camera is null || LevelData is null)
        {
            throw new InvalidOperationException(
                "Landing Site camera and level data must be initialized before grounding debug Samus.");
        }

        // Radius is pose data, so construct/refresh pose $01 before deriving the surface
        // center. The temporary Y=0 is never rendered or stepped.
        InitializeDebugSamus(xPosition, yPosition: 0);
        ushort yRadius = Samus!.Kinematics.YRadius;

        if ((uint)minimumFloorBlockY >= (uint)LevelData.HeightInBlocks)
            throw new ArgumentOutOfRangeException(nameof(minimumFloorBlockY));

        int blockX = xPosition >> 4;
        for (int blockY = minimumFloorBlockY; blockY < LevelData.HeightInBlocks; blockY++)
        {
            RoomCollisionBlock floor = LevelData.GetCollisionBlock(blockX, blockY);

            byte height;
            if (floor.CollisionType == 8)
            {
                // Solid block type $8 uses the block's top edge as its floor. A height of
                // zero expresses that edge in the same block-local coordinate system used
                // by slope profiles below.
                height = 0;
            }
            else if (floor.CollisionType == 1 &&
                     (floor.Behavior & 0x1f) >= 5 &&
                     (floor.Behavior & 0x80) == 0)
            {
                // Upright non-square slopes use the cartridge's sixteen-sample profile.
                // Square slopes are translated too, but selecting one as a spawn surface
                // would require choosing the occupied quadrant rather than one scalar Y.
                height = SamusSlopePhysics.ReadAlignmentHeight(
                    _addressSpace,
                    floor.Behavior,
                    xPosition);
            }
            else
            {
                continue;
            }

            ushort restingY = unchecked((ushort)(blockY * 16 + height - yRadius));

            // Normal right-facing distance slot zero targets layer1X = SamusX-$60. Seed the
            // camera at that exact target so the first moved frame follows smoothly instead
            // of spending several frames correcting the cutscene door's unrelated X=$400.
            // Vertical framing remains the explicit host stimulus supplied by the caller.
            Camera.SetPosition(xPosition - 0x60, restingY - desiredScreenY);
            Samus.YPosition = restingY;
            GroundedSamusMovementEnabled = true;
            LastGroundedSamusMovement = null;
            LastAerialSamusMovement = null;
            LastMorphBallMovement = null;
            LastBombJumpMovement = null;
            LastKnockbackMovement = null;
            BombProjectiles.Reset();
            Projectiles.Reset();
            return new DebugGroundedSamusPlacement(
                xPosition,
                restingY,
                desiredScreenY,
                blockX,
                blockY,
                floor,
                height);
        }

        throw new InvalidOperationException(
            $"No supported solid or upright non-square floor exists beneath " +
            $"Landing Site X=${xPosition:X4} from block row ${minimumFloorBlockY:X2}.");
    }

    /// <summary>
    /// Finds a plain, cartridge-authored Landing Site floor/wall corner and places a
    /// right-facing standing Samus exactly one prospective running pixel from that wall.
    /// </summary>
    /// <remarks>
    /// This is debugger setup, not gameplay logic: retail Landing Site's cinematic door
    /// does not supply a normal Samus spawn. The scan chooses only a host test location.
    /// Every inspected collision nibble, the resting height, the eventual one-pixel probe,
    /// pose selection, movement, animation, camera tracking, tile art, and rendering still
    /// come from the user's ROM and the translated bank-$90/$91/$94 routines.
    ///
    /// Requiring type-<c>$8</c> floor and wall blocks with type-<c>$0</c> body clearance is deliberate.
    /// It makes this regression exercise the ordinary solid-block dispatcher and prevents
    /// an unrelated slope, PLM, door, or untranslated special-block family from becoming
    /// an accidental prerequisite for testing movement type `$15`.
    /// </remarks>
    public DebugRanIntoWallSamusPlacement InitializeDebugRanIntoWallSamus(
        ushort desiredScreenY = 166,
        int minimumFloorBlockY = 3)
    {
        if (Camera is null || LevelData is null)
        {
            throw new InvalidOperationException(
                "Landing Site camera and level data must be initialized before finding a debug wall.");
        }

        // Pose `$01` owns the body radii used by the prospective-run probe. Load the real
        // record before scanning so the host does not smuggle guessed dimensions into the
        // collision scenario. This temporary origin is never stepped or rendered.
        InitializeDebugSamus(xPosition: 0, yPosition: 0);
        int xRadius = Samus!.Kinematics.XRadius;
        int yRadius = Samus.Kinematics.YRadius;

        int firstFloorRow = Math.Max(minimumFloorBlockY, 3);
        if ((uint)firstFloorRow >= (uint)LevelData.HeightInBlocks)
            throw new ArgumentOutOfRangeException(nameof(minimumFloorBlockY));

        for (int floorBlockY = firstFloorRow;
             floorBlockY < LevelData.HeightInBlocks;
             floorBlockY++)
        {
            for (int wallBlockX = 1;
                 wallBlockX < LevelData.WidthInBlocks;
                 wallBlockX++)
            {
                // At this center X, the standing body's current right edge is the final
                // pixel before the wall. `$91:EADE`'s +1.0000 move therefore samples the
                // wall column and must stop at the unchanged last-safe center.
                int centerX = wallBlockX * 16 - xRadius;
                int centerY = floorBlockY * 16 - yRadius;
                int bodyBlockX = (centerX + xRadius - 1) >> 4;
                int bodyTopBlockY = (centerY - yRadius) >> 4;
                int bodyBottomBlockY = (centerY + yRadius - 1) >> 4;

                // This assertion documents the geometry relied on by the scan. Should a
                // future suit/body-radius translation span another column, fail this test
                // fixture instead of quietly selecting partially occupied terrain.
                if (bodyBlockX != wallBlockX - 1)
                    continue;

                RoomCollisionBlock floor =
                    LevelData.GetCollisionBlock(bodyBlockX, floorBlockY);
                if (floor.CollisionType != 8)
                    continue;

                bool hasPlainClearanceAndWall = true;
                for (int blockY = bodyTopBlockY; blockY <= bodyBottomBlockY; blockY++)
                {
                    RoomCollisionBlock clearance =
                        LevelData.GetCollisionBlock(bodyBlockX, blockY);
                    RoomCollisionBlock wall =
                        LevelData.GetCollisionBlock(wallBlockX, blockY);
                    if (clearance.CollisionType != 0 || wall.CollisionType != 8)
                    {
                        hasPlainClearanceAndWall = false;
                        break;
                    }
                }

                if (!hasPlainClearanceAndWall)
                    continue;

                DebugGroundedSamusPlacement grounded = InitializeDebugGroundedSamus(
                    unchecked((ushort)centerX),
                    desiredScreenY,
                    floorBlockY);
                return new DebugRanIntoWallSamusPlacement(
                    grounded,
                    wallBlockX,
                    bodyTopBlockY,
                    bodyBottomBlockY);
            }
        }

        throw new InvalidOperationException(
            $"No plain type-$8 Landing Site floor/wall corner with type-$0 clearance exists " +
            $"from floor row ${firstFloorRow:X2}.");
    }

    /// <summary>
    /// Creates an explicit already-connected grapple stimulus above the Landing Site floor.
    /// </summary>
    /// <remarks>
    /// The room currently has no translated grapple projectile/block-acquisition pass. This
    /// method supplies only that missing producer's anchor result; pose records, sine table,
    /// pendulum integration, release velocity, animation art, camera, and rendering continue
    /// through cartridge-backed runtime code. It is intentionally named Debug so gameplay
    /// code cannot mistake the host-selected anchor for a room-authored grapple block.
    /// </remarks>
    public DebugGroundedSamusPlacement InitializeDebugGrappleSwing()
    {
        DebugGroundedSamusPlacement placement = InitializeDebugGroundedSamus();
        SamusState samus = Samus!;

        // Angle $4000 points down from this anchor in $94:A957's coordinate convention.
        // A 52-pixel rope keeps the initial body above the selected floor and within the
        // viewport. The nonzero positive angular velocity makes the first frame visibly
        // advance even before the script begins pumping with Left.
        SamusGrappleMovement.ConnectUnobstructedSwing(
            _addressSpace,
            samus,
            anchorX: placement.XPosition,
            anchorY: unchecked((ushort)(placement.YPosition - 72)),
            ropeLength: 52,
            angle: 0x4000,
            angularVelocity: 0x0180,
            faceRight: true);

        // LoadProjectilePalette(2) follows firing initialization at $9B:C51E. Pointer table
        // $90:C3C9 names sixteen bank-$90 colors for OBJ palette seven (CGRAM 224..239).
        // The 65816 stores this pointer little-endian. Keep both byte reads visible here:
        // this debug initializer intentionally has no general ROM-parser dependency, and the
        // explicit expression makes the exact cartridge address easy to inspect in a debugger.
        const int grapplePalettePointerAddress = 0x90c3c9 + 2 * 2;
        ushort grapplePalettePointer = (ushort)(
            _addressSpace.ReadByte(grapplePalettePointerAddress) |
            (_addressSpace.ReadByte(grapplePalettePointerAddress + 1) << 8));
        Cgram.LoadFromBus(
            _addressSpace,
            0x900000 | grapplePalettePointer,
            colorCount: 16,
            destinationIndex: 224);
        Cgram.SetColor(223, 32657);
        LastGrappleMovement = null;
        return placement;
    }

    /// <summary>
    /// Selects grapple as the debug HUD item without fabricating a beam or target. The next
    /// new Shoot edge flows through the pose direction, ROM velocity/origin tables, live
    /// room collision, 128-pixel cutoff, and cancellation logic.
    /// </summary>
    public void EnableDebugGrappleItemSelection()
    {
        if (Samus is null || !GroundedSamusMovementEnabled || LevelData is null)
        {
            throw new InvalidOperationException(
                "Debug grapple selection requires initialized gameplay Samus and room level data.");
        }

        DebugGrappleItemSelected = true;

        // LoadProjectilePalette(2) is part of $9B:C51E firing initialization. The actual
        // HUD selector is not translated, but palette table $90:C3C9 and all sixteen colors
        // remain cartridge policy. CGRAM color 223 is the adjacent fixed beam-flare color.
        const int grapplePalettePointerAddress = 0x90c3c9 + 2 * 2;
        ushort grapplePalettePointer = (ushort)(
            _addressSpace.ReadByte(grapplePalettePointerAddress) |
            (_addressSpace.ReadByte(grapplePalettePointerAddress + 1) << 8));
        Cgram.LoadFromBus(
            _addressSpace,
            0x900000 | grapplePalettePointer,
            colorCount: 16,
            destinationIndex: 224);
        Cgram.SetColor(223, 32657);
    }

    /// <summary>
    /// Runs the complete Samus-side X-ray admission path after the HUD has selected the
    /// equipped scope and observed held Dash, matching `$90:DDD3 -> $91:CAD6 -> $91:E16D`.
    /// </summary>
    /// <remarks>
    /// HUD cursor movement is not yet a general runtime subsystem, so this narrow entry
    /// point represents only the already-selected item. It still requires equipped bit
    /// `$8000` and passes the live bomb cooldown/count, power-bomb phase, current/previous
    /// movement types, vertical velocity, landing-pose gate, and speed divisor to native
    /// setup logic. The DebugRunner uses it without substituting a host pose or art frame.
    /// </remarks>
    public bool TryBeginXrayFromSelectedHudItem(ushort gameState = 8)
    {
        if (Samus is null || !GroundedSamusMovementEnabled || LevelData is null)
        {
            throw new InvalidOperationException(
                "X-ray requires initialized gameplay Samus and room level data.");
        }

        // `$90:C5AE` prevents selecting the HUD item without the scope. The lower-level
        // `$91:E16D` routine assumes that selector has already succeeded.
        if (!Samus.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope))
            return false;

        return Samus.Xray.TryBegin(
            _addressSpace,
            Samus,
            PreviousMovementTypeForXray,
            gameState,
            PowerBombExplosionStatus,
            BombProjectiles.CooldownTimer,
            BombProjectiles.BombCounter);
    }

    /// <summary>
    /// Enters `$9B:B3A7` after the outer death game state has completed its music wait.
    /// Fatal-damage detection, blackout, and queued-music polling are deliberately not
    /// fabricated by this narrow runtime seam.
    /// </summary>
    public SamusDeathSequenceStartResult BeginDeathSequenceAfterMusicWait()
    {
        if (Samus is null || Camera is null || !GroundedSamusMovementEnabled)
        {
            throw new InvalidOperationException(
                "Death sequence requires initialized gameplay Samus and a layer-1 camera.");
        }

        SamusDeathSequenceStartResult result = Samus.DeathSequence.Begin(
            _addressSpace,
            Samus,
            Camera.XPosition,
            Camera.YPosition);

        // The next accepted NMI must see pose `$D7/$D8` definitions. Native game state 15
        // draws once before setup and state 16 draws the new pose on its following call;
        // priming here preserves that same first-visible-frame contract in this thin host.
        Samus.PrimeGraphics(_addressSpace);
        return result;
    }

    /// <summary>
    /// Publishes the exact bank-$88 power-bomb-cleanup attempt to Crystal Flash's native
    /// initiation routine.
    /// </summary>
    /// <remarks>
    /// Normal gameplay now invokes this path automatically from translated bank-$88
    /// cleanup. This public seam remains useful for focused Crystal Flash debugger tests:
    /// callers provide exactly the controller sample that `$88:8B5F` would have observed.
    /// </remarks>
    public bool TryBeginCrystalFlashFromPowerBombCleanup(
        ushort controllerInput,
        ushort shotBinding = (ushort)SnesButton.X,
        bool titleDemo = false)
    {
        if (Samus is null || !GroundedSamusMovementEnabled || LevelData is null)
        {
            throw new InvalidOperationException(
                "Crystal Flash requires initialized gameplay Samus and room level data.");
        }

        return Samus.CrystalFlash.TryBegin(
            _addressSpace,
            Samus,
            controllerInput,
            shotBinding,
            skipInputCheck: titleDemo);
    }

    /// <summary>Initializes the ROM-authored standing pose at an explicitly supplied point.</summary>
    private void InitializeDebugSamus(ushort xPosition, ushort yPosition)
    {
        Samus = new SamusState
        {
            Pose = SamusState.FacingRightNormalPose,
            AnimationFrame = 0,
            XPosition = xPosition,
            YPosition = yPosition,
        };

        Samus.LoadPowerSuitPalette(_addressSpace, Cgram);
        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.InitializeAnimation(_addressSpace);
        if (LandingSiteEntry is not null)
        {
            // FootstepGraphics dispatches on the literal area and room-index bytes. Keep
            // those room-owned inputs beside the FX surface state instead of hard-coding
            // “Landing Site” behavior into the generic atmospheric renderer.
            Samus.LiquidPhysics.AreaIndex = LandingSiteEntry.AreaIndex;
            Samus.LiquidPhysics.RoomIndex = _addressSpace.ReadByte(0x8f91f8);
        }
        PreviousMovementTypeForXray = Samus.ReadMovementType(_addressSpace);

        // StepFrame begins with NMI, so prime the definitions now. Otherwise frame one's
        // OAM would name Samus tiles before any corresponding graphics reached VRAM.
        Samus.PrimeGraphics(_addressSpace);
    }

    /// <summary>
    /// Queues the two number/label tile transfers in the Ceres table at
    /// <c>$A6:C4CB-$A6:C4D8</c>. The following entries are typewriter BG text and are not
    /// needed by the OAM escape-timer renderer.
    /// </summary>
    public void QueueEscapeTimerSpriteTiles()
    {
        VramWrites.Enqueue(sizeInBytes: 0x0200, sourceAddress: 0xb0c000, encodedVramDestination: 0x7e00);
        VramWrites.Enqueue(sizeInBytes: 0x0120, sourceAddress: 0xb0c200, encodedVramDestination: 0x7f00);
    }

    /// <summary>Low-byte NMI frame counter at WRAM <c>$05B5</c>.</summary>
    public byte NmiFrameCounter8 { get; private set; }

    /// <summary>16-bit accepted-NMI frame counter at WRAM <c>$05B6</c>.</summary>
    public ushort NmiFrameCounter { get; private set; }

    /// <summary>Consecutive NMIs for which the main loop had no pending request.</summary>
    public ushort NmiLagCounter { get; private set; }

    /// <summary>Largest observed consecutive lag count.</summary>
    public ushort MaximumNmiLag { get; private set; }

    /// <summary>Counter incremented by every NMI, accepted or lagged.</summary>
    public ushort NmiCounterIncludingLag { get; private set; }

    /// <summary>Runs one accepted NMI followed by the translated per-frame timer logic.</summary>
    /// <param name="drawHighPriorityEnemyProjectiles">
    /// Optional bank-$86 high-priority enemy-projectile draw pass. The generic runtime does
    /// not own a room-specific enemy pool yet, so the Mother Brain debugger injects its
    /// translated pool here while preserving `$A0:885D`'s exact position in global OAM order.
    /// </param>
    /// <param name="drawLowPriorityEnemyProjectiles">
    /// Optional bank-$86 low-priority enemy-projectile draw pass at enemy layer six,
    /// immediately after Samus/projectiles as selected by `$A0:887C`.
    /// </param>
    public RuntimeFrameResult StepFrame(
        ushort controller1Input,
        Action<OamBuffer>? drawHighPriorityEnemyProjectiles = null,
        Action<OamBuffer>? drawLowPriorityEnemyProjectiles = null,
        bool allowCeresElevatorDeparture = true)
    {
        RunNmi(controller1Input, mainLoopRequestedNmi: true);

        // Ridley's `$90:E119` request is issued by room main after Samus movement in the
        // cartridge. The translated Ridley visual currently publishes it during EnemyMain,
        // so promote that pending request only at the next frame boundary. This preserves
        // both the first `$90:E12E` gamma call and the prior frame's ordinary pose input.
        if (Samus is { } frameSamus)
            frameSamus.CeresRidleyEjection.BeginFrame(frameSamus);

        // HDMA object pre-instructions run once near the start of each ordinary gameplay
        // pass. A transformation spawned when the preceding message returned therefore
        // receives its first stage call on this following accepted frame, while every
        // normal owner below continues to run around the input-locked, centered Samus.
        if (SuitPickup.IsActive)
        {
            if (Samus is null)
                throw new InvalidOperationException("A suit transformation has no Samus owner.");
            SuitPickup.Step(_addressSpace, Samus, Cgram);
        }

        // DisplayMessageBox waits on NMI while retaining the already-published room OAM,
        // music, and sound engines. The translated runtime has no audio mixer yet, but it
        // must still latch controller input and block every gameplay owner during those
        // waits. This early seam is shared by all permanent-item identities.
        if (MessageBox.IsActive)
        {
            MessageBox.Step(Controller1.Current);
            if (MessageBox.IsActive)
                return Snapshot(escapeTimerExpired: false);

            // The final zero-radius close NMI returns directly to the suspended item-PLM
            // instruction list. Varia/Gravity immediately call their shared setup routine;
            // all other items simply continue the rest of this gameplay pass.
            if (_pendingSuitPickup is { } pendingSuit)
            {
                if (Samus is null || Camera is null)
                {
                    throw new InvalidOperationException(
                        "A pending suit transformation requires an active Samus and room camera.");
                }
                ElevatorStatus = 0;
                SuitPickup.Begin(
                    _addressSpace,
                    Samus,
                    Camera.XPosition,
                    Camera.YPosition,
                    pendingSuit);
                _pendingSuitPickup = null;
            }
        }

        // `$0B14/$0B16` retain the unsigned horizontal distance accepted during the prior
        // gameplay frame. Capture the live fixed-point origin before enemies and bank $90
        // run; the tail below publishes the modular absolute delta for next frame's Yard
        // kick calculation.
        uint samusXAtFrameStart = Samus is null
            ? 0
            : ((uint)Samus.Kinematics.XPosition << 16) | Samus.Kinematics.XSubposition;

        // MainScrollingRoutine compares Samus's final coordinates with the position saved
        // at the beginning of the gameplay pass, not merely with the position immediately
        // before bank-$90's beta handler. This distinction is normally invisible because
        // ordinary Samus movement happens in beta. The post-Ceres gunship, however, moves
        // locked Samus during EnemyMain. Capturing all four words here lets the same generic
        // scrolling routine follow that cartridge-authored movement instead of allowing the
        // descending ship to leave the enemy scheduler's on-screen processing window.
        SamusCameraPoint? samusCameraPointAtFrameStart = Samus is null
            ? null
            : new SamusCameraPoint(
                Samus.XPosition,
                Samus.Kinematics.XSubposition,
                Samus.YPosition,
                Samus.Kinematics.YSubposition);

        // Gameplay state eight calls `$8D:C527` before `$91:8000` dispatches Samus and
        // before `$A0:868F` processes enemies. Controller function three is invoked by the
        // Baby during that later enemy phase, so a newly spawned `$E1F0` object naturally
        // begins on the following frame. Debug scripts that grant Hyper Beam before their
        // first StepFrame get the same handler-first behavior immediately.
        LastHyperBeamPaletteFxStep = Samus?.Drained.HyperBeamPaletteFx.Step(
            _addressSpace,
            Cgram);

        // The bank-$82 main loop clears high OAM and resets its stack before dispatching
        // game state, then finalizes unused entries afterward. Samus is emitted before the
        // escape timer so lower OAM indices retain their normal overlap precedence.
        Oam.BeginFrame();
        LastSamusBodyDrawn = false;
        LastShinesparkCrashDrawingHandlerActive = false;
        LastGrappleDrawingHandlerActive = false;
        LastGrappleBeamSpecificDrawingPath = false;
        LastGrappleFlareDrawn = false;
        bool escapeTimerExpired = EscapeTimer.Process(NmiFrameCounter);

        // GameState_8 selects active enemies and executes EnemyMain before bank $90 moves
        // Samus. The collision index list is selected from pre-AI positions but consumers
        // dereference the post-AI slot words, which RoomEnemySystem publishes as bodies.
        if (Camera is not null && Enemies.IsLoaded)
        {
            // Samus's bank-$94 collision phase follows EnemyMain. A pseudo-door contact
            // published last frame therefore becomes `$0E16=1` immediately before this
            // frame's elevator actor dispatcher, preserving the native producer order.
            if (LevelData?.ConsumeElevatorDoorContact() == true)
                Enemies.PublishElevatorDoorContact();
            if (Samus is not null)
            {
                // `$94:9B60-$9B72` clears all four external-displacement words before
                // EnemyMain. Rideable enemies then accumulate accepted platform deltas and
                // bank `$90` consumes those live values during Samus movement below. They
                // are producer-owned words, so movement deliberately does not clear them.
                Samus.Kinematics.ExtraXSubdisplacement = 0;
                Samus.Kinematics.ExtraXDisplacement = 0;
                Samus.Kinematics.ExtraYSubdisplacement = 0;
                Samus.Kinematics.ExtraYDisplacement = 0;
            }
            Enemies.StepFrame(
                Camera.XPosition,
                Camera.YPosition,
                Samus?.Xray.TimeIsFrozen ?? false,
                Samus,
                Controller1.NewlyPressed,
                LevelData,
                Controller1.Current,
                Projectiles,
                NmiFrameCounter8,
                ActiveSamusMode7Transform,
                BombProjectiles,
                VramWrites);
            if (Enemies.CeresEscapeStartedThisFrame)
            {
                // $A6:C117 publishes these global side effects on the same EnemyMain call
                // that changes ceres_status from one to two. Keep the actor as producer,
                // but apply timer and boss state in their existing runtime-owned systems.
                EscapeTimer.RequestCeresStart();
                if (ActiveRoom is null)
                    throw new InvalidOperationException("Ceres escape started without an active room.");
                System.SetBossBits(ActiveRoom.AreaIndex, BossBits.AreaBoss);
            }
            if (Enemies.RequestedShitroidCameraX is ushort shitroidCameraX)
            {
                // `$A9:EFE6` writes layer1_x_pos during EnemyMain, before the ordinary
                // scrolling routine later in this frame. Do not route it through entry
                // placement, which would clear subposition and ideal-camera state.
                Camera.SetLayerOneXFromEnemyAi(shitroidCameraX);
                BackgroundScroll.Layer1XPosition = shitroidCameraX;
            }
            if (Enemies.ElevatorClearedProjectileData)
            {
                // `$90:ADB7` clears all ten projectile slots and their counters. Ordinary
                // beam/missile slots were reset inside the actor call; bombs and the shared
                // cooldown live in this companion owner and complete that same operation.
                BombProjectiles.Reset();
            }
            // `$A6:A2DF` does not install the post-enemy hook until Ridley's animation word
            // becomes nonzero. Before the reveal it branches directly into `$A6:A2E3`
            // during EnemyMain, so emit the Baby/door OBJ now—before queued enemy layers.
            Enemies.DrawCeresRidleyImmediateBabyAndDoor(
                Oam,
                Camera.XPosition,
                Camera.YPosition);
            if (Samus is not null && !(Samus.Xray.TimeIsFrozen))
            {
                Enemies.ResolveRidleySamusContact(
                    Samus,
                    Controller1.Current);
                Enemies.ResolveOrdinarySamusContact(
                    Samus,
                    Controller1.Current,
                    LevelData);
            }
            if (LevelData is not null && !(Samus?.Xray.TimeIsFrozen ?? false))
                Enemies.StepEnemyProjectiles(
                    LevelData,
                    Samus,
                    Controller1.Current,
                    Camera.XPosition,
                    Camera.YPosition,
                    NmiFrameCounter8,
                    BombProjectiles);
            if (Samus is not null)
            {
                Samus.Kinematics.InteractiveEnemies = Enemies.InteractiveCollisionBodies;
                if (Enemies.LastGunshipEvent == GunshipFrameEvent.EntryStarted)
                {
                    // MakeSamusFaceForward performs a direct suit-palette reload and the
                    // ship explicitly clears elevator status after installing locked demo
                    // handlers. The enemy system owns pose/motion; these two global words
                    // remain runtime-owned and are applied on its typed event boundary.
                    Samus.LoadSuitPalette(_addressSpace, Cgram);
                    ElevatorStatus = 0;
                }
                else if (Enemies.LastGunshipEvent == GunshipFrameEvent.LandingCompleted)
                {
                    // `$A2:A987` restores the ordinary Samus handler pair only after the
                    // closing-pad hold. Enemy AI has already unlocked Samus.InputLocked;
                    // publish the runtime's matching movement gate at the same boundary.
                    GroundedSamusMovementEnabled = true;
                }
            }
        }
        if (Samus is not null && Camera is not null)
        {
            // `$90:E725` clears contact-damage index before dispatching beta movement.
            // Speed Booster stage four (or later spin/shinespark families) must republish
            // its value on every applicable frame; stale contact damage cannot leak onward.
            Samus.HorizontalSpeed.ContactDamageIndex = 0;

            // X-ray's HDMA object is not part of the Samus handler. Advance its explicit
            // setup/beam/deactivation state before alpha so state five can restore the
            // ordinary pose and handler pair before this frame samples pose input. The
            // revealed-block BG2 copies/window table remain a separate renderer boundary.
            LastXrayBeamStep = null;
            LastXrayPoseInput = null;
            LastDeathSequenceStep = null;
            if (Samus.Xray.IsActive && !Samus.DeathSequence.IsActive)
            {
                LastXrayBeamStep = Samus.Xray.StepBeam(
                    _addressSpace,
                    Samus,
                    Controller1.Current);
            }

            // The beginning-of-pass sample above precedes both enemy-owned motion and the
            // bank-$90 frame handler. It therefore serves ordinary motion and scripted
            // carriers through one native camera path.
            SamusCameraPoint previousCameraPoint = samusCameraPointAtFrameStart!.Value;

            // Retain the dispatch pose because command $F8 can replace Samus.Pose during
            // animation later in this same frame. Native alpha/beta/transition phases all
            // agree on that order; using the mutable value afterward would apply an input
            // match selected for the old pose to the newly installed one.
            byte poseAtFrameStart = Samus.Pose;
            byte movementTypeAtFrameStart = Samus.ReadMovementType(_addressSpace);

            // Direction bits already use the transition table's canonical layout. Input
            // matching belongs to frame-handler alpha, before beta moves the CURRENT pose;
            // the winning pose is not installed until after animation below.
            // `$91:804D-$8065` gives front-view `$00/$9B` one special input rule: a
            // nonzero elevator status returns immediately, before the transition-table
            // pointer is even read. Pose zero's pointer bytes otherwise overlap legitimate
            // data and can appear to select nonsense transitions after several frames, so
            // suppressing only their application would be too late and observably wrong.
            bool deathOwnsSamus = Samus.DeathSequence.IsActive;
            // Sound queues are global persistent engines on hardware, but this typed host
            // publication is scoped to one Samus handler. Begin before movement because
            // `$91:F046` queues landing sounds during collision, before AnimateSamus.
            Samus.LiquidPhysics.BeginFrameSoundRequests();
            bool xrayOwnsPoseInput = Samus.Xray.IsActive && !deathOwnsSamus;
            if (xrayOwnsPoseInput)
            {
                LastXrayPoseInput = Samus.Xray.HandlePoseInput(
                    _addressSpace,
                    Samus,
                    Controller1.Current);
            }

            bool actorLocksPoseInput = Samus.InputLocked ||
                (SamusState.IsForwardFacingPose(Samus.Pose) && ElevatorStatus != 0);
            ProspectiveSamusPose = deathOwnsSamus || xrayOwnsPoseInput || actorLocksPoseInput
                ? null
                : SamusPoseTransitionTable.Find(
                    _addressSpace,
                    Samus.Pose,
                    Controller1.Current,
                    Controller1.NewlyPressed);
            ProspectiveSamusFallbackPose = null;
            ProspectiveSamusWallCollisionPose = null;
            LastRanIntoWallProbe = null;

            // Bomb overlap is published by GameState_8 after the preceding frame's alpha.
            // $90:DE78 consumes it during this frame's alpha before beta dispatches motion.
            // This one-frame seam is observable: timer eight does not move Samus yet.
            Samus.TrySetupPublishedBombJump(
                _addressSpace,
                LevelData ?? throw new InvalidOperationException(
                    "A published bomb-jump direction requires active room level data."),
                timeIsFrozen: Samus.Xray.TimeIsFrozen,
                nmiFrameCounter: NmiFrameCounter);

            // With no controller bits, $91:82D9 consults pose-definition byte two. Running
            // poses $09/$0A store fallbacks $01/$02, but Samus_Pose_Func2 first preserves
            // the running pose while base speed is nonzero and selects momentum routine
            // one (deceleration). Capture this before movement, where native alpha does.
            if (GroundedSamusMovementEnabled &&
                (SamusState.IsRightFacingRunningPose(Samus.Pose) ||
                 SamusState.IsLeftFacingRunningPose(Samus.Pose)) &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                ProspectiveSamusFallbackPose = Samus.HorizontalSpeed.BaseFixed != 0
                    ? Samus.Pose
                    : Samus.ReadNoInputFallbackPose(_addressSpace);
            }

            // Grounded rolling poses `$1E/$1F` use the same momentum-command-one seam as
            // ordinary running: release keeps the moving pose while base speed remains,
            // then definition byte two selects stable `$1D/$41` at exact zero.
            if (GroundedSamusMovementEnabled &&
                (Samus.Pose is SamusState.MorphBallMovingRightPose or
                    SamusState.MorphBallMovingLeftPose or
                    SamusState.SpringBallMovingRightPose or
                    SamusState.SpringBallMovingLeftPose) &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                ProspectiveSamusFallbackPose = Samus.HorizontalSpeed.BaseFixed != 0
                    ? Samus.Pose
                    : Samus.ReadNoInputFallbackPose(_addressSpace);
            }

            // Pose-definition byte two returns standing aim `$03-$08` to `$01/$02` and
            // crouched aim `$71-$74/$85/$86` to `$27/$28`. This path is separate from the
            // transition table: `$91:81A9` exits before reading a record when the entire
            // controller word is zero, then `$91:82D9` installs the definition fallback.
            if (GroundedSamusMovementEnabled &&
                ((Samus.Pose is
                      SamusState.StandingAimUpRightPose or
                      SamusState.StandingAimUpLeftPose or
                      SamusState.StandingAimDiagonalUpRightPose or
                      SamusState.StandingAimDiagonalUpLeftPose or
                      SamusState.StandingAimDiagonalDownRightPose or
                      SamusState.StandingAimDiagonalDownLeftPose) ||
                 SamusState.IsAimedCrouchingPose(Samus.Pose)) &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                ProspectiveSamusFallbackPose = Samus.ReadNoInputFallbackPose(_addressSpace);
            }

            // Movement type `$10` uses prospective command two, not running's deceleration
            // command one. With the entire controller released, `$91:82D9` therefore reads
            // definition byte two immediately: `$49/$75/$77 -> $02/$06/$08`, mirrored to
            // `$01/$05/$07`. The resulting standing body keeps the current X speed words;
            // the following standing movement frame clears them exactly as native does.
            if (GroundedSamusMovementEnabled &&
                SamusState.IsMoonwalkingPose(Samus.Pose) &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                ProspectiveSamusFallbackPose = Samus.ReadNoInputFallbackPose(_addressSpace);
            }

            // `$CF-$D2` use definition byte two to return to `$89/$8A` when the entire
            // controller is released. The unaimed pair store `$FF`, meaning “keep pose,”
            // and therefore never publish a fallback here.
            if (GroundedSamusMovementEnabled &&
                SamusState.IsAimedRanIntoWallPose(Samus.Pose) &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                ProspectiveSamusFallbackPose = Samus.ReadNoInputFallbackPose(_addressSpace);
            }

            // Active aimed jump/fall poses use the same no-controller pose-definition
            // fallback seam: `$15/$69/$6B -> $51`, mirrored left to `$52`, and aimed
            // falling to `$29/$2A`. Transition poses `$55-$5A` store `$FF` and are left
            // to their `$FD` animation command instead.
            if (GroundedSamusMovementEnabled &&
                SamusState.IsAimedAerialPose(Samus.Pose) &&
                Samus.Pose is not (
                    SamusState.NormalJumpTransitionAimUpRightPose or
                    SamusState.NormalJumpTransitionAimUpLeftPose or
                    SamusState.NormalJumpTransitionAimDiagonalUpRightPose or
                    SamusState.NormalJumpTransitionAimDiagonalUpLeftPose or
                    SamusState.NormalJumpTransitionAimDiagonalDownRightPose or
                    SamusState.NormalJumpTransitionAimDiagonalDownLeftPose) &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                byte fallback = Samus.ReadNoInputFallbackPose(_addressSpace);
                // Compact straight-down `$17/$18/$2D/$2E` store `$FF`, so `$91:82D9`
                // leaves them unchanged when all input is released. The other admitted
                // aimed bodies publish real `$29/$2A/$51/$52` fallback poses.
                if (fallback != 0xff)
                    ProspectiveSamusFallbackPose = fallback;
            }

            // Wall-jump records `$83/$84` use definition fallback `$19/$1A` when the
            // controller is fully released. Like every definition fallback this is sampled
            // in alpha and committed only after beta movement and animation below.
            if (GroundedSamusMovementEnabled &&
                SamusState.IsWallJumpPose(Samus.Pose) &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                ProspectiveSamusFallbackPose = Samus.ReadNoInputFallbackPose(_addressSpace);
            }

            // `$BB-$BE/$ED-$F0` store their neutral same-facing pose in definition byte
            // two. `$BA/$EC` store `$FF`, so a completely released controller keeps them.
            // This is sampled in alpha even though Draygon's installed movement handler is
            // RTS and the enemy actor owns world position later in the gameplay frame.
            if (GroundedSamusMovementEnabled &&
                SamusState.IsDraygonGrabbedPose(Samus.Pose) &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                byte fallback = Samus.ReadNoInputFallbackPose(_addressSpace);
                if (fallback != 0xff)
                    ProspectiveSamusFallbackPose = fallback;
            }

            if (GroundedSamusMovementEnabled)
            {
                if (LevelData is null)
                    throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

                // Frame-handler alpha calls `$90:9C5B` after pose input/radius setup and
                // before beta movement. Refreshing the split gravity words every frame is
                // essential at a liquid surface: a jump can cross from water to air without
                // any pose transition that would otherwise reinitialize acceleration.
                SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, Samus);

                // Alpha order is cooldown -> movement-type HUD projectile producer ->
                // HandleProjectile. The outer gameplay loop then runs bank-$A0 overlap
                // before beta movement. A newly placed bomb therefore counts 60 -> 59 and
                // selects its first bank-$93 art record in the placement frame itself.
                if (!Samus.Xray.TimeIsFrozen && !deathOwnsSamus)
                {
                    // `$90:C4E7` runs before the movement-type HUD projectile producer.
                    // Consequently a Select edge can choose missiles and an X edge can
                    // fire one during this same alpha pass. Input-locked message/elevator
                    // handlers do not execute the normal selection owner.
                    if (!Samus.InputLocked && Samus.HandleHudSelection(
                            Controller1.Current,
                            Controller1.NewlyPressed))
                    {
                        Projectiles.CancelChargeForHudSelection();
                        Samus.ProjectileFlareCounter = 0;
                    }

                    // `$91:E231-$E23D` disables enemy projectiles, PLMs, animated tiles,
                    // and palette FX while time is frozen. Normal bombs cannot be placed or
                    // advanced through this translated producer during the X-ray interval.
                    BombProjectiles.StepFrame(
                        _addressSpace,
                        LevelData,
                        Samus,
                        Controller1.Current,
                        Controller1.NewlyPressed,
                        Plms);

                    // The bomb half above has already decremented shared cooldown $0CCC.
                    // `$90:DD31` then dispatches the humanoid HUD producer before the same
                    // HandleProjectile pass. Passing the live layer-1 position also gives
                    // `$90:B16A` its exact off-screen deletion window.
                    Projectiles.StepFrame(
                        _addressSpace,
                        LevelData,
                        Samus,
                        Controller1.Current,
                        Controller1.NewlyPressed,
                        Camera.XPosition,
                        Camera.YPosition,
                        BombProjectiles,
                        projectileProducerEnabled: !DebugGrappleItemSelected,
                        roomPlms: Plms);

                    // Bank-$A0's enemy collision index list was selected before EnemyMain;
                    // bank-$90 has now advanced the projectile slots, and the enemy-specific
                    // shot handler owns the overlap result. Ceres Ridley deliberately counts
                    // contacts instead of losing his pinned $7FFF health word.
                Enemies.ResolveCeresRidleyProjectileHits(
                    _addressSpace,
                    Projectiles,
                    BombProjectiles);
                Enemies.ResolveKraidProjectileHits(
                    _addressSpace,
                    Projectiles,
                    BombProjectiles);
                Enemies.ResolvePhantoonProjectileHits(
                    _addressSpace,
                    Projectiles,
                    BombProjectiles);
                Enemies.ResolveEnemyProjectileSamusProjectileHits(
                    _addressSpace,
                    Projectiles,
                    BombProjectiles);
                Enemies.ResolveOrdinaryProjectileHits(
                    _addressSpace,
                    Projectiles,
                    BombProjectiles,
                    Samus);

                // `$A0:A236` is not part of the five-slot beam/missile collision walk.
                // It scans physical bomb slots five through nine after their bank-$90
                // update and dispatches each overlapping enemy's shot AI only once the
                // bomb's fuse/variable word is zero. This is the shared normal-bomb damage
                // route as well as the special family-$0500 Metroid detach path.
                Enemies.ResolveOrdinaryBombHits(
                    BombProjectiles,
                    Projectiles,
                    Samus);

                // GameState_8 calls `$A0:A306` every gameplay frame after the enemy-
                // projectile passes. The native routine reads the high byte of live WRAM
                // radius `$0CEE`; a zero byte is its own inactive/pre-explosion guard.
                // Invincibility timers inside the enemy pass prevent the persistent
                // expansion/afterglow radius from damaging one actor every frame.
                SamusPowerBombExplosionState powerBomb =
                    BombProjectiles.PowerBombExplosion;
                Enemies.ResolveOrdinaryPowerBombHits(
                    _addressSpace,
                    powerBomb.XPosition,
                    powerBomb.YPosition,
                    unchecked((byte)(powerBomb.ExplosionRadius >> 8)),
                    Samus);

                    // `$90:E6C0` dispatches the selected HUD producer and `$90:EB20`
                    // immediately clears `$0B5E`. Pose initialization occurs later in the
                    // new-state handler, so a bridge published below survives precisely
                    // until this point in the following gameplay frame.
                    Samus.ClearPoseTransitionShotDirection();

                    // `$0CD0` is one shared WRAM word, not independent projectile/movement
                    // state. Samus's spin/wall-jump contact-damage handlers run in beta and
                    // must see the counter that `$90:B80D` just updated during alpha.
                    Samus.ProjectileFlareCounter = Projectiles.FlareCounter;
                }

                // Native HandleProjectile runs `$90:D4D2` during alpha, before Samus's beta
                // movement handler. Departing crash echoes therefore remain centered on the
                // position at the start of this frame and are not advanced on their spawn
                // frame. Their viewport test uses the live layer-1 camera exactly as the
                // fixed projectile slots do.
                if (!deathOwnsSamus)
                {
                    Samus.Shinespark.StepReleasedCrashEchoProjectiles(
                        _addressSpace,
                        Samus,
                        Camera.XPosition,
                        Camera.YPosition);
                }

                // $90:E725 dispatches movement type before animation. Every admitted pose
                // below has its own verified direction/mode path; a newly reachable pose
                // cannot accidentally inherit generic standing or running physics.
                LastGroundedSamusMovement = null;
                LastAerialSamusMovement = null;
                LastMorphBallMovement = null;
                LastBombJumpMovement = null;
                LastKnockbackMovement = null;
                LastCeresRidleyEjection = null;
                LastGrappleMovement = null;
                LastShinesparkMovement = null;
                LastCrystalFlashMovement = null;
                LastXrayAnimationFrame = null;
                // X-ray publishes its pose-input and beam results above because both run
                // outside the normal movement-type dispatcher. Its movement result is the
                // directly inspectable frame returned by `StepMovement` below.
                LastDrainedSamusMovement = null;
                LastDraygonGrabbedMovement = null;
                LastDraygonEscape = null;

                // GrappleBeamHandler precedes beta movement, but only connected/release
                // functions own Samus's position. An extending or cancelling beam coexists
                // with the current pose's ordinary movement in the same frame.
                bool grappleOwnsMovement = false;
                if (deathOwnsSamus)
                {
                    // Game states `$15-$18` do not call the ordinary Samus alpha/beta
                    // handlers. The death state advances later at the animation seam.
                }
                else if (Samus.DraygonGrabbed.IsActive)
                {
                    // `$90:E23B` installs an RTS movement-handler pointer. Bank `$A5`
                    // may already have called ApplyOwnerPosition this actor frame; no bank-
                    // `$90` motion or collision is allowed to alter that placement here.
                }
                else if (Samus.Grapple.Phase == GrapplePhase.Firing)
                {
                    LastGrappleMovement = SamusGrappleMovement.StepFiring(
                        _addressSpace,
                        LevelData,
                        Samus,
                        Controller1.Current,
                        Plms,
                        Enemies.ResolveGrappleEndpoint);
                    grappleOwnsMovement = LastGrappleMovement.Value.OwnsMovement;
                }
                else if (Samus.Grapple.Phase == GrapplePhase.CancelPending)
                {
                    LastGrappleMovement =
                        SamusGrappleMovement.CompleteFiringCancellation(_addressSpace, Samus);
                    grappleOwnsMovement = LastGrappleMovement.Value.OwnsMovement;
                }
                else if (Samus.Grapple.Phase is
                    GrapplePhase.ConnectedSwinging or GrapplePhase.ReleaseFromSwing or
                    GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab or
                    GrapplePhase.WallGrabRelease or GrapplePhase.WallJumping or
                    GrapplePhase.Dropped)
                {
                    LastGrappleMovement = SamusGrappleMovement.Step(
                        _addressSpace,
                        LevelData,
                        Samus,
                        Controller1.Current,
                        Controller1.NewlyPressed,
                        NmiFrameCounter,
                        Enemies.ResolveGrappleEndpoint);
                    grappleOwnsMovement = true;
                }
                else if (DebugGrappleItemSelected &&
                         (Controller1.NewlyPressed & (ushort)SnesButton.X) != 0)
                {
                    // This is the one untranslated producer seam: the real HUD item index
                    // would choose GrappleBeamHandler instead of ordinary beam/bomb fire.
                    // Everything after selection is the bank-$9B/$94 implementation.
                    SamusGrappleMovement.BeginFiring(_addressSpace, Samus);
                    LastGrappleMovement = new GrappleMovementResult(
                        GrapplePhase.Firing,
                        Released: false,
                        ReleaseQueued: false,
                        Fired: true,
                        Connected: false,
                        CancelQueued: false,
                        Cancelled: false,
                        OwnsMovement: false);
                }

                // `$9B:C4B1-$C4EA` runs after every grapple function, including inactive.
                // Swing calculations above therefore consumed last frame's bit; this write
                // publishes the current bottom-boundary result for the next frame exactly
                // where the native bank-$9B handler does.
                SamusGrappleMovement.RefreshLiquidPhysicsFlag(Samus);

                if (Samus.Grapple.ReleasedMovementActive)
                {
                    // `$9B:C7C1` replaces the normal beta movement-handler pointer on the
                    // release-input frame. It remains independent of the beam function, so
                    // it must also run after `$9B:CB8B` has made that function inactive.
                    SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, Samus);
                    LastAerialSamusMovement = SamusAerialMovement.StepReleasedFromGrapple(
                        _addressSpace,
                        LevelData,
                        Samus,
                        Controller1.Current,
                        NmiFrameCounter,
                        Plms);
                    grappleOwnsMovement = true;
                }

                if (LastGrappleMovement is
                    { CameraPreviousX: ushort grapplePreviousX,
                      CameraPreviousY: ushort grapplePreviousY })
                {
                    // `$9B:BAD5-$BB5F` clamps native previous-position words to twelve
                    // pixels after a close-collision snap. Feed those corrected whole words
                    // to bank-$90 tracking while preserving the pre-frame subpositions.
                    previousCameraPoint = new SamusCameraPoint(
                        grapplePreviousX,
                        previousCameraPoint.XSubposition,
                        grapplePreviousY,
                        previousCameraPoint.YSubposition);
                }

                if (deathOwnsSamus)
                {
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;
                }
                else if (grappleOwnsMovement)
                {
                    // Movement type $16's beta handler is empty. Discard input transitions
                    // sampled from the pre-grapple pose once connection installs $B2/$B3.
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;
                }
                // The real grabbed route uses the explicit RTS handler installed at
                // `$90:E262`, not the otherwise reachable type-$1A normal dispatcher. Keep
                // the latter in the pose switch below for direct/diagnostic dispatcher use.
                else if (Samus.DraygonGrabbed.IsActive)
                {
                    // Deliberately empty: Draygon's bank-$A5 actor owns coordinates.
                }
                // Special prospective-pose command five installs `$90:E94F` instead of the
                // normal movement-type dispatcher. Stable X-ray bodies select one of five
                // angle frames; type-`$0E` turning deliberately performs no beta movement.
                else if (Samus.Xray.IsActive)
                {
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;
                    LastXrayAnimationFrame = Samus.Xray.StepMovement(_addressSpace, Samus);
                }
                // `$90:E119` installs a separate movement/gamma-handler pair for Ceres
                // Ridley's retreat. It must precede ordinary damage knockback because pose
                // `$53/$54` is shared while ownership, duration, and termination are not.
                else if (Samus.CeresRidleyEjection.IsActive)
                {
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;
                    LastCeresRidleyEjection = Samus.CeresRidleyEjection.Step(
                        _addressSpace,
                        LevelData,
                        Samus,
                        Camera.XPosition,
                        NmiFrameCounter);
                }
                // Knockback's `$90:DF38` handler takes precedence over the normal movement-
                // type dispatcher. Unlike bomb jump, normal pose input remains active so
                // `$53/$54` can still select the retail damage-boost escape chord.
                else if (Samus.KnockbackActive)
                {
                    SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, Samus);
                    LastKnockbackMovement = SamusKnockbackMovement.Step(
                        _addressSpace,
                        LevelData,
                        Samus,
                        NmiFrameCounter);
                }
                // Special command three replaces both movement and pose-input handlers.
                // Preserve the current ball pose/animation and ignore any transition that
                // the normal matcher calculated earlier in this host frame.
                else if (Samus.BombJumpStarting || Samus.BombJumpActive)
                {
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;

                    // The shared alpha environment pass above already selected these words;
                    // repeat the explicit special-handler publication here to preserve the
                    // old diagnostic seam while allowing water/lava instead of forcing air.
                    SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, Samus);
                    LastBombJumpMovement = Samus.BombJumpStarting
                        ? SamusBombJumpMovement.Start(_addressSpace, Samus)
                        : SamusBombJumpMovement.Step(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter);
                }
                // Drained controller functions install movement-type-$1B poses whose normal
                // beta dispatcher is RTS. Only animation command `$F7` replaces the handler
                // with `$90:94CB`; the other phases stay motionless while enemy AI controls
                // pose/timing. All active drain phases retain the locked-input contract.
                else if (Samus.Drained.Phase != DrainedSamusPhase.Inactive)
                {
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;
                    if (Samus.Drained.Phase == DrainedSamusPhase.Falling)
                    {
                        LastDrainedSamusMovement = Samus.Drained.StepFalling(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter);
                    }
                }
                // `$90:D5A2` installs one of three Crystal Flash movement pointers and an
                // RTS pose-input handler. Clear the transition sampled at the top of this
                // host frame: normal input is not allowed to interrupt `$D3/$D4` art.
                else if (Samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive)
                {
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;
                    LastCrystalFlashMovement = Samus.CrystalFlash.Step(
                        _addressSpace,
                        Samus,
                        NmiFrameCounter);
                    if (LastCrystalFlashMovement.Value.PhaseAtStart == CrystalFlashPhase.Raising &&
                        LastCrystalFlashMovement.Value.BubbleHdmaRequested)
                    {
                        // `$90:D6AE` clears `$0CEA`, copies Samus's raised center into the
                        // shared explosion words, then spawns `$88:A2BD/$A32A`. This beta
                        // pass occurs after bomb alpha, so the retained type-`$0300` slot
                        // observes the released flag and deletes on the following frame.
                        BombProjectiles.PowerBombExplosion.BeginCrystalFlash(
                            Samus.XPosition,
                            Samus.YPosition);
                    }
                }
                // `$90:CFFA` replaces the normal movement-handler pointer. Windup, active
                // launch, and crash therefore own beta movement regardless of the pose's
                // table index; ordinary type-$1B physics does not exist to fall back to.
                else if (Samus.Shinespark.Phase is
                    ShinesparkPhase.Windup or ShinesparkPhase.Horizontal or
                    ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal or
                    ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle or
                    ShinesparkPhase.CrashFinish)
                {
                    // Determine_Samus_YAcceleration supplies the environment-selected pair
                    // used as spark acceleration. Projectile_Func7 does not replace it.
                    SamusAerialMovement.ConfigureEnvironmentGravity(_addressSpace, Samus);
                    LastShinesparkMovement = Samus.Shinespark.Step(
                        _addressSpace,
                        LevelData,
                        Samus,
                        NmiFrameCounter,
                        BombProjectiles.BombCounter,
                        Plms);
                    if (LastShinesparkMovement.Value.WindupTimedOut)
                    {
                        // The movement handler publishes an interrupted vertical pose. That
                        // priority beats an ordinary input record sampled from `$C7/$C8`
                        // earlier in alpha during the timeout frame.
                        ProspectiveSamusPose = null;
                        ProspectiveSamusFallbackPose = null;
                    }
                }
                // SamusCode_00 installs an alpha handler which advances existing
                // projectiles but a beta handler which performs no movement at all. All
                // translated special movement owners above take priority; an otherwise
                // locked standing body must not run generic ground collision or drift while
                // state $20 waits at the Ceres elevator.
                else if (Samus.InputLocked &&
                    !(SamusState.IsForwardFacingPose(Samus.Pose) && ElevatorStatus != 0))
                {
                    // Controller-locked commands normally install beta no-op. Ordinary
                    // elevators are the sole exception: every nonzero `$0E18` status
                    // selects the forward-facing `$90:A392` terrain scan. Destination-side
                    // statuses two/three need it to wake scroll triggers while input remains
                    // locked; only their duplicate pseudo-door publication is suppressed.
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;
                }
                else switch (Samus.Pose)
                {
                    case SamusState.ForwardFacingPowerSuitPose:
                    case SamusState.ForwardFacingSuitedPose:
                        // `$00/$9B` enter through controller-locked demo/elevator commands.
                        // The actor-owned `$0E18` word remains an explicit producer, while
                        // this consumer now includes both the zero-status no-op and nonzero
                        // `$94:9763` one-pixel downward block scan.
                        SamusGroundedMovement.StepFacingForward(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            elevatorIsMoving:
                                Enemies.ElevatorStatus != ElevatorActorStatus.Inactive,
                            plms: Plms,
                            publishDoorSideEffects:
                                ShouldPublishFacingForwardElevatorDoorSideEffects(
                                    Enemies.ElevatorStatus));
                        break;
                    case SamusState.FacingRightNormalPose:
                    case SamusState.StandingAimUpRightPose:
                    case SamusState.StandingAimDiagonalUpRightPose:
                    case SamusState.StandingAimDiagonalDownRightPose:
                        LastGroundedSamusMovement = SamusGroundedMovement.StepStandingRight(
                        _addressSpace,
                        LevelData,
                        Samus,
                        NmiFrameCounter,
                        Plms);
                        break;
                    case SamusState.MovingRightNormalPose:
                    case SamusState.MovingRightGunExtendedPose:
                    case SamusState.RunningAimUpRightPose:
                    case SamusState.RunningAimDiagonalUpRightPose:
                    case SamusState.RunningAimDiagonalDownRightPose:
                        LastGroundedSamusMovement = SamusGroundedMovement.StepRunningRight(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Controller1.Current,
                            Plms);
                        break;
                    case SamusState.FacingLeftNormalPose:
                    case SamusState.StandingAimUpLeftPose:
                    case SamusState.StandingAimDiagonalUpLeftPose:
                    case SamusState.StandingAimDiagonalDownLeftPose:
                        LastGroundedSamusMovement = SamusGroundedMovement.StepStandingLeft(
                        _addressSpace,
                        LevelData,
                        Samus,
                        NmiFrameCounter,
                        Plms);
                        break;
                    case SamusState.MovingLeftNormalPose:
                    case SamusState.MovingLeftGunExtendedPose:
                    case SamusState.RunningAimUpLeftPose:
                    case SamusState.RunningAimDiagonalUpLeftPose:
                    case SamusState.RunningAimDiagonalDownLeftPose:
                        LastGroundedSamusMovement = SamusGroundedMovement.StepRunningLeft(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Controller1.Current,
                            Plms);
                        break;
                    case SamusState.MoonwalkFacingLeftPose:
                    case SamusState.MoonwalkFacingRightPose:
                    case SamusState.MoonwalkAimUpLeftPose:
                    case SamusState.MoonwalkAimUpRightPose:
                    case SamusState.MoonwalkAimDownLeftPose:
                    case SamusState.MoonwalkAimDownRightPose:
                        LastGroundedSamusMovement = SamusGroundedMovement.StepMoonwalking(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.RanIntoWallRightPose:
                    case SamusState.RanIntoWallLeftPose:
                    case SamusState.RanIntoWallAimUpRightPose:
                    case SamusState.RanIntoWallAimUpLeftPose:
                    case SamusState.RanIntoWallAimDownRightPose:
                    case SamusState.RanIntoWallAimDownLeftPose:
                        LastGroundedSamusMovement = SamusGroundedMovement.StepRanIntoWall(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.TurningRightToLeftPose:
                    case SamusState.TurningLeftToRightPose:
                    case SamusState.TurningRightToLeftAimUpPose:
                    case SamusState.TurningLeftToRightAimUpPose:
                    case SamusState.TurningRightToLeftAimDiagonalUpPose:
                    case SamusState.TurningLeftToRightAimDiagonalUpPose:
                    case SamusState.TurningRightToLeftAimDiagonalDownPose:
                    case SamusState.TurningLeftToRightAimDiagonalDownPose:
                    case SamusState.TurningRightToLeftCrouchingPose:
                    case SamusState.TurningLeftToRightCrouchingPose:
                    case SamusState.TurningRightToLeftCrouchingAimUpPose:
                    case SamusState.TurningLeftToRightCrouchingAimUpPose:
                    case SamusState.TurningRightToLeftCrouchingAimDiagonalUpPose:
                    case SamusState.TurningLeftToRightCrouchingAimDiagonalUpPose:
                    case SamusState.TurningRightToLeftCrouchingAimDiagonalDownPose:
                    case SamusState.TurningLeftToRightCrouchingAimDiagonalDownPose:
                    case SamusState.MoonwalkTurnJumpLeftPose:
                    case SamusState.MoonwalkTurnJumpRightPose:
                    case SamusState.MoonwalkTurnJumpAimUpLeftPose:
                    case SamusState.MoonwalkTurnJumpAimUpRightPose:
                    case SamusState.MoonwalkTurnJumpAimDownLeftPose:
                    case SamusState.MoonwalkTurnJumpAimDownRightPose:
                        // Most type-$17` crouched turns are grounded, but the native
                        // `$90:A790` dispatcher does not assume that. A live displacement or
                        // actor can give the same pose nonzero Y direction, in which case it
                        // executes the ordinary turning-in-air path for this frame.
                        if (Samus.Kinematics.YDirection == 0)
                        {
                            LastGroundedSamusMovement = SamusGroundedMovement.StepTurningOnGround(
                                _addressSpace,
                                LevelData,
                                Samus,
                                NmiFrameCounter,
                                Plms);
                        }
                        else
                        {
                            LastAerialSamusMovement = SamusAerialMovement.StepTurningInAir(
                                _addressSpace,
                                LevelData,
                                Samus,
                                NmiFrameCounter,
                                Plms);
                        }
                        break;
                    case SamusState.NormalLandingRightPose:
                    case SamusState.NormalLandingLeftPose:
                    case SamusState.SpinLandingRightPose:
                    case SamusState.SpinLandingLeftPose:
                    case SamusState.LandingAimUpRightPose:
                    case SamusState.LandingAimUpLeftPose:
                    case SamusState.LandingAimDiagonalUpRightPose:
                    case SamusState.LandingAimDiagonalUpLeftPose:
                    case SamusState.LandingAimDiagonalDownRightPose:
                    case SamusState.LandingAimDiagonalDownLeftPose:
                    case SamusState.FiringLandingRightPose:
                    case SamusState.FiringLandingLeftPose:
                        LastGroundedSamusMovement = SamusGroundedMovement.StepLanding(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.MorphBallGroundRightPose:
                    case SamusState.MorphBallGroundLeftPose:
                    case SamusState.MorphBallMovingRightPose:
                    case SamusState.MorphBallMovingLeftPose:
                    case SamusState.SpringBallGroundRightPose:
                    case SamusState.SpringBallGroundLeftPose:
                    case SamusState.SpringBallMovingRightPose:
                    case SamusState.SpringBallMovingLeftPose:
                        LastMorphBallMovement = SamusMorphBallMovement.StepGrounded(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.MorphBallFallingRightPose:
                    case SamusState.MorphBallFallingLeftPose:
                    case SamusState.SpringBallFallingRightPose:
                    case SamusState.SpringBallFallingLeftPose:
                        LastMorphBallMovement = SamusMorphBallMovement.StepFalling(
                            _addressSpace,
                            LevelData,
                            Samus,
                            Controller1.Current,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.SpringBallJumpRightPose:
                    case SamusState.SpringBallJumpLeftPose:
                        LastMorphBallMovement = SamusMorphBallMovement.StepSpringBallInAir(
                            _addressSpace,
                            LevelData,
                            Samus,
                            Controller1.Current,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.NeutralJumpTransitionRightPose:
                    case SamusState.NeutralJumpTransitionLeftPose:
                    case SamusState.NeutralJumpRightPose:
                    case SamusState.NeutralJumpLeftPose:
                    case SamusState.NormalJumpGunExtendedRightPose:
                    case SamusState.NormalJumpGunExtendedLeftPose:
                    case SamusState.NormalJumpForwardRightPose:
                    case SamusState.NormalJumpForwardLeftPose:
                    case SamusState.NormalJumpAimUpRightPose:
                    case SamusState.NormalJumpAimUpLeftPose:
                    case SamusState.NormalJumpTransitionAimUpRightPose:
                    case SamusState.NormalJumpTransitionAimUpLeftPose:
                    case SamusState.NormalJumpTransitionAimDiagonalUpRightPose:
                    case SamusState.NormalJumpTransitionAimDiagonalUpLeftPose:
                    case SamusState.NormalJumpTransitionAimDiagonalDownRightPose:
                    case SamusState.NormalJumpTransitionAimDiagonalDownLeftPose:
                    case SamusState.NormalJumpAimDiagonalUpRightPose:
                    case SamusState.NormalJumpAimDiagonalUpLeftPose:
                    case SamusState.NormalJumpAimDiagonalDownRightPose:
                    case SamusState.NormalJumpAimDiagonalDownLeftPose:
                    case SamusState.NormalJumpAimDownRightPose:
                    case SamusState.NormalJumpAimDownLeftPose:
                        LastAerialSamusMovement = SamusAerialMovement.StepNormalJump(
                            _addressSpace,
                            LevelData,
                            Samus,
                            Controller1.Current,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.SpinJumpRightPose:
                    case SamusState.SpinJumpLeftPose:
                    case SamusState.SpaceJumpRightPose:
                    case SamusState.SpaceJumpLeftPose:
                    case SamusState.ScrewAttackRightPose:
                    case SamusState.ScrewAttackLeftPose:
                        LastAerialSamusMovement = SamusAerialMovement.StepSpinJump(
                            _addressSpace,
                            LevelData,
                            Samus,
                            Controller1.Current,
                            NmiFrameCounter,
                            Controller1.NewlyPressed,
                            Plms);
                        break;
                    case SamusState.WallJumpRightPose:
                    case SamusState.WallJumpLeftPose:
                        LastAerialSamusMovement = SamusAerialMovement.StepWallJump(
                            _addressSpace,
                            LevelData,
                            Samus,
                            Controller1.Current,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.DamageBoostRightPose:
                    case SamusState.DamageBoostLeftPose:
                        LastAerialSamusMovement = SamusAerialMovement.StepDamageBoost(
                            _addressSpace,
                            LevelData,
                            Samus,
                            Controller1.Current,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.TurningRightToLeftJumpPose:
                    case SamusState.TurningLeftToRightJumpPose:
                    case SamusState.TurningRightToLeftJumpAimUpPose:
                    case SamusState.TurningLeftToRightJumpAimUpPose:
                    case SamusState.TurningRightToLeftJumpAimDownPose:
                    case SamusState.TurningLeftToRightJumpAimDownPose:
                    case SamusState.TurningRightToLeftJumpAimDiagonalUpPose:
                    case SamusState.TurningLeftToRightJumpAimDiagonalUpPose:
                    case SamusState.TurningRightToLeftFallingPose:
                    case SamusState.TurningLeftToRightFallingPose:
                    case SamusState.TurningRightToLeftFallingAimUpPose:
                    case SamusState.TurningLeftToRightFallingAimUpPose:
                    case SamusState.TurningRightToLeftFallingAimDownPose:
                    case SamusState.TurningLeftToRightFallingAimDownPose:
                    case SamusState.TurningRightToLeftFallingAimDiagonalUpPose:
                    case SamusState.TurningLeftToRightFallingAimDiagonalUpPose:
                        LastAerialSamusMovement = SamusAerialMovement.StepTurningInAir(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.FallingRightPose:
                    case SamusState.FallingLeftPose:
                    case SamusState.FallingGunExtendedRightPose:
                    case SamusState.FallingGunExtendedLeftPose:
                    case SamusState.FallingAimUpRightPose:
                    case SamusState.FallingAimUpLeftPose:
                    case SamusState.FallingAimDiagonalUpRightPose:
                    case SamusState.FallingAimDiagonalUpLeftPose:
                    case SamusState.FallingAimDiagonalDownRightPose:
                    case SamusState.FallingAimDiagonalDownLeftPose:
                    case SamusState.FallingAimDownRightPose:
                    case SamusState.FallingAimDownLeftPose:
                        LastAerialSamusMovement = SamusAerialMovement.StepFalling(
                            _addressSpace,
                            LevelData,
                            Samus,
                            Controller1.Current,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.CrouchingRightPose:
                    case SamusState.CrouchingLeftPose:
                    case SamusState.CrouchingAimUpRightPose:
                    case SamusState.CrouchingAimUpLeftPose:
                    case SamusState.CrouchingAimDiagonalUpRightPose:
                    case SamusState.CrouchingAimDiagonalUpLeftPose:
                    case SamusState.CrouchingAimDiagonalDownRightPose:
                    case SamusState.CrouchingAimDiagonalDownLeftPose:
                        LastGroundedSamusMovement = SamusPostureMovement.StepCrouching(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter);
                        break;
                    case SamusState.KnockbackRightPose:
                    case SamusState.KnockbackLeftPose:
                        // `$90:A5FC` is the normal type-$0A movement-table entry. Active
                        // knockback and Ceres ejection were intercepted above; this branch
                        // is their authentic one-frame normal-dispatch tail before bank-$91
                        // installs the input/no-input target chosen during alpha.
                        SamusGroundedMovement.StepKnockbackOrCrystalFlashEnding(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.CrouchingTransitionRightPose:
                    case SamusState.CrouchingTransitionLeftPose:
                    case SamusState.StandingTransitionRightPose:
                    case SamusState.StandingTransitionLeftPose:
                    case SamusState.CrouchingTransitionAimUpRightPose:
                    case SamusState.CrouchingTransitionAimUpLeftPose:
                    case SamusState.CrouchingTransitionAimDiagonalUpRightPose:
                    case SamusState.CrouchingTransitionAimDiagonalUpLeftPose:
                    case SamusState.CrouchingTransitionAimDiagonalDownRightPose:
                    case SamusState.CrouchingTransitionAimDiagonalDownLeftPose:
                    case SamusState.StandingTransitionAimUpRightPose:
                    case SamusState.StandingTransitionAimUpLeftPose:
                    case SamusState.StandingTransitionAimDiagonalUpRightPose:
                    case SamusState.StandingTransitionAimDiagonalUpLeftPose:
                    case SamusState.StandingTransitionAimDiagonalDownRightPose:
                    case SamusState.StandingTransitionAimDiagonalDownLeftPose:
                        LastGroundedSamusMovement = SamusPostureMovement.StepCrouchStandTransition(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter);
                        break;
                    case SamusState.MorphingTransitionRightPose:
                    case SamusState.MorphingTransitionLeftPose:
                    case SamusState.UnmorphingTransitionRightPose:
                    case SamusState.UnmorphingTransitionLeftPose:
                        LastMorphBallMovement = SamusMorphBallMovement.StepTransition(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter,
                            Plms);
                        break;
                    case SamusState.DraygonGrabbedNeutralLeftPose:
                    case SamusState.DraygonGrabbedAimUpLeftPose:
                    case SamusState.DraygonGrabbedFiringLeftPose:
                    case SamusState.DraygonGrabbedAimDownLeftPose:
                    case SamusState.DraygonGrabbedMovingLeftPose:
                    case SamusState.DraygonGrabbedNeutralRightPose:
                    case SamusState.DraygonGrabbedAimUpRightPose:
                    case SamusState.DraygonGrabbedFiringRightPose:
                    case SamusState.DraygonGrabbedAimDownRightPose:
                    case SamusState.DraygonGrabbedMovingRightPose:
                        // This is the normal dispatcher at `$90:A7D2`, which only clears
                        // the vertical solid-collision result. The actual boss grab above
                        // runs its separately installed RTS handler instead.
                        LastDraygonGrabbedMovement =
                            Samus.DraygonGrabbed.StepMovement(Samus);
                        break;
                    default:
                        // Every pose reachable from the translated retail input, animation,
                        // enemy-grab, death, drained, grapple, Morph Ball, and shinespark
                        // producers has an explicit branch above. Reaching this point means
                        // the pose byte no longer agrees with the cartridge-backed producer
                        // set; it is corrupt state, not a recoverable movement family.
                        throw new InvalidDataException(
                            $"Runtime movement dispatcher received invalid pose ${Samus.Pose:X2}.");
                }
            }

            // Samus_MovementHandler_Normal calls `$90:EEE7` after the selected movement
            // function and before animation/pose transitions. Every fourth gameplay frame
            // at boost stage four, it alternates between two exact world-position snapshots.
            // Keeping this producer here means the draw handler below consumes post-motion
            // coordinates with the same frame ordering as the cartridge.
            if (LastShinesparkMovement is null)
            {
                Samus.HorizontalSpeed.CaptureSpeedEchoPosition(
                    NmiFrameCounter,
                    Samus.XPosition,
                    Samus.YPosition);
            }

            // Every ordinary downward collision calls `$91:F046` inside movement, before
            // the timer hack, liquid animation, prospective-pose selection, and velocity
            // cleanup. Ordinary-air, Morph/Spring, knockback, and drained special movement
            // all publish `Landed`; the latter two also carry the magnitude they clear
            // immediately after collision so hard/soft sound selection remains exact.
            bool ordinaryOrBallLanded =
                LastAerialSamusMovement is { Landed: true } ||
                LastMorphBallMovement is { Landed: true };
            bool knockbackLanded = LastKnockbackMovement is { Landed: true };
            bool drainedLanded = LastDrainedSamusMovement is { Landed: true };
            if (!deathOwnsSamus && (ordinaryOrBallLanded || knockbackLanded || drainedLanded))
            {
                ushort impactYSpeed = knockbackLanded
                    ? LastKnockbackMovement!.Value.ImpactYSpeed
                    : drainedLanded
                        ? LastDrainedSamusMovement!.Value.ImpactYSpeed
                        : Samus.Kinematics.YSpeed;
                ushort impactYSubspeed = knockbackLanded
                    ? LastKnockbackMovement!.Value.ImpactYSubspeed
                    : drainedLanded
                        ? LastDrainedSamusMovement!.Value.ImpactYSubspeed
                        : Samus.Kinematics.YSubspeed;
                Samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
                    _addressSpace,
                    Samus,
                    movementTypeAtFrameStart,
                    poseAtFrameStart,
                    impactYSpeed,
                    impactYSubspeed);
            }

            // `$90:E738` runs the timer/hack handler after movement and before animation.
            // Commands five/$18 install the Mother Brain-specific Up-edge branches; the
            // method is a cheap no-op for every ordinary gameplay state.
            Samus.Drained.StepGetUpHandler(Samus, Controller1.NewlyPressed);

            if (Samus.DraygonGrabbed.IsActive)
            {
                // `$90:E738` calls this installed hack handler after movement and before
                // animation. A locked grapple cancels the prospective pose chosen in alpha;
                // an escape release additionally clears every pending route while installing
                // ordinary `$01/$02` and zeroing the documented velocity words.
                LastDraygonEscape = Samus.DraygonGrabbed.StepEscapeHandler(
                    _addressSpace,
                    Samus,
                    Controller1.NewlyPressed,
                    Samus.Grapple.Phase == GrapplePhase.ConnectedLocked);
                if (LastDraygonEscape.Value.SuppressProspectivePose ||
                    LastDraygonEscape.Value.Released)
                {
                    ProspectiveSamusPose = null;
                    ProspectiveSamusFallbackPose = null;
                }
            }

            // Normal gameplay advances animation during frame-handler beta, before the
            // pose-transition handler, draw handler, and next-NMI tile selection.
            // Animation sees the same held-input word as movement. Ordinary Dash uses B
            // both to accumulate `$0B42.$0B44` and to intercept the running delay-list
            // command at `$90:852C`; passing a reconstructed input later would skew art.
            if (deathOwnsSamus)
            {
                LastDeathSequenceStep = Samus.DeathSequence.Step(
                    _addressSpace,
                    Samus,
                    Cgram,
                    VramWrites);
            }
            else
            {
                Samus.AnimateNoFx(
                    _addressSpace,
                    Controller1.Current,
                    NmiFrameCounter,
                    System,
                    beginLiquidSoundRequestFrame: false);
            }

            // Native gameplay state eight reaches PLM_Handler after Samus's new-state and
            // enemy-projectile passes but before MainScrollingRoutine. A grapple block
            // acquired above therefore consumes its initial timer and draws $E0B7 in this
            // same frame; when it later becomes air, anchor validation has already run and
            // sees that mutation on the following Samus frame.
            if (!deathOwnsSamus && !Samus.Xray.TimeIsFrozen)
            {
                if (LevelData is null || BackgroundStreamer is null || Camera is null)
                    throw new InvalidOperationException("The PLM handler requires an active room and camera.");

                // Boss AI and its later collision callbacks publish hardcoded bank-$84
                // entries at different points in this gameplay frame. Consume all of them
                // at the native PLM-handler seam so a projectile/contact death cannot lose
                // its request to the enemy system's next frame-publication reset.
                ApplyPendingBotwoonWallPlm();
                ApplyPendingSporeSpawnCeilingPlm();
                ApplyPendingCrocomireArenaPlms();
                ApplyPendingMotherBrainPlms();
                ApplyPendingShitroidWallPlms();

                IReadOnlyList<PlmTilemapUpdate> plmUpdates = Plms.Step(
                    _addressSpace,
                    LevelData,
                    BackgroundStreamer,
                    Camera.XPosition,
                    Camera.YPosition,
                    BackgroundScroll.Bg1XOffset,
                    Camera.Scrolls,
                    Enemies.EnemiesKilled,
                    Enemies.DeathQuota);
                foreach (PlmTilemapUpdate update in plmUpdates)
                    update.ExecuteTo(Vram);

                // PLM opcode $87E5 appends a normal seven-byte VRAM record. It must share
                // the runtime queue so the next accepted NMI performs the transfer in the
                // same order as HUD, beam, and room-main uploads.
                foreach (PlmVramWriteRequest request in Plms.VramWriteRequests)
                {
                    VramWrites.Enqueue(
                        request.SizeInBytes,
                        request.SourceAddress,
                        request.EncodedVramDestination);
                }

                // Item PLMs publish acquisition only after their native trigger and
                // handler pass. Apply the hardware-facing consequences at that same seam:
                // beam combinations replace the projectile character/palette staging, and
                // suit changes defer their visible palette reveal until bank-$88 stage
                // three after the message. Resource tanks and other equipment bits are
                // consumed directly by the HUD/movement owners.
                foreach (CollectiblePickupEvent pickup in Plms.CollectiblePickupEvents)
                {
                    if (pickup.Kind is
                        InWorldCollectibleKind.ChargeBeam or
                        InWorldCollectibleKind.IceBeam or
                        InWorldCollectibleKind.WaveBeam or
                        InWorldCollectibleKind.SpazerBeam or
                        InWorldCollectibleKind.PlasmaBeam)
                    {
                        Projectiles.QueueBeamTilesAndLoadPalette(
                            _addressSpace,
                            VramWrites,
                            Cgram,
                            Samus.EquippedBeams);
                    }
                    else if (pickup.Kind is
                             InWorldCollectibleKind.VariaSuit or
                             InWorldCollectibleKind.GravitySuit)
                    {
                        if (_pendingSuitPickup is not null || SuitPickup.IsActive)
                        {
                            throw new InvalidDataException(
                                "A second suit pickup attempted to replace an active transformation.");
                        }
                        _pendingSuitPickup = pickup.Kind == InWorldCollectibleKind.VariaSuit
                            ? SamusSuitPickupKind.Varia
                            : SamusSuitPickupKind.Gravity;
                    }

                    if (MessageBox.IsActive)
                    {
                        throw new InvalidDataException(
                            "Multiple permanent items attempted to enter the synchronous " +
                            "bank-$85 message routine during one PLM pass.");
                    }
                    MessageBox.Begin(_addressSpace, pickup.MessageBoxIndex);
                }
                foreach (MotherBrainGlassProjectileRequest request in
                         Plms.MotherBrainGlassProjectileRequests)
                {
                    Enemies.SpawnMotherBrainGlassProjectile(request);
                }
                foreach (BombTorizoStatueProjectileRequest request in
                         Plms.BombTorizoStatueProjectileRequests)
                {
                    Enemies.SpawnBombTorizoStatueBreakingProjectile(request);
                }
            }

            if (GroundedSamusMovementEnabled && !deathOwnsSamus)
            {
                // Command $F8's command-three “super-special” transition wins at this seam.
                // $25/$26 publish $02/$01 from their ROM byte streams without rerunning the
                // ordinary input transition selected earlier in the frame.
                bool animationTransitionApplied =
                    Samus.ApplyPendingVerifiedAnimationTransition(_addressSpace);

                // `$91:EADE` runs inside UpdateSamusPose after beta movement/animation and
                // only when no super-special animation command has already won. Its first
                // branch consumes the X-speed-killed flag produced by CURRENT type-one
                // running movement. This creates a wall-stop pose even with no input match.
                if (!animationTransitionApplied &&
                    LastGroundedSamusMovement is
                        { Vertical.Collided: true } groundedForWallCheck)
                {
                    bool currentRunHitWall =
                        (SamusState.IsRightFacingRunningPose(poseAtFrameStart) ||
                         SamusState.IsLeftFacingRunningPose(poseAtFrameStart)) &&
                        groundedForWallCheck.Horizontal.Collided;
                    byte? prospectiveRunningPose =
                        ProspectiveSamusPose is { ProspectivePose: <= byte.MaxValue } prospective
                            ? unchecked((byte)prospective.ProspectivePose)
                            : null;
                    ProspectiveSamusWallCollisionPose =
                        Samus.CheckProspectiveRunningPoseForWall(
                            _addressSpace,
                            LevelData ?? throw new InvalidOperationException(
                                "Ran-into-wall probe requires active room level data."),
                            prospectiveRunningPose,
                            currentRunHitWall,
                            out BlockMoveResult? onePixelProbe,
                            Plms);
                    LastRanIntoWallProbe = onePixelProbe;
                }

                if (!animationTransitionApplied &&
                    ProspectiveSamusWallCollisionPose is { } wallCollisionPose)
                {
                    // The selector result replaces the ordinary input target. Applying it
                    // here also covers the no-input “running speed was killed” branch.
                    Samus.ApplyRanIntoWallPoseChange(_addressSpace, wallCollisionPose);
                    animationTransitionApplied = true;
                }

                // Morph landing has its own prospective-pose and collision tables. A hard
                // impact retains `$31/$32` and launches bounce one; the next collision
                // launches bounce two; only a gentle/second-bounce collision installs
                // grounded `$1D/$41`. Treat every branch as a consumed transition so input
                // selected before collision cannot override the cartridge's bounce result.
                if (!animationTransitionApplied &&
                    LastMorphBallMovement is { Landed: true } &&
                    (SamusState.IsAirborneMorphBallPose(poseAtFrameStart) ||
                     (Samus.BombJumpActive == false &&
                      SamusState.IsGroundedMorphBallPose(poseAtFrameStart) &&
                      Samus.Kinematics.YDirection != 0)))
                {
                    Samus.ApplyMorphBallLanding(_addressSpace);
                    animationTransitionApplied = true;
                }

                // `$90:9D35` publishes solid-vertical collision result five and returns
                // carry set, preventing vertical motion. The matching bank-$91 command
                // installs `$83/$84`, clears old momentum, and launches from ROM constants.
                if (!animationTransitionApplied &&
                    LastAerialSamusMovement is { WallJumpTriggered: true })
                {
                    Samus.ApplyWallJumpTrigger(_addressSpace);
                    animationTransitionApplied = true;
                }

                // Spring Ball's three movement families share collision result three, but
                // `$91:F25E` makes held Jump an immediate relaunch and stores `$0601/$0602`
                // during automatic rebounds. Keep it distinct from ordinary-ball state.
                if (!animationTransitionApplied &&
                    LastMorphBallMovement is { Landed: true } &&
                    (SamusState.IsAirborneSpringBallPose(poseAtFrameStart) ||
                     (Samus.BombJumpActive == false &&
                      SamusState.IsGroundedSpringBallPose(poseAtFrameStart) &&
                      Samus.Kinematics.YDirection != 0)))
                {
                    Samus.ApplySpringBallLanding(_addressSpace, Controller1.Current);
                    animationTransitionApplied = true;
                }

                // A downward collision publishes result one. $91:E95D chooses normal or
                // spin landing from the movement type that executed this frame, then pose
                // command five clears both velocity axes. This ordinary prospective pose
                // is lower priority than an animation command-three transition above.
                if (!animationTransitionApplied &&
                    LastAerialSamusMovement is { Landed: true } &&
                    !SamusState.IsAerialTurnPose(poseAtFrameStart))
                {
                    if (SamusState.IsCompactAerialPose(poseAtFrameStart))
                    {
                        Samus.TryApplyCompactAerialLanding(
                            _addressSpace,
                            LevelData ?? throw new InvalidOperationException(
                                "Compact landing requires active room level data."),
                            NmiFrameCounter);
                    }
                    else
                    {
                        bool wasSpinning = SamusState.IsSpinJumpPose(poseAtFrameStart) ||
                            SamusState.IsWallJumpPose(poseAtFrameStart);
                        Samus.ApplyAerialLanding(
                            _addressSpace,
                            wasSpinning,
                            Controller1.Current);
                    }
                    animationTransitionApplied = true;
                }

                // A failed grounding probe publishes result two. For admitted standing,
                // running, and stable crouching types, data zero at `$90:E65A` selects
                // airborne family zero and `$91:E8F2` chooses falling art from pose metadata.
                // Turn types `$0E/$17` contain `$04` (no pose change) in that literal table;
                // they finish `$F8`, then the destination pose detects the missing floor.
                if (!animationTransitionApplied &&
                    LastGroundedSamusMovement is { Vertical.Collided: false } &&
                    (SamusState.IsRightFacingStandingPose(poseAtFrameStart) ||
                     SamusState.IsLeftFacingStandingPose(poseAtFrameStart) ||
                     SamusState.IsRightFacingRunningPose(poseAtFrameStart) ||
                     SamusState.IsLeftFacingRunningPose(poseAtFrameStart) ||
                     SamusState.IsMoonwalkingPose(poseAtFrameStart) ||
                     SamusState.IsMoonwalkTurnJumpPose(poseAtFrameStart) ||
                     SamusState.IsRanIntoWallPose(poseAtFrameStart) ||
                     SamusState.IsRightFacingCrouchingPose(poseAtFrameStart) ||
                     SamusState.IsLeftFacingCrouchingPose(poseAtFrameStart)))
                {
                    byte fallingPose = Samus.SelectFallingPoseForCurrentAim(_addressSpace);
                    Samus.ApplyWalkedOffFloorTransition(_addressSpace, fallingPose);
                    animationTransitionApplied = true;
                }

                // Movement type four publishes ordinary airborne `$31/$32` when its
                // no-speed grounding probe finds no floor. The shared rolling animation is
                // preserved, while Y speed starts at zero/down exactly like `$91:E8F2`.
                if (!animationTransitionApplied &&
                    LastMorphBallMovement is { Vertical.Collided: false } &&
                    (SamusState.IsGroundedMorphBallPose(poseAtFrameStart) ||
                     SamusState.IsGroundedSpringBallPose(poseAtFrameStart)))
                {
                    Samus.ApplyMorphBallWalkOff(_addressSpace);
                    animationTransitionApplied = true;
                }

                if (!animationTransitionApplied && ProspectiveSamusPose is { } inputTransition)
                {
                    byte targetPose = unchecked((byte)inputTransition.ProspectivePose);

                    // Self-transitions in the turn tables mean “remain in this animation.”
                    // Reinitializing frame zero would prevent $F8 from ever being reached.
                    if (targetPose != poseAtFrameStart)
                    {
                        switch ((poseAtFrameStart, targetPose))
                        {
                            case var (source, target)
                                when SamusState.IsDraygonGrabbedPose(source) &&
                                     SamusState.IsDraygonGrabbedPose(target):
                                // Both ROM tables stay within one facing. The helper makes
                                // that invariant explicit and initializes the target's real
                                // `$91:B2B4/$B53C` animation list.
                                Samus.ApplyDraygonGrabbedPoseChange(
                                    _addressSpace,
                                    targetPose);
                                break;
                            case var (source, target)
                                when (((SamusState.IsRightFacingStandingPose(source) ||
                                        SamusState.IsRightFacingRanIntoWallPose(source) ||
                                        SamusState.IsRightFacingLandingPose(source)) &&
                                       SamusState.IsMoonwalkingFacingRightPose(target)) ||
                                      ((SamusState.IsLeftFacingStandingPose(source) ||
                                        SamusState.IsLeftFacingRanIntoWallPose(source) ||
                                        SamusState.IsLeftFacingLandingPose(source)) &&
                                       SamusState.IsMoonwalkingFacingLeftPose(target)) ||
                                      (SamusState.IsMoonwalkingPose(source) &&
                                       (SamusState.IsMoonwalkingPose(target) ||
                                        target is SamusState.MovingRightNormalPose or
                                            SamusState.MovingLeftNormalPose))):
                                Samus.ApplyMoonwalkPoseChange(
                                    _addressSpace,
                                    targetPose,
                                    MoonwalkEnabled);
                                break;
                            case var (source, target)
                                when SamusState.IsMoonwalkingPose(source) &&
                                     SamusState.IsMoonwalkTurnJumpPose(target):
                                // The stable `$49/$4A/$75-$78` table publishes a generic
                                // reversal target. `$91:F8D3` validates it against the old
                                // shot direction, folds momentum, and leaves `$BF-$C4`
                                // grounded until either its input table or `$F8` starts jump.
                                Samus.ApplyMoonwalkTurnJump(_addressSpace, targetPose);
                                break;
                            case var (source, target)
                                when SamusState.IsMoonwalkTurnJumpLeftPose(source) &&
                                         target is SamusState.SpinJumpLeftPose or
                                             SamusState.NeutralJumpTransitionLeftPose ||
                                     SamusState.IsMoonwalkTurnJumpRightPose(source) &&
                                         target is SamusState.SpinJumpRightPose or
                                             SamusState.NeutralJumpTransitionRightPose:
                                Samus.ApplyOrdinaryJumpTransition(
                                    _addressSpace,
                                    targetPose,
                                    Controller1.NewlyPressed);
                                break;
                            case (SamusState.KnockbackRightPose, SamusState.DamageBoostRightPose):
                            case (SamusState.KnockbackLeftPose, SamusState.DamageBoostLeftPose):
                                // `$91:8113` makes a fresh jump when the `$53/$54` input
                                // table crosses out of movement type `$0A`; `$91:F8AE`
                                // then restores the normal movement handler for type `$19`.
                                SamusKnockbackMovement.ApplyDamageBoostTransition(
                                    _addressSpace,
                                    Samus,
                                    poseAtFrameStart,
                                    targetPose);
                                break;
                            case (SamusState.DamageBoostRightPose,
                                  SamusState.NeutralJumpRightPose or SamusState.NormalJumpForwardRightPose):
                            case (SamusState.DamageBoostLeftPose,
                                  SamusState.NeutralJumpLeftPose or SamusState.NormalJumpForwardLeftPose):
                                SamusKnockbackMovement.ApplyDamageBoostPoseTransition(
                                    _addressSpace,
                                    Samus,
                                    targetPose);
                                break;
                            case var (source, target)
                                when ((SamusState.IsRightFacingNormalJumpPose(source) &&
                                       target == SamusState.TurningRightToLeftJumpPose) ||
                                      (SamusState.IsLeftFacingNormalJumpPose(source) &&
                                       target == SamusState.TurningLeftToRightJumpPose) ||
                                      (SamusState.IsRightFacingFallingPose(source) &&
                                       target == SamusState.TurningRightToLeftFallingPose) ||
                                      (SamusState.IsLeftFacingFallingPose(source) &&
                                       target == SamusState.TurningLeftToRightFallingPose)):
                                // `$2F/$30/$87/$88` are generic table outputs. The helper
                                // reads the source shot-direction record, chooses the exact
                                // `$8F-$A1` art when needed, folds momentum, and runs compact
                                // pose-expansion collision before committing the turn.
                                Samus.TryApplyAerialTurn(
                                    _addressSpace,
                                    LevelData ?? throw new InvalidOperationException(
                                        "Aerial turn requires active room level data."),
                                    target,
                                    NmiFrameCounter);
                                break;
                            case var (source, target)
                                when (SamusState.IsSpinJumpPose(source) ||
                                      SamusState.IsWallJumpPose(source)) &&
                                     SamusState.IsSpinJumpPose(target):
                                Samus.ApplySpinJumpDirectionTransition(_addressSpace, targetPose);
                                break;
                            case var (source, target)
                                when SamusState.IsStableBallPose(source) &&
                                     SamusState.IsStableBallPose(target) &&
                                     !(SamusState.IsGroundedSpringBallPose(source) &&
                                       target is SamusState.SpringBallJumpRightPose or
                                           SamusState.SpringBallJumpLeftPose):
                                // `$1D/$1E/$1F/$31/$32/$41` all share delay list `$B378`.
                                // The initializer preserves frame/timer and applies mode-one
                                // reversal momentum only when direction actually changes.
                                Samus.ApplyMorphBallPoseChange(_addressSpace, target);
                                break;
                            case var (source, target)
                                when SamusState.IsGroundedSpringBallPose(source) &&
                                     target is SamusState.SpringBallJumpRightPose or
                                         SamusState.SpringBallJumpLeftPose:
                                Samus.ApplySpringBallJump(_addressSpace, target);
                                break;
                            case var (source, target)
                                when ((SamusState.ReadPoseXDirection(_addressSpace, source) == 8 &&
                                       target == SamusState.MorphingTransitionRightPose) ||
                                      (SamusState.ReadPoseXDirection(_addressSpace, source) == 4 &&
                                       target == SamusState.MorphingTransitionLeftPose) ||
                                      (SamusState.IsStableBallPose(source) &&
                                       target is SamusState.UnmorphingTransitionRightPose or
                                           SamusState.UnmorphingTransitionLeftPose)):
                                Samus.TryApplyMorphTransition(
                                    _addressSpace,
                                    LevelData ?? throw new InvalidOperationException(
                                        "Morph transition requires active room level data."),
                                    targetPose,
                                    NmiFrameCounter);
                                break;
                            case var (source, target)
                                when (SamusState.IsCompactAerialPose(source) ||
                                      SamusState.IsCompactAerialPose(target)) &&
                                     ((SamusState.IsRightFacingNormalJumpPose(source) &&
                                       SamusState.IsRightFacingNormalJumpPose(target)) ||
                                      (SamusState.IsLeftFacingNormalJumpPose(source) &&
                                       SamusState.IsLeftFacingNormalJumpPose(target)) ||
                                      (SamusState.IsRightFacingFallingPose(source) &&
                                       SamusState.IsRightFacingFallingPose(target)) ||
                                      (SamusState.IsLeftFacingFallingPose(source) &&
                                       SamusState.IsLeftFacingFallingPose(target))):
                                Samus.TryApplyCompactAerialTransition(
                                    _addressSpace,
                                    LevelData ?? throw new InvalidOperationException(
                                        "Compact aerial transition requires active room level data."),
                                    targetPose,
                                    NmiFrameCounter);
                                break;
                            case var (source, target)
                                when (SamusState.IsGroundedAimPose(source) ||
                                      SamusState.IsGroundedAimPose(target) ||
                                      SamusState.IsGunExtendedPose(source) ||
                                      SamusState.IsGunExtendedPose(target)) &&
                                     (((SamusState.IsRightFacingStandingPose(source) ||
                                        SamusState.IsRightFacingRunningPose(source) ||
                                        SamusState.IsRightFacingRanIntoWallPose(source) ||
                                        SamusState.IsRightFacingLandingPose(source)) &&
                                       (SamusState.IsRightFacingStandingPose(target) ||
                                        SamusState.IsRightFacingRunningPose(target) ||
                                        SamusState.IsRightFacingRanIntoWallPose(target))) ||
                                      ((SamusState.IsLeftFacingStandingPose(source) ||
                                        SamusState.IsLeftFacingRunningPose(source) ||
                                        SamusState.IsLeftFacingRanIntoWallPose(source) ||
                                        SamusState.IsLeftFacingLandingPose(source)) &&
                                       (SamusState.IsLeftFacingStandingPose(target) ||
                                        SamusState.IsLeftFacingRunningPose(target) ||
                                        SamusState.IsLeftFacingRanIntoWallPose(target))) ||
                                      (SamusState.IsRightFacingCrouchingPose(source) &&
                                       SamusState.IsRightFacingCrouchingPose(target)) ||
                                      (SamusState.IsLeftFacingCrouchingPose(source) &&
                                       SamusState.IsLeftFacingCrouchingPose(target))):
                                Samus.ApplyGroundedAimTransition(_addressSpace, targetPose);
                                break;
                            case var (source, target)
                                when SamusState.IsSameFacingAerialAimFireOrForwardTransition(
                                    source,
                                    target):
                                Samus.ApplyAerialAimTransition(_addressSpace, targetPose);
                                break;
                            case (SamusState.FacingRightNormalPose, SamusState.MovingRightNormalPose):
                                Samus.ApplyStandingRightToRunningRight(_addressSpace);
                                break;
                            case (SamusState.FacingLeftNormalPose, SamusState.MovingLeftNormalPose):
                                Samus.ApplyStandingLeftToRunningLeft(_addressSpace);
                                break;
                            case var (source, target)
                                when SamusState.IsRanIntoWallPose(source) &&
                                     (SamusState.IsRightFacingRunningPose(target) ||
                                      SamusState.IsLeftFacingRunningPose(target)):
                                Samus.ApplyRanIntoWallToRunning(_addressSpace, target);
                                break;
                            case var (forwardSource, forwardTarget)
                                when SamusState.IsForwardFacingPose(forwardSource) &&
                                     forwardTarget is
                                         SamusState.TurningRightToLeftPose or
                                         SamusState.TurningLeftToRightPose:
                                // Ceres releases controls while `$00` is still active.
                                // Its own ROM transition table maps Left/Right to generic
                                // `$25/$26`; `$91:F8D3` recognizes old pose `$00/$9B` and
                                // preserves that target without inventing a source facing.
                                Samus.ApplyGroundedTurn(_addressSpace, targetPose);
                                break;
                            case var (rightSource, rightTarget)
                                when rightTarget is
                                         SamusState.TurningRightToLeftPose or
                                         SamusState.TurningRightToLeftCrouchingPose &&
                                     (SamusState.IsRightFacingStandingPose(rightSource) ||
                                      SamusState.IsRightFacingRunningPose(rightSource) ||
                                      SamusState.IsMoonwalkingFacingRightPose(rightSource) ||
                                      SamusState.IsRightFacingRanIntoWallPose(rightSource) ||
                                      SamusState.IsRightFacingCrouchingPose(rightSource) ||
                                      SamusState.IsRightFacingLandingPose(rightSource)):
                            case var (leftSource, leftTarget)
                                when leftTarget is
                                         SamusState.TurningLeftToRightPose or
                                         SamusState.TurningLeftToRightCrouchingPose &&
                                     (SamusState.IsLeftFacingStandingPose(leftSource) ||
                                      SamusState.IsLeftFacingRunningPose(leftSource) ||
                                      SamusState.IsMoonwalkingFacingLeftPose(leftSource) ||
                                      SamusState.IsLeftFacingRanIntoWallPose(leftSource) ||
                                      SamusState.IsLeftFacingCrouchingPose(leftSource) ||
                                      SamusState.IsLeftFacingLandingPose(leftSource)):
                                Samus.ApplyGroundedTurn(_addressSpace, targetPose);
                                break;
                            case var (source, target)
                                when ((SamusState.IsRightFacingStandingPose(source) ||
                                       SamusState.IsRightFacingRanIntoWallPose(source)) &&
                                      SamusState.IsRightFacingNormalJumpPose(target) &&
                                      target is
                                          SamusState.NeutralJumpTransitionRightPose or
                                          SamusState.NormalJumpTransitionAimUpRightPose or
                                          SamusState.NormalJumpTransitionAimDiagonalUpRightPose or
                                          SamusState.NormalJumpTransitionAimDiagonalDownRightPose) ||
                                     ((SamusState.IsLeftFacingStandingPose(source) ||
                                       SamusState.IsLeftFacingRanIntoWallPose(source)) &&
                                      SamusState.IsLeftFacingNormalJumpPose(target) &&
                                      target is
                                          SamusState.NeutralJumpTransitionLeftPose or
                                          SamusState.NormalJumpTransitionAimUpLeftPose or
                                          SamusState.NormalJumpTransitionAimDiagonalUpLeftPose or
                                          SamusState.NormalJumpTransitionAimDiagonalDownLeftPose) ||
                                     SamusState.IsLandingToNormalJumpTransition(source, target):
                                // All `$A4-$A7/$E0-$E7` landing records expose the same
                                // standing input table while their brief streams are active.
                                // A fresh Jump edge can therefore preserve any authored
                                // normal-jump aim target, even from horizontal-fire `$E6/$E7`
                                // before `$F8` fallback.
                            case (SamusState.MovingRightNormalPose,
                                  SamusState.SpinJumpRightPose):
                            case (SamusState.MovingRightGunExtendedPose,
                                  SamusState.SpinJumpRightPose):
                            case (SamusState.RunningAimUpRightPose or
                                  SamusState.RunningAimDiagonalUpRightPose or
                                  SamusState.RunningAimDiagonalDownRightPose,
                                  SamusState.SpinJumpRightPose):
                            case (SamusState.MovingLeftNormalPose,
                                  SamusState.SpinJumpLeftPose):
                            case (SamusState.MovingLeftGunExtendedPose,
                                  SamusState.SpinJumpLeftPose):
                            case (SamusState.RunningAimUpLeftPose or
                                  SamusState.RunningAimDiagonalUpLeftPose or
                                  SamusState.RunningAimDiagonalDownLeftPose,
                                  SamusState.SpinJumpLeftPose):
                            case var (turnSourcePose, turnTargetPose)
                                when SamusState.IsStandingGroundTurnToJumpTransition(
                                    turnSourcePose,
                                    turnTargetPose):
                                // Every standing `$0E` turn—including the aimed `$8B-$8E`
                                // and `$9C/$9D` records—continues consulting `$91:8142`.
                                // That table can interrupt the art with either same-facing
                                // spin or neutral jump. Preserve newly pressed Shoot as well:
                                // the normal-jump initializer owns the simultaneous
                                // Jump+Fire bridge when the selected body is type two.
                                Samus.ApplyOrdinaryJumpTransition(
                                    _addressSpace,
                                    targetPose,
                                    Controller1.NewlyPressed);
                                break;
                            case var (source, target)
                                when ((SamusState.IsRightFacingCrouchingPose(source) &&
                                       target == SamusState.NeutralJumpTransitionRightPose) ||
                                      (SamusState.IsLeftFacingCrouchingPose(source) &&
                                       target == SamusState.NeutralJumpTransitionLeftPose)):
                                // `$91:A66C/$91:A6BC` use the ordinary `$4B/$4C` transition
                                // art for every crouched aim direction. The helper preserves
                                // `$91:FC7D`'s literal `$27/$28`-only ten-pixel adjustment.
                                Samus.TryApplyCrouchJumpTransition(
                                    _addressSpace,
                                    LevelData ?? throw new InvalidOperationException(
                                        "Crouch jump requires active room level data."),
                                    targetPose,
                                    NmiFrameCounter);
                                break;
                            case var (source, target)
                                when SamusState.IsSpinJumpPose(source) &&
                                     (SamusState.IsRightFacingNormalJumpPose(target) ||
                                      SamusState.IsLeftFacingNormalJumpPose(target)):
                                // Aim and Fire cancel the compact spinning body through the
                                // same normal-jump initializer without restarting the jump
                                // arc. `$1A -> $18`, for example, is the ordinary left-spin
                                // Down input used to shoot a floor hatch. The shared helper
                                // performs changed-pose collision for every radius expansion;
                                // a rejected body leaves the source spin pose installed.
                                Samus.TryApplySpinToNormalJumpTransition(
                                    _addressSpace,
                                    LevelData ?? throw new InvalidOperationException(
                                        "Spin-to-normal-jump transition requires active room level data."),
                                    targetPose,
                                    NmiFrameCounter,
                                    Controller1.NewlyPressed);
                                break;
                            case var (source, target)
                                when (source == SamusState.UnmorphingTransitionRightPose &&
                                      SamusState.IsRightFacingFallingPose(target)) ||
                                     (source == SamusState.UnmorphingTransitionLeftPose &&
                                      SamusState.IsLeftFacingFallingPose(target)):
                                // `$3D/$3E` still process controller table records until
                                // animation command `$FD` chooses stable crouch. Falling out
                                // of a tunnel can therefore interrupt the visible unmorph
                                // with any same-facing type-six body. Route that complete
                                // family through the ordinary expansion collision pass while
                                // preserving the fall already in progress.
                                Samus.TryApplyUnmorphToFallingTransition(
                                    _addressSpace,
                                    LevelData ?? throw new InvalidOperationException(
                                        "Unmorph-to-falling transition requires active room level data."),
                                    targetPose,
                                    NmiFrameCounter);
                                break;
                            case var (source, target)
                                when ((SamusState.IsRightFacingCrouchingPose(source) &&
                                       target == SamusState.FacingRightNormalPose) ||
                                      (SamusState.IsLeftFacingCrouchingPose(source) &&
                                       target == SamusState.FacingLeftNormalPose)):
                                // These direct `$01/$02` records bypass the animated
                                // `$F7-$FC` stand-up family, but still run pose-expansion
                                // collision before installing the final standing body.
                                Samus.TryApplyDirectCrouchToStandingTransition(
                                    _addressSpace,
                                    LevelData ?? throw new InvalidOperationException(
                                        "Direct crouch exit requires active room level data."),
                                    targetPose,
                                    NmiFrameCounter);
                                break;
                            case var (landingSource, landingTarget)
                                when (landingTarget == SamusState.MovingRightNormalPose &&
                                      SamusState.IsRightFacingLandingPose(landingSource)) ||
                                     (landingTarget == SamusState.MovingLeftNormalPose &&
                                      SamusState.IsLeftFacingLandingPose(landingSource)):
                                Samus.ApplyLandingToRunning(_addressSpace, targetPose);
                                break;
                            case var (source, postureTarget)
                                when SamusState.IsCrouchStandTransitionPose(postureTarget) &&
                                     (SamusState.IsRightFacingStandingPose(source) ||
                                      SamusState.IsLeftFacingStandingPose(source) ||
                                      SamusState.IsRightFacingRunningPose(source) ||
                                      SamusState.IsLeftFacingRunningPose(source) ||
                                      SamusState.IsMoonwalkingPose(source) ||
                                      SamusState.IsRanIntoWallPose(source) ||
                                      SamusState.IsRightFacingLandingPose(source) ||
                                      SamusState.IsLeftFacingLandingPose(source) ||
                                      SamusState.IsRightFacingCrouchingPose(source) ||
                                      SamusState.IsLeftFacingCrouchingPose(source)):
                                Samus.TryApplyPostureTransition(
                                    _addressSpace,
                                    LevelData ?? throw new InvalidOperationException(
                                        "Posture transition requires active room level data."),
                                    targetPose,
                                    NmiFrameCounter);
                                break;
                            case var (source, sparkTarget)
                                when source is
                                         SamusState.ShinesparkWindupRightPose or
                                         SamusState.ShinesparkWindupLeftPose &&
                                     sparkTarget is
                                         SamusState.ShinesparkHorizontalRightPose or
                                         SamusState.ShinesparkHorizontalLeftPose or
                                         SamusState.ShinesparkVerticalRightPose or
                                         SamusState.ShinesparkVerticalLeftPose or
                                         SamusState.ShinesparkDiagonalRightPose or
                                         SamusState.ShinesparkDiagonalLeftPose:
                                // `$91:AD6C/$AD80` are ordinary held/new input records, but
                                // `$91:F80F` installs a special movement pointer instead of
                                // invoking a normal movement-type initializer.
                                Samus.ApplyShinesparkDirectionTransition(
                                    _addressSpace,
                                    targetPose);
                                break;
                            default:
                                // MOVEMENT_COVERAGE exhaustively audits every active retail
                                // transition pair entering this dispatcher. A ROM match not
                                // owned by one of the explicit families above means the pose
                                // table or state was mixed from an incompatible revision.
                                throw new InvalidDataException(
                                    $"Grounded input transition ${poseAtFrameStart:X2} -> " +
                                    $"${targetPose:X2} escaped the exhaustive retail dispatcher.");
                        }
                    }
                }
                else if (!animationTransitionApplied &&
                         SamusState.IsDraygonGrabbedPose(poseAtFrameStart) &&
                         ProspectiveSamusFallbackPose is { } draygonFallback)
                {
                    // Zero input uses pose-definition byte two, returning aimed/firing/
                    // struggling art to neutral `$BA/$EC` without touching owner placement.
                    Samus.ApplyDraygonGrabbedPoseChange(
                        _addressSpace,
                        unchecked((byte)draygonFallback));
                }
                else if (!animationTransitionApplied &&
                         SamusState.IsWallJumpPose(poseAtFrameStart) &&
                         ProspectiveSamusFallbackPose is
                             SamusState.SpinJumpRightPose or SamusState.SpinJumpLeftPose)
                {
                    // Definition byte two leaves the launch animation for ordinary spin
                    // art. `$91:F624` starts that target on frame one and preserves reversal
                    // momentum only if the fallback changes facing.
                    Samus.ApplySpinJumpDirectionTransition(
                        _addressSpace,
                        unchecked((byte)ProspectiveSamusFallbackPose.Value));
                }
                else if (!animationTransitionApplied &&
                         (poseAtFrameStart is SamusState.MorphBallMovingRightPose or
                             SamusState.MorphBallMovingLeftPose or
                             SamusState.SpringBallMovingRightPose or
                             SamusState.SpringBallMovingLeftPose) &&
                         ProspectiveSamusFallbackPose == poseAtFrameStart)
                {
                    // Prospective command one rechecks the post-movement base words. A
                    // residue selects mode two deceleration; exact zero falls through to
                    // command two's stopped mode and definition fallback on the next frame.
                    Samus.HorizontalSpeed.AccelerationMode =
                        Samus.HorizontalSpeed.BaseFixed != 0 ? (ushort)2 : (ushort)0;
                }
                else if (!animationTransitionApplied &&
                         (poseAtFrameStart is SamusState.MorphBallMovingRightPose or
                             SamusState.MorphBallMovingLeftPose or
                             SamusState.SpringBallMovingRightPose or
                             SamusState.SpringBallMovingLeftPose) &&
                         ProspectiveSamusFallbackPose is
                             SamusState.MorphBallGroundRightPose or SamusState.MorphBallGroundLeftPose or
                             SamusState.SpringBallGroundRightPose or SamusState.SpringBallGroundLeftPose)
                {
                    Samus.HorizontalSpeed.AccelerationMode = 0;
                    Samus.ApplyMorphBallPoseChange(
                        _addressSpace,
                        unchecked((byte)ProspectiveSamusFallbackPose.Value));
                }
                else if (!animationTransitionApplied &&
                         (SamusState.IsRightFacingRunningPose(poseAtFrameStart) ||
                          SamusState.IsLeftFacingRunningPose(poseAtFrameStart)) &&
                         ProspectiveSamusFallbackPose == poseAtFrameStart)
                {
                    // Momentum routine one at $91:EC50 rechecks speed AFTER movement. Any
                    // residue selects mode two; collision or final underflow leaves zero.
                    Samus.HorizontalSpeed.AccelerationMode =
                        Samus.HorizontalSpeed.BaseFixed != 0 ? (ushort)2 : (ushort)0;
                }
                else if (!animationTransitionApplied &&
                         SamusState.IsRightFacingRunningPose(poseAtFrameStart) &&
                         ProspectiveSamusFallbackPose == SamusState.FacingRightNormalPose)
                {
                    Samus.HorizontalSpeed.AccelerationMode = 0;
                    if (poseAtFrameStart == SamusState.MovingRightNormalPose)
                        Samus.ApplyRunningRightToStandingRight(_addressSpace);
                    else
                        Samus.ApplyGroundedAimTransition(_addressSpace, SamusState.FacingRightNormalPose);
                }
                else if (!animationTransitionApplied &&
                         SamusState.IsLeftFacingRunningPose(poseAtFrameStart) &&
                         ProspectiveSamusFallbackPose == SamusState.FacingLeftNormalPose)
                {
                    Samus.HorizontalSpeed.AccelerationMode = 0;
                    if (poseAtFrameStart == SamusState.MovingLeftNormalPose)
                        Samus.ApplyRunningLeftToStandingLeft(_addressSpace);
                    else
                        Samus.ApplyGroundedAimTransition(_addressSpace, SamusState.FacingLeftNormalPose);
                }
                else if (!animationTransitionApplied &&
                         SamusState.IsMoonwalkingPose(poseAtFrameStart) &&
                         ProspectiveSamusFallbackPose is { } moonwalkFallback)
                {
                    // Command two selected the literal standing/aim fallback during alpha.
                    // Applying it after movement preserves the native one-last-step timing.
                    Samus.ApplyMoonwalkPoseChange(
                        _addressSpace,
                        unchecked((byte)moonwalkFallback),
                        MoonwalkEnabled);
                }
                else if (!animationTransitionApplied &&
                         SamusState.IsAimedRanIntoWallPose(poseAtFrameStart) &&
                         ProspectiveSamusFallbackPose is { } wallFallback)
                {
                    // `$CF-$D2` store `$89/$8A` in pose-definition byte two. The stable
                    // target shares radius, animation list, and type-$15 cleanup physics.
                    Samus.ApplyGroundedAimTransition(
                        _addressSpace,
                        unchecked((byte)wallFallback));
                }
                else if (!animationTransitionApplied &&
                         (poseAtFrameStart is
                              SamusState.StandingAimUpRightPose or
                              SamusState.StandingAimUpLeftPose or
                              SamusState.StandingAimDiagonalUpRightPose or
                              SamusState.StandingAimDiagonalUpLeftPose or
                              SamusState.StandingAimDiagonalDownRightPose or
                              SamusState.StandingAimDiagonalDownLeftPose ||
                          SamusState.IsAimedCrouchingPose(poseAtFrameStart)) &&
                         ProspectiveSamusFallbackPose is { } aimFallback)
                {
                    // Only the zero-controller path above populates this value. Validate
                    // and apply it through the same radius/animation seam as held-input
                    // aim changes; never assign the ROM byte directly to Pose.
                    Samus.ApplyGroundedAimTransition(
                        _addressSpace,
                        unchecked((byte)aimFallback));
                }
                else if (!animationTransitionApplied &&
                         SamusState.IsAimedAerialPose(poseAtFrameStart) &&
                         ProspectiveSamusFallbackPose is { } aerialFallback)
                {
                    // Unlike grounded momentum fallback, the live jump/fall velocity is
                    // untouched. Only pose metadata, radius (asserted equal), animation,
                    // and next-NMI tile definitions change at this seam.
                    Samus.ApplyAerialAimTransition(
                        _addressSpace,
                        unchecked((byte)aerialFallback));
                }
            }

            // Native GameState_8 runs bank-$86 enemy projectiles immediately after Samus's
            // movement handler and before MainScrollingRoutine. The two fresh-Ceres actors
            // use that slot to lower Samus one pixel at a time; moving this call below the
            // camera would make the viewport trail one frame behind the cartridge.
            if (!deathOwnsSamus && CeresElevatorArrival is { IsComplete: false } arrival)
            {
                if (arrival.Step(Samus))
                {
                    // Command $0E installed a locked Samus handler before the projectiles
                    // were spawned. Re-enable the translated ordinary movement path only
                    // after the pad reaches the native Y=$0048 deletion condition.
                    Samus.InputLocked = false;
                    GroundedSamusMovementEnabled = true;
                }
            }

            if (!deathOwnsSamus)
            {
                if (LevelData is null || BackgroundStreamer is null)
                    throw new InvalidOperationException("Gameplay camera tracking requires active room stream data.");

                ActiveRoomGeometry roomGeometry = GetActiveRoomGeometry();

                var currentCameraPoint = new SamusCameraPoint(
                    Samus.XPosition,
                    Samus.Kinematics.XSubposition,
                    Samus.YPosition,
                    Samus.Kinematics.YSubposition);

                // GameState_8 calls MainScrollingRoutine after movement/PLMs and before
                // DrawSamusEnemiesAndProjectiles regardless of which Samus input handler is
                // installed. InputLocked suppresses controller transitions; it does not
                // suppress scrolling. That is why the station-eighteen gunship can carry
                // Samus down several screens while her ordinary movement handler is absent.
                Camera.TrackMovedSamusHorizontally(
                    previousCameraPoint,
                    currentCameraPoint,
                    new HorizontalCameraContext(
                        KnockbackDirection: Samus.KnockbackDirection,
                        MovementType: Samus.ReadMovementType(_addressSpace),
                        XAccelerationMode: Samus.HorizontalSpeed.AccelerationMode,
                        PoseXDirection: Samus.ReadPoseXDirection(_addressSpace),
                        CameraDistanceIndex: 0));
                Camera.TrackMovedSamusVertically(
                    previousCameraPoint,
                    currentCameraPoint,
                    new VerticalCameraContext(
                        YDirection: Samus.Kinematics.YDirection,
                        UpScroller: roomGeometry.UpScroller,
                        DownScroller: roomGeometry.DownScroller));

                // The camera can cross a 16-pixel boundary in the same main-loop pass.
                // Build and execute the exact row/column staging transfers now so both the
                // live PPU diagnostic and the following frame see the newly exposed edge.
                IReadOnlyList<BackgroundUpdateRequest> backgroundRequests =
                    UpdateBackgroundScrollingFromCamera();
                foreach (BackgroundUpdateRequest request in backgroundRequests)
                {
                    TilemapStreamUpdate update = BackgroundStreamer.Build(request)
                        ?? throw new InvalidOperationException("Active room unexpectedly entered Mode 7 streaming.");
                    update.ExecuteTo(Vram);
                }

                // UpdateMinimap belongs to Samus's normal frame-handler beta; it uses world
                // position, not camera position. Fresh debug sessions begin without an area
                // map, so only tiles actually visited by this runtime are revealed.
                Hud.UpdateMinimap(
                    _addressSpace,
                    System,
                    roomGeometry.AreaIndex,
                    roomGeometry.MapX,
                    roomGeometry.MapY,
                    LevelData.WidthInBlocks,
                    LevelData.HeightInBlocks,
                    Samus.XPosition,
                    Samus.YPosition,
                    NmiFrameCounter8);
            }

            if (!deathOwnsSamus)
            {
                // `$90:E74D` consumes the lava/acid words produced during AnimateSamus.
                // X-ray freezes time and therefore clears rather than applies accumulated
                // damage; fatal zero-energy game-state acquisition remains the outer seam.
                Samus.LiquidPhysics.ApplyPeriodicDamage(
                    Samus,
                    timeIsFrozen: Samus.Xray.TimeIsFrozen);
            }

            // $A0:884D draws bomb/projectile explosions before reaching the enemy-layer
            // phase that calls DrawSamusAndProjectiles. Preserve that OAM ordering.
            if (!deathOwnsSamus)
            {
                BombProjectiles.Draw(_addressSpace, Oam, Camera.XPosition, Camera.YPosition);
                Projectiles.DrawExplosions(_addressSpace, Oam, Camera.XPosition, Camera.YPosition);
            }

            // `$A0:885D` calls `$86:8390` after bomb/projectile explosions and before the
            // layer loop reaches Samus at layer three. Room-specific actors may supply this
            // pass without teaching the reusable Landing Site runtime how to own enemies.
            if (!deathOwnsSamus)
            {
                // Both Ceres elevator definitions carry properties $3000, including bit
                // $1000 selected by Draw_HighPriority_EnemyProjectile at `$86:8390`.
                // That named pass occurs here, before enemy layers zero through two and
                // Samus; lower OAM indices retain their native same-priority overlap win.
                CeresElevatorArrival?.Draw(Oam, Camera.XPosition, Camera.YPosition);
                if (Enemies.IsLoaded)
                    Enemies.DrawEnemyProjectiles(Oam, Camera.XPosition, Camera.YPosition);
                drawHighPriorityEnemyProjectiles?.Invoke(Oam);
            }

            // The global layer loop emits layers zero through two before its phase-three
            // call to DrawSamusAndProjectiles. Landing Site's gunship definition selects
            // layer two, so its hull correctly precedes (and can sit behind) Samus OAM.
            if (!deathOwnsSamus && Enemies.IsLoaded)
                Enemies.DrawLayers(Oam, Camera.XPosition, Camera.YPosition, 0, 2);

            // `$91:D6F7` updates Samus's palette buffer during gameplay. The software PPU
            // reads CGRAM directly, so perform the literal ROM pointer/table copy immediately
            // before the matching draw phase. This covers dry-room Speed Booster stage four
            // and the normal-suit restoration requested by `$91:DE53` cancellation.
            // A negative super-special flag is the first branch of `$91:D6F7` and returns
            // immediately after the drained/rainbow handler. Its one-shot normal restore
            // also models the direct palette loads in controller zero and command `$17`.
            bool drainedOwnsSamusPalette = !deathOwnsSamus &&
                Samus.Drained.UpdatePalette(_addressSpace, Cgram, Samus.EquippedItems);
            LastHurtFlashPaletteStep = default;
            LastVisorPaletteStep = default;
            if (!deathOwnsSamus && !drainedOwnsSamusPalette)
            {
                // `$91:D708` always runs charge/post-shot handling before dispatching the
                // selected special Samus palette. Ordinary charged shots paint colors 1-15
                // white for three calls and restore the ROM suit on call four; Hyper shots
                // step ten complete bank-$9B palettes on alternating calls before restore.
                LastBeamChargePaletteStep = Projectiles.UpdateBeamChargePalette(
                    _addressSpace,
                    Cgram,
                    Samus,
                    LayerBlendingDefaultConfig);
                LastVisorPaletteStep = Projectiles.LastVisorPaletteStep;

                Samus.HorizontalSpeed.UpdateSpeedBoosterPalette(
                    _addressSpace,
                    Cgram,
                    Samus.ReadMovementType(_addressSpace),
                    Samus.AnimationFrame,
                    Samus.EquippedItems,
                    suppressActiveSpeedBoosterPalette:
                        Samus.Shinespark.PaletteType != 0 ||
                        Samus.CrystalFlash.SpecialPaletteKind ==
                            SamusSpecialPaletteType.CrystalFlash ||
                        Samus.Xray.SpecialPaletteKind == SamusSpecialPaletteType.Xray,
                    bottomBoundarySubmerged:
                        Samus.LiquidPhysics.IsBottomBoundarySubmerged(Samus));
            }
            // Palette handlers one and six run at the same `$91:D6F7` dispatch point. They
            // intentionally execute after a cancellation-requested normal copy and replace
            // it with the stored/spark palette in this visible frame.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette)
            {
                Samus.Shinespark.UpdatePalette(
                    _addressSpace,
                    Cgram,
                    Samus.EquippedItems);
            }
            // Handler seven owns all sixteen colors of sprite palette six during Crystal
            // Flash. It is mutually exclusive with shinespark/X-ray special handlers but
            // intentionally runs at the same `$91:D6F7` dispatch point.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette)
            {
                Samus.CrystalFlash.UpdatePalette(
                    _addressSpace,
                    Cgram,
                    Samus);
            }
            // Handler eight changes only visor color four while active; `$FFFF` teardown
            // restores the complete ROM-selected Power/Varia/Gravity suit palette once.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette)
            {
                Samus.Xray.UpdatePalette(
                    _addressSpace,
                    Cgram,
                    Samus.EquippedItems);
            }
            // `$91:D8A5` runs after every charge and special-palette family. A Metroid's
            // nonnegative super-special flag takes the alternating boost/normal branch and
            // returns before ordinary hurt flash, just as drained rainbow's negative flag
            // took the routine's earlier branch.
            bool metroidOwnsSamusPalette = !deathOwnsSamus &&
                !drainedOwnsSamusPalette &&
                SamusSpecialSuperPalette.Update(_addressSpace, Cgram, Samus);
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !metroidOwnsSamusPalette)
            {
                LastHurtFlashPaletteStep = SamusHurtFlashPalette.Update(
                    _addressSpace,
                    Cgram,
                    Samus,
                    Controller1.Current);
            }

            if (deathOwnsSamus)
            {
                if (LastDeathSequenceStep is { DrawPose: true })
                {
                    LastSamusBodyDrawn = Samus.Draw(
                        _addressSpace,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition,
                        mode7Transform: ActiveSamusMode7Transform);
                }
                else if (LastDeathSequenceStep is { DrawExplosion: true })
                    Samus.DeathSequence.DrawExplosion(_addressSpace, Oam);
            }
            else
            {
                // `DrawSamusSprites` begins with `$90:C5C4` before dispatching the default
                // drawing handler. HUD-selection stability, cover transition, and the
                // pose-authored before/after-body mode must therefore settle before charge
                // flare, atmospheric, or body OAM is appended.
                LastArmCannonUpdate = Samus.ArmCannon.Update(_addressSpace, Samus);
                LastArmCannonDraw = default;

                // `$90:EB86` replaces the ordinary `$90:EB52` display handler while an
                // elevator owns a front-facing body. It still arrives after `$90:C5C4`, so
                // the arm-cannon cover state above advances every frame. Odd NMIs return
                // immediately. Even NMIs call the fatal/no-animation body renderer, which
                // deliberately bypasses ordinary invincibility flicker and every auxiliary
                // Samus layer: atmosphere, charge flare, cannon OBJ, speed/shinespark
                // echoes, and grapple graphics. Projectile drawing remains in the shared
                // tail below, exactly as `DrawSamusAndProjectiles` does after this handler.
                bool elevatorOwnsSamusDrawing =
                    ElevatorStatus != 0 && SamusState.IsForwardFacingPose(Samus.Pose);
                if (elevatorOwnsSamusDrawing)
                {
                    if ((NmiFrameCounter & 1) == 0)
                    {
                        // Omitting the live counter intentionally selects `Draw`'s default
                        // even value. Native `$90:EB86` calls `$90:85D2`, below `$85E2`'s
                        // ordinary invincibility test, so this path cannot flicker twice.
                        LastSamusBodyDrawn = Samus.Draw(
                            _addressSpace,
                            Oam,
                            Camera.XPosition,
                            Camera.YPosition,
                            mode7Transform: ActiveSamusMode7Transform);
                    }
                }
                else if (Samus.Shinespark.Phase is
                         ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle)
                {
                    // `$90:EBF3` is installed by `$90:D2BA` at the instant an active
                    // shinespark crashes. Unlike the default handler, it intentionally
                    // omits charging flare/audio, atmosphere, both arm-cannon priority
                    // paths, ordinary departing speed echoes, and grapple graphics. It
                    // draws the live body first, then crash slot one before slot zero on
                    // odd NMIs. The shared projectile/trail tail below still follows.
                    LastShinesparkCrashDrawingHandlerActive = true;
                    LastSamusBodyDrawn = Samus.Draw(
                        _addressSpace,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition,
                        NmiFrameCounter,
                        ActiveSamusMode7Transform);
                    Samus.DrawShinesparkCrashEchoes(
                        _addressSpace,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition,
                        NmiFrameCounter);
                }
                else
                {
                    bool grappleHandlerInstalled =
                        SamusGrappleMovement.UsesGrappleDrawingHandler(Samus.Grapple.Phase);
                    bool grappleBeamSpecificPath = grappleHandlerInstalled &&
                        SamusGrappleMovement.UsesBeamSpecificDrawingPath(Samus.Grapple.Phase);
                    LastGrappleDrawingHandlerActive = grappleHandlerInstalled;
                    LastGrappleBeamSpecificDrawingPath = grappleBeamSpecificPath;

                    if (grappleBeamSpecificPath)
                    {
                        // Active `$90:EB86` begins with `$9B:C036`. Firing also refreshes
                        // its hand origins here from Samus's post-movement position. This
                        // flare is a different animation owner from the charge flare below.
                        LastGrappleFlareDrawn = SamusGrappleMovement.DrawFlareBeforeSamus(
                            _addressSpace,
                            Samus,
                            Oam,
                            Camera.XPosition,
                            Camera.YPosition);
                    }
                    else if (!grappleHandlerInstalled)
                    {
                        // `$90:EB52` advances/emits the ordinary charge flare before it
                        // falls through to `$90:EB55`. Grapple's handler never calls this,
                        // including its cancel/release fallback frames.
                        Projectiles.HandleChargeFlareAndDraw(
                            _addressSpace,
                            Oam,
                            Samus,
                            Camera.XPosition,
                            Camera.YPosition,
                            ActiveSamusMode7Transform);
                    }

                    // `$90:EB55` begins here for all three routes: ordinary, active grapple,
                    // and grapple teardown. Reverse atmospheric-slot order is therefore
                    // earlier in OAM than cannon/body, but later than either applicable
                    // flare. This exact overlap order is visible through transparent pixels.
                    Samus.LiquidPhysics.AtmosphericEffects.UpdateAndDraw(
                        _addressSpace,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition,
                        Samus.LiquidPhysics.FxYPosition);

                    // Drawing modes one and two differ only in OAM priority: one appends the
                    // cannon before the body, while two appends it after. Mode zero suppresses
                    // the independent object even if a HUD-driven cover frame remains nonzero.
                    if (Samus.ArmCannon.EffectiveDrawingMode != 0 &&
                        Samus.ArmCannon.EffectiveDrawingMode != 2)
                    {
                        LastArmCannonDraw = Samus.ArmCannon.Draw(
                            _addressSpace,
                            Oam,
                            VramWrites,
                            Samus,
                            Camera.XPosition,
                            Camera.YPosition,
                            NmiFrameCounter);
                    }
                    LastSamusBodyDrawn = Samus.Draw(
                        _addressSpace,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition,
                        NmiFrameCounter,
                        ActiveSamusMode7Transform);
                    if (Samus.ArmCannon.EffectiveDrawingMode == 2)
                    {
                        LastArmCannonDraw = Samus.ArmCannon.Draw(
                            _addressSpace,
                            Oam,
                            VramWrites,
                            Samus,
                            Camera.XPosition,
                            Camera.YPosition,
                            NmiFrameCounter);
                    }

                    if (grappleBeamSpecificPath)
                    {
                        // Active `$90:EB86` deliberately omits `Samus_DrawEchoes`. It updates
                        // endpoint/segment tiles after Samus, increments flare time, and only
                        // then emits rope pieces when length is nonzero.
                        SamusGrappleMovement.DrawConnectedBeam(
                            _addressSpace,
                            Samus.Grapple,
                            Oam,
                            VramWrites,
                            Camera.XPosition,
                            Camera.YPosition);
                    }
                    else
                    {
                        // Ordinary `$90:EB55` and grapple's signed-range fallback share this
                        // exact echo tail. The latter still suppresses charge flare and rope
                        // because `$90:EB52` and the active half of `$90:EB86` were bypassed.
                        Samus.DrawSpeedBoosterEchoes(
                            _addressSpace,
                            Oam,
                            Camera.XPosition,
                            Camera.YPosition);
                        Samus.DrawReleasedShinesparkCrashEchoes(
                            _addressSpace,
                            Oam,
                            Camera.XPosition,
                            Camera.YPosition,
                            NmiFrameCounter);
                    }
                }

                // `$90:EB3B` draws Samus first and immediately calls `$93:8254`. This is
                // deliberately after the grapple beam too in the translated composite pass;
                // all live beam art still receives later OAM indices than Samus's body.
                Projectiles.DrawLiveProjectiles(
                    _addressSpace,
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition,
                    NmiFrameCounter);
                // `$93:82F7` immediately follows the ordinary projectile draw with bank
                // `$90:B6A9`. Trails are detached world-space objects, so they must keep
                // animating after their source beam has collided or left the viewport.
                Projectiles.HandleTrailsAndDraw(
                    _addressSpace,
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition,
                    Samus.Xray.TimeIsFrozen);

                // `$90:F576` follows DrawSamusAndProjectiles. A counter-forty hurt update
                // may have armed this latch above; consuming it here preserves both the
                // same-frame charging sound and native ordering after projectile drawing.
                SamusHurtFlashPalette.ConsumeResumeChargingBeamSound(
                    Samus,
                    Controller1.Current);
            }

            if (!deathOwnsSamus && Enemies.IsLoaded)
            {
                // At phase three Samus/projectiles are emitted before enemy layer three;
                // ordinary enemy layers four and five follow without another insertion.
                Enemies.DrawLayers(Oam, Camera.XPosition, Camera.YPosition, 3, 5);
            }

            // Phase six inserts high-priority enemy projectiles before layer-six actors.
            // Keep the historical delegate name for API compatibility even though the
            // native source calls this pass DrawHighPriorityEprojs.
            if (!deathOwnsSamus)
                drawLowPriorityEnemyProjectiles?.Invoke(Oam);

            if (!deathOwnsSamus && Enemies.IsLoaded)
                Enemies.DrawLayers(Oam, Camera.XPosition, Camera.YPosition, 6, 7);

            if (!deathOwnsSamus && Enemies.IsLoaded)
            {
                // CeresRidley_Main installs $A6:A2F2 as EnemyGraphicsDrawnHook. It runs
                // after every ordinary enemy layer and is the only producer of the Baby
                // Metroid OBJ (plus the arena-door overlay). Keeping this after layer seven
                // preserves the hook's actual OAM position instead of inventing a Baby slot.
                Enemies.DrawCeresRidleyPostEnemyHook(
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition);
            }
        }
        if (EscapeTimer.IsActive)
            EscapeTimerRenderer.Draw(EscapeTimer, Oam, _addressSpace);
        Oam.FinalizeFrame();

        // $80:9B44 appends the mutable three-row HUD upload during main-thread logic; it
        // becomes visible when the next accepted NMI drains this queue. The first frame's
        // initialization upload has already been consumed earlier in this StepFrame call.
        if (Hud.IsInitialized)
        {
            // `$80:9B44` rebuilds live energy/ammo words before appending the HUD transfer.
            // Initialization alone is insufficient: Ridley contact and fireballs mutate
            // Samus during this frame, and those values must enter the next accepted NMI.
            if (Samus is not null)
                Hud.UpdateGameplayCounters(_addressSpace, Samus);
            Hud.QueueUpload(_addressSpace, VramWrites);
        }

        // Landing Site's room main ASM appends four sky rows after ordinary gameplay logic;
        // they become visible when the following accepted NMI drains the queue.
        if (ScrollingSky is not null && Camera is not null)
        {
            ScrollingSky.ProcessFrame(
                Camera.YPosition,
                timeIsFrozen: Samus?.Xray.TimeIsFrozen ?? false,
                VramWrites);
        }

        // Execute the active room's bank-$8F wrapper at the same seam as Landing Site's
        // room main above: after gameplay drawing/HUD work and before global shaking. The
        // Ceres routine publishes a matrix for the following frame's presentation and may
        // change the outer dispatcher to state $20 only when its caller is state eight.
        LastCeresElevatorShaftRoomMain = CeresElevatorShaft.Step(
            _addressSpace,
            Samus,
            Enemies.CeresStatus,
            allowDeparture: allowCeresElevatorDeparture);
        if (LastCeresElevatorShaftRoomMain.MatrixChanged)
            ActiveSamusMode7Transform = LastCeresElevatorShaftRoomMain.Transform;

        // HandleSamusOutOfHealthAndGameTile advances the four-word gameplay clock after
        // room main and before shaking. Message-box frames returned above, exactly as the
        // suspended native coroutine does, so item fanfare time is not counted here.
        GameTime.Step();

        // `$82:8BAF` executes room shaking after room main ASM and game-time handling, but
        // before the active-enemy lists are cleared. Enemy attacks above may have installed
        // a new global quake this frame; consuming it here gives that request its first
        // displacement/decrement immediately and preserves the list used for actor shake.
        if (Enemies.IsLoaded)
            Enemies.HandleRoomShaking(Samus?.Xray.TimeIsFrozen ?? false);

        // Gameplay state eight calls `$A0:9169` after Samus, enemies, drawing, HUD/BG
        // bookkeeping, room main ASM, the energy-zero check, and room shaking. Keep this
        // shared tail out of individual movement handlers: in particular, grapple spike
        // reactions set `$18A8/$18AA` without installing the bank-$90 knockback handler.
        // Death game states do not execute the gameplay-state tail at all.
        if (Samus is not null && !Samus.DeathSequence.IsActive)
            Samus.DecrementHurtTimers();

        // `$0A11` is a one-byte previous-movement snapshot used by X-ray admission on the
        // following gameplay frame. Update it only after every pose/animation transition
        // and special teardown above has settled on the frame's final pose.
        if (Samus is not null)
        {
            PreviousMovementTypeForXray = Samus.ReadMovementType(_addressSpace);

            uint samusXAtFrameEnd =
                ((uint)Samus.Kinematics.XPosition << 16) | Samus.Kinematics.XSubposition;
            int signedDistance = unchecked((int)(samusXAtFrameEnd - samusXAtFrameStart));
            Samus.AbsoluteMovedLastFrameXFixed = signedDistance < 0
                ? unchecked((uint)-signedDistance)
                : unchecked((uint)signedDistance);
        }

        return Snapshot(escapeTimerExpired);
    }

    /// <summary>Runs the NMI handler portion currently translated from <c>$80:9583</c>.</summary>
    /// <param name="controller1Input">Host-provided raw SNES controller word.</param>
    /// <param name="mainLoopRequestedNmi">
    /// Equivalent of WRAM's NMI request flag. False models a lag NMI: it skips transfers,
    /// input, and accepted-frame counters but still advances the all-NMI counter.
    /// </param>
    public void RunNmi(ushort controller1Input, bool mainLoopRequestedNmi)
    {
        if (mainLoopRequestedNmi)
        {
            // Preserve $80:95A1 -> $80:95D0 -> $80:95E1 order: dedicated Samus graphics
            // DMA precedes the general video queue, and both precede controller latching.
            // $80:959E uploads the finalized main-loop OAM image immediately before that
            // Samus DMA; retain a second buffer so software rendering sees the same phase.
            DisplayedOam.CopyFinalizedFrom(Oam);
            Samus?.TileTransfers.TransferToVram(_addressSpace, Vram);
            VramWrites.DrainTo(Vram, _addressSpace);
            Controller1.Latch(controller1Input);

            NmiLagCounter = 0;
            NmiFrameCounter8 = unchecked((byte)(NmiFrameCounter8 + 1));
            NmiFrameCounter = unchecked((ushort)(NmiFrameCounter + 1));
        }
        else
        {
            NmiLagCounter = unchecked((ushort)(NmiLagCounter + 1));
            if (NmiLagCounter >= MaximumNmiLag)
                MaximumNmiLag = NmiLagCounter;
        }

        // $80:95F9 lies after the accepted/lagged branches rejoin, so it always advances.
        NmiCounterIncludingLag = unchecked((ushort)(NmiCounterIncludingLag + 1));
    }

    /// <summary>
    /// Invokes the pause-menu held-input filter with the controller sample latched by the
    /// most recent accepted NMI. The retail caller passes reset value three.
    /// </summary>
    public void UpdatePauseHeldInput(ushort timerReset = 3)
    {
        System.UpdateHeldInput(timerReset, Controller1.Current, Controller1.NewlyPressed);
    }

    private RuntimeFrameResult Snapshot(bool escapeTimerExpired) => new(
        NmiFrameCounter,
        Controller1.Current,
        Controller1.NewlyPressed,
        EscapeTimer.State,
        escapeTimerExpired);
}

/// <summary>
/// Small immutable return value for loggers and debugger watches after a frame step.
/// Mutable detail remains available on <see cref="SuperMetroidRuntime"/> itself.
/// </summary>
public readonly record struct RuntimeFrameResult(
    ushort FrameNumber,
    ushort ControllerInput,
    ushort ControllerNewInput,
    EscapeTimerState EscapeTimerState,
    bool EscapeTimerExpired);

/// <summary>
/// Auditable host placement chosen by <see cref="SuperMetroidRuntime.InitializeDebugGroundedSamus"/>.
/// </summary>
public readonly record struct DebugGroundedSamusPlacement(
    ushort XPosition,
    ushort YPosition,
    ushort DesiredScreenY,
    int BlockX,
    int BlockY,
    RoomCollisionBlock FloorBlock,
    byte FloorHeight);

/// <summary>
/// Host-selected wall regression placement whose referenced blocks all belong to the
/// decompressed cartridge room. The nested grounded record describes the supporting floor.
/// </summary>
public readonly record struct DebugRanIntoWallSamusPlacement(
    DebugGroundedSamusPlacement Grounded,
    int WallBlockX,
    int WallTopBlockY,
    int WallBottomBlockY);

/// <summary>Debugger-visible work performed by the room's force-blank tilemap fill.</summary>
public readonly record struct InitialViewportResult(int UpdateRequestCount, int DmaSegmentCount);
