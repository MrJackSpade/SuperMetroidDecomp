using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain's repeatable phase-two rainbow-beam, finish-off, and Baby-drain/corpse
/// function chains at <c>$A9:B8EB-$A9:BFCF</c>.
/// </summary>
/// <remarks>
/// The earlier general attack-selection logic and the spawned Baby Metroid's independent AI
/// remain separate actor phases. This class owns the neck extension, both charge waits, body
/// walks and posture changes, active beam, low-energy attack selection, final charge, the four
/// frame-spread Baby tile transfers, spawn request, final-beam hold, painful stagger, live neck
/// angles/brain coordinates, rear-room retreat, crouch, grey transition, and corpse handshake.
/// Palette, HDMA, projectile, earthquake, VRAM, spawn, and sound writes are retained as
/// inspectable requests; coordinate, resource, timer, instruction-list, and Samus-command
/// mutations execute directly.
/// </remarks>
public sealed partial class MotherBrainRainbowBeamAttackSequence
{
    // These are literal bank-$A9 instruction-list addresses installed by the AI. Keeping
    // the pointers visible lets the debugger prove which native animation is active.
    public const ushort HeadNeutralPhase2InstructionList = 0x9c87;
    public const ushort HeadChargingRainbowInstructionList = 0x9f6c;
    public const ushort HeadFiringRainbowInstructionList = 0x9c77;
    public const ushort HeadAttackingBombPhase2InstructionList = 0x9ecc;
    public const ushort HeadAttackingTwoOnionRingsPhase2InstructionList = 0x9d7f;
    public const ushort HeadStretchingPhase2InstructionList = 0x9b7f;
    public const ushort HeadStretchingPhase3InstructionList = 0x9bb3;
    public const ushort HeadHyperBeamRecoilInstructionList = 0x9be7;
    public const ushort HeadDecapitatedInstructionList = 0x9c29;
    public const ushort HeadCorpseInstructionList = 0x9d25;
    public const ushort HeadAttackingBabyMetroidInstructionList = 0x9db1;
    public const ushort HeadAttackingFourOnionRingsPhase3InstructionList = 0x9dbb;
    public const ushort HeadAttackingBombPhase3InstructionList = 0x9f00;
    public const ushort HeadNeutralPhase3InstructionList = 0x9cb9;
    public const ushort BodyWalkingForwardReallySlowInstructionList = 0x9818;
    public const ushort BodyWalkingForwardReallyFastInstructionList = 0x9730;
    public const ushort BodyWalkingForwardFastInstructionList = 0x976a;
    public const ushort BodyWalkingForwardMediumInstructionList = 0x97a4;
    public const ushort BodyWalkingForwardSlowInstructionList = 0x97de;
    public const ushort BodyWalkingBackwardReallySlowInstructionList = 0x993a;
    public const ushort BodyWalkingBackwardReallyFastInstructionList = 0x988c;
    public const ushort BodyWalkingBackwardFastInstructionList = 0x98c6;
    public const ushort BodyWalkingBackwardMediumInstructionList = 0x9900;
    public const ushort BodyWalkingBackwardSlowInstructionList = 0x9852;
    public const ushort BodyStandingUpAfterCrouchingFastInstructionList = 0x99c6;
    public const ushort BodyStandingUpAfterLeaningDownInstructionList = 0x99e2;
    public const ushort BodyLeaningDownInstructionList = 0x99f2;
    public const ushort BodyCrouchingFastInstructionList = 0x9a26;
    public const ushort HeadDyingDroolInstructionList = 0x9c39;

    // `$A9:BEEE-$BF0D` stores 16-bit words, but the animation-delay lookup deliberately
    // masks to the low byte. Stages 0/1 are the fastest stagger; each later pair slows both
    // the walk animation and the pause before Mother Brain reverses direction.
    private static ReadOnlySpan<ushort> PainfulWalkingAnimationDelays =>
        [0x0002, 0x0002, 0x0006, 0x0006, 0x0008, 0x0008, 0x000a, 0x000a];

    private static ReadOnlySpan<ushort> PainfulWalkingNeckAngleDeltas =>
        [0x0500, 0x0500, 0x0200, 0x0200, 0x00c0, 0x00c0, 0x0040, 0x0040];

    private static ReadOnlySpan<ushort> PainfulWalkingFunctionTimers =>
        [0x0010, 0x0010, 0x0020, 0x0020, 0x0030, 0x0030, 0x0040, 0x0040];

    // `$A9:8FE5-$9002` contains four 0x200-byte chunks. ProcessSpriteTilesTransfers
    // publishes exactly one entry per call, so these records also encode the exact four-call
    // loading delay before `$A9:BDDA` retracts the head and spawns the cutscene enemy.
    private static ReadOnlySpan<uint> BabyMetroidTileSources =>
        [0xb18400, 0xb18600, 0xb18800, 0xb18a00];

    private static ReadOnlySpan<ushort> BabyMetroidTileDestinations =>
        [0x7c00, 0x7d00, 0x7e00, 0x7f00];

