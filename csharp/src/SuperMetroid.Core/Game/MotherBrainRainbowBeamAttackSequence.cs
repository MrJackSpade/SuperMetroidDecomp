using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain's repeatable phase-two rainbow-beam, finish-off, and initial Baby-drain
/// interruption function chains at <c>$A9:B8EB-$A9:BE95</c>.
/// </summary>
/// <remarks>
/// The earlier general attack-selection logic and the spawned Baby Metroid's independent AI
/// remain separate actor phases. This class owns the neck extension, both charge waits, body
/// walks and posture changes, active beam, low-energy attack selection, final charge, the four
/// frame-spread Baby tile transfers, spawn request, and final-beam hold. Palette, HDMA,
/// projectile, earthquake, VRAM, spawn, and sound writes are retained as inspectable requests;
/// coordinate, resource, timer, instruction-list, and Samus-command mutations execute directly.
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
    public const ushort BodyWalkingForwardReallySlowInstructionList = 0x9818;
    public const ushort BodyWalkingBackwardReallySlowInstructionList = 0x993a;
    public const ushort BodyWalkingBackwardReallyFastInstructionList = 0x988c;
    public const ushort BodyStandingUpAfterCrouchingFastInstructionList = 0x99c6;
    public const ushort BodyStandingUpAfterLeaningDownInstructionList = 0x99e2;
    public const ushort BodyLeaningDownInstructionList = 0x99f2;

    // `$A9:8FE5-$9002` contains four 0x200-byte chunks. ProcessSpriteTilesTransfers
    // publishes exactly one entry per call, so these records also encode the exact four-call
    // loading delay before `$A9:BDDA` retracts the head and spawns the cutscene enemy.
    private static ReadOnlySpan<uint> BabyMetroidTileSources =>
        [0xb18400, 0xb18600, 0xb18800, 0xb18a00];

    private static ReadOnlySpan<ushort> BabyMetroidTileDestinations =>
        [0x7c00, 0x7d00, 0x7e00, 0x7f00];

    // `$A9:BCA6/$BCB6` are indexed after incrementing the explosion index. Keeping all eight
    // signed records here preserves the initial index-zero -> record-one behavior.
    private static ReadOnlySpan<short> ExplosionXOffsets =>
        [-8, 6, -4, 2, 3, -6, 8, 0];

    private static ReadOnlySpan<short> ExplosionYOffsets =>
        [-7, 2, 5, -4, 6, -2, -6, 7];

    private readonly MotherBrainRainbowBeamSamusMovement _movement = new();

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

    /// <summary>Brain instruction timer, reset to one whenever the AI changes its list.</summary>
    public ushort HeadInstructionTimer { get; private set; }

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

    /// <summary>Rainbow-beam palette animation index reset immediately before the final shot.</summary>
    public ushort RainbowBeamPaletteAnimationIndex { get; private set; }

    /// <summary>True once `$A9:BE1B` has requested the cutscene Baby enemy population entry.</summary>
    public bool BabyMetroidSpawned { get; private set; }

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

    /// <summary>Executes one call through the current Mother Brain body function.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort enemyFrameCounter,
        ushort mainEnemyExecutionCounter,
        bool powerBombActive = false,
        ushort randomNumberSeed = 0)
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
                    LowerNeckMovementIndex = 2;
                    UpperNeckMovementIndex = 4;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam:
                // Painful-walking `$A9:BE96+` is the next translation seam. Palette handling
                // remains active, while the Baby independently waits for corpse-state change.
                paletteRequested = true;
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
            finalBeamSoundQueued);
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

    private void SetHeadInstructionList(ushort pointer)
    {
        // `$A9:C447` writes the separate brain-list pointer and timer. Unlike ordinary
        // enemy programs it has no loop-counter write here.
        HeadInstructionList = pointer;
        HeadInstructionTimer = 1;
    }

    private void RetractHead()
    {
        // NTSC takes `$0050` from the regional `$0050/$0063` constant at `$A9:BB51`.
        NeckAngleDelta = 0x0050;
        NeckMovementEnabled = 1;
        LowerNeckMovementIndex = 8;
        UpperNeckMovementIndex = 6;
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
    bool FinalBeamSoundQueued);
