using SuperMetroid.Core.Assets;
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
    /// <summary>$A9:9C87, neutral phase-two head instruction list.</summary>
    public const ushort HeadNeutralPhase2InstructionList = 0x9c87;
    /// <summary>$A9:9F6C, rainbow-beam charging head instruction list.</summary>
    public const ushort HeadChargingRainbowInstructionList = 0x9f6c;
    /// <summary>$A9:9C77, active rainbow-beam firing head instruction list.</summary>
    public const ushort HeadFiringRainbowInstructionList = 0x9c77;
    /// <summary>$A9:9ECC, phase-two bomb attack head instruction list.</summary>
    public const ushort HeadAttackingBombPhase2InstructionList = 0x9ecc;
    /// <summary>$A9:9D7F, phase-two two-onion-ring head instruction list.</summary>
    public const ushort HeadAttackingTwoOnionRingsPhase2InstructionList = 0x9d7f;
    /// <summary>$A9:9B7F, phase-two neck-stretch head instruction list.</summary>
    public const ushort HeadStretchingPhase2InstructionList = 0x9b7f;
    /// <summary>$A9:9BB3, phase-three neck-stretch head instruction list.</summary>
    public const ushort HeadStretchingPhase3InstructionList = 0x9bb3;
    /// <summary>$A9:9BE7, Hyper Beam recoil head instruction list.</summary>
    public const ushort HeadHyperBeamRecoilInstructionList = 0x9be7;
    /// <summary>$A9:9C29, decapitated head instruction list.</summary>
    public const ushort HeadDecapitatedInstructionList = 0x9c29;
    /// <summary>$A9:9D25, inert Mother Brain corpse head instruction list.</summary>
    public const ushort HeadCorpseInstructionList = 0x9d25;
    /// <summary>$A9:9DB1, head instruction list attacking the Baby Metroid.</summary>
    public const ushort HeadAttackingBabyMetroidInstructionList = 0x9db1;
    /// <summary>$A9:9DBB, phase-three four-onion-ring head instruction list.</summary>
    public const ushort HeadAttackingFourOnionRingsPhase3InstructionList = 0x9dbb;
    /// <summary>$A9:9F00, phase-three bomb attack head instruction list.</summary>
    public const ushort HeadAttackingBombPhase3InstructionList = 0x9f00;
    /// <summary>$A9:9818, really-slow forward body walk instruction list.</summary>
    public const ushort BodyWalkingForwardReallySlowInstructionList = 0x9818;
    /// <summary>$A9:9730, really-fast forward body walk instruction list.</summary>
    public const ushort BodyWalkingForwardReallyFastInstructionList = 0x9730;
    /// <summary>$A9:976A, fast forward body walk instruction list.</summary>
    public const ushort BodyWalkingForwardFastInstructionList = 0x976a;
    /// <summary>$A9:97A4, medium forward body walk instruction list.</summary>
    public const ushort BodyWalkingForwardMediumInstructionList = 0x97a4;
    /// <summary>$A9:97DE, slow forward body walk instruction list.</summary>
    public const ushort BodyWalkingForwardSlowInstructionList = 0x97de;
    /// <summary>$A9:993A, really-slow backward body walk instruction list.</summary>
    public const ushort BodyWalkingBackwardReallySlowInstructionList = 0x993a;
    /// <summary>$A9:988C, really-fast backward body walk instruction list.</summary>
    public const ushort BodyWalkingBackwardReallyFastInstructionList = 0x988c;
    /// <summary>$A9:98C6, fast backward body walk instruction list.</summary>
    public const ushort BodyWalkingBackwardFastInstructionList = 0x98c6;
    /// <summary>$A9:9900, medium backward body walk instruction list.</summary>
    public const ushort BodyWalkingBackwardMediumInstructionList = 0x9900;
    /// <summary>$A9:9852, slow backward body walk instruction list.</summary>
    public const ushort BodyWalkingBackwardSlowInstructionList = 0x9852;
    /// <summary>$A9:99C6, fast stand-up-after-crouching body instruction list.</summary>
    public const ushort BodyStandingUpAfterCrouchingFastInstructionList = 0x99c6;
    /// <summary>$A9:99E2, stand-up-after-leaning body instruction list.</summary>
    public const ushort BodyStandingUpAfterLeaningDownInstructionList = 0x99e2;
    /// <summary>$A9:99F2, leaning-down body instruction list.</summary>
    public const ushort BodyLeaningDownInstructionList = 0x99f2;
    /// <summary>$A9:9A26, fast crouching body instruction list.</summary>
    public const ushort BodyCrouchingFastInstructionList = 0x9a26;
    /// <summary>$A9:9C39, dying-drool head instruction list.</summary>
    public const ushort HeadDyingDroolInstructionList = 0x9c39;

    // `$A9:BCA6/$BCB6` are indexed after incrementing the explosion index. Keeping all eight
    // signed records here preserves the initial index-zero -> record-one behavior.
    /// <summary>Signed horizontal offsets applied to successive rainbow-beam explosion effects.</summary>
    private static ReadOnlySpan<short> ExplosionXOffsets =>
        [-8, 6, -4, 2, 3, -6, 8, 0];

    /// <summary>Signed vertical offsets paired by index with <see cref="ExplosionXOffsets"/>.</summary>
    private static ReadOnlySpan<short> ExplosionYOffsets =>
        [-7, 2, 5, -4, 6, -2, -6, 7];

    /// <summary>Owns translated Samus movement and the live rainbow-beam aim angle.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement = new();

    /// <summary>Tracks the native corpse graphics and rotting-table progression.</summary>
    private readonly MotherBrainCorpseRottingState _corpseRotting = new();

    /// <summary>Optional installed corpse atlas required when live corpse rotting is initialized.</summary>
    private readonly RoomCharacterAtlas? _corpseArtwork;

    /// <summary>Standalone cartridge fixtures omit artwork; installed rooms bind the PNG.</summary>
    public MotherBrainRainbowBeamAttackSequence(RoomCharacterAtlas? corpseArtwork = null) =>
        _corpseArtwork = corpseArtwork;

    /// <summary>
    /// Body enemy-slot animation state. The caller advances its enemy-instruction stage
    /// after <see cref="Step"/>, matching the native AI-then-instruction order.
    /// </summary>
    public MotherBrainBodyAnimationState Body { get; } = new();

    /// <summary>Current body-function equivalent.</summary>
    public MotherBrainRainbowBeamAttackPhase Phase { get; private set; } =
        MotherBrainRainbowBeamAttackPhase.Inactive;

    /// <summary>Mother Brain brain-slot X used by `$A9:BB82`'s aim vector.</summary>
    public ushort BrainXPosition { get; set; }

    /// <summary>Mother Brain brain-slot Y used by `$A9:BB8F`'s aim vector.</summary>
    public ushort BrainYPosition { get; set; }

    /// <summary>Current brain instruction list installed through `$A9:C447`.</summary>
    public ushort HeadInstructionList { get; private set; }

    /// <summary>
    /// Monotonic witness for calls to `$A9:C447`. The native body AI may deliberately
    /// reinstall the same list address, so pointer equality alone cannot identify a new
    /// request at the live physical head-slot boundary.
    /// </summary>
    private uint _headInstructionListRequestSerial;

    /// <summary>Brain instruction timer, reset to one whenever the AI changes its list.</summary>
    public ushort HeadInstructionTimer { get; private set; }

    /// <summary>
    /// Live bank-$A9 instruction cursor. <see cref="HeadInstructionList"/> retains the last
    /// list installed by body AI; this word advances through timed frames and command words.
    /// </summary>
    public ushort HeadInstructionPointer { get; private set; }

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
    public SnesAngle RainbowBeamAngle => _movement.RainbowBeamAngle;

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
    /// Count of body-AI writes to <see cref="SmallPurpleBreathGenerationEnabled"/>. The live
    /// flag has a second native writer, the brain-list opcode $A9:9F8E, so the live state
    /// publishes this flag only when the body actually wrote it.
    /// </summary>
    public uint SmallPurpleBreathGenerationWriteCount { get; private set; }

    /// <summary>Publishes a body-AI change to the small-purple-breath flag and records its write.</summary>
    /// <param name="enabled">Whether subsequent corpse updates may generate the small purple breath.</param>
    private void WriteSmallPurpleBreathGeneration(bool enabled)
    {
        SmallPurpleBreathGenerationEnabled = enabled;
        SmallPurpleBreathGenerationWriteCount++;
    }

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

    /// <summary>
    /// Native word <c>$0FF0</c>: the explosion interval timer of <c>$A9:B03E</c> and
    /// <c>$A9:B346</c>, aliased with the body sub-function pointer.
    /// </summary>
    public ushort DeathExplosionIntervalTimer { get; private set; }

    /// <summary>
    /// Native word <c>$0FF2</c>, one word under three names: the ascent dust's body
    /// sub-function timer, the seven-record death-explosion index (<c>$A9:B046</c>) and the
    /// four-record escape-door dust index (<c>$A9:B355</c>). Each effect inherits the value
    /// the previous one left unless native code explicitly clears it.
    /// </summary>
    public ushort DeathAndEscapeExplosionIndex { get; private set; }

    /// <summary>
    /// Inherits word <c>$0FF2</c> from the room's encounter state when the live sequence
    /// attaches; the fake-death ascent dust last wrote it.
    /// </summary>
    internal void InheritDeathAndEscapeExplosionIndex(ushort bodySubFunctionTimer) =>
        DeathAndEscapeExplosionIndex = bodySubFunctionTimer;

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
    public void InitializeCorpseRotting(ISnesAddressSpace bus) =>
        _corpseRotting.Initialize(bus, _corpseArtwork ?? throw new InvalidDataException(
            "Mother Brain corpse rotting requires installed tile artwork."));

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
        ushort bodyProperties,
        ushort bodyExtraProperties,
        ushort brainX,
        ushort brainY,
        ushort brainHealth,
        ushort brainProperties,
        ushort brainExtraProperties,
        ushort lowerNeckAngle,
        ushort upperNeckAngle,
        bool neckMovementEnabled,
        ushort lowerNeckMovementIndex,
        ushort upperNeckMovementIndex,
        ushort bombCounter,
        bool hitboxesEnabled)
    {
        Body.XPosition = bodyX;
        Body.YPosition = bodyY;
        Body.Pose = (ushort)bodyPose;
        Body.Form = form;
        BodyProperties = bodyProperties;
        BodyProperties2 = bodyExtraProperties;
        BrainXPosition = brainX;
        BrainYPosition = brainY;
        BrainHealth = brainHealth;
        BrainProperties = brainProperties;
        BrainProperties2 = brainExtraProperties;
        LowerNeckAngle = lowerNeckAngle;
        UpperNeckAngle = upperNeckAngle;
        NeckMovementEnabled = neckMovementEnabled ? (ushort)1 : (ushort)0;
        LowerNeckMovementIndex = lowerNeckMovementIndex;
        UpperNeckMovementIndex = upperNeckMovementIndex;
        BombCounter = bombCounter;
        HitboxesEnabled = hitboxesEnabled;
    }

    /// <summary>
    /// The list most recently requested by the state machine's body helper. Gameplay copies
    /// this pointer into its physical body slot only when the step result reports a walk or
    /// posture request; it never advances this private mirror's instruction timer.
    /// </summary>
    internal ushort RequestedBodyInstructionList => Body.InstructionPointer;

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
    /// Applies physical head opcode <c>$A9:9EA3</c>. Live gameplay advances the ordinary
    /// room instruction list, while the reusable head verifier advances its private list;
    /// both must mutate this one native saturating counter.
    /// </summary>
    internal void IncrementLiveBabyMetroidAttackCounter() =>
        BabyMetroidAttackCounter = Math.Min(
            unchecked((ushort)(BabyMetroidAttackCounter + 1)),
            (ushort)0x000c);

    /// <summary>Applies physical head opcode <c>$A9:9EB5</c>.</summary>
    internal void ResetLiveBabyMetroidAttackCounter() => BabyMetroidAttackCounter = 0;

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
    /// Applies the movement/recoil half of Mother Brain's shared phase-two/three shot
    /// reaction at <c>$A9:B562-$B5C4</c>. The ordinary enemy-shot routine owns damage,
    /// projectile deletion, and flash time; this method intentionally does not duplicate
    /// any of those separately translated systems.
    /// </summary>
    public void ApplyPhase2Or3ShotReaction(MotherBrainProjectileType projectileType)
    {
        // `$B58E` masks the projectile type to three bits before indexing an eight-byte
        // table. Validate the host enum so a caller cannot accidentally smuggle a larger
        // value past that native domain.
        MotherBrainShotReactionResult reaction =
            MotherBrainShotReaction.Resolve(Body.Form, projectileType, Phase3WalkCounter);
        Phase3WalkCounter = reaction.WalkCounter;
        if (reaction.HyperBeamRecoil)
        {
            // The neck function itself runs on Mother Brain's next ordinary phase-three
            // main call.
            Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil;
            FunctionTimer = 0;
        }
    }

    /// <summary>
    /// Ports the Baby's <c>$A9:C879</c> call to the standard backwards-walk helper using
    /// animation-delay index two and target <c>Body.X-1</c>.
    /// </summary>
    public bool RequestBabyStumbleBackward()
    {
        ushort targetX = unchecked((ushort)(Body.XPosition - 1));
        return MakeBodyWalkBackwards(targetX, BodyWalkingBackwardReallyFastInstructionList).Requested;
    }
}