    // `$A9:9003-$902E` replaces the four attack pages with six pieces of Mother Brain's
    // corpse. Although each source advances by `$200`, the transfer size is only `$1C0`:
    // the final two tile rows in every source page are deliberately skipped.
    private static ReadOnlySpan<uint> CorpseTileSources =>
        [0xb7ce00, 0xb7d000, 0xb7d200, 0xb7d400, 0xb7d600, 0xb7d800];

    private static ReadOnlySpan<ushort> CorpseTileDestinations =>
        [0x7a00, 0x7b00, 0x7c00, 0x7d00, 0x7e00, 0x7f00];

    // NTSC `$A6:C4CB-$C4FC`: two number pages followed by five typewriter-text pages.
    // The final text page is only `$100` bytes. ProcessSpriteTilesTransfers emits one
    // record per call and reports completion on the same call that emits entry six.
    private static readonly MotherBrainSpriteTileTransferRequest[] EscapeTimerTileTransfers =
    [
        new(0, 0x0200, 0xb0c000, 0x7e00),
        new(1, 0x0120, 0xb0c200, 0x7f00),
        new(2, 0x0200, 0xb7da00, 0x7820),
        new(3, 0x0200, 0xb7dc00, 0x7920),
        new(4, 0x0200, 0xb7de00, 0x7a20),
        new(5, 0x0200, 0xb7e000, 0x7b20),
        new(6, 0x0100, 0xb7e200, 0x7c20),
    ];

    // `$A9:902F-$903E` replaces the destroyed escape door's two sprite pages. Because the
    // escape-timer list falls through, entry zero is emitted on the timer list's final call.
    private static readonly MotherBrainSpriteTileTransferRequest[] ExplodedDoorTileTransfers =
    [
        new(0, 0x0200, 0xabf400, 0x7000),
        new(1, 0x0200, 0xabf600, 0x7100),
    ];

    // Seven records of four interleaved (X,Y) pairs at `$A9:B099-$B108`. The native
    // explosion index counts backward and wraps to six, so a zero-initialized sequence emits
    // record six first. Signed offsets are added to the body enemy's current world position.
    private static readonly (short X, short Y)[] DeathExplosionOffsets =
    [
        (0x0024, -0x0025), (-0x0013, -0x000f), (-0x0004, 0x000d), (0x001d, 0x0019),
        (0x0011, -0x0037), (0x001e, -0x0016), (-0x0003, -0x0005), (0x0000, 0x0028),
        (0x0034, -0x0022), (-0x0003, -0x000f), (0x000c, 0x0013), (0x0019, 0x002c),
        (0x0004, -0x002b), (-0x000c, -0x0016), (0x000d, -0x0002), (-0x0008, 0x0034),
        (-0x0002, -0x0021), (0x000a, -0x000a), (-0x000e, 0x0010), (0x0006, 0x003b),
        (0x0014, -0x0029), (0x0004, -0x0016), (-0x0014, 0x0003), (-0x001b, 0x0039),
        (0x000a, -0x001f), (-0x0014, -0x0008), (0x0000, 0x0017), (0x001e, 0x003d),
    ];

    // `$A9:BCA6/$BCB6` are indexed after incrementing the explosion index. Keeping all eight
    // signed records here preserves the initial index-zero -> record-one behavior.
    private static ReadOnlySpan<short> ExplosionXOffsets =>
        [-8, 6, -4, 2, 3, -6, 8, 0];

    private static ReadOnlySpan<short> ExplosionYOffsets =>
        [-7, 2, 5, -4, 6, -2, -6, 7];

    private readonly MotherBrainRainbowBeamSamusMovement _movement = new();
    private readonly MotherBrainCorpseRottingState _corpseRotting = new();

    /// <summary>
    /// Body enemy-slot animation state. The caller advances its enemy-instruction stage
    /// after <see cref="Step"/>, matching the native AI-then-instruction order.
    /// </summary>
    public MotherBrainBodyAnimationState Body { get; } = new();

    /// <summary>
    /// Shared row-by-row corpse graphics processor initialized by the brain enemy slot.
    /// Its WRAM table and graphics buffer remain public debugger evidence rather than being
    /// hidden behind a host-only opacity value.
    /// </summary>
    public MotherBrainCorpseRottingState CorpseRotting => _corpseRotting;

    /// <summary>Current body-function equivalent.</summary>
    public MotherBrainRainbowBeamAttackPhase Phase { get; private set; } =
        MotherBrainRainbowBeamAttackPhase.Inactive;

    /// <summary>Mother Brain brain-slot X used by `$A9:BB82`'s aim vector.</summary>
    public ushort BrainXPosition { get; set; }

    /// <summary>Mother Brain brain-slot Y used by `$A9:BB8F`'s aim vector.</summary>
    public ushort BrainYPosition { get; set; }

    /// <summary>Current brain instruction list installed through `$A9:C447`.</summary>
    public ushort HeadInstructionList { get; private set; }

    /// <summary>Brain instruction timer, reset to one whenever the AI changes its list.</summary>
    public ushort HeadInstructionTimer { get; private set; }

    /// <summary>
    /// Live bank-$A9 instruction cursor. <see cref="HeadInstructionList"/> retains the last
    /// list installed by body AI; this word advances through timed frames and command words.
    /// </summary>
    public ushort HeadInstructionPointer { get; private set; }

