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
public sealed class MotherBrainRainbowBeamAttackSequence
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
            // The lower handler runs first. Its new angle/index is immediately visible to
            // the upper handler on this same brain-slot call.
            switch (LowerNeckMovementIndex)
            {
                case 0:
                    break;
                case 2: // Bob down toward `$2800`, then reverse upward.
                {
                    ushort candidate = unchecked((ushort)(LowerNeckAngle - NeckAngleDelta));
                    if (candidate < 0x2800)
                    {
                        candidate = 0x2800;
                        LowerNeckMovementIndex = 4;
                    }
                    LowerNeckAngle = candidate;
                    break;
                }
                case 4: // Bob up toward `$9000`, unless the brain is already high.
                    if (unchecked((short)(BrainYPosition - 0x003c)) < 0)
                    {
                        LowerNeckMovementIndex = 2;
                    }
                    else
                    {
                        ushort candidate = unchecked((ushort)(LowerNeckAngle + NeckAngleDelta));
                        if (candidate >= 0x9000)
                        {
                            candidate = 0x9000;
                            LowerNeckMovementIndex = 2;
                        }
                        LowerNeckAngle = candidate;
                    }
                    break;
                case 6: // One-way lower used by rainbow-beam setup.
                {
                    ushort candidate = unchecked((ushort)(LowerNeckAngle - NeckAngleDelta));
                    if (candidate < 0x3000)
                    {
                        candidate = 0x3000;
                        LowerNeckMovementIndex = 0;
                    }
                    LowerNeckAngle = candidate;
                    break;
                }
                case 8: // One-way raise used by the Baby interruption.
                {
                    ushort candidate = unchecked((ushort)(LowerNeckAngle + NeckAngleDelta));
                    if (candidate >= 0x9000)
                    {
                        candidate = 0x9000;
                        LowerNeckMovementIndex = 0;
                    }
                    LowerNeckAngle = candidate;
                    break;
                }
                default:
                    throw new InvalidOperationException(
                        $"Unsupported lower-neck movement index ${LowerNeckMovementIndex:X4}.");
            }

            switch (UpperNeckMovementIndex)
            {
                case 0:
                    break;
                case 2: // Bob down; Samus below the brain forces both segments upward instead.
                    if (unchecked((short)(BrainYPosition + 4 - samus.YPosition)) >= 0)
                    {
                        LowerNeckMovementIndex = 4;
                        UpperNeckMovementIndex = 4;
                    }
                    else
                    {
                        ushort candidate = unchecked((ushort)(UpperNeckAngle - NeckAngleDelta));
                        if (candidate < 0x2000)
                        {
                            candidate = 0x2000;
                            UpperNeckMovementIndex = 4;
                        }
                        UpperNeckAngle = candidate;
                    }
                    break;
                case 4: // Follow eight angle-units above the lower segment, then bob down.
                {
                    ushort target = unchecked((ushort)(LowerNeckAngle + 0x0800));
                    ushort candidate = unchecked((ushort)(UpperNeckAngle + NeckAngleDelta));
                    if (candidate >= target)
                    {
                        candidate = target;
                        UpperNeckMovementIndex = 2;
                    }
                    UpperNeckAngle = candidate;
                    break;
                }
                case 6: // One-way lower to `$2000`.
                {
                    ushort candidate = unchecked((ushort)(UpperNeckAngle - NeckAngleDelta));
                    if (candidate < 0x2000)
                    {
                        candidate = 0x2000;
                        UpperNeckMovementIndex = 0;
                    }
                    UpperNeckAngle = candidate;
                    break;
                }
                case 8: // One-way raise to the newly updated lower angle plus `$0800`.
                {
                    ushort target = unchecked((ushort)(LowerNeckAngle + 0x0800));
                    ushort candidate = unchecked((ushort)(UpperNeckAngle + NeckAngleDelta));
                    if (candidate >= target)
                    {
                        candidate = target;
                        UpperNeckMovementIndex = 0;
                    }
                    UpperNeckAngle = candidate;
                    break;
                }
                default:
                    throw new InvalidOperationException(
                        $"Unsupported upper-neck movement index ${UpperNeckMovementIndex:X4}.");
            }
        }

        // `$91B8-$92AA` anchors segment two at body+(32,-50), then adds two twenty-pixel
        // signed sine/cosine vectors. The brain enemy slot is segment four, so every Baby
        // latch/shake target sees these newly computed whole-pixel coordinates.
        byte lowerAngle = unchecked((byte)(LowerNeckAngle >> 8));
        byte upperAngle = unchecked((byte)(UpperNeckAngle >> 8));
        BrainXPosition = unchecked((ushort)(
            Body.XPosition + 0x0020 +
            CalculateSignedNeckComponent(bus, lowerAngle, 0x0014) +
            CalculateSignedNeckComponent(bus, upperAngle, 0x0014)));
        BrainYPosition = unchecked((ushort)(
            Body.YPosition - 0x0032 +
            CalculateSignedNeckComponent(bus, unchecked((byte)(lowerAngle + 0x40)), 0x0014) +
            CalculateSignedNeckComponent(bus, unchecked((byte)(upperAngle + 0x40)), 0x0014)));
    }

    /// <summary>
    /// Executes the retail head instruction stage for the `$9DB1-$9DF5` Baby-murder lists.
    /// Call after the brain-slot neck AI and before the later Baby enemy slot. Commands run
    /// without consuming a frame until a duration/spritemap pair is loaded, matching the
    /// common enemy-instruction processor's old-timer-equals-one rule.
    /// </summary>
    public MotherBrainHeadAnimationStepResult StepBabyMurderHeadAnimation(
        ISnesAddressSpace bus,
        SamusState samus,
        BabyMetroidCutsceneState baby)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(baby);

        ushort pointerBefore = HeadInstructionPointer;
        ushort timerBefore = HeadInstructionTimer;
        bool loadedFrame = false;
        bool attackCounterIncremented = false;
        bool attackCounterReset = false;
        ushort? queuedSoundLibraryTwo = null;
        ushort? queuedSoundLibraryThree = null;
        MotherBrainOnionRingSpawnRequest? onionRing = null;

        // Other translated rainbow/corpse lists are still represented by their installed
        // pointer only. Do not let this specialised processor reinterpret their opcodes as
        // Baby-murder commands merely because their ordinary timer happens to reach one.
        if (HeadInstructionPointer < 0x9db1 || HeadInstructionPointer > 0x9df5)
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

                default:
                    throw new InvalidOperationException(
                        $"Unsupported Mother Brain Baby-murder head instruction ${word:X4} " +
                        $"at $A9:{commandAddress:X4}.");
            }
        }

        throw new InvalidOperationException(
            "Mother Brain Baby-murder head list did not reach a timed frame within 24 commands.");

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

    /// <summary>Executes one call through the current Mother Brain body function.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort enemyFrameCounter,
        ushort mainEnemyExecutionCounter,
        bool powerBombActive = false,
        ushort randomNumberSeed = 0,
        Func<ushort>? nextRandomNumber = null,
        bool alternateEscapeText = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (Phase == MotherBrainRainbowBeamAttackPhase.Inactive)
            throw new InvalidOperationException("The active rainbow-beam sequence has not started.");

        MotherBrainRainbowBeamAttackPhase phaseBefore = Phase;
        MotherBrainForcedSamusMovementResult? movement = null;
        bool soundQueued = false;
        bool paletteRequested = false;
        MotherBrainRainbowExplosionRequest? explosion = null;
        ushort healthBefore = samus.Health;
        ushort missilesBefore = samus.Missiles;
        ushort supersBefore = samus.SuperMissiles;
        ushort bombsBefore = samus.PowerBombs;
        bool unlockedSamus = false;
        bool chargeSoundQueued = false;
        bool bodyWalkRequested = false;
        bool bodyPostureRequested = false;
        MotherBrainFinishOffAttackKind? finishOffAttack = null;
        MotherBrainSpriteTileTransferRequest? spriteTileTransfer = null;
        bool babySpawnRequested = false;
        bool finalBeamSoundQueued = false;
        MotherBrainPhase3AttackKind? phase3Attack = null;
        var deathExplosions = new List<MotherBrainDeathExplosionRequest>(capacity: 4);
        var corpseRottingVramTransfers =
            new List<MotherBrainSpriteTileTransferRequest>(capacity: 6);
        var corpseDustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);
        bool musicStopQueued = false;
        bool escapeMusicQueued = false;
        var escapeSequenceTileTransfers =
            new List<MotherBrainSpriteTileTransferRequest>(capacity: 2);
        bool explodedDoorPaletteRequested = false;
        bool escapeMusicTrackQueued = false;
        var escapePaletteFxRequests = new List<ushort>(capacity: 4);
        bool escapeTypewriterSetupRequested = false;

        switch (Phase)
        {
            case MotherBrainRainbowBeamAttackPhase.RepeatAttack:
                // `$A9:BB13` only installs `$B8EB`; the setup executes on the next enemy
                // AI call and deliberately does not fall through into its 256 timer.
                BeginExtendingNeckForAttack();
                break;

            case MotherBrainRainbowBeamAttackPhase.StartCharging:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetHeadInstructionList(HeadChargingRainbowInstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.RetractNeck;
                    goto case MotherBrainRainbowBeamAttackPhase.RetractNeck;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.RetractNeck:
                bodyWalkRequested = RequestWalkBackwardReallySlow(targetX: 0x0028);
                if (HasReachedBackwardTarget(targetX: 0x0028))
                {
                    RetractHead();
                    Phase = MotherBrainRainbowBeamAttackPhase.WaitForCharge;
                    FunctionTimer = 0x0100;
                    goto case MotherBrainRainbowBeamAttackPhase.WaitForCharge;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.WaitForCharge:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    chargeSoundQueued = true; // Sound library two, effect `$71`.
                    Phase = MotherBrainRainbowBeamAttackPhase.ExtendNeckDown;
                    goto case MotherBrainRainbowBeamAttackPhase.ExtendNeckDown;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.ExtendNeckDown:
                SamusProjectileCooldownTimer = 8;
                LowerNeckMovementIndex = 6;
                UpperNeckMovementIndex = 6;
                NeckAngleDelta = 0x0500; // NTSC value of regional `$0500/$0700`.
                Phase = MotherBrainRainbowBeamAttackPhase.StartFiring;
                FunctionTimer = 0x0010; // NTSC value of regional `$0010/$000C`.
                goto case MotherBrainRainbowBeamAttackPhase.StartFiring;

            case MotherBrainRainbowBeamAttackPhase.StartFiring:
                // `$A9:B975` widens/aims even while an active power bomb freezes the timer.
                IncreaseWidthAndAim(samus);
                if (!powerBombActive)
                {
                    FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                    if ((FunctionTimer & 0x8000) != 0)
                    {
                        // The cartridge redundantly rereads the power-bomb flag here. The
                        // parameter is one stable WRAM sample for this translated AI call.
                        SamusProjectileCooldownTimer = 0;
                        StartActiveBeam(bus, samus);
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall:
                RunContinuingBeamEffects(
                    samus,
                    enemyFrameCounter,
                    increaseWidth: true,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                movement = _movement.MoveTowardWall(bus, samus);
                if (movement.Value.NativeCarry)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.OneFrameDelay;
                    FunctionTimer = 0;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.OneFrameDelay:
                RunContinuingBeamEffects(
                    samus,
                    enemyFrameCounter,
                    increaseWidth: true,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                movement = _movement.MoveTowardWall(bus, samus);

                // `$A9:BA0F` decrements zero to `$FFFF`; BPL fails on this very call.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    EarthquakeType = 8;
                    EarthquakeTimer = 8;
                    Phase = MotherBrainRainbowBeamAttackPhase.StartDrainingSamus;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.StartDrainingSamus:
                // `$A9:BA27` installs draining, seeds both timers with `$012B`, and falls
                // through into the draining function in the same enemy-processing call.
                Phase = MotherBrainRainbowBeamAttackPhase.DrainingSamus;
                FunctionTimer = 0x012b;
                EarthquakeTimer = 0x012b;
                EarthquakeType = 8;
                goto case MotherBrainRainbowBeamAttackPhase.DrainingSamus;

            case MotherBrainRainbowBeamAttackPhase.DrainingSamus:
                RunContinuingBeamEffects(
                    samus,
                    enemyFrameCounter,
                    increaseWidth: true,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                DamageSamusDueToRainbowBeam(samus);
                DecrementAmmoDueToRainbowBeam(samus, mainEnemyExecutionCounter);
                movement = _movement.MoveTowardMiddleOfWall(samus);

                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.FinishFiring;
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishFiring:
                RunContinuingBeamEffects(
                    samus,
                    enemyFrameCounter,
                    increaseWidth: false,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);

                // `$A9:BA6A-$BA7E` narrows first, then compares the wrapped signed result
                // with `$0200`. Width is pinned only after it crosses below that floor.
                AngularWidth = unchecked((ushort)(AngularWidth - 0x0180));
                if (!NativeAtLeast(AngularWidth, 0x0200))
                {
                    AngularWidth = 0x0200;
                    _movement.BeginFallingAfterRainbowBeam();
                    HdmaActive = false;
                    EarthquakeTimer = 0;
                    RainbowBeamSoundPlaying = false;
                    samus.InputLocked = false; // Samus command one at `$90:F117`.
                    unlockedSamus = true;
                    SamusProjectileCooldownTimer = 8;
                    Phase = MotherBrainRainbowBeamAttackPhase.LetSamusFall;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.LetSamusFall:
                // Controller zero changes the pose/radius, then `$A9:BACB` installs the wait
                // function and falls straight through to its first custom movement call.
                samus.Drained.LetFall(bus, samus);
                Phase = MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand;
                goto case MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand;

            case MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand:
                movement = _movement.StepFallingAfterRainbowBeam(samus);
                if (movement.Value.NativeCarry)
                    Phase = MotherBrainRainbowBeamAttackPhase.LowerHead;
                break;

            case MotherBrainRainbowBeamAttackPhase.LowerHead:
                // Neck words are presentation/actor-animation state. This phase's movement-
                // relevant effect is the exact function/timer handoff to `$A9:BB06`.
                Phase = MotherBrainRainbowBeamAttackPhase.DecideNextAction;
                FunctionTimer = 0x0080;
                break;

            case MotherBrainRainbowBeamAttackPhase.DecideNextAction:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    if (NativeAtLeast(samus.Health, 0x0190))
                    {
                        Phase = MotherBrainRainbowBeamAttackPhase.RepeatAttack;
                    }
                    else
                    {
                        // `$A9:BB1A-$BB24` performs this call before installing the later
                        // finish-off function. It starts one complete really-slow forward
                        // body animation when Mother Brain is standing and left of `$80`.
                        bodyWalkRequested = RequestWalkForwardReallySlow(
                            unchecked((ushort)(Body.XPosition + 0x0010)));
                        BabyMetroidTileTransferIndex = 0;
                        BabyMetroidSpawned = false;
                        Phase = MotherBrainRainbowBeamAttackPhase.FinishSamusOff;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishSamusOff:
                // `$A9:BD45-$BD54` asks whether one more ordinary phase-two hit could put
                // Samus at the encounter's intended low-energy floor. The first nominal
                // damage is `$50`, suit-divided, multiplied by four, then padded by twenty.
                // The native BPL comparison treats equality as done and changes the body
                // function without falling into the stand-up handler on this call.
                if (NativeAtLeast(CalculateFinishOffHealthThreshold(samus, 0x0050), samus.Health))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.FinishStandUp;
                    break;
                }

                // Above the floor, 4000/4096 low-twelve-bit RNG values do nothing. Every
                // 32nd enemy frame that idle route may ask the posture helper to alternate
                // between standing and leaning down. This is visible body bytecode, not a
                // direct pose assignment, so retain the separate instruction-stage delay.
                if ((randomNumberSeed & 0x0fff) < 0x0fa0)
                {
                    if ((enemyFrameCounter & 0x001f) == 0)
                        bodyPostureRequested = MaybeRequestStandUpOrLeanDown(randomNumberSeed);
                    break;
                }

                // A weaker `$A0` nominal-hit threshold biases wounded Samus toward onion
                // rings. Only when Samus is above it does the top 16/4096 RNG band choose a
                // bomb; all remaining admitted values still choose two onion rings.
                if (NativeAtLeast(CalculateFinishOffHealthThreshold(samus, 0x00a0), samus.Health) ||
                    (randomNumberSeed & 0x0fff) < 0x0ff0)
                {
                    SetHeadInstructionList(HeadAttackingTwoOnionRingsPhase2InstructionList);
                    finishOffAttack = MotherBrainFinishOffAttackKind.TwoOnionRings;
                }
                else
                {
                    SetHeadInstructionList(HeadAttackingBombPhase2InstructionList);
                    finishOffAttack = MotherBrainFinishOffAttackKind.Bomb;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishStandUp:
                // `$A9:C670` reports carry only if pose was already zero at call entry.
                // Crouched/leaning poses request their matching stand animation and return
                // clear; walking/transition poses simply wait for their current bytecode.
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone;
                    FunctionTimer = 0x0010;
                    goto case MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone:
                // Stand-up falls through here, so the freshly written `$10` becomes `$0F`
                // on the same AI call. BPL accepts zero and expires only after underflow.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetHeadInstructionList(HeadStretchingPhase2InstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam;
                    FunctionTimer = 0x0100;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetHeadInstructionList(HeadChargingRainbowInstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles;
                    goto case MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles:
                // `$A9:C5BE` processes exactly one seven-byte table entry per call. The
                // fourth entry sees the following zero terminator, clears its saved pointer,
                // and returns carry set on that same call.
                spriteTileTransfer = CreateNextBabyMetroidTileTransfer();
                if (BabyMetroidTileTransferIndex == BabyMetroidTileSources.Length)
                {
                    RetractHead();
                    BabyMetroidSpawned = true;
                    babySpawnRequested = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam;
                    FunctionTimer = 0x0100;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    RainbowBeamPaletteAnimationIndex = 0;
                    SetHeadInstructionList(HeadFiringRainbowInstructionList);
                    LowerNeckMovementIndex = 6;
                    UpperNeckMovementIndex = 6;
                    NeckAngleDelta = 0x0500;
                    finalBeamSoundQueued = true; // Sound library two, effect `$71`.
                    Phase = MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding:
                // `$A9:BE14` stores the address of its own RTS. Mother Brain remains here
                // until the independently spawned Baby actor overwrites her body function
                // with `$BE38` after latching onto her head.
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback:
                // `$A9:BE38` is not executed by the Baby. It runs on Mother Brain's next
                // actor turn, changes form/neck geometry, seeds `$30`, and falls through so
                // that this first regain-balance call immediately decrements it to `$2F`.
                Body.Form = 3;
                LowerNeckMovementIndex = 8;
                UpperNeckMovementIndex = 8;
                NeckAngleDelta = 0x0700;
                Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance;
                FunctionTimer = 0x0030;
                goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance:
                paletteRequested = true;
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    // `$BE80-$BE83` contains an apparent missing STA: it loads one and then
                    // immediately reloads the neck-enable flag. Preserve that no-op rather
                    // than silently "fixing" the cartridge.
                    PainfulWalkingForward = true;
                    PainfulWalkingStage = 0;
                    PainfulWalkingAnimationDelay = 2;
                    PainfulWalkingFunctionTimer = 0;
                    LowerNeckMovementIndex = 2;
                    UpperNeckMovementIndex = 4;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam:
                // `$BE96` refreshes the shake timer only if another subsystem has cleared it.
                // The head animation owns any decrement; retaining the write gate avoids
                // fabricating a body-side countdown that does not exist in this routine.
                if (BrainMainShakeTimer == 0)
                    BrainMainShakeTimer = 0x0032;
                paletteRequested = true;
                bodyWalkRequested = StepPainfulWalking();

                // The outer function updates delay and neck delta after *every* nested call.
                // Stage six is therefore observed on the exact call which ends its prior
                // pause, before the stage-six forward walk has even begun.
                if (PainfulWalkingStage < PainfulWalkingAnimationDelays.Length)
                {
                    PainfulWalkingAnimationDelay =
                        PainfulWalkingAnimationDelays[PainfulWalkingStage];
                    NeckAngleDelta = PainfulWalkingNeckAngleDeltas[PainfulWalkingStage];
                }
                if (PainfulWalkingStage == 6)
                {
                    RainbowBeamSoundPlaying = false;
                    BrainPaletteHandlingEnabled = false;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut;
                    soundQueued = true; // Sound library one, effect `$02`.
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut:
                bodyWalkRequested = StepPainfulWalking();
                if (PainfulWalkingStage >= 8)
                {
                    NeckAngleDelta = 0x0040;
                    LowerNeckMovementIndex = 8;
                    UpperNeckMovementIndex = 8;
                    SetHeadInstructionList(HeadDyingDroolInstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom;
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom:
                // `$BF41` does not reload Y before the shared walk helper. The surrounding
                // sequence's final painful-animation selector is `$000A`, so the observable
                // retail list is the really-slow backward program at `$993A`.
                bodyWalkRequested = RequestWalkBackwardReallySlow(targetX: 0x0028);
                if (HasReachedBackwardTarget(targetX: 0x0028))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode;
                    UpperNeckMovementIndex = 0;
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode:
                // The brain slot independently advances the one-way neck raises. The body
                // must wait for both indices and its walk pose to return to zero.
                if ((LowerNeckMovementIndex | UpperNeckMovementIndex) == 0 && Body.Pose == 0)
                {
                    DroolGenerationEnabled = false;
                    Body.SetInstructionList(BodyCrouchingFastInstructionList);
                    bodyPostureRequested = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidPrepareTransitionToGrey;
                    FunctionTimer = 0x0040;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidPrepareTransitionToGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
                    FunctionTimer = 0x0010;
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    FunctionTimer = 0x0010;

                    // Bank `$AD:EF4A` has eight nonzero palette pointers followed by zero.
                    // The caller increments its counter, passes the old index, and treats
                    // the ninth (index-eight) probe as completion without copying colours.
                    ushort paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex >= 8)
                    {
                        BrainHealth = 0x8ca0;
                        Phase2CorpseState = 1;
                        SmallPurpleBreathGenerationEnabled = false;
                        Body.Form = 2;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey:
                // `$C059` only installs the next function and its `$0300` timer. It does not
                // fall through, so the first pre-decrement belongs to the following call.
                Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfShowSignsOfLife;
                FunctionTimer = 0x0300;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfShowSignsOfLife:
                // BPL accepts zero: 768 reaches zero without advancing and only the 769th
                // call underflows to `$FFFF`. Revival then re-enables both breath producers,
                // seeds `$E0`, and falls through two function installations to `$C08F`.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SmallPurpleBreathGenerationEnabled = true;
                    DroolGenerationEnabled = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey;
                    FunctionTimer = 0x00e0;
                    GreyTransitionCounter = 0;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    FunctionTimer = 0x0010;

                    // `$AD:ED9C` reverses the same eight grey palettes used by the corpse
                    // transition and follows them with zero. The body counter is incremented
                    // before the old index is passed, so index eight is a terminating probe
                    // with no copy. The existing result flag exposes each real copy cadence.
                    ushort paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex >= 8)
                    {
                        BrainPaletteHandlingEnabled = true;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp;
                        goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp:
                // `$C670` returns carry only when pose zero was already visible at entry.
                // A corpse pose therefore installs `$99C6` and waits for the ordinary body
                // instruction stage to publish standing before setting the neck/timer words.
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    LowerNeckMovementIndex = 6;
                    UpperNeckMovementIndex = 6;
                    NeckAngleDelta = 0x0500;
                    NeckMovementEnabled = 1;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch;
                    FunctionTimer = 0x0010;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetHeadInstructionList(HeadStretchingPhase3InstructionList);
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid;
                    FunctionTimer = 0x0080;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid:
                // Once `$80` underflows, the timer deliberately remains negative and the
                // helper is retried after every body-program call until X has overshot `$50`.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    bodyWalkRequested = RequestWalkForward(0x0050, 0x0004);
                    bool reachedWalkTarget = unchecked((short)(0x0050 - Body.XPosition)) < 0 ||
                        NativeAtLeast(Body.XPosition, 0x0080);
                    if (reachedWalkTarget)
                    {
                        Phase2CorpseState = 2;
                        HealthBasedPaletteHandlingEnabled = true;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfPrepareNeckForBabyMetroidDeath;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfPrepareNeckForBabyMetroidDeath:
                // `$C11E` is a one-call setup function which immediately falls into `$C147`.
                // The separate body instruction stage may still be finishing the last walk.
                BabyMetroidAttackCounter = 0;
                NeckMovementEnabled = 1;
                LowerNeckMovementIndex = 2;
                UpperNeckMovementIndex = 4;
                NeckAngleDelta = 0x0040;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath;
                goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath:
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                    bodyWalkRequested = RequestWalkForward(0x0050, 0x000a);
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack:
                bodyPostureRequested = MaybeRequestStandUpOrLeanDown(randomNumberSeed);
                if ((randomNumberSeed & 0x8000) != 0)
                {
                    // A nonzero Baby enemy index selects `$9DB1`; zero instead targets Samus.
                    // This translated cutscene owns a spawned Baby, so retain the live actor
                    // condition instead of unconditionally substituting the desired list.
                    SetHeadInstructionList(BabyMetroidSpawned
                        ? HeadAttackingBabyMetroidInstructionList
                        : (ushort)0x9dbb);
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttackCooldown;
                    FunctionTimer = 0x0040;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttackCooldown:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                break;

            case MotherBrainRainbowBeamAttackPhase.PrepareForFinalBabyMetroidAttack:
                // This native function has no state transition. It requests stand-up and a
                // fast backward walk toward `$40` every call until the Baby overwrites it.
                MakeBodyStandUp(out bodyPostureRequested);
                bodyWalkRequested |= RequestWalkBackward(0x0040, 0x0004);
                break;

            case MotherBrainRainbowBeamAttackPhase.ExecuteFinalBabyMetroidAttack:
                SetHeadInstructionList(HeadAttackingBabyMetroidInstructionList);
                Phase = MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding;
                break;

            case MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding:
                // `$C1A6` is the installed RTS. The last ring volley owns the remaining
                // health reduction and the Baby actor owns its death/recovery sequence.
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance:
                // `$C1CF` changes form before installing the setup wait. Its backward-walk
                // request uses target X-14 and delay index two; the ordinary helper may
                // legitimately reject it at the arena's hard left limit.
                Body.Form = 4;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting;
                FunctionTimer = 0x0020;
                bodyWalkRequested = RequestWalkBackward(
                    unchecked((ushort)(Body.XPosition - 0x000e)),
                    animationDelay: 0x0002);
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting:
                // `$20` remains valid through zero and expires only when DEC wraps negative,
                // yielding 33 setup calls after the one-frame `$C1CF` producer above.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                    Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;

                    // `$C1F0-$C205` has no RTS after installing the three function pointers;
                    // execution falls directly into `$C209` on this same enemy AI call.
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3FightingMain:
                // `$C209` tests death before either sub-handler. The death producer itself
                // is kept as an explicit seam until its explosion/fade sequence is ported;
                // no phase-two behavior is allowed to leak through here.
                if (BrainHealth == 0)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom;
                    break;
                }

                StepPhase3NeckHandler();
                bodyWalkRequested = StepPhase3WalkingHandler(randomNumberSeed);

                // Walking bytecode changes pose later in the enemy instruction stage. The
                // AI therefore still sees pose zero on the call that requests a walk and may
                // also select an attack, exactly as the source's post-handler `$C21B` load.
                if (Body.Pose != 0 || Phase3DisableAttacks != 0 ||
                    (randomNumberSeed & 0x8000) == 0)
                    break;

                if ((randomNumberSeed & 0x00ff) < 0x0080)
                {
                    SetHeadInstructionList(HeadAttackingBombPhase3InstructionList);
                    phase3Attack = MotherBrainPhase3AttackKind.Bomb;
                }
                else
                {
                    SetHeadInstructionList(HeadAttackingFourOnionRingsPhase3InstructionList);
                    phase3Attack = MotherBrainPhase3AttackKind.FourOnionRings;
                }

                Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown;
                FunctionTimer = 0x0040;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown:
                // `$C24E` accepts timer zero and returns to main only after underflow. It
                // does not fall through, so neck and walking handlers pause for all 65 calls.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom:
                // `$A9:AEE1` makes both enemy slots non-interactive and disables the shared
                // hitboxes on every call, then reuses the ordinary medium-speed backward
                // walk producer. Carry means X `$28` has been reached/overshot; the new `$80`
                // timer falls through so the first smoky explosion and decrement happen now.
                BodyProperties |= 0x0400;
                BrainProperties |= 0x0400;
                HitboxesEnabled = false;
                bodyWalkRequested = RequestWalkBackward(0x0028, animationDelay: 0x0006);
                if (HasReachedBackwardTarget(0x0028))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding;
                    FunctionTimer = 0x0080;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding:
                GenerateDeathExplosions(
                    mixed: false, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom:
                // `$AF21` emits smoke before asking for a really-fast forward walk to `$60`.
                // Equality still starts a complete body program; only signed overshoot or the
                // independent `$80` arena clamp reports carry and advances the death script.
                GenerateDeathExplosions(
                    mixed: false, nextRandomNumber, deathExplosions);
                bodyWalkRequested = RequestWalkForward(0x0060, animationDelay: 0x0002);
                if (unchecked((short)(0x0060 - Body.XPosition)) < 0 ||
                    NativeAtLeast(Body.XPosition, 0x0080))
                {
                    SetHeadInstructionList(HeadDyingDroolInstructionList);
                    LowerNeckMovementIndex = 6;
                    UpperNeckMovementIndex = 6;
                    NeckAngleDelta = 0x0500;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects;
                    FunctionTimer = 0x0020;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects:
                GenerateDeathExplosions(
                    mixed: false, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    // `$AF5C-$AF94` removes the neck/breath/palette producers, copies sprite
                    // palette 1 into palette 7, performs one health-palette refresh, and then
                    // deliberately falls through with the already-negative timer.
                    LowerNeckMovementIndex = 0;
                    UpperNeckMovementIndex = 0;
                    DroolGenerationEnabled = false;
                    SmallPurpleBreathGenerationEnabled = false;
                    BrainPaletteHandlingEnabled = false;
                    HealthBasedPaletteHandlingEnabled = false;
                    BrainPaletteIndex = 0x0e00;
                    paletteRequested = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut:
                GenerateDeathExplosions(
                    mixed: true, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody;
                    FunctionTimer = 0;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody:
                // Body flickering runs every AI call, independently of the seventeen-call
                // palette cadence. Counter values 0..15 copy blackening palettes; value 16
                // is the null terminator and completes the transition without a copy.
                BodyFlickerCallCount++;
                GenerateDeathExplosions(
                    mixed: true, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    FunctionTimer = 0x0010;
                    ushort paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested |= paletteIndex < 16;
                    if (paletteIndex >= 16)
                    {
                        EnemyBg2TilemapClearRequested = true;
                        BodyProperties = unchecked((ushort)((BodyProperties | 0x0100) & 0xdfff));
                        BodyProperties2 = 0;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions;
                        FunctionTimer = 0x0010;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions:
                GenerateDeathExplosions(
                    mixed: true, nextRandomNumber, deathExplosions);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceRealizeDecapitation;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceRealizeDecapitation:
                // The body installs the decapitated list and a separate brain-slot draw
                // setup, clears its 8.8 falling velocity, then falls through immediately.
                SetHeadInstructionList(HeadDecapitatedInstructionList);
                BrainDrawSetupRequested = true;
                FunctionTimer = 0;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround;
                goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround:
                // `$B12D` treats FunctionTimer as unsigned 8.8 velocity: add `$20`, use only
                // its high byte as this frame's whole-pixel displacement, and accumulate Y.
                // Crossing `$C4` clamps the head and publishes earthquake type two for 20.
                FunctionTimer = unchecked((ushort)(FunctionTimer + 0x0020));
                ushort fallingDisplacement = (ushort)(FunctionTimer >> 8);
                ushort candidateBrainY = unchecked((ushort)(BrainYPosition + fallingDisplacement));
                if (candidateBrainY >= 0x00c4)
                {
                    EarthquakeType = 2;
                    EarthquakeTimer = 20;
                    BrainYPosition = 0x00c4;
                    CorpseTileTransferIndex = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles;
                    FunctionTimer = 0x0100;
                }
                else
                {
                    BrainYPosition = candidateBrainY;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles:
                // ProcessSpriteTilesTransfers handles one record per call and notices the
                // following zero terminator on the sixth call, just like the Baby tile list.
                spriteTileTransfer = CreateNextCorpseTileTransfer();
                if (CorpseTileTransferIndex == CorpseTileSources.Length)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey;
                    FunctionTimer = 0x0020;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey;
                    FunctionTimer = 0;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    ushort paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex < 8)
                    {
                        FunctionTimer = 0x0010;
                    }
                    else
                    {
                        SetHeadInstructionList(HeadCorpseInstructionList);
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver;
                        FunctionTimer = 0x0100;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway;
                    BrainProperties |= 0x0400;
                    HitboxesEnabled = false;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway:
                // Retail initialization happens when the brain enemy slot is created, many
                // phases before death. Keep an explicit public initializer for that normal
                // route, but lazily perform the same work for focused debugger fixtures that
                // start at a late phase. Nothing touches this private table/buffer in between,
                // so the delayed call is state-equivalent while preventing a fake host setup.
                if (!_corpseRotting.IsInitialized)
                    _corpseRotting.Initialize(bus);

                MotherBrainCorpseRottingStepResult corpseRotting = _corpseRotting.Step(
                    bus,
                    BrainXPosition,
                    BrainYPosition,
                    randomNumberSeed,
                    mainEnemyExecutionCounter);
                corpseRottingVramTransfers.AddRange(corpseRotting.VramTransfers);
                corpseDustRequests.AddRange(corpseRotting.DustRequests);
                if (corpseRotting.StillRotting)
                    break;

                // Carry clear at `$B1DB` makes the brain invisible/non-solid, clears its
                // second property word, and queues stop + escape music through the ordinary
                // eight-frame-delay music queue. `$14` is loaded and immediately decremented
                // by the fallthrough into `$B211`, leaving observable timer `$13` now.
                BrainProperties = unchecked((ushort)((BrainProperties | 0x0100) & 0xdfff));
                BrainProperties2 = 0;
                musicStopQueued = true;   // Queue music value `$0000`.
                escapeMusicQueued = true; // Queue music value `$FF24`.
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay;
                FunctionTimer = 0x0014;
                goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    // The decapitated brain remains at the corpse coordinates for the full
                    // delay. `$B216/$B219` finally park it at (0,0) before escape-timer tile
                    // loading begins; this is a real actor-coordinate mutation, not rendering.
                    BrainXPosition = 0;
                    BrainYPosition = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles:
                escapeSequenceTileTransfers.Add(CreateNextEscapeTimerTileTransfer());
                if (EscapeTimerTileTransferIndex == EscapeTimerTileTransfers.Length)
                {
                    // Carry set after entry six clears the shared list cursor, installs
                    // `$B26D`, and falls through far enough to emit exploded-door entry zero
                    // during this same enemy AI call.
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape:
                escapeSequenceTileTransfers.Add(CreateNextExplodedDoorTileTransfer());
                if (ExplodedDoorTileTransferIndex != ExplodedDoorTileTransfers.Length)
                    break;

                // The second door record observes the zero terminator and performs the full
                // `$B275-$B2CD` handoff on that call: copy fourteen colors (palette zero is
                // skipped), start escape music, hold the quake indefinitely, create four
                // palette-FX objects, disable the old unpause hook, and initialize text.
                explodedDoorPaletteRequested = true; // `$A9:9534`, 14 colors -> target `$0122`.
                escapeMusicTrackQueued = true;        // Eight-frame-delay music value `$0007`.
                EarthquakeType = 5;
                EarthquakeTimer = 0xffff;
                escapePaletteFxRequests.AddRange([0xffc9, 0xffcd, 0xffd1, 0xffd5]);
                MotherBrainUnpauseHookEnabled = false;
                escapeTypewriterSetupRequested = true;
                FunctionTimer = 0x0020;
                Phase = alternateEscapeText
                    ? MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSpawnTimeBombSetSubtitle
                    : MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSpawnTimeBombSetSubtitle:
            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText:
                // `$B2D1/$B2E3` consume the typewriter subsystem and subtitle projectile.
                // They are the next explicit producer seam; do not guess text completion.
                break;

            default:
                throw new InvalidOperationException($"Unsupported rainbow-beam phase {Phase}.");
        }

        return new MotherBrainRainbowBeamAttackStepResult(
            phaseBefore,
            Phase,
            movement,
            healthBefore,
            samus.Health,
            missilesBefore,
            samus.Missiles,
            supersBefore,
            samus.SuperMissiles,
            bombsBefore,
            samus.PowerBombs,
            soundQueued,
            paletteRequested,
            explosion,
            unlockedSamus,
            AngularWidth,
            FunctionTimer,
            EarthquakeType,
            EarthquakeTimer,
            chargeSoundQueued,
            bodyWalkRequested,
            HeadInstructionList,
            NeckAngleDelta,
            LowerNeckMovementIndex,
            UpperNeckMovementIndex,
            bodyPostureRequested,
            finishOffAttack,
            spriteTileTransfer,
            babySpawnRequested,
            finalBeamSoundQueued,
            phase3Attack,
            deathExplosions,
            corpseRottingVramTransfers,
            corpseDustRequests,
            musicStopQueued,
            escapeMusicQueued,
            escapeSequenceTileTransfers,
            explodedDoorPaletteRequested,
            escapeMusicTrackQueued,
            escapePaletteFxRequests,
            escapeTypewriterSetupRequested);
    }

    private void BeginExtendingNeckForAttack()
    {
        // `$A9:B8EB-$B916` resets the neutral phase-two head program and selects the
        // ordinary 2/4 neck geometry before beginning its first 256-count wait.
        SetHeadInstructionList(HeadNeutralPhase2InstructionList);
        NeckAngleDelta = 0x0040;
        NeckMovementEnabled = 1;
        LowerNeckMovementIndex = 2;
        UpperNeckMovementIndex = 4;
        Phase = MotherBrainRainbowBeamAttackPhase.StartCharging;
        FunctionTimer = 0x0100;
    }

    private static short CalculateSignedNeckComponent(
        ISnesAddressSpace bus,
        byte angle,
        byte distance)
    {
        // `$A9:C46C` writes the sign-extended sine word to the SNES signed multiplicand,
        // writes the segment distance to its signed eight-bit multiplier, then reads product
        // bits 8..23 from `$2135`. All retail distances here fit the positive byte domain.
        int address = 0xa0b443 + angle * 2;
        short sine = unchecked((short)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
        int product = sine * unchecked((sbyte)distance);
        return unchecked((short)(product >> 8));
    }

    private void SetHeadInstructionList(ushort pointer)
    {
        // `$A9:C447` writes the separate brain-list pointer and timer. Unlike ordinary
        // enemy programs it has no loop-counter write here.
        HeadInstructionList = pointer;
        HeadInstructionPointer = pointer;
        HeadInstructionTimer = 1;
    }

    private void AimOnionRings(short deltaX, short deltaY)
    {
        // `$A0:C0B1` returns the game's byte angle. `$A9:9E77` converts it to the projectile
        // convention (`$80-angle`) and performs a circular signed clamp: `$10-$47` survive,
        // `$48-$BF` clamp to `$48`, and `$C0-$FF/$00-$0F` clamp to `$10`.
        byte sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(deltaX, deltaY);
        byte candidate = unchecked((byte)(0x80 - sourceAngle));
        OnionRingTargetAngle = candidate switch
        {
            >= 0x10 and < 0x48 => candidate,
            >= 0x48 and < 0xc0 => 0x48,
            _ => 0x10,
        };
    }

    private static ushort ReadBankA9Word(ISnesAddressSpace bus, ushort address)
    {
        int lowAddress = 0xa90000 | address;
        return unchecked((ushort)(
            bus.ReadByte(lowAddress) |
            (bus.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8)));
    }

    private void RetractHead()
    {
        // NTSC takes `$0050` from the regional `$0050/$0063` constant at `$A9:BB51`.
        NeckAngleDelta = 0x0050;
        NeckMovementEnabled = 1;
        LowerNeckMovementIndex = 8;
        UpperNeckMovementIndex = 6;
    }

    private bool StepPainfulWalking()
    {
        // The native stores four separate function pointers (request forward, wait forward,
        // request backward, wait backward). Two booleans express the same state without
        // hiding when a body instruction list is actually installed.
        if (PainfulWalkingFunctionTimer != 0)
        {
            PainfulWalkingFunctionTimer = unchecked((ushort)(PainfulWalkingFunctionTimer - 1));
            if (PainfulWalkingFunctionTimer == 0)
            {
                PainfulWalkingStage++;
                PainfulWalkingForward = !PainfulWalkingForward;
            }
            return false;
        }

        bool reachedTarget;
        bool requested;
        if (PainfulWalkingForward)
        {
            requested = RequestWalkForward(0x0048, PainfulWalkingAnimationDelay);
            reachedTarget = unchecked((short)(0x0048 - Body.XPosition)) < 0 ||
                NativeAtLeast(Body.XPosition, 0x0080);
        }
        else
        {
            requested = RequestWalkBackward(0x0028, PainfulWalkingAnimationDelay);
            reachedTarget = HasReachedBackwardTarget(0x0028);
        }

        if (reachedTarget)
        {
            int timerIndex = Math.Min(PainfulWalkingStage, (ushort)7);
            PainfulWalkingFunctionTimer = PainfulWalkingFunctionTimers[timerIndex];
        }
        return requested;
    }

    private bool StepPhase3WalkingHandler(ushort randomNumberSeed)
    {
        // `$C25A` refuses to call the walking function while body bytecode advertises any
        // nonzero pose. This makes each request edge-triggered: the requested walk runs to
        // its standing opcode before the scheduler is allowed to inspect its target again.
        if (Body.Pose != 0)
            return false;

        switch (Phase3WalkingPhase)
        {
            case MotherBrainPhase3WalkingPhase.Inactive:
                return false;

            case MotherBrainPhase3WalkingPhase.TryToInchForward:
                if (Phase3WalkCounter == 0)
                {
                    // Zero credit does not merely pause. `$C2A1` chooses a point fourteen
                    // pixels left, installs the quick-retreat function, and falls into it.
                    Phase3TargetXPosition = unchecked((ushort)(Body.XPosition - 0x000e));
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatQuickly;
                    goto case MotherBrainPhase3WalkingPhase.RetreatQuickly;
                }

                // Native ADC/CMP is unsigned here. A wrapped counter can therefore spend
                // more calls below `$0100`; no host integer widening or saturation belongs.
                Phase3WalkCounter = unchecked((ushort)(Phase3WalkCounter + 0x0020));
                if (Phase3WalkCounter < 0x0100)
                    return false;

                Phase3TargetXPosition = unchecked((ushort)(Body.XPosition + 1));
                ushort forwardDelay = unchecked((ushort)((randomNumberSeed & 2) + 4));

                // MakeMotherBrainWalkForwards reports carry only for a strictly overshot
                // target or the independent X `$80` limit. Equality still requests a walk.
                bool reachedForwardTarget =
                    unchecked((short)(Phase3TargetXPosition - Body.XPosition)) < 0 ||
                    NativeAtLeast(Body.XPosition, 0x0080);
                if (reachedForwardTarget)
                {
                    Phase3WalkCounter = 0x0080;
                    return false;
                }

                return RequestWalkForward(Phase3TargetXPosition, forwardDelay);

            case MotherBrainPhase3WalkingPhase.RetreatQuickly:
                if (!HasReachedBackwardTarget(Phase3TargetXPosition))
                    return RequestWalkBackward(Phase3TargetXPosition, animationDelay: 0x0002);

                // Reaching the first target chooses another point fourteen pixels left but
                // does not fall through to the slow request. That request starts only on the
                // next standing AI call.
                Phase3TargetXPosition = unchecked((ushort)(Body.XPosition - 0x000e));
                Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatSlowly;
                return false;

            case MotherBrainPhase3WalkingPhase.RetreatSlowly:
                if (!HasReachedBackwardTarget(Phase3TargetXPosition))
                    return RequestWalkBackward(Phase3TargetXPosition, animationDelay: 0x0004);

                // `$C313` writes all three words: forty walk-credit, the inch-forward
                // function, and a one-pixel-ahead target used by later calls.
                SetPhase3WalkingToTryToInchForward(0x0040);
                return false;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three walking function {Phase3WalkingPhase}.");
        }
    }

    private void SetPhase3WalkingToTryToInchForward(ushort counter)
    {
        Phase3WalkCounter = counter;
        Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;
        Phase3TargetXPosition = unchecked((ushort)(Body.XPosition + 1));
    }

    private void StepPhase3NeckHandler()
    {
        switch (Phase3NeckPhase)
        {
            case MotherBrainPhase3NeckPhase.Inactive:
                return;

            case MotherBrainPhase3NeckPhase.Normal:
                // `$C330` briefly writes lower index one and immediately overwrites it with
                // two before any other code can observe the word. Preserve the observable
                // result while documenting the retail no-op (`>_<` in the disassembly).
                NeckAngleDelta = 0x0080;
                LowerNeckMovementIndex = 2;
                UpperNeckMovementIndex = 4;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Inactive;
                return;

            case MotherBrainPhase3NeckPhase.SetupRecoilRecovery:
                NeckMovementEnabled = 1;
                NeckAngleDelta = 0x0500;
                LowerNeckMovementIndex = 6;
                UpperNeckMovementIndex = 6;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.RecoilRecovery;
                Phase3NeckFunctionTimer = 0x0010;

                // `$C354` falls directly into `$C37B`; the newly loaded sixteen is already
                // fifteen when this single handler call returns.
                goto case MotherBrainPhase3NeckPhase.RecoilRecovery;

            case MotherBrainPhase3NeckPhase.RecoilRecovery:
                if (Phase3NeckFunctionTimer != 0)
                {
                    Phase3NeckFunctionTimer =
                        unchecked((ushort)(Phase3NeckFunctionTimer - 1));
                    return;
                }

                // DEC of zero branches negative without storing `$FFFF`, so the readable
                // timer remains zero while the four-ring recovery list is installed.
                SetHeadInstructionList(HeadAttackingFourOnionRingsPhase3InstructionList);
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                return;

            case MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil:
                NeckMovementEnabled = 1;
                Phase3DisableAttacks = 1;
                SetHeadInstructionList(HeadHyperBeamRecoilInstructionList);
                BrainMainShakeTimer = 0x0032;
                NeckAngleDelta = 0x0900;
                LowerNeckMovementIndex = 8;
                UpperNeckMovementIndex = 8;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.HyperBeamRecoil;
                Phase3NeckFunctionTimer = 0x000b;

                // Setup also falls through, making the externally visible first timer `$0A`.
                goto case MotherBrainPhase3NeckPhase.HyperBeamRecoil;

            case MotherBrainPhase3NeckPhase.HyperBeamRecoil:
                if (Phase3NeckFunctionTimer != 0)
                {
                    Phase3NeckFunctionTimer =
                        unchecked((ushort)(Phase3NeckFunctionTimer - 1));
                    return;
                }

                NeckAngleDelta = 0x0080;
                Phase3DisableAttacks = 0;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupRecoilRecovery;
                return;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three neck function {Phase3NeckPhase}.");
        }
    }

    private bool RequestWalkForward(ushort targetX, ushort animationDelay)
    {
        if (unchecked((short)(targetX - Body.XPosition)) < 0 || Body.Pose != 0 ||
            NativeAtLeast(Body.XPosition, 0x0080))
            return false;

        ushort pointer = animationDelay switch
        {
            0x0002 => BodyWalkingForwardReallyFastInstructionList,
            0x0004 => BodyWalkingForwardFastInstructionList,
            0x0006 => BodyWalkingForwardMediumInstructionList,
            0x0008 => BodyWalkingForwardSlowInstructionList,
            0x000a => BodyWalkingForwardReallySlowInstructionList,
            _ => throw new InvalidOperationException(
                $"Unsupported painful forward animation delay ${animationDelay:X4}."),
        };
        Body.SetInstructionList(pointer);
        return true;
    }

    private bool RequestWalkBackward(ushort targetX, ushort animationDelay)
    {
        if (HasReachedBackwardTarget(targetX) || Body.Pose != 0)
            return false;

        ushort pointer = animationDelay switch
        {
            0x0002 => BodyWalkingBackwardReallyFastInstructionList,
            0x0004 => BodyWalkingBackwardFastInstructionList,
            0x0006 => BodyWalkingBackwardMediumInstructionList,
            0x0008 => BodyWalkingBackwardSlowInstructionList,
            0x000a => BodyWalkingBackwardReallySlowInstructionList,
            _ => throw new InvalidOperationException(
                $"Unsupported painful backward animation delay ${animationDelay:X4}."),
        };
        Body.SetInstructionList(pointer);
        return true;
    }

    private bool RequestWalkForwardReallySlow(ushort targetX)
    {
        // `$A9:C601` first performs signed CMP/BMI against the target. At equality it still
        // examines pose and the hard `$80` arena limit; only a strictly overshot target sets
        // carry at the first branch.
        if (unchecked((short)(targetX - Body.XPosition)) < 0)
            return false;
        if (Body.Pose != 0)
            return false;
        if (NativeAtLeast(Body.XPosition, 0x0080))
            return false;

        Body.SetInstructionList(BodyWalkingForwardReallySlowInstructionList);
        return true;
    }

    private bool RequestWalkBackwardReallySlow(ushort targetX)
    {
        if (HasReachedBackwardTarget(targetX) || Body.Pose != 0)
            return false;

        Body.SetInstructionList(BodyWalkingBackwardReallySlowInstructionList);
        return true;
    }

    private bool HasReachedBackwardTarget(ushort targetX) =>
        // `$A9:C647` reports carry at target/left of target or below the independent `$30`
        // room limit. This check runs before pose, so an in-progress walk can complete the AI
        // phase on the exact frame one of its movement opcodes reaches X `$28`.
        unchecked((short)(targetX - Body.XPosition)) >= 0 ||
        unchecked((short)(Body.XPosition - 0x0030)) < 0;

    private static ushort CalculateFinishOffHealthThreshold(SamusState samus, ushort nominalDamage)
    {
        // `$A0:A45E` gives Gravity Suit priority and divides damage by four. Otherwise
        // Varia's bit zero divides by two; Power Suit leaves it untouched. `$A9:BD4C`
        // then multiplies the divided `$50` result by four, while `$BD68` deliberately
        // does not multiply the divided `$A0` result. The caller selects that distinction.
        ushort divided = (samus.EquippedItems & 0x0020) != 0
            ? (ushort)(nominalDamage >> 2)
            : (samus.EquippedItems & 0x0001) != 0
                ? (ushort)(nominalDamage >> 1)
                : nominalDamage;

        return nominalDamage == 0x0050
            ? unchecked((ushort)(divided * 4 + 0x0014))
            : unchecked((ushort)(divided + 0x0014));
    }

    private bool MaybeRequestStandUpOrLeanDown(ushort randomNumberSeed)
    {
        // `$A9:C1A7` ignores every pose except standing (zero) and leaning (six), and its
        // low-byte threshold is inclusive at `$C0`. A request still executes as body
        // instruction bytecode later in the enemy-processing stage.
        if ((randomNumberSeed & 0x00ff) < 0x00c0)
            return false;

        if (Body.Pose == 0)
        {
            Body.SetInstructionList(BodyLeaningDownInstructionList);
            return true;
        }

        if (Body.Pose == 6)
        {
            MakeBodyStandUp(out bool requested);
            return requested;
        }

        return false;
    }

    private bool MakeBodyStandUp(out bool animationRequested)
    {
        animationRequested = false;
        if (Body.Pose == 0)
            return true;

        ushort instructionList = Body.Pose switch
        {
            3 => BodyStandingUpAfterCrouchingFastInstructionList,
            6 => BodyStandingUpAfterLeaningDownInstructionList,
            _ => 0,
        };
        if (instructionList != 0)
        {
            Body.SetInstructionList(instructionList);
            animationRequested = true;
        }

        return false;
    }

    private MotherBrainSpriteTileTransferRequest CreateNextBabyMetroidTileTransfer()
    {
        int index = BabyMetroidTileTransferIndex;
        if ((uint)index >= (uint)BabyMetroidTileSources.Length)
            throw new InvalidOperationException("Baby Metroid sprite-tile transfer list is already complete.");

        var request = new MotherBrainSpriteTileTransferRequest(
            EntryIndex: (ushort)index,
            Size: 0x0200,
            SourceAddress: BabyMetroidTileSources[index],
            VramDestination: BabyMetroidTileDestinations[index]);
        BabyMetroidTileTransferIndex++;
        return request;
    }

    private MotherBrainSpriteTileTransferRequest CreateNextCorpseTileTransfer()
    {
        int index = CorpseTileTransferIndex;
        if ((uint)index >= (uint)CorpseTileSources.Length)
            throw new InvalidOperationException("Mother Brain corpse tile transfer list is already complete.");

        var request = new MotherBrainSpriteTileTransferRequest(
            EntryIndex: (ushort)index,
            Size: 0x01c0,
            SourceAddress: CorpseTileSources[index],
            VramDestination: CorpseTileDestinations[index]);
        CorpseTileTransferIndex++;
        return request;
    }

    private MotherBrainSpriteTileTransferRequest CreateNextEscapeTimerTileTransfer()
    {
        int index = EscapeTimerTileTransferIndex;
        if ((uint)index >= (uint)EscapeTimerTileTransfers.Length)
            throw new InvalidOperationException("Escape-timer sprite-tile transfer list is already complete.");

        MotherBrainSpriteTileTransferRequest request = EscapeTimerTileTransfers[index];
        EscapeTimerTileTransferIndex++;
        return request;
    }

    private MotherBrainSpriteTileTransferRequest CreateNextExplodedDoorTileTransfer()
    {
        int index = ExplodedDoorTileTransferIndex;
        if ((uint)index >= (uint)ExplodedDoorTileTransfers.Length)
            throw new InvalidOperationException("Exploded-door sprite-tile transfer list is already complete.");

        MotherBrainSpriteTileTransferRequest request = ExplodedDoorTileTransfers[index];
        ExplodedDoorTileTransferIndex++;
        return request;
    }

    private void GenerateDeathExplosions(
        bool mixed,
        Func<ushort>? nextRandomNumber,
        List<MotherBrainDeathExplosionRequest> requests)
    {
        // `$B03E` decrements before testing BPL. A cleared timer therefore wraps and emits
        // immediately. Crucially, the interval literal remains in A across the memory DEC,
        // so the smoky/mixed caller writes exactly `$10`/`$08` after an expiry.
        DeathExplosionIntervalTimer = unchecked((ushort)(DeathExplosionIntervalTimer - 1));
        if ((DeathExplosionIntervalTimer & 0x8000) == 0)
            return;

        DeathExplosionIntervalTimer = mixed ? (ushort)0x0008 : (ushort)0x0010;
        DeathExplosionIndex = unchecked((ushort)(DeathExplosionIndex - 1));
        if ((DeathExplosionIndex & 0x8000) != 0)
            DeathExplosionIndex = 6;

        int simultaneousCount = mixed ? 4 : 2;
        int pairIndex = DeathExplosionIndex * 4;
        for (int explosionIndex = 0; explosionIndex < simultaneousCount; explosionIndex++)
        {
            // The global RNG is called once per projectile, not once per visual batch. A
            // callback is required here so the owning frame runtime advances its real global
            // state; silently deriving private random values would desynchronize later AI.
            if (nextRandomNumber is null)
            {
                throw new InvalidOperationException(
                    "Mother Brain death explosions require the global next-random-number producer.");
            }

            ushort random = nextRandomNumber();
            ushort parameter = mixed
                ? random < 0x4000 ? (ushort)0 : random < 0xe000 ? (ushort)1 : (ushort)2
                : (ushort)1;
            (short xOffset, short yOffset) = DeathExplosionOffsets[pairIndex + explosionIndex];
            requests.Add(new MotherBrainDeathExplosionRequest(
                PatternIndex: DeathExplosionIndex,
                XOffset: xOffset,
                YOffset: yOffset,
                XPosition: unchecked((ushort)(Body.XPosition + xOffset)),
                YPosition: unchecked((ushort)(Body.YPosition + yOffset)),
                ProjectileParameter: parameter,
                SoundEffect: 0x0013));
        }
    }

    private void IncreaseWidthAndAim(SamusState samus)
    {
        ushort widened = unchecked((ushort)(AngularWidth + 0x0180));
        AngularWidth = NativeAtLeast(widened, 0x0c00) ? (ushort)0x0c00 : widened;
        AimBeamAtSamus(samus);
    }

    private void RunContinuingBeamEffects(
        SamusState samus,
        ushort enemyFrameCounter,
        bool increaseWidth,
        ref bool soundQueued,
        ref bool paletteRequested,
        ref MotherBrainRainbowExplosionRequest? explosion)
    {
        // `$A9:BB2E` checks the signed counter before DEC. Starting at six therefore queues
        // on values 6,5,4,3,2,1,0 and leaves `$FFFF` to suppress all later calls.
        if ((SoundQueueCount & 0x8000) == 0)
        {
            SoundQueueCount = unchecked((ushort)(SoundQueueCount - 1));
            RainbowBeamSoundPlaying = true;
            soundQueued = true;
        }

        // Palette handling runs only when enemy frame-counter bit one is set. The palette
        // program itself remains presentation work, but its native cadence is observable.
        paletteRequested = (enemyFrameCounter & 2) != 0;

        if (increaseWidth)
            IncreaseWidthAndAim(samus);
        else
            AimBeamAtSamus(samus);
        explosion = StepExplosionTimer();
    }

    private void AimBeamAtSamus(SamusState samus)
    {
        // `$A9:BB82` aims from brain position (+16,+4) to Samus. Bank `$A0:C0B1` consumes
        // signed 16-bit offsets; `$A9:BBA0-$BBA8` converts its clockwise-up angle to the
        // rainbow beam's anti-clockwise-down convention with `-$angle + $80`.
        short deltaX = unchecked((short)(samus.XPosition - BrainXPosition - 0x0010));
        short deltaY = unchecked((short)(samus.YPosition - BrainYPosition - 0x0004));
        byte sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(deltaX, deltaY);
        _movement.RainbowBeamAngle = unchecked((byte)(0x80 - sourceAngle));
    }

    private MotherBrainRainbowExplosionRequest? StepExplosionTimer()
    {
        ExplosionTimer = unchecked((ushort)(ExplosionTimer - 1));
        if ((ExplosionTimer & 0x8000) == 0)
            return null;

        ExplosionTimer = 8;
        ExplosionIndex++;
        int offsetIndex = ExplosionIndex & 7;
        return new MotherBrainRainbowExplosionRequest(
            ExplosionIndex,
            ExplosionXOffsets[offsetIndex],
            ExplosionYOffsets[offsetIndex],
            SoundEffect: 0x0024);
    }

    private static void DamageSamusDueToRainbowBeam(SamusState samus)
    {
        // `$A9:C57D` loads `$FFFE` in both branches. The meaningful Varia distinction is
        // carry left by LSR EquippedItems: bit zero adds one back, producing -1 with Varia
        // and -2 without it. CMP #1/BPL then clamps wrapped/below-one health to zero.
        int damage = (samus.EquippedItems & 1) != 0 ? 1 : 2;
        ushort candidate = unchecked((ushort)(samus.Health - damage));
        samus.Health = NativeAtLeast(candidate, 1) ? candidate : (ushort)0;
    }

    private static void DecrementAmmoDueToRainbowBeam(
        SamusState samus,
        ushort mainEnemyExecutionCounter)
    {
        // Missiles and supers are visited only every fourth main-enemy pass. Power bombs
        // are visited every call, matching the branch layout rather than sharing the gate.
        if ((mainEnemyExecutionCounter & 3) == 0)
        {
            samus.Missiles = DecrementOneAmmoType(samus, samus.Missiles, selectedItem: 1);
            samus.SuperMissiles = DecrementOneAmmoType(samus, samus.SuperMissiles, selectedItem: 2);
        }

        samus.PowerBombs = DecrementOneAmmoType(samus, samus.PowerBombs, selectedItem: 3);
    }

    private static ushort DecrementOneAmmoType(
        SamusState samus,
        ushort current,
        ushort selectedItem)
    {
        if (current == 0)
            return 0;

        ushort decremented = unchecked((ushort)(current - 1));
        if (NativeAtLeast(decremented, 1))
            return decremented;

        // The 65816 briefly reloads A with SelectedHUDItem for the comparison, but both the
        // matching and nonmatching branches converge at a label whose first instruction is
        // `LDA #$0000`. Therefore the depleted count is always zero; only the selected-item
        // store itself is conditional. Spell that convergence out to avoid the tempting but
        // incorrect interpretation that the HUD item number leaks into the ammo count.
        if (samus.SelectedHudItem == selectedItem)
            samus.SelectedHudItem = 0;

        samus.AutoCancelHudItemIndex = 0;
        return 0;
    }

    private static bool NativeAtLeast(ushort value, ushort threshold) =>
        unchecked((short)(value - threshold)) >= 0;
}

/// <summary>Exact active-attack function-pointer phases admitted by the sequence.</summary>
public enum MotherBrainRainbowBeamAttackPhase
{
    Inactive,
    StartCharging,
    RetractNeck,
    WaitForCharge,
    ExtendNeckDown,
    StartFiring,
    MoveSamusTowardWall,
    OneFrameDelay,
    StartDrainingSamus,
    DrainingSamus,
    FinishFiring,
    LetSamusFall,
    WaitForSamusToLand,
    LowerHead,
    DecideNextAction,
    RepeatAttack,
    FinishSamusOff,
    FinishStandUp,
    AdmireJobWellDone,
    ChargeFinalRainbowBeam,
    LoadBabyMetroidTiles,
    FireFinalRainbowBeam,
    FinalRainbowBeamHolding,
    DrainedByBabyMetroidTakenAback,
    DrainedByBabyMetroidRegainBalance,
    DrainedByBabyMetroidFiringRainbowBeam,
    DrainedByBabyMetroidRainbowBeamRunOut,
    DrainedByBabyMetroidMoveToBackOfRoom,
    DrainedByBabyMetroidGoIntoLowPowerMode,
    DrainedByBabyMetroidPrepareTransitionToGrey,
    DrainedByBabyMetroidTransitionToGrey,
    Phase2ReviveSelfInanimateGrey,
    Phase2ReviveSelfShowSignsOfLife,
    Phase2ReviveSelfTransitionFromGrey,
    Phase2ReviveSelfWakeUp,
    Phase2ReviveSelfWakeUpStretch,
    Phase2ReviveSelfWalkUpToBabyMetroid,
    Phase2ReviveSelfPrepareNeckForBabyMetroidDeath,
    Phase2ReviveSelfFinishPreparingForBabyMetroidDeath,
    Phase2MurderBabyMetroidAttack,
    Phase2MurderBabyMetroidAttackCooldown,
    PrepareForFinalBabyMetroidAttack,
    ExecuteFinalBabyMetroidAttack,
    FinalBabyMetroidAttackHolding,
    Phase3RecoverFromCutsceneMakeSomeDistance,
    Phase3RecoverFromCutsceneSetupForFighting,
    Phase3FightingMain,
    Phase3FightingAttackCooldown,
    Phase3DeathSequenceMoveToBackOfRoom,
    Phase3DeathSequenceIdleWhilstExploding,
    Phase3DeathSequenceStumbleToMiddleOfRoom,
    Phase3DeathSequenceDisableBrainEffects,
    Phase3DeathSequenceSetupBodyFadeOut,
    Phase3DeathSequenceFadeOutBody,
    Phase3DeathSequenceFinalFewExplosions,
    Phase3DeathSequenceRealizeDecapitation,
    Phase3DeathSequenceBrainFallsToGround,
    Phase3DeathSequenceLoadCorpseTiles,
    Phase3DeathSequenceSetupFadeToGrey,
    Phase3DeathSequenceFadeToGrey,
    Phase3DeathSequenceCorpseTipsOver,
    Phase3DeathSequenceCorpseRotsAway,
    Phase3DeathSequence20FrameDelay,
    Phase3DeathSequenceLoadEscapeTimerTiles,
    Phase3DeathSequenceStartEscape,
    Phase3DeathSequenceSpawnTimeBombSetSubtitle,
    Phase3DeathSequenceTypeOutZebesEscapeText,
}

/// <summary>Reachable third-phase walking function pointers at `$A9:C26A-$C326`.</summary>
public enum MotherBrainPhase3WalkingPhase
{
    Inactive,
    TryToInchForward,
    RetreatQuickly,
    RetreatSlowly,
}

/// <summary>Reachable third-phase neck function pointers at `$A9:C330-$C3EE`.</summary>
public enum MotherBrainPhase3NeckPhase
{
    Inactive,
    Normal,
    SetupRecoilRecovery,
    RecoilRecovery,
    SetupHyperBeamRecoil,
    HyperBeamRecoil,
}

/// <summary>Low-three-bit projectile classes consumed by `$A9:B58E`.</summary>
public enum MotherBrainProjectileType
{
    Beam = 0,
    Missile = 1,
    SuperMissile = 2,
    PowerBomb = 3,
    UnusedFour = 4,
    Bomb = 5,
    UnusedSix = 6,
    BeamExplosion = 7,
}

/// <summary>Phase-three head attack selected by `$A9:C22C-$C23E`.</summary>
public enum MotherBrainPhase3AttackKind
{
    Bomb,
    FourOnionRings,
}

/// <summary>Head-projectile animation selected by `$A9:BD71-$BD83`.</summary>
public enum MotherBrainFinishOffAttackKind
{
    TwoOnionRings,
    Bomb,
}

/// <summary>
/// One native seven-byte sprite-tile transfer entry consumed by `$A9:C5BE`.
/// </summary>
public readonly record struct MotherBrainSpriteTileTransferRequest(
    ushort EntryIndex,
    ushort Size,
    uint SourceAddress,
    ushort VramDestination);

/// <summary>One requested beam explosion projectile and its native sound number.</summary>
public readonly record struct MotherBrainRainbowExplosionRequest(
    ushort SequenceIndex,
    short XOffset,
    short YOffset,
    ushort SoundEffect);

/// <summary>One projectile in a simultaneous `$A9:B03E` death-explosion batch.</summary>
public readonly record struct MotherBrainDeathExplosionRequest(
    ushort PatternIndex,
    short XOffset,
    short YOffset,
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter,
    ushort SoundEffect);

/// <summary>One `$86:CB4B` blue-ring spawn emitted by head opcode `$A9:9E29`.</summary>
public readonly record struct MotherBrainOnionRingSpawnRequest(byte Angle);

/// <summary>Debugger witness for one Baby-murder head instruction-processing call.</summary>
public readonly record struct MotherBrainHeadAnimationStepResult(
    ushort InstructionPointerBefore,
    ushort InstructionPointerAfter,
    ushort InstructionTimerBefore,
    ushort InstructionTimerAfter,
    ushort SpritemapPointer,
    bool LoadedFrame,
    bool BabyAttackCounterIncremented,
    bool BabyAttackCounterReset,
    byte OnionRingTargetAngle,
    MotherBrainOnionRingSpawnRequest? OnionRingSpawn,
    ushort? QueuedSoundLibraryTwo,
    ushort? QueuedSoundLibraryThree);

/// <summary>Debugger witness for one Mother Brain active-rainbow body-function call.</summary>
public readonly record struct MotherBrainRainbowBeamAttackStepResult(
    MotherBrainRainbowBeamAttackPhase PhaseBefore,
    MotherBrainRainbowBeamAttackPhase PhaseAfter,
    MotherBrainForcedSamusMovementResult? Movement,
    ushort HealthBefore,
    ushort HealthAfter,
    ushort MissilesBefore,
    ushort MissilesAfter,
    ushort SuperMissilesBefore,
    ushort SuperMissilesAfter,
    ushort PowerBombsBefore,
    ushort PowerBombsAfter,
    bool SoundQueued,
    bool PaletteRequested,
    MotherBrainRainbowExplosionRequest? Explosion,
    bool UnlockedSamus,
    ushort AngularWidth,
    ushort FunctionTimer,
    ushort EarthquakeType,
    ushort EarthquakeTimer,
    bool ChargeSoundQueued,
    bool BodyWalkRequested,
    ushort HeadInstructionList,
    ushort NeckAngleDelta,
    ushort LowerNeckMovementIndex,
    ushort UpperNeckMovementIndex,
    bool BodyPostureRequested,
    MotherBrainFinishOffAttackKind? FinishOffAttack,
    MotherBrainSpriteTileTransferRequest? SpriteTileTransfer,
    bool BabySpawnRequested,
    bool FinalBeamSoundQueued,
    MotherBrainPhase3AttackKind? Phase3Attack,
    IReadOnlyList<MotherBrainDeathExplosionRequest> DeathExplosions,
    IReadOnlyList<MotherBrainSpriteTileTransferRequest> CorpseRottingVramTransfers,
    IReadOnlyList<MotherBrainCorpseDustRequest> CorpseDustRequests,
    bool MusicStopQueued,
    bool EscapeMusicQueued,
    IReadOnlyList<MotherBrainSpriteTileTransferRequest> EscapeSequenceTileTransfers,
    bool ExplodedDoorPaletteRequested,
    bool EscapeMusicTrackQueued,
    IReadOnlyList<ushort> EscapePaletteFxRequests,
    bool EscapeTypewriterSetupRequested);