    /// <summary>Current Mother Brain head spritemap selected by a timed instruction pair.</summary>
    public ushort HeadSpritemapPointer { get; private set; }

    /// <summary>Byte-angle used by newly spawned Mother Brain blue-ring projectiles.</summary>
    public byte OnionRingTargetAngle { get; private set; }

    /// <summary>Native neck angular delta at Mother Brain body extra word `$0FBC`.</summary>
    public ushort NeckAngleDelta { get; private set; }

    /// <summary>Native enable-neck-movement flag.</summary>
    public ushort NeckMovementEnabled { get; private set; }

    /// <summary>Lower neck movement index selected by the current actor phase.</summary>
    public ushort LowerNeckMovementIndex { get; private set; }

    /// <summary>Upper neck movement index selected by the current actor phase.</summary>
    public ushort UpperNeckMovementIndex { get; private set; }

    /// <summary>Native body function timer at enemy word <c>$0FB0</c>.</summary>
    public ushort FunctionTimer { get; private set; }

    /// <summary>Rainbow-beam angular width, initialized and floored at <c>$0200</c>.</summary>
    public ushort AngularWidth { get; private set; }

    /// <summary>Native signed SFX queue countdown. Six produces seven queue attempts.</summary>
    public ushort SoundQueueCount { get; private set; }

    /// <summary>Rainbow-beam explosion timer at Mother Brain body extra word <c>$0FA8</c>.</summary>
    public ushort ExplosionTimer { get; private set; }

    /// <summary>Monotonic explosion index; only its low three bits select an offset pair.</summary>
    public ushort ExplosionIndex { get; private set; }

    /// <summary>Host-readable equivalent of the spawned rainbow-beam HDMA object's enable.</summary>
    public bool HdmaActive { get; private set; }

    /// <summary>Native rainbow-beam SFX-playing flag.</summary>
    public bool RainbowBeamSoundPlaying { get; private set; }

    /// <summary>
    /// Current byte angle produced by `$A9:BBA9` for the bank-$88 rainbow HDMA renderer.
    /// Exposing the translated movement owner's word lets the live room adapter publish the
    /// real beam geometry without maintaining a second aim calculation.
    /// </summary>
    public byte RainbowBeamAngle => _movement.RainbowBeamAngle;

    /// <summary>
    /// Lower neck angle updated by the brain-slot handler at <c>$A9:9072</c>. The final
    /// beam has already lowered it to <c>$3000</c> before the Baby interrupts the body.
    /// </summary>
    public ushort LowerNeckAngle { get; private set; } = 0x3000;

    /// <summary>Upper neck angle, correspondingly lowered to <c>$2000</c>.</summary>
    public ushort UpperNeckAngle { get; private set; } = 0x2000;

    /// <summary>Main brain-shake timer seeded to fifty by <c>$A9:BE96</c> when clear.</summary>
    public ushort BrainMainShakeTimer { get; private set; }

    /// <summary>Current zero-based painful-walk stage at WRAM <c>$7E:802A</c>.</summary>
    public ushort PainfulWalkingStage { get; private set; }

    /// <summary>Low-byte animation-delay selector derived from the current stage.</summary>
    public ushort PainfulWalkingAnimationDelay { get; private set; }

    /// <summary>Pause timer used after each completed forward/backward body list.</summary>
    public ushort PainfulWalkingFunctionTimer { get; private set; }

    /// <summary>True while the nested painful-walk function is on its forward half.</summary>
    public bool PainfulWalkingForward { get; private set; }

    /// <summary>Native brain-palette handling flag cleared when the rainbow beam expires.</summary>
    public bool BrainPaletteHandlingEnabled { get; private set; } = true;

    /// <summary>Drool generation flag cleared as Mother Brain enters low-power mode.</summary>
    public bool DroolGenerationEnabled { get; private set; } = true;

    /// <summary>Small-purple-breath flag cleared when the grey corpse is published.</summary>
    public bool SmallPurpleBreathGenerationEnabled { get; private set; } = true;

    /// <summary>
    /// Native Mother Brain health-based body-palette flag. Revival writes one only after
    /// the walk to X `$50` has really completed; it is not synonymous with the separate
    /// brain-slot palette handler above.
    /// </summary>
    public bool HealthBasedPaletteHandlingEnabled { get; private set; }

    /// <summary>Palette-transition record index at WRAM <c>$7E:802E</c>.</summary>
    public ushort GreyTransitionCounter { get; private set; }

    /// <summary>
    /// Cross-actor corpse handshake at WRAM <c>$7E:8030</c>. The Baby polls this exact
    /// word; value one means Mother Brain has completed the ninth grey-table probe.
    /// </summary>
    public ushort Phase2CorpseState { get; private set; }

    /// <summary>
    /// Saturating `$00-$0C` counter incremented by head instruction `$A9:9EA3` at the start
    /// of each four-ring attack against the Baby. It also selects the intended cry pitch.
    /// </summary>
    public ushort BabyMetroidAttackCounter { get; private set; }

    /// <summary>
    /// Number of live Mother Brain bomb enemy projectiles at body WRAM <c>$7E:802A</c>.
    /// </summary>
    /// <remarks>
    /// The counter belongs to Mother Brain even though bank <c>$86</c> owns each bomb's
    /// motion. Bomb initialization increments it and both native deletion paths decrement
    /// it. Keeping the mutation behind these two methods makes that cross-bank ownership
    /// visible instead of silently deriving a count from the host projectile collection.
    /// </remarks>
    public ushort BombCounter { get; private set; }

    /// <summary>Applies the wrapping 16-bit increment performed by <c>$86:C4BE-C4C3</c>.</summary>
    public void RegisterBombSpawn() => BombCounter = unchecked((ushort)(BombCounter + 1));

    /// <summary>Applies the wrapping 16-bit decrement shared by both bomb deletion paths.</summary>
    public void RegisterBombDeletion() => BombCounter = unchecked((ushort)(BombCounter - 1));

    /// <summary>Brain-slot health rewritten to 36,000 when corpse state one is published.</summary>
    public ushort BrainHealth { get; private set; } = 0x0bb8;

    /// <summary>Earthquake type word written by the one-frame delay and drain initializer.</summary>
    public ushort EarthquakeType { get; private set; }

    /// <summary>
    /// Earthquake timer as last written by this actor. The global earthquake updater owns
    /// independent per-frame decrements, so this class does not fabricate them.
    /// </summary>
    public ushort EarthquakeTimer { get; private set; }

    /// <summary>Samus projectile cooldown word written at beam shutdown.</summary>
    public ushort SamusProjectileCooldownTimer { get; private set; }

    /// <summary>
    /// Zero-based index of the next Baby Metroid sprite-tile transfer. Four means the
    /// terminating zero entry has been observed and the spawn handoff has run.
    /// </summary>
    public ushort BabyMetroidTileTransferIndex { get; private set; }

    /// <summary>
    /// Phase-three walking function selected through native long word <c>$7E:801E</c>.
    /// This is deliberately separate from the visible body instruction list: the function
    /// decides what Mother Brain wants to do only while the bytecode-owned pose is standing.
    /// </summary>
    public MotherBrainPhase3WalkingPhase Phase3WalkingPhase { get; private set; } =
        MotherBrainPhase3WalkingPhase.Inactive;

    /// <summary>
    /// Native phase-three walk counter at <c>$7E:8026</c>. Ordinary projectiles subtract
    /// <c>$0100</c>; Hyper Beam subtracts <c>$010A</c>; the walking function adds
    /// <c>$0020</c> whenever it gets a standing AI call.
    /// </summary>
    public ushort Phase3WalkCounter { get; private set; }

    /// <summary>Current phase-three walking target at <c>$7E:803A</c>.</summary>
    public ushort Phase3TargetXPosition { get; private set; }

    /// <summary>Phase-three neck function selected through native long word <c>$7E:801A</c>.</summary>
    public MotherBrainPhase3NeckPhase Phase3NeckPhase { get; private set; } =
        MotherBrainPhase3NeckPhase.Inactive;

    /// <summary>Signed-underflow timer shared by the two phase-three recoil functions.</summary>
    public ushort Phase3NeckFunctionTimer { get; private set; }

    /// <summary>
    /// Native attack-disable word at <c>$7E:803E</c>. Hyper Beam recoil sets one and the
    /// recoil timer clears it before the recovery pose begins.
    /// </summary>
    public ushort Phase3DisableAttacks { get; private set; }

    /// <summary>
    /// Exact body enemy property word touched by `$A9:AEE1` and `$A9:AFF7`. Keeping the raw
    /// flags avoids guessing names for engine-wide enemy-property bits before bank `$A0` is
    /// fully translated: death sets `$0400`, then fade completion sets `$0100` and clears
    /// `$2000` with the cartridge's literal OR/AND sequence.
    /// </summary>
    public ushort BodyProperties { get; private set; }

    /// <summary>Exact brain enemy property word; death/rotting sets raw bits `$0400/$0100`.</summary>
    public ushort BrainProperties { get; private set; }

    /// <summary>Second brain property word cleared when the rot animation completes.</summary>
    public ushort BrainProperties2 { get; private set; }

    /// <summary>Second body property word cleared when the faded body becomes non-interactive.</summary>
    public ushort BodyProperties2 { get; private set; }

    /// <summary>Shared Mother Brain hitbox-enable word cleared at both death boundaries.</summary>
    public bool HitboxesEnabled { get; private set; } = true;

    /// <summary>Death-explosion interval timer at the body extra word used by `$A9:B03E`.</summary>
    public ushort DeathExplosionIntervalTimer { get; private set; }

    /// <summary>Backward-cycling seven-record death-explosion index used by `$A9:B046`.</summary>
    public ushort DeathExplosionIndex { get; private set; }

    /// <summary>
    /// Backward-cycling four-record escape-door dust index at <c>$A9:B355</c>.
    /// This is a different native word from <see cref="DeathExplosionIndex"/> even though
    /// both effects reuse <see cref="DeathExplosionIntervalTimer"/>.
    /// </summary>
    public ushort EscapeDoorIndex { get; private set; }

    /// <summary>Palette selector forced to `$0E00` when the dying brain effects shut down.</summary>
    public ushort BrainPaletteIndex { get; private set; }

    /// <summary>Number of calls made to the body flicker producer at `$A9:AFB6`.</summary>
    public uint BodyFlickerCallCount { get; private set; }

    /// <summary>
    /// Host witness for the `$02C6..0` BG2 tilemap clear and following NMI transfer request.
    /// Rendering code can consume this without pretending the global WRAM tilemap is local.
    /// </summary>
    public bool EnemyBg2TilemapClearRequested { get; private set; }

    /// <summary>Index of the next of six `$A9:9003` corpse sprite-tile DMA records.</summary>
    public ushort CorpseTileTransferIndex { get; private set; }

    /// <summary>Index of the next NTSC escape-timer tile record at <c>$A6:C4CB</c>.</summary>
    public ushort EscapeTimerTileTransferIndex { get; private set; }

    /// <summary>Index of the next exploded-door tile record at <c>$A9:902F</c>.</summary>
    public ushort ExplodedDoorTileTransferIndex { get; private set; }

    /// <summary>Unpause-hook enable word cleared when the escape typewriter is installed.</summary>
    public bool MotherBrainUnpauseHookEnabled { get; private set; } = true;

    /// <summary>
    /// Set when `$A9:B11B` installs the brain-slot draw setup before the decapitated head
    /// starts falling. The separate brain AI is not silently folded into the body function.
    /// </summary>
    public bool BrainDrawSetupRequested { get; private set; }

    /// <summary>Rainbow-beam palette animation index reset immediately before the final shot.</summary>
    public ushort RainbowBeamPaletteAnimationIndex { get; private set; }

    /// <summary>True once `$A9:BE1B` has requested the cutscene Baby enemy population entry.</summary>
    public bool BabyMetroidSpawned { get; private set; }

    /// <summary>
    /// Executes the corpse-table and graphics-buffer half of head initialization
    /// <c>$A9:8705-$870B</c>. A real encounter calls this once when the brain enemy spawns.
    /// </summary>
    public void InitializeCorpseRotting(ISnesAddressSpace bus) => _corpseRotting.Initialize(bus);

    /// <summary>
    /// Starts the repeatable rainbow-beam cycle at `$A9:B8EB`, before its two charge waits.
    /// </summary>
    public void StartAttackCycle()
    {
        BabyMetroidTileTransferIndex = 0;
        BabyMetroidSpawned = false;
        RainbowBeamPaletteAnimationIndex = 0;
        BeginExtendingNeckForAttack();
    }

    /// <summary>
    /// Imports the physical room actor words before one live scheduler call. The standalone
    /// verifier may continue to own <see cref="Body"/> directly; gameplay instead lets the
    /// ordinary enemy bytecode move the real body and feeds those resulting words back here.
    /// This deliberately does not copy instruction pointers or timers: there must be only
    /// one visible animation interpreter in a live room.
    /// </summary>
    internal void SynchronizeLiveActor(
        ushort bodyX,
        ushort bodyY,
        MotherBrainBodyPose bodyPose,
        ushort form,
        ushort brainX,
        ushort brainY,
        ushort lowerNeckAngle,
        ushort upperNeckAngle,
        ushort bombCounter)
    {
        Body.XPosition = bodyX;
        Body.YPosition = bodyY;
        Body.Pose = (ushort)bodyPose;
        Body.Form = form;
        BrainXPosition = brainX;
        BrainYPosition = brainY;
        LowerNeckAngle = lowerNeckAngle;
        UpperNeckAngle = upperNeckAngle;
        BombCounter = bombCounter;
    }

    /// <summary>
    /// The list most recently requested by the state machine's body helper. Gameplay copies
    /// this pointer into its physical body slot only when the step result reports a walk or
    /// posture request; it never advances this private mirror's instruction timer.
    /// </summary>
    internal ushort RequestedBodyInstructionList => Body.InstructionPointer;

    /// <summary>
    /// Starts at the low-health handoff in `$A9:BB1A`, including its immediate really-slow
    /// forward-walk request. This is the exact debugger entry point reached after the `$BB06`
    /// decision timer; it does not skip the later `$BD45` health-selection loop.
    /// </summary>
    public void StartFinishOffSequence()
    {
        RequestWalkForwardReallySlow(unchecked((ushort)(Body.XPosition + 0x0010)));
        BabyMetroidTileTransferIndex = 0;
        BabyMetroidSpawned = false;
        Phase = MotherBrainRainbowBeamAttackPhase.FinishSamusOff;
    }

    /// <summary>
    /// Starts at `$A9:B983`, immediately after the power-bomb gate and charge countdown.
    /// </summary>
    public void StartActiveBeam(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        SetHeadInstructionList(HeadFiringRainbowInstructionList);
        AngularWidth = 0x0200;
        ExplosionIndex = 0;
        ExplosionTimer = 0;
        SoundQueueCount = 6;
        RainbowBeamSoundPlaying = false;
        HdmaActive = true;
        EarthquakeType = 0;
        EarthquakeTimer = 0;
        SamusProjectileCooldownTimer = 0;
        NeckAngleDelta = 0x0040;
        NeckMovementEnabled = 1;
        LowerNeckMovementIndex = 2;
        UpperNeckMovementIndex = 4;

        // `$A9:B9C5-$B9D3` selects command five at 700 energy or above and command `$18`
        // below it. CPY/BPL is a signed 16-bit branch, retained by NativeAtLeast.
        if (NativeAtLeast(samus.Health, 0x02bc))
            samus.Drained.SetupForRainbowBeamAbleToStand(bus, samus);
        else
            samus.Drained.SetupForRainbowBeamUnableToStand(bus, samus);

        Phase = MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall;
    }

    /// <summary>
    /// Ports the cross-enemy function-pointer write at <c>$A9:C8CD</c>. The Baby actor
    /// installs <c>$BE38</c> after it has reached the brain; Mother Brain executes that new
    /// function on her next enemy-AI turn rather than inside the Baby's current turn.
    /// </summary>
    public void InterruptFinalBeamForBabyDrain()
    {
        Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback;
        RainbowBeamSoundPlaying = true; // `$A9:C8DA-$C8DD` writes the shared SFX flag.
    }

    /// <summary>
    /// Ports the Baby's cross-enemy write at <c>$A9:CB23</c>. The native routine does not
    /// wait for Mother Brain's stand-up animation before repeatedly requesting the backward
    /// walk; the later Baby route decides when to replace this function with <c>$C19A</c>.
    /// </summary>
    public void PrepareForFinalBabyMetroidAttack() =>
        Phase = MotherBrainRainbowBeamAttackPhase.PrepareForFinalBabyMetroidAttack;

    /// <summary>
    /// Ports the Baby's cross-enemy write at <c>$A9:CBA8</c>. Mother Brain installs one last
    /// `$9DB1` four-ring head program and then leaves her body function at the native RTS.
    /// </summary>
    public void ExecuteFinalBabyMetroidAttack() =>
        Phase = MotherBrainRainbowBeamAttackPhase.ExecuteFinalBabyMetroidAttack;

    /// <summary>
    /// Ports the Baby's final cross-enemy write at <c>$A9:CD02-$CD05</c>. The body does
    /// not execute <c>$C1CF</c> inside the Baby's slot; increasing-slot enemy order makes
    /// this installed phase visible on Mother Brain's following frame.
    /// </summary>
    public void BeginPhase3RecoveryFromBabyCutscene()
    {
        Phase = MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance;

        // Native clears only the shared enemy-slot index. This host flag is the equivalent
        // liveness witness used by head attack selection after the actor has deleted itself.
        BabyMetroidSpawned = false;
    }

    /// <summary>
    /// Applies damage already calculated by the ordinary enemy-shot engine. Bank `$A0` owns
    /// beam/item damage multipliers, invulnerability, projectile deletion, and hit flashing;
    /// this actor owns only the resulting health word and the phase-three zero-health branch.
    /// Keeping that boundary explicit lets a live projectile producer call the real actor
    /// without embedding a guessed copy of the still-untranslated generic damage routine.
    /// </summary>
    public void ApplyCalculatedBrainDamage(ushort damage)
    {
        // Generic enemy damage saturates at zero. A host subtraction with ushort wrapping
        // would resurrect a nearly dead boss, so perform the borrow test before the write.
        BrainHealth = damage >= BrainHealth
            ? (ushort)0
            : unchecked((ushort)(BrainHealth - damage));
    }

    /// <summary>
    /// Applies the movement/recoil half of Mother Brain's shared phase-two/three shot
    /// reaction at <c>$A9:B562-$B5C4</c>. The ordinary enemy-shot routine owns damage,
    /// projectile deletion, and flash time; this method intentionally does not invent any
    /// of those still-untranslated systems.
    /// </summary>
    public void ApplyPhase2Or3ShotReaction(MotherBrainProjectileType projectileType)
    {
        // `$B58E` masks the projectile type to three bits before indexing an eight-byte
        // table. Validate the host enum so a caller cannot accidentally smuggle a larger
        // value past that native domain.
        if ((uint)projectileType > 7)
            throw new ArgumentOutOfRangeException(nameof(projectileType));

        // The table returns two for beams, one for missiles/supers, and zero for every
        // remaining projectile class. Form four gives only reaction type two the special
        // Hyper Beam path; ordinary phase-two beams continue through the generic branch.
        ushort reactionType = projectileType switch
        {
            MotherBrainProjectileType.Beam => 2,
            MotherBrainProjectileType.Missile or MotherBrainProjectileType.SuperMissile => 1,
            _ => 0,
        };
        if (Body.Form == 4 && reactionType == 2)
        {
            ushort candidate = unchecked((ushort)(Phase3WalkCounter - 0x010a));
            if ((candidate & 0x8000) == 0)
            {
                // BPL at `$B5B1` keeps the nonnegative remainder and does not recoil. This
                // is why sustained Hyper Beam fire first consumes accumulated walk credit.
                Phase3WalkCounter = candidate;
                return;
            }

            // On underflow the native accumulator is replaced by zero before the common
            // store: do not retain the wrapped subtraction. The neck function itself runs
            // on Mother Brain's next ordinary phase-three main call.
            Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil;
            FunctionTimer = 0;
            Phase3WalkCounter = 0;
            return;
        }

        // DEC turns reaction one into zero, sending either missile kind directly to the
        // zero label. Reaction zero wraps to `$FFFF`; reaction two outside form four leaves
        // one. Both nonzero cases subtract `$0100` and clamp signed underflow to zero.
        reactionType = unchecked((ushort)(reactionType - 1));
        if (reactionType == 0)
        {
            Phase3WalkCounter = 0;
            return;
        }

        ushort genericCandidate = unchecked((ushort)(Phase3WalkCounter - 0x0100));
        Phase3WalkCounter = (genericCandidate & 0x8000) == 0
            ? genericCandidate
            : (ushort)0;
    }

    /// <summary>
    /// Ports the Baby's <c>$A9:C879</c> call to the standard backwards-walk helper using
    /// animation-delay index two and target <c>Body.X-1</c>.
    /// </summary>
    public bool RequestBabyStumbleBackward()
    {
        ushort targetX = unchecked((ushort)(Body.XPosition - 1));
        if (HasReachedBackwardTarget(targetX) || Body.Pose != 0)
            return false;
        Body.SetInstructionList(BodyWalkingBackwardReallyFastInstructionList);
        return true;
    }

    /// <summary>
    /// Executes the movement half of <c>$A9:9072-$91B7</c> on Mother Brain's later brain
    /// enemy slot. Call this after the body AI/body instruction stage and before the still
    /// later Baby slot, matching the retail increasing-slot enemy loop.
    /// </summary>
    public void StepNeckMovement(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (NeckMovementEnabled != 0)
        {
            ushort lowerAngle = LowerNeckAngle;
            ushort upperAngle = UpperNeckAngle;
            ushort lowerIndex = LowerNeckMovementIndex;
            ushort upperIndex = UpperNeckMovementIndex;
            MotherBrainNeckKinematics.StepAngles(
                ref lowerAngle,
                ref upperAngle,
                ref lowerIndex,
                ref upperIndex,
                NeckAngleDelta,
                BrainYPosition,
                samus.YPosition);
            LowerNeckAngle = lowerAngle;
            UpperNeckAngle = upperAngle;
            LowerNeckMovementIndex = lowerIndex;
            UpperNeckMovementIndex = upperIndex;
        }

        MotherBrainNeckGeometry geometry = MotherBrainNeckKinematics.CalculateGeometry(
            bus,
            Body.XPosition,
            Body.YPosition,
            LowerNeckAngle,
            UpperNeckAngle);
        BrainXPosition = geometry.Segment4.X;
        BrainYPosition = geometry.Segment4.Y;
    }

    /// <summary>
    /// Executes the translated retail head instruction stage for the Baby-murder, phase-three
    /// bomb, and phase-three neutral lists.
    /// Call after the brain-slot neck AI and before the later Baby enemy slot. Commands run
    /// without consuming a frame until a duration/spritemap pair is loaded, matching the
    /// common enemy-instruction processor's old-timer-equals-one rule.
    /// </summary>
    public MotherBrainHeadAnimationStepResult StepHeadAnimation(
        ISnesAddressSpace bus,
        SamusState samus,
        BabyMetroidCutsceneState? baby,
        ushort randomNumberSeed = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        ushort pointerBefore = HeadInstructionPointer;
        ushort timerBefore = HeadInstructionTimer;
        bool loadedFrame = false;
        bool attackCounterIncremented = false;
        bool attackCounterReset = false;
        ushort? queuedSoundLibraryTwo = null;
        ushort? queuedSoundLibraryThree = null;
        MotherBrainOnionRingSpawnRequest? onionRing = null;
        MotherBrainBombSpawnRequest? bomb = null;
        bool purpleBreathBigSpawnRequested = false;

        // Other rainbow/corpse lists are still represented only by their installed pointer.
        // Accept precisely the three contiguous native ranges translated here. In particular,
        // `$9F00` must run even after the cutscene Baby has deleted itself: phase-three bombs
        // are ordinary combat attacks and have no Baby dependency.
        bool isPhaseThreeNeutral = HeadInstructionPointer is >= 0x9cb9 and <= 0x9ce1;
        bool isBabyMurderOrFourRings = HeadInstructionPointer is >= 0x9db1 and <= 0x9df5;
        bool isPhaseThreeBomb = HeadInstructionPointer is >= 0x9f00 and <= 0x9f32;
        if (!isPhaseThreeNeutral && !isBabyMurderOrFourRings && !isPhaseThreeBomb)
            return CreateResult();

        ushort oldTimer = HeadInstructionTimer;
        HeadInstructionTimer = unchecked((ushort)(HeadInstructionTimer - 1));
        if (oldTimer != 1)
            return CreateResult();

        for (int commandCount = 0; commandCount < 24; commandCount++)
        {
            ushort word = ReadBankA9Word(bus, HeadInstructionPointer);
            if ((word & 0x8000) == 0)
            {
                HeadInstructionTimer = word;
                HeadSpritemapPointer = ReadBankA9Word(
                    bus,
                    unchecked((ushort)(HeadInstructionPointer + 2)));
                HeadInstructionPointer = unchecked((ushort)(HeadInstructionPointer + 4));
                loadedFrame = true;
                return CreateResult();
            }

            ushort commandAddress = HeadInstructionPointer;
            HeadInstructionPointer = unchecked((ushort)(HeadInstructionPointer + 2));
            switch (word)
            {
                case 0x9ea3: // Increment and saturate Baby attack counter at twelve.
                    BabyMetroidAttackCounter = Math.Min(
                        unchecked((ushort)(BabyMetroidAttackCounter + 1)),
                        (ushort)0x000c);
                    attackCounterIncremented = true;
                    break;

                case 0x9eb5: // Samus-target list explicitly resets the Baby counter.
                    BabyMetroidAttackCounter = 0;
                    attackCounterReset = true;
                    break;

                case 0x9b20: // Disable neck movement.
                    NeckMovementEnabled = 0;
                    break;

                case 0x9e37: // Aim rings at the Baby's live enemy position.
                    if (baby is null)
                    {
                        throw new InvalidOperationException(
                            "Mother Brain's Baby-targeting head opcode ran without the Baby enemy slot.");
                    }
                    AimOnionRings(
                        unchecked((short)(baby.XPosition - BrainXPosition - 0x000a)),
                        unchecked((short)(baby.YPosition - BrainYPosition - 0x0010)));
                    break;

                case 0x9e5b: // Fallback list aims the identical program at Samus.
                    AimOnionRings(
                        unchecked((short)(samus.XPosition - BrainXPosition - 0x000a)),
                        unchecked((short)(samus.YPosition - BrainYPosition - 0x0010)));
                    break;

                case 0x9b0f: // Unconditional go-to operand.
                    HeadInstructionPointer = ReadBankA9Word(bus, HeadInstructionPointer);
                    break;

                case 0x9b14: // Enable neck movement and go to operand.
                    NeckMovementEnabled = 1;
                    HeadInstructionPointer = ReadBankA9Word(bus, HeadInstructionPointer);
                    break;

                case 0x9df7: // Intended counter-indexed cry; retail bug always reads entry 0.
                    if (BabyMetroidAttackCounter != 0x000b)
                        queuedSoundLibraryTwo = 0x006f;
                    break;

                case 0x9e29: // Spawn one `$86:CB4B` blue-ring enemy projectile.
                    onionRing = new MotherBrainOnionRingSpawnRequest(OnionRingTargetAngle);
                    break;

                case 0x9b32: // Queue sound [[X]], library three; consume its operand.
                    queuedSoundLibraryThree = ReadBankA9Word(bus, HeadInstructionPointer);
                    HeadInstructionPointer = unchecked((ushort)(HeadInstructionPointer + 2));
                    break;

                case 0x9b28: // Queue sound [[X]], library two; consume its operand.
                    queuedSoundLibraryTwo = ReadBankA9Word(bus, HeadInstructionPointer);
                    HeadInstructionPointer = unchecked((ushort)(HeadInstructionPointer + 2));
                    break;

                case 0x9ebd: // Spawn `$86:CB59`; operand is the later afterburn count.
                    bomb = new MotherBrainBombSpawnRequest(
                        ReadBankA9Word(bus, HeadInstructionPointer));
                    HeadInstructionPointer = unchecked((ushort)(HeadInstructionPointer + 2));
                    break;

                case 0x9b6d: // Spawn the large purple-breath accompaniment.
                    purpleBreathBigSpawnRequested = true;
                    break;

                case 0x9d0d: // Retail's unconditional BRA skips its tempting cry branch.
                    // The processor has already advanced X past the opcode to `$9CDB`.
                    // Only low-twelve-bit values below `$EC0` replace X with `$9CD1`.
                    if ((randomNumberSeed & 0x0fff) < 0x0ec0)
                        HeadInstructionPointer = 0x9cd1;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported translated Mother Brain head instruction ${word:X4} " +
                        $"at $A9:{commandAddress:X4}.");
            }
        }

        throw new InvalidOperationException(
            "Mother Brain head list did not reach a timed frame within 24 commands.");

        MotherBrainHeadAnimationStepResult CreateResult() => new(
            pointerBefore,
            HeadInstructionPointer,
            timerBefore,
            HeadInstructionTimer,
            HeadSpritemapPointer,
            loadedFrame,
            attackCounterIncremented,
            attackCounterReset,
            OnionRingTargetAngle,
            onionRing,
            bomb,
            purpleBreathBigSpawnRequested,
            queuedSoundLibraryTwo,
            queuedSoundLibraryThree);
    }

    /// <summary>
    /// Executes the body-owned shake countdown consumed while the later graphics hook draws
    /// Mother Brain's brain at <c>$A9:9382-$939A</c>. Call after all enemy slots, matching the
    /// renderer: `$BE96` can then observe zero and reseed fifty on the following frame.
    /// </summary>
    public ushort StepBrainShakeForDraw()
    {
        if (BrainMainShakeTimer != 0)
            BrainMainShakeTimer = unchecked((ushort)(BrainMainShakeTimer - 1));
        return unchecked((ushort)(BrainMainShakeTimer & 6));
    }

}
