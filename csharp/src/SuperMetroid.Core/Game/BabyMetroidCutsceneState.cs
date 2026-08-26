using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal stateful translation of the Baby Metroid cutscene entrance, Mother Brain drain,
/// release, ceiling retreat, route to Samus, latch, and healing at
/// <c>$A9:C710-$A9:CABC</c> and its shared movement helpers.
/// </summary>
/// <remarks>
/// The retail routine does not use floating point, a spline, or a host physics engine. It
/// carries an 8.8 velocity, updates only byte <c>+1</c> of each enemy subposition, and lets
/// the carry from that byte addition enter the whole-pixel position. The curved entrance
/// also reads the cartridge's signed sine table at <c>$A0:B443</c>. Keeping those details
/// here is important: replacing them with visually similar interpolation changes the exact
/// frame on which the Baby's native collision rectangle reaches Mother Brain's brain slot.
/// </remarks>
public sealed class BabyMetroidCutsceneState
{
    // These are literal bank-$A9 instruction-list addresses. They are public debugger
    // witnesses, not arbitrary host animation IDs.
    public const ushort InitialInstructionList = 0xcfa2;
    public const ushort DrainingMotherBrainInstructionList = 0xcfb8;
    public const ushort CeilingToSamusMovementTable = 0xca24;

    // The movement records store native bank-$A9 code pointers, not a host enum. Keeping
    // their literal values lets the route reader reject corrupt or wrong-region ROM data
    // instead of silently assigning visually plausible acceleration.
    private const ushort GradualAccelerationExtraEightFunction = 0xf45f;
    private const ushort GradualAccelerationExtraTenFunction = 0xf466;
    private const ushort LatchOntoSamusFunction = 0xca66;

    // `$A9:93BB-$93CA` is shared by Mother Brain's brain shake and the latched Baby.
    // `Enemy.frameCounter & 6` is a byte offset into these four 16-bit entries.
    private static ReadOnlySpan<short> ShakingXOffsets => [0, -1, 0, 1];
    private static ReadOnlySpan<short> ShakingYOffsets => [0, 1, -1, 1];

    // `$A9:CDFC-$CE22` is stored as ten interleaved X/Y word pairs. The death handler
    // increments its shared index before looking up a pair, so a freshly cleared counter
    // deliberately begins at entry one rather than entry zero.
    private static ReadOnlySpan<short> DeathExplosionXOffsets =>
        [-24, -20, 16, 30, 14, -2, -2, -31, -4, 19];

    private static ReadOnlySpan<short> DeathExplosionYOffsets =>
        [-24, 20, -30, -3, -13, 18, -32, 8, -10, 19];

    // Killing the Baby restores Mother Brain's attack graphics over the four OBJ rows that
    // `$A9:8FE5` temporarily replaced. These are the literal `$A9:8FC7` source/destination
    // records consumed one per `$CCC0` call.
    private static ReadOnlySpan<uint> MotherBrainAttackTileSources =>
        [0xb7a000, 0xb7a200, 0xb7a400, 0xb7a600];

    private static ReadOnlySpan<ushort> MotherBrainAttackTileDestinations =>
        [0x7c00, 0x7d00, 0x7e00, 0x7f00];

    // Enemy header `$A0:ECBF` declares width/height `$24`. Despite the header macro's
    // friendly names, `$A0:8AFF/$8B05` copy those words directly into the enemy slot's
    // X/Y *radius* fields; there is no diameter-to-radius division anywhere in between.
    public const ushort XHitboxRadius = 0x0024;
    public const ushort YHitboxRadius = 0x0024;

    // Shared bank-$86 component math indexes this 16-bit sign-extended table with an
    // eight-bit angle. Angle zero points down; positive rotation is anti-clockwise.
    private const int SignedSineTable = 0xa0b443;

    /// <summary>Current native function-pointer equivalent.</summary>
    public BabyMetroidCutscenePhase Phase { get; private set; } =
        BabyMetroidCutscenePhase.Inactive;

    /// <summary>Enemy properties after initialization ORs in <c>$3000</c>.</summary>
    public ushort Properties { get; private set; }

    /// <summary>Enemy palette word. Flashing may later alternate this with zero.</summary>
    public ushort Palette { get; private set; }

    /// <summary>Enemy graphics offset selecting the four transferred Baby tile rows.</summary>
    public ushort GraphicsOffset { get; private set; }

    /// <summary>Current bank-$A9 enemy instruction list.</summary>
    public ushort InstructionList { get; private set; }

    /// <summary>Enemy instruction timer reset to one whenever the list changes.</summary>
    public ushort InstructionTimer { get; private set; }

    /// <summary>Enemy instruction loop counter.</summary>
    public ushort InstructionLoopCounter { get; private set; }

    /// <summary>Whole-pixel world X coordinate.</summary>
    public ushort XPosition { get; private set; }

    /// <summary>Whole-pixel world Y coordinate.</summary>
    public ushort YPosition { get; private set; }

    /// <summary>
    /// Native enemy X subposition. Only its high byte participates in this actor's mover;
    /// the low byte is deliberately retained and untouched.
    /// </summary>
    public ushort XSubposition { get; private set; }

    /// <summary>Native enemy Y subposition, with the same byte-<c>+1</c> behavior.</summary>
    public ushort YSubposition { get; private set; }

    /// <summary>Signed 8.8 horizontal velocity.</summary>
    public ushort XVelocity { get; private set; }

    /// <summary>Signed 8.8 vertical velocity.</summary>
    public ushort YVelocity { get; private set; }

    /// <summary>Unsigned 8.8 magnitude used by the curved entrance.</summary>
    public ushort Speed { get; private set; }

    /// <summary>8.8 angle; only the high byte is passed to shared component math.</summary>
    public ushort Angle { get; private set; }

    /// <summary>Native function timer, decremented with 16-bit wrap and BMI tests.</summary>
    public ushort FunctionTimer { get; private set; }

    /// <summary>Normal-palette handler delay initialized to ten and changed to one at latch.</summary>
    public ushort PaletteHandlerDelay { get; private set; }

    /// <summary>Initial request to permit the Baby cry effect.</summary>
    public bool CrySoundEnabled { get; private set; }

    /// <summary>Health-palette gate, initially disabled for the entrance.</summary>
    public bool HealthBasedPaletteEnabled { get; private set; }

    /// <summary>
    /// Bank-$A9 movement-table pointer installed after the ceiling collision. Zero means
    /// the table-driven ceiling-to-Samus route has not started yet.
    /// </summary>
    public ushort MovementTablePointer { get; private set; }

    /// <summary>
    /// Native enemy health. Header <c>$A0:ECBF</c> supplies 3,200; Mother Brain later drains
    /// this word after the healing hold, so it belongs to actor state rather than rendering.
    /// </summary>
    public ushort Health { get; private set; }

    /// <summary>
    /// Extra enemy word <c>$7E:7806,x</c>. Mother Brain's onion-ring collision handler
    /// writes <c>$10</c> here. Main AI observes the nonzero value as a doubled shake, then
    /// the later flashing handler decrements it and alternates the enemy palette.
    /// </summary>
    public ushort OnionRingHitFlashTimer { get; private set; }

    /// <summary>
    /// The low-health palette countdown installed by <c>$A9:CADA</c> when the ordinary
    /// ring volleys have reduced health to zero. This is separate from the hit-flash word.
    /// </summary>
    public ushort LowHealthPaletteTimer { get; private set; }

    /// <summary>Saved origin used by the fatal-blow shake at <c>$A9:CC3E</c>.</summary>
    public ushort FatalBlowOriginX { get; private set; }

    /// <summary>Saved origin used by the fatal-blow shake at <c>$A9:CC3E</c>.</summary>
    public ushort FatalBlowOriginY { get; private set; }

    /// <summary>Reload-eight timer producing the native nine-call black-palette cadence.</summary>
    public ushort FadeToBlackPaletteTimer { get; private set; }

    /// <summary>Current `$AD:E8E2` black-fade palette index; valid stored values are 0..6.</summary>
    public ushort FadeToBlackPaletteIndex { get; private set; }

    /// <summary>Five-call cadence word for the dust explosions surrounding the dying Baby.</summary>
    public ushort DeathExplosionTimer { get; private set; }

    /// <summary>
    /// Host name for the native layout alias called Mother Brain body's walk counter.
    /// Long-indexed access uses the Baby's slot, so this actor owns its increment/wrap.
    /// </summary>
    public ushort DeathExplosionPatternIndex { get; private set; }

    /// <summary>Zero-based index of the next `$A9:8FC7` attack-tile DMA record.</summary>
    public ushort AttackTileTransferIndex { get; private set; }

    /// <summary>Palette-table argument used while restoring the phase-three room lights.</summary>
    public ushort RoomLightsTransitionCounter { get; private set; }

    /// <summary>Fractional `$0300` accumulator used to slow the rainbow Samus palette.</summary>
    public ushort SamusRainbowPaletteAnimationCounter { get; private set; }

    /// <summary>Which of the two native rainbow-palette handlers is currently installed.</summary>
    public BabyMetroidSamusRainbowPhase SamusRainbowPhase { get; private set; }

    /// <summary>True when enemy property `$0100` suppresses the Baby's spritemap.</summary>
    public bool IsInvisible => (Properties & 0x0100) != 0;

    /// <summary>True after enemy property `$0200` marks the cutscene actor deleted.</summary>
    public bool IsDeleted => (Properties & 0x0200) != 0;

    /// <summary>
    /// Ports <c>$A9:C710</c>. The population record supplies <c>$2800</c>; initialization
    /// ORs <c>$3000</c>, overwrites the population coordinates, and waits at X/Y
    /// <c>$140/$60</c> before beginning the dash.
    /// </summary>
    public void Initialize(ushort populationProperties = 0x2800)
    {
        Properties = unchecked((ushort)(populationProperties | 0x3000));
        Palette = 0x0e00;
        GraphicsOffset = 0x00a0;
        SetInstructionList(InitialInstructionList);
        CrySoundEnabled = true;
        PaletteHandlerDelay = 0x000a;
        HealthBasedPaletteEnabled = false;
        MovementTablePointer = 0;
        Health = 3200;
        OnionRingHitFlashTimer = 0;
        LowHealthPaletteTimer = 0;
        FatalBlowOriginX = 0;
        FatalBlowOriginY = 0;
        FadeToBlackPaletteTimer = 0;
        FadeToBlackPaletteIndex = 0;
        DeathExplosionTimer = 0;
        DeathExplosionPatternIndex = 0;
        AttackTileTransferIndex = 0;
        RoomLightsTransitionCounter = 0;
        SamusRainbowPaletteAnimationCounter = 0;
        SamusRainbowPhase = BabyMetroidSamusRainbowPhase.Inactive;
        XPosition = 0x0140;
        YPosition = 0x0060;
        XSubposition = 0;
        YSubposition = 0;
        XVelocity = 0;
        YVelocity = 0;
        Speed = 0;
        Angle = 0;
        Phase = BabyMetroidCutscenePhase.DashOntoScreen;
        FunctionTimer = 0x00f8;
    }

    /// <summary>
    /// Executes one complete main-AI call: function first, then the unconditional enemy
    /// velocity mover at <c>$A9:C782</c>. Flash/palette presentation is intentionally kept
    /// as inspectable state because it does not alter this entrance's coordinates.
    /// </summary>
    public BabyMetroidCutsceneStepResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        ushort layer1X = 0,
        ushort layer1Y = 0,
        ushort enemyFrameCounter = 0,
        ushort randomNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(motherBrain);
        if (Phase == BabyMetroidCutscenePhase.Inactive)
            throw new InvalidOperationException("The cutscene Baby Metroid has not been initialized.");

        BabyMetroidCutscenePhase phaseBefore = Phase;
        BabyMetroidCutscenePoint before = Capture();
        bool brainCollision = false;
        bool samusStandingRequested = false;
        bool bodyStumbleRequested = false;
        bool motherBrainInterrupted = false;
        bool latchSoundQueued = false;
        bool dustCloudsRequested = false;
        bool samusCrouchingRequested = false;
        bool ambientCrySoundQueued = false;
        bool samusTouchCollision = false;
        bool healingCompleted = false;
        bool samusRainbowActivated = false;
        bool samusAnimationFrozen = false;
        bool samusRainbowDisabled = false;
        bool hyperBeamEnabled = false;
        bool phaseThreeHandoff = false;
        BabyMetroidDeathExplosionRequest? deathExplosion = null;
        BabyMetroidPaletteTransferRequest? babyPaletteTransfer = null;
        MotherBrainSpriteTileTransferRequest? attackTileTransfer = null;
        MotherBrainBackgroundPaletteTransferRequest? backgroundPaletteTransfer = null;

        switch (Phase)
        {
            case BabyMetroidCutscenePhase.DashOntoScreen:
                // `$F8` is decremented before BMI, so calls 1..248 leave the Baby still and
                // call 249 wraps zero to `$FFFF`. The expiry call immediately executes the
                // first curve update before the common mover consumes its new velocity.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Angle = 0xd800;
                    Speed = 0x0a00;
                    Phase = BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                    FunctionTimer = 0x000a;
                    goto case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                }
                break;

            case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead:
                UpdateSpeedAndAngle(bus, angleDelta: 0xfe80, targetAngle: 0xb000, targetSpeed: 0x0a00);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace;
                    FunctionTimer = 0x0009;
                }
                break;

            case BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace:
                UpdateSpeedAndAngle(bus, angleDelta: 0xfa00, targetAngle: 0x8200, targetSpeed: 0x0e00);
                brainCollision = CollidesWithRectangle(
                    motherBrain.BrainXPosition,
                    motherBrain.BrainYPosition,
                    rectangleXRadius: 4,
                    rectangleYRadius: 4);

                // Collision skips the timer decrement. Otherwise `$0009` permits ten
                // approach calls and expires when the tenth decrement wraps to `$FFFF`.
                if (!brainCollision)
                {
                    FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                    brainCollision = (FunctionTimer & 0x8000) != 0;
                }

                if (brainCollision)
                {
                    Phase = BabyMetroidCutscenePhase.LatchOntoMotherBrain;
                    samus.Drained.PutStanding(bus, samus); // Drained Samus command one.
                    samusStandingRequested = true;
                }
                break;

            case BabyMetroidCutscenePhase.LatchOntoMotherBrain:
            {
                ushort targetY = unchecked((ushort)(motherBrain.BrainYPosition - 0x0018));
                GraduallyAccelerateTowardsPoint(
                    motherBrain.BrainXPosition,
                    targetY,
                    accelerationDivisor: 0x10,
                    wrongWayOffScreenXSpeed: 0x0400,
                    layer1X,
                    layer1Y);
                brainCollision = CollidesWithRectangle(
                    motherBrain.BrainXPosition,
                    targetY,
                    rectangleXRadius: 8,
                    rectangleYRadius: 8);
                if (brainCollision)
                    Phase = BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack;
                break;
            }

            case BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack:
                // `$C879` passes animation-delay index two and target Body.X-1. With a
                // standing body this always installs the native really-fast backward list.
                bodyStumbleRequested = motherBrain.RequestBabyStumbleBackward();
                Phase = BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;
                goto case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;

            case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody:
            {
                ushort targetX = motherBrain.BrainXPosition;
                ushort targetY = unchecked((ushort)(motherBrain.BrainYPosition - 0x0018));
                bool reachedTarget = AccelerateTowardsPoint(targetX, targetY, acceleration: 0x0200);
                if (reachedTarget)
                {
                    // The helper predicts whole-pixel overshoot but does not itself store
                    // its returned target coordinate. `$C8A4-$C8B7` performs the exact pin.
                    XVelocity = 0;
                    YVelocity = 0;
                    XPosition = targetX;
                    YPosition = targetY;
                    SetInstructionList(DrainingMotherBrainInstructionList);
                    Phase = BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse;
                    PaletteHandlerDelay = 1;
                    motherBrain.InterruptFinalBeamForBabyDrain();
                    motherBrainInterrupted = true;
                    latchSoundQueued = true; // Sound library one, effect `$40`.
                }
                break;
            }

            case BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse:
            {
                int shakingIndex = (enemyFrameCounter & 6) >> 1;
                XPosition = unchecked((ushort)(
                    motherBrain.BrainXPosition + ShakingXOffsets[shakingIndex]));
                YPosition = unchecked((ushort)(
                    motherBrain.BrainYPosition + ShakingYOffsets[shakingIndex] - 0x0018));
                if (motherBrain.Phase2CorpseState != 0)
                {
                    Phase = BabyMetroidCutscenePhase.StopDraining;
                    FunctionTimer = 0x0040;
                }
                break;
            }

            case BabyMetroidCutscenePhase.StopDraining:
                // Shaking ceases immediately: every wait call pins to the unoffset brain
                // coordinate before decrementing `$40`. BMI expires only after 65 calls.
                XPosition = motherBrain.BrainXPosition;
                YPosition = unchecked((ushort)(motherBrain.BrainYPosition - 0x0018));
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetInstructionList(InitialInstructionList);
                    PaletteHandlerDelay = 0x000a;
                    Phase = BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds;
                    FunctionTimer = 0x0020;
                    XVelocity = 0;
                    YVelocity = 0;
                }
                break;

            case BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds:
                // `$C94B` branches *into* `$C959` while the timer is nonnegative. Thus the
                // Baby accelerates ceilingward for all 32 visible release calls; the dust
                // burst happens on call 33 and that call also performs another acceleration.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    dustCloudsRequested = true;
                    Phase = BabyMetroidCutscenePhase.MoveToTheCeiling;
                }
                goto case BabyMetroidCutscenePhase.MoveToTheCeiling;

            case BabyMetroidCutscenePhase.MoveToTheCeiling:
            {
                ushort targetX = motherBrain.BrainXPosition;
                const ushort targetY = 0;
                GraduallyAccelerateTowardsPoint(
                    targetX,
                    targetY,
                    accelerationDivisor: 0x10,
                    wrongWayOffScreenXSpeed: 0x0400,
                    layer1X,
                    layer1Y);
                bool ceilingCollision = CollidesWithRectangle(
                    targetX,
                    targetY,
                    rectangleXRadius: 4,
                    rectangleYRadius: 4);
                brainCollision = ceilingCollision;
                if (ceilingCollision)
                {
                    samus.Drained.PutCrouchingOrFalling(bus, samus);
                    samusCrouchingRequested = true;
                    Phase = BabyMetroidCutscenePhase.MoveToSamus;
                    MovementTablePointer = CeilingToSamusMovementTable;
                }
                break;
            }

            case BabyMetroidCutscenePhase.MoveToSamus:
            {
                // `$C9C3-$CA23` clears the ordinary cry request and enables health-based
                // palette selection on *every* route call. The random cry comparison uses
                // only the low twelve bits and succeeds for `$FA0-$FFF` (96 of 4096 seeds).
                CrySoundEnabled = false;
                HealthBasedPaletteEnabled = true;
                ambientCrySoundQueued = (randomNumber & 0x0fff) >= 0x0fa0;

                // `$CA24-$CA64` is eight overlapping records. Bytes +0/+2 are the target,
                // +4 is the acceleration-divisor-table index, +6 is a wrapper function,
                // and +8 is either the following record's X coordinate or, on the final
                // record, the signed `$CA66` function pointer. Reading this from the bus is
                // intentional: the private cartridge remains the authority for route data.
                int recordAddress = 0xa90000 | MovementTablePointer;
                ushort targetX = ReadWord(bus, recordAddress);
                ushort targetY = ReadWord(bus, recordAddress + 2);
                ushort divisorIndex = ReadWord(bus, recordAddress + 4);
                ushort movementFunction = ReadWord(bus, recordAddress + 6);

                // `$F56A` is `[10,0F,...,01]`; every retail `$CA24` record uses index zero,
                // but implementing all legal indices costs nothing and catches malformed
                // data before a divide-by-zero or an invented fallback can hide it.
                if (divisorIndex > 0x000f)
                {
                    throw new InvalidDataException(
                        $"Baby route ${MovementTablePointer:X4} has invalid divisor index ${divisorIndex:X4}.");
                }
                ushort accelerationDivisor = unchecked((ushort)(0x0010 - divisorIndex));
                ushort wrongWayExtra = movementFunction switch
                {
                    GradualAccelerationExtraEightFunction => 0x0008,
                    GradualAccelerationExtraTenFunction => 0x0010,
                    _ => throw new InvalidDataException(
                        $"Baby route ${MovementTablePointer:X4} names unknown movement function ${movementFunction:X4}."),
                };

                GraduallyAccelerateTowardsPoint(
                    targetX,
                    targetY,
                    accelerationDivisor,
                    wrongWayExtra,
                    layer1X,
                    layer1Y);

                // The route's 4x4 rectangle test occurs before the common velocity mover.
                // On ordinary records, +8 is merely the next record's target X and the
                // pointer advances eight bytes. The final record deliberately overlaps its
                // +8 word with `$CA66`, so BMI installs the next AI function instead.
                if (CollidesWithRectangle(targetX, targetY, 4, 4))
                {
                    ushort nextWord = ReadWord(bus, recordAddress + 8);
                    if ((nextWord & 0x8000) != 0)
                    {
                        if (nextWord != LatchOntoSamusFunction)
                        {
                            throw new InvalidDataException(
                                $"Baby route terminates at unexpected function ${nextWord:X4}.");
                        }
                        Phase = BabyMetroidCutscenePhase.LatchOntoSamus;
                    }
                    else
                    {
                        MovementTablePointer = unchecked((ushort)(MovementTablePointer + 8));
                    }
                }
                break;
            }

            case BabyMetroidCutscenePhase.LatchOntoSamus:
                // `$CA66` itself only steers toward Samus's centre minus twenty pixels. It
                // never performs the state change. That transition belongs to enemy-touch
                // handler `$CF03`, executed after main AI and the common mover below.
                GraduallyAccelerateTowardsPoint(
                    samus.XPosition,
                    unchecked((ushort)(samus.YPosition - 0x0014)),
                    accelerationDivisor: 0x10,
                    wrongWayOffScreenXSpeed: 0x0400,
                    layer1X,
                    layer1Y);
                break;

            case BabyMetroidCutscenePhase.HealSamusToFullHealth:
            {
                // `$CA7A` pins the actor to the four-entry shake table before healing. The
                // subsequent common mover is harmless because touch `$CF24/$CF27` zeroed
                // both velocities when it installed this function.
                CrySoundEnabled = false;
                int shakingIndex = (enemyFrameCounter & 6) >> 1;
                XPosition = unchecked((ushort)(samus.XPosition + ShakingXOffsets[shakingIndex]));
                YPosition = unchecked((ushort)(
                    samus.YPosition + ShakingYOffsets[shakingIndex] - 0x0014));

                // `$C59F` adds exactly one energy per enemy-processing call. CMP/BMI is a
                // signed 16-bit comparison; normal game maxima stay in its positive range.
                ushort candidateHealth = unchecked((ushort)(samus.Health + 1));
                if (unchecked((short)(candidateHealth - samus.MaxHealth)) < 0)
                {
                    samus.Health = candidateHealth;
                }
                else
                {
                    samus.Health = samus.MaxHealth;
                    samus.ReserveEnergy = samus.MaxReserveEnergy;
                    Phase = BabyMetroidCutscenePhase.IdleUntilNoHealth;
                    healingCompleted = true;
                }
                break;
            }

            case BabyMetroidCutscenePhase.IdleUntilNoHealth:
                // `$A9:CABD` consumes Mother Brain's one-shot cry flag before inspecting
                // the hit-flash word. Sound queuing is returned by the ring system; this
                // actor owns only the resulting shake and health-to-function transition.
                if (OnionRingHitFlashTimer != 0)
                {
                    int shakingIndex = (OnionRingHitFlashTimer & 6) >> 1;
                    XPosition = unchecked((ushort)(
                        samus.XPosition + 2 * ShakingXOffsets[shakingIndex]));
                    YPosition = unchecked((ushort)(
                        samus.YPosition + 2 * ShakingYOffsets[shakingIndex] - 0x0014));
                }

                if (Health == 0)
                {
                    // The zero reached by the ordinary volleys is a state trigger, not the
                    // health carried into the final charge. Native code deliberately gives
                    // the Baby `$0140` health, changes to the low-health palette function,
                    // disables health-band palettes, and lets `$CB13` run next frame.
                    Health = 0x0140;
                    Phase = BabyMetroidCutscenePhase.ReleaseSamus;
                    LowHealthPaletteTimer = 0x000a;
                    // `$A9:CB05` stores ten in the same enemy variable used by the normal
                    // palette handler.  This is not an immediate/zero-delay switch: the
                    // low-health palette routine counts through its own ten-frame cadence.
                    PaletteHandlerDelay = 0x000a;
                    HealthBasedPaletteEnabled = false;
                }
                break;

            case BabyMetroidCutscenePhase.ReleaseSamus:
                // `$A9:CB13` is a one-call cross-actor dispatcher. The native write to the
                // brain-slot speed word is presentation state; the meaningful body write
                // starts Mother Brain's fast retreat while this same call falls through to
                // the first flight update.
                motherBrain.PrepareForFinalBabyMetroidAttack();
                Phase = BabyMetroidCutscenePhase.StareDownMotherBrain;
                goto case BabyMetroidCutscenePhase.StareDownMotherBrain;

            case BabyMetroidCutscenePhase.StareDownMotherBrain:
                // Literal target from `$CB2D`: four pixels left of Samus and Y `$60`.
                // The helper's Y index zero means divisor `$10`; its wrong-way X extra is
                // zero here, unlike the earlier off-screen-safe entrance helpers.
                GraduallyAccelerateTowardsPoint(
                    unchecked((ushort)(samus.XPosition - 4)),
                    0x0060,
                    accelerationDivisor: 0x0010,
                    wrongWayOffScreenXSpeed: 0,
                    layer1X,
                    layer1Y);
                if (CollidesWithRectangle(
                    unchecked((ushort)(samus.XPosition - 4)), 0x0060, 4, 4))
                {
                    Phase = BabyMetroidCutscenePhase.FlyOffScreen;
                }
                break;

            case BabyMetroidCutscenePhase.FlyOffScreen:
                // Despite the function's historical name, this leg targets the fixed point
                // `(272,64)` just beyond the right edge of the original 256-pixel camera.
                GraduallyAccelerateTowardsPoint(
                    0x0110,
                    0x0040,
                    accelerationDivisor: 0x0010,
                    wrongWayOffScreenXSpeed: 0,
                    layer1X,
                    layer1Y);
                if (CollidesWithRectangle(0x0110, 0x0040, 4, 4))
                    Phase = BabyMetroidCutscenePhase.MoveToFinalChargeStart;
                break;

            case BabyMetroidCutscenePhase.MoveToFinalChargeStart:
                // `$CB7B` stages the Baby at `(305,160)`. Reaching that rectangle assigns
                // exactly 79 HP so the final four-ring volley kills on its first collision,
                // clears the brain-slot speed flag, and replaces Mother Brain's retreat AI.
                GraduallyAccelerateTowardsPoint(
                    0x0131,
                    0x00a0,
                    accelerationDivisor: 0x0010,
                    wrongWayOffScreenXSpeed: 0,
                    layer1X,
                    layer1Y);
                if (CollidesWithRectangle(0x0131, 0x00a0, 4, 4))
                {
                    Health = 0x004f;
                    motherBrain.ExecuteFinalBabyMetroidAttack();
                    Phase = BabyMetroidCutscenePhase.InitiateFinalCharge;
                }
                break;

            case BabyMetroidCutscenePhase.InitiateFinalCharge:
                // Divisor-table index `$A` maps to `$10-$A == 6`. This helper family adds
                // `$400` only when an off-screen actor is moving the wrong horizontal way.
                GraduallyAccelerateTowardsPoint(
                    0x0122,
                    0x0080,
                    accelerationDivisor: 0x0006,
                    wrongWayOffScreenXSpeed: 0x0400,
                    layer1X,
                    layer1Y);
                if (CollidesWithRectangle(0x0122, 0x0080, 4, 4))
                    Phase = BabyMetroidCutscenePhase.FinalCharge;
                break;

            case BabyMetroidCutscenePhase.FinalCharge:
                // The final target follows the brain enemy slot, 32 pixels above its
                // centre. Index `$C` maps to divisor four and therefore accelerates much
                // more aggressively than the staging leg.
                GraduallyAccelerateTowardsPoint(
                    motherBrain.BrainXPosition,
                    unchecked((ushort)(motherBrain.BrainYPosition - 0x0020)),
                    accelerationDivisor: 0x0004,
                    wrongWayOffScreenXSpeed: 0x0400,
                    layer1X,
                    layer1Y);
                if (Health == 0)
                {
                    // `$A9:CBF2-$CC3D` changes graphics/list state, freezes the body AI,
                    // clears velocity, remembers the shake origin, and falls through into
                    // the first of 17 fatal-blow shake calls.
                    GraphicsOffset = 0x10a0;
                    SetInstructionList(0xcfce);
                    XVelocity = 0;
                    YVelocity = 0;
                    Phase = BabyMetroidCutscenePhase.TakeFinalBlow;
                    FunctionTimer = 0x0010;
                    FatalBlowOriginX = XPosition;
                    FatalBlowOriginY = YPosition;
                    goto case BabyMetroidCutscenePhase.TakeFinalBlow;
                }
                break;

            case BabyMetroidCutscenePhase.TakeFinalBlow:
            {
                // `$A9:CE4C` uses the same four offsets, doubled, around a saved origin.
                int shakingIndex = (enemyFrameCounter & 6) >> 1;
                XPosition = unchecked((ushort)(FatalBlowOriginX + 2 * ShakingXOffsets[shakingIndex]));
                YPosition = unchecked((ushort)(FatalBlowOriginY + 2 * ShakingYOffsets[shakingIndex]));
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    XPosition = FatalBlowOriginX;
                    YPosition = FatalBlowOriginY;
                    Phase = BabyMetroidCutscenePhase.PlaySamusTheme;
                    FunctionTimer = 0x0038;
                    goto case BabyMetroidCutscenePhase.PlaySamusTheme;
                }
                break;
            }

            case BabyMetroidCutscenePhase.PlaySamusTheme:
                // The expiry call queues the two delayed music words and immediately
                // performs the first `$000C` prepare-Hyper-Beam decrement.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                    FunctionTimer = 0x000c;
                    goto case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                }
                break;

            case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    // Samus command `$19` freezes the drained animation at byte-index 28.
                    // `$CC8B` also installs the first rainbow handler; neither handler is
                    // executed until the Baby's following enemy-AI call.
                    samus.Drained.FreezeForHyperBeamAcquisition(samus);
                    samusAnimationFrozen = true;
                    SamusRainbowPhase = BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow;
                    Phase = BabyMetroidCutscenePhase.DeathSequence;
                }
                break;

            case BabyMetroidCutscenePhase.DeathSequence:
                StepSamusRainbowPaletteAnimation(samus, ref samusRainbowActivated);
                AccelerateDownwardsForDeath();
                if (StepFadeToBlack(ref babyPaletteTransfer))
                {
                    // Completion does not write palette index seven. It hides the actor,
                    // installs `$CCC0`, and waits through `$80..0` before the first DMA.
                    Properties |= 0x0100;
                    Phase = BabyMetroidCutscenePhase.UnloadTiles;
                    FunctionTimer = 0x0080;
                }
                else
                {
                    deathExplosion = StepDeathExplosion();
                    // `$CE24` clears invisibility on odd enemy frames and sets it on even.
                    Properties = (enemyFrameCounter & 1) != 0
                        ? unchecked((ushort)(Properties & 0xfeff))
                        : unchecked((ushort)(Properties | 0x0100));
                }
                break;

            case BabyMetroidCutscenePhase.UnloadTiles:
                StepSamusRainbowPaletteAnimation(samus, ref samusRainbowActivated);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    int transferIndex = AttackTileTransferIndex;
                    if ((uint)transferIndex >= (uint)MotherBrainAttackTileSources.Length)
                        throw new InvalidOperationException("Mother Brain attack-tile transfer list is already complete.");

                    attackTileTransfer = new MotherBrainSpriteTileTransferRequest(
                        EntryIndex: AttackTileTransferIndex,
                        Size: 0x0200,
                        SourceAddress: MotherBrainAttackTileSources[transferIndex],
                        VramDestination: MotherBrainAttackTileDestinations[transferIndex]);
                    AttackTileTransferIndex++;

                    if (AttackTileTransferIndex == MotherBrainAttackTileSources.Length)
                    {
                        // ProcessSpriteTilesTransfers observes the zero terminator after
                        // publishing entry four. Native falls straight into `$CCDE`, so the
                        // newly written `$B0` becomes `$AF` on this same call.
                        Phase = BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                        FunctionTimer = 0x00b0;
                        goto case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                    }
                }
                break;

            case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = BabyMetroidCutscenePhase.FinalCutscene;
                    RoomLightsTransitionCounter = 0;
                    goto case BabyMetroidCutscenePhase.FinalCutscene;
                }
                break;

            case BabyMetroidCutscenePhase.FinalCutscene:
            {
                // `$CCF0` increments the shared counter, passes its previous value to
                // `$AD:F24B`, and treats pointer-table entry seven as the carry-set end.
                ushort paletteIndex = RoomLightsTransitionCounter;
                RoomLightsTransitionCounter = unchecked((ushort)(RoomLightsTransitionCounter + 1));
                if (paletteIndex < 7)
                {
                    backgroundPaletteTransfer = new MotherBrainBackgroundPaletteTransferRequest(
                        PaletteIndex: paletteIndex,
                        SourceAddress: unchecked((uint)(0xadf3d3 - paletteIndex * 0x38)),
                        FirstDestinationColorIndex: 0x0062,
                        SecondDestinationColorIndex: 0x00a2,
                        ColorsPerDestination: 0x000e);
                    break;
                }

                motherBrain.BeginPhase3RecoveryFromBabyCutscene();
                samus.Drained.DisableRainbowAndStartStandingAnimation(samus);
                samusRainbowDisabled = true;
                samus.Drained.EnableHyperBeam(samus);
                hyperBeamEnabled = true;
                Properties |= 0x0200;
                phaseThreeHandoff = true;
                break;
            }

            default:
                throw new InvalidOperationException($"Unsupported Baby Metroid phase {Phase}.");
        }

        // Main AI calls this even on phase transitions and target snaps. In the snap case
        // both velocities were explicitly zeroed, so the pin remains exact.
        MoveAccordingToVelocity();

        // Generic enemy processing invokes touch AI after main AI. `$CF03` is active only
        // during `$CA66`; broad Samus/enemy hitboxes gate it, then its own acceleration-$10
        // helper decides when both axes have actually reached the latch point. Its velocity
        // changes therefore apply on the following main-AI mover, exactly like the SNES.
        if (Phase == BabyMetroidCutscenePhase.LatchOntoSamus &&
            CollidesWithRectangle(
                samus.XPosition,
                samus.YPosition,
                samus.Kinematics.XRadius,
                samus.Kinematics.YRadius))
        {
            samusTouchCollision = true;
            bool reachedLatchPoint = AccelerateTowardsPoint(
                samus.XPosition,
                unchecked((ushort)(samus.YPosition - 0x0014)),
                acceleration: 0x0010);
            if (reachedLatchPoint)
            {
                XVelocity = 0;
                YVelocity = 0;
                Phase = BabyMetroidCutscenePhase.HealSamusToFullHealth;
            }
        }

        // `$A9:C79C` is called after the common velocity mover. It decrements the same
        // extra word that onion-ring hits write and shows palette zero while bit one of the
        // decremented value is set. Starting from `$10`, the first post-hit enemy call sees
        // `$10` for its shake above, then displays the normal palette with timer `$0F`.
        if (OnionRingHitFlashTimer != 0)
        {
            OnionRingHitFlashTimer = unchecked((ushort)(OnionRingHitFlashTimer - 1));
            Palette = (OnionRingHitFlashTimer & 2) != 0 ? (ushort)0 : (ushort)0x0e00;
        }
        else
        {
            Palette = 0x0e00;
        }

        return new BabyMetroidCutsceneStepResult(
            phaseBefore,
            Phase,
            before,
            Capture(),
            XVelocity,
            YVelocity,
            Speed,
            Angle,
            FunctionTimer,
            brainCollision,
            samusStandingRequested,
            bodyStumbleRequested,
            motherBrainInterrupted,
            latchSoundQueued,
            InstructionList,
            dustCloudsRequested,
            samusCrouchingRequested,
            MovementTablePointer,
            ambientCrySoundQueued,
            samusTouchCollision,
            healingCompleted,
            Health,
            samusRainbowActivated,
            samusAnimationFrozen,
            samusRainbowDisabled,
            hyperBeamEnabled,
            phaseThreeHandoff,
            deathExplosion,
            babyPaletteTransfer,
            attackTileTransfer,
            backgroundPaletteTransfer);
    }

    /// <summary>
    /// Applies <c>$86:C381-$C3A8</c>'s Baby half of an onion-ring collision. The projectile
    /// system owns explosion/deletion and the Mother Brain cry flag; the enemy slot owns
    /// this flash timer and saturating health subtraction.
    /// </summary>
    public BabyMetroidOnionRingHitResult ApplyMotherBrainOnionRingHit(ushort damage = 0x0050)
    {
        ushort healthBefore = Health;
        if (healthBefore == 0)
            return new BabyMetroidOnionRingHitResult(false, healthBefore, healthBefore, OnionRingHitFlashTimer);

        OnionRingHitFlashTimer = 0x0010;
        Health = healthBefore < damage
            ? (ushort)0
            : unchecked((ushort)(healthBefore - damage));
        return new BabyMetroidOnionRingHitResult(true, healthBefore, Health, OnionRingHitFlashTimer);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private void StepSamusRainbowPaletteAnimation(
        SamusState samus,
        ref bool samusRainbowActivated)
    {
        switch (SamusRainbowPhase)
        {
            case BabyMetroidSamusRainbowPhase.Inactive:
                return;

            case BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow:
                // `$CD30` uses a signed CMP/BMI after adding sixteen to the Baby's Y.
                if (unchecked((short)(YPosition + 0x0010 - samus.YPosition)) < 0)
                    return;
                samus.Drained.EnableRainbow(samus);
                samusRainbowActivated = true;
                SamusRainbowPhase = BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown;
                return;

            case BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown:
            {
                uint sum = (uint)SamusRainbowPaletteAnimationCounter + 0x0300;
                SamusRainbowPaletteAnimationCounter = unchecked((ushort)sum);
                if (sum > ushort.MaxValue)
                    samus.Drained.IncrementRainbowPaletteFrame(maximumFrame: 10);
                return;
            }

            default:
                throw new InvalidOperationException($"Unsupported Samus rainbow phase {SamusRainbowPhase}.");
        }
    }

    private void AccelerateDownwardsForDeath()
    {
        // `$CE40` subtracts `$20` from the magnitude, clamps at zero, reapplies the old
        // sign, and independently adds two to vertical 8.8 velocity with 16-bit wrap.
        bool negative = (XVelocity & 0x8000) != 0;
        ushort magnitude = negative ? unchecked((ushort)-XVelocity) : XVelocity;
        magnitude = magnitude >= 0x0020 ? unchecked((ushort)(magnitude - 0x0020)) : (ushort)0;
        XVelocity = negative ? unchecked((ushort)-magnitude) : magnitude;
        YVelocity = unchecked((ushort)(YVelocity + 2));
    }

    private bool StepFadeToBlack(ref BabyMetroidPaletteTransferRequest? paletteTransfer)
    {
        // No timer changes occur until the actor's centre reaches Y `$80`.
        if (unchecked((short)(YPosition - 0x0080)) < 0)
            return false;

        FadeToBlackPaletteTimer = unchecked((ushort)(FadeToBlackPaletteTimer - 1));
        if ((FadeToBlackPaletteTimer & 0x8000) == 0)
            return false;

        FadeToBlackPaletteTimer = 8;
        ushort nextIndex = unchecked((ushort)(FadeToBlackPaletteIndex + 1));
        if (nextIndex >= 7)
            return true;

        FadeToBlackPaletteIndex = nextIndex;
        paletteTransfer = new BabyMetroidPaletteTransferRequest(
            PaletteIndex: nextIndex,
            SourceAddress: unchecked((uint)(0xade8f0 + nextIndex * 0x1c)),
            DestinationColorIndex: 0x01e2,
            ColorCount: 0x000e);
        return false;
    }

    private BabyMetroidDeathExplosionRequest? StepDeathExplosion()
    {
        DeathExplosionTimer = unchecked((ushort)(DeathExplosionTimer - 1));
        if ((DeathExplosionTimer & 0x8000) == 0)
            return null;

        DeathExplosionTimer = 4;
        DeathExplosionPatternIndex++;
        if (DeathExplosionPatternIndex >= 10)
            DeathExplosionPatternIndex = 0;
        int index = DeathExplosionPatternIndex;
        return new BabyMetroidDeathExplosionRequest(
            PatternIndex: DeathExplosionPatternIndex,
            XPosition: unchecked((ushort)(XPosition + DeathExplosionXOffsets[index])),
            YPosition: unchecked((ushort)(YPosition + DeathExplosionYOffsets[index])),
            ProjectileParameter: 3,
            SoundEffect: 0x0013);
    }

    private void SetInstructionList(ushort pointer)
    {
        InstructionList = pointer;
        InstructionTimer = 1;
        InstructionLoopCounter = 0;
    }

    private void UpdateSpeedAndAngle(
        ISnesAddressSpace bus,
        ushort angleDelta,
        ushort targetAngle,
        ushort targetSpeed)
    {
        // `$CF31` changes speed by exactly `$20` and clamps rather than crossing the target.
        if (Speed != targetSpeed)
        {
            if (targetSpeed < Speed)
                Speed = Speed - 0x20 < targetSpeed ? targetSpeed : unchecked((ushort)(Speed - 0x20));
            else
                Speed = Speed + 0x20 >= targetSpeed ? targetSpeed : unchecked((ushort)(Speed + 0x20));
        }

        ushort candidateAngle = unchecked((ushort)(Angle + angleDelta));
        if (unchecked((short)angleDelta) < 0)
        {
            Angle = unchecked((short)(candidateAngle - targetAngle)) >= 0
                ? candidateAngle
                : targetAngle;
        }
        else
        {
            Angle = unchecked((short)(candidateAngle - targetAngle)) < 0
                ? candidateAngle
                : targetAngle;
        }

        byte angleByte = unchecked((byte)(Angle >> 8));
        XVelocity = CalculateVelocityComponent(bus, Speed, angleByte);
        YVelocity = CalculateVelocityComponent(bus, Speed, unchecked((byte)(angleByte + 0x40)));
    }

    private static ushort CalculateVelocityComponent(
        ISnesAddressSpace bus,
        ushort speed,
        byte sineIndex)
    {
        int address = SignedSineTable + sineIndex * 2;
        short sine = unchecked((short)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

        // Bank `$86:C27A` multiplies unsigned speed by the absolute signed-table value,
        // returns product bits 8..23, then reapplies the original sign.
        uint product = unchecked((uint)(speed * Math.Abs((int)sine)));
        ushort magnitude = unchecked((ushort)(product >> 8));
        return sine < 0 ? unchecked((ushort)-magnitude) : magnitude;
    }

    private void MoveAccordingToVelocity()
    {
        XPosition = AddNativeEightEightVelocity(XPosition, XSubposition, XVelocity, out ushort xSubposition);
        XSubposition = xSubposition;
        YPosition = AddNativeEightEightVelocity(YPosition, YSubposition, YVelocity, out ushort ySubposition);
        YSubposition = ySubposition;
    }

    private static ushort AddNativeEightEightVelocity(
        ushort wholePosition,
        ushort subposition,
        ushort velocity,
        out ushort newSubposition)
    {
        // SEP #$20 adds velocity's low byte to subposition byte +1. REP #$20 then sign-
        // extends velocity's high byte, retaining the 8-bit ADC carry for whole pixels.
        int fractionalSum = (subposition >> 8) + (velocity & 0x00ff);
        newSubposition = unchecked((ushort)(
            ((byte)fractionalSum << 8) | (subposition & 0x00ff)));
        int wholeDelta = unchecked((sbyte)(velocity >> 8)) + (fractionalSum > 0xff ? 1 : 0);
        return unchecked((ushort)(wholePosition + wholeDelta));
    }

    private bool CollidesWithRectangle(
        ushort centerX,
        ushort centerY,
        ushort rectangleXRadius,
        ushort rectangleYRadius)
    {
        // `$A9:EF06` adds both radii and one, then rejects `abs(delta) >= sum+1`.
        // In the normal room-coordinate domain that is equivalently `abs(delta) <= sum`.
        int xDistance = Math.Abs(unchecked((short)(centerX - XPosition)));
        if (xDistance > rectangleXRadius + XHitboxRadius)
            return false;
        int yDistance = Math.Abs(unchecked((short)(centerY - YPosition)));
        return yDistance <= rectangleYRadius + YHitboxRadius;
    }

    private void GraduallyAccelerateTowardsPoint(
        ushort targetX,
        ushort targetY,
        ushort accelerationDivisor,
        ushort wrongWayOffScreenXSpeed,
        ushort layer1X,
        ushort layer1Y)
    {
        GraduallyAccelerateHorizontally(
            targetX,
            accelerationDivisor,
            wrongWayOffScreenXSpeed,
            layer1X,
            layer1Y);

        short signedDistance = unchecked((short)(YPosition - targetY));
        if (signedDistance == 0)
            return;
        int acceleration = Math.Max(1, Math.Abs((int)signedDistance) / accelerationDivisor);
        int velocity = unchecked((short)YVelocity);
        if (signedDistance < 0)
        {
            velocity += velocity < 0 ? 8 + acceleration * 2 : acceleration;
            YVelocity = unchecked((ushort)Math.Min(velocity, 0x0500));
        }
        else
        {
            velocity -= velocity >= 0 ? 8 + acceleration * 2 : acceleration;
            YVelocity = unchecked((ushort)Math.Max(velocity, -0x0500));
        }
    }

    private void GraduallyAccelerateHorizontally(
        ushort targetX,
        ushort accelerationDivisor,
        ushort wrongWayOffScreenSpeed,
        ushort layer1X,
        ushort layer1Y)
    {
        short signedDistance = unchecked((short)(XPosition - targetX));
        if (signedDistance == 0)
            return;
        int acceleration = Math.Max(1, Math.Abs((int)signedDistance) / accelerationDivisor);
        int velocity = unchecked((short)XVelocity);
        bool offScreen = IsVaguelyOffScreen(layer1X, layer1Y);

        if (signedDistance < 0)
        {
            if (velocity < 0)
            {
                // The off-screen helper returns carry set. The following ADC therefore
                // adds `$0401`, an easily missed one-unit native asymmetry.
                if (offScreen)
                    velocity += wrongWayOffScreenSpeed + 1;
                velocity += 8 + acceleration * 2;
            }
            else
            {
                velocity += acceleration;
            }
            XVelocity = unchecked((ushort)Math.Min(velocity, 0x0800));
        }
        else
        {
            if (velocity >= 0)
            {
                if (offScreen)
                    velocity -= wrongWayOffScreenSpeed;
                velocity -= 8 + acceleration * 2;
            }
            else
            {
                velocity -= acceleration;
            }
            XVelocity = unchecked((ushort)Math.Max(velocity, -0x0800));
        }
    }

    private bool IsVaguelyOffScreen(ushort layer1X, ushort layer1Y)
    {
        // This is the signed-branch sequence at `$A9:F57A`; its generous rectangle extends
        // 16 pixels left/right and 96 pixels vertically beyond the ordinary 256x224 view.
        if (unchecked((short)YPosition) < 0)
            return true;
        short relativeY = unchecked((short)(YPosition + 0x0060 - layer1Y));
        if (relativeY < 0 || relativeY >= 0x01a0)
            return true;
        if (unchecked((short)XPosition) < 0)
            return true;
        short relativeX = unchecked((short)(XPosition + 0x0010 - layer1X));
        return relativeX < 0 || relativeX >= 0x0120;
    }

    private bool AccelerateTowardsPoint(ushort targetX, ushort targetY, ushort acceleration)
    {
        // `$F5A6` counts axes whose next whole-pixel prediction reaches/crosses target,
        // then shifts the count twice. Carry is set only for count two.
        bool reachedX = AccelerateTowardsXPosition(targetX, acceleration);
        bool reachedY = AccelerateTowardsYPosition(targetY, acceleration);
        return reachedX && reachedY;
    }

    private bool AccelerateTowardsYPosition(ushort targetY, ushort acceleration)
    {
        short difference = unchecked((short)(YPosition - targetY));
        if (difference == 0)
            return true;

        if (difference < 0)
        {
            int velocity = Math.Min(unchecked((short)YVelocity) + acceleration, 0x0500);
            YVelocity = unchecked((ushort)velocity);
            ushort prediction = unchecked((ushort)(YPosition + unchecked((sbyte)(YVelocity >> 8))));
            if (unchecked((short)(prediction - targetY)) < 0)
                return false;
        }
        else
        {
            int velocity = Math.Max(unchecked((short)YVelocity) - acceleration, -0x0500);
            YVelocity = unchecked((ushort)velocity);
            ushort prediction = unchecked((ushort)(YPosition + unchecked((sbyte)(YVelocity >> 8))));
            short predictedDifference = unchecked((short)(prediction - targetY));
            if (predictedDifference > 0)
                return false;
        }

        YVelocity = 0;
        return true;
    }

    private bool AccelerateTowardsXPosition(ushort targetX, ushort acceleration)
    {
        short difference = unchecked((short)(XPosition - targetX));
        if (difference < 0)
        {
            int velocity = Math.Min(unchecked((short)XVelocity) + acceleration, 0x0500);
            XVelocity = unchecked((ushort)velocity);
            ushort prediction = unchecked((ushort)(XPosition + unchecked((sbyte)(XVelocity >> 8))));
            if (unchecked((short)(prediction - targetX)) < 0)
                return false;
        }
        else
        {
            // Native deliberately sends exact equality through the left branch; unlike Y,
            // there is no BEQ before BPL at `$A9:F619-$F61B`.
            int velocity = Math.Max(unchecked((short)XVelocity) - acceleration, -0x0500);
            XVelocity = unchecked((ushort)velocity);
            ushort prediction = unchecked((ushort)(XPosition + unchecked((sbyte)(XVelocity >> 8))));
            short predictedDifference = unchecked((short)(prediction - targetX));
            if (predictedDifference > 0)
                return false;
        }

        XVelocity = 0;
        return true;
    }

    private BabyMetroidCutscenePoint Capture() => new(
        XPosition,
        XSubposition,
        YPosition,
        YSubposition);
}

/// <summary>Named equivalents of the entrance's bank-$A9 function pointers.</summary>
public enum BabyMetroidCutscenePhase
{
    Inactive,
    DashOntoScreen,
    CurveTowardMotherBrainHead,
    GetRightUpInMotherBrainsFace,
    LatchOntoMotherBrain,
    SetMotherBrainToStumbleBack,
    ActivateRainbowBeamAndMotherBrainBody,
    WaitForMotherBrainToTurnToCorpse,
    StopDraining,
    LetGoAndSpawnDustClouds,
    MoveToTheCeiling,
    MoveToSamus,
    LatchOntoSamus,
    HealSamusToFullHealth,
    IdleUntilNoHealth,
    ReleaseSamus,
    StareDownMotherBrain,
    FlyOffScreen,
    MoveToFinalChargeStart,
    InitiateFinalCharge,
    FinalCharge,
    TakeFinalBlow,
    PlaySamusTheme,
    PrepareSamusForHyperBeam,
    DeathSequence,
    UnloadTiles,
    LetSamusRainbowSomeMore,
    FinalCutscene,
}

/// <summary>Named equivalents of `$A9:CD30/$CD4B`'s indirect palette functions.</summary>
public enum BabyMetroidSamusRainbowPhase
{
    Inactive,
    ActivateWhenEnemyIsLow,
    GraduallySlowAnimationDown,
}

/// <summary>Health/flash mutation produced by one colliding Mother Brain blue ring.</summary>
public readonly record struct BabyMetroidOnionRingHitResult(
    bool Applied,
    ushort HealthBefore,
    ushort HealthAfter,
    ushort FlashTimer);

/// <summary>Whole/subpixel coordinates before or after one cutscene-enemy main-AI call.</summary>
public readonly record struct BabyMetroidCutscenePoint(
    ushort XPosition,
    ushort XSubposition,
    ushort YPosition,
    ushort YSubposition);

/// <summary>One `$86:E509` dust explosion requested during the Baby's death.</summary>
public readonly record struct BabyMetroidDeathExplosionRequest(
    ushort PatternIndex,
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter,
    ushort SoundEffect);

/// <summary>One fourteen-colour write from `$AD:E90C-$E998` to sprite palette seven.</summary>
public readonly record struct BabyMetroidPaletteTransferRequest(
    ushort PaletteIndex,
    uint SourceAddress,
    ushort DestinationColorIndex,
    ushort ColorCount);

/// <summary>One phase-three room-light palette pair selected by `$AD:F24B`.</summary>
public readonly record struct MotherBrainBackgroundPaletteTransferRequest(
    ushort PaletteIndex,
    uint SourceAddress,
    ushort FirstDestinationColorIndex,
    ushort SecondDestinationColorIndex,
    ushort ColorsPerDestination);

/// <summary>One-call debugger witness for the Baby entrance and latch chain.</summary>
public readonly record struct BabyMetroidCutsceneStepResult(
    BabyMetroidCutscenePhase PhaseBefore,
    BabyMetroidCutscenePhase PhaseAfter,
    BabyMetroidCutscenePoint Before,
    BabyMetroidCutscenePoint After,
    ushort XVelocity,
    ushort YVelocity,
    ushort Speed,
    ushort Angle,
    ushort FunctionTimer,
    bool BrainCollision,
    bool SamusStandingRequested,
    bool BodyStumbleRequested,
    bool MotherBrainInterrupted,
    bool LatchSoundQueued,
    ushort InstructionList,
    bool DustCloudsRequested,
    bool SamusCrouchingRequested,
    ushort MovementTablePointer,
    bool AmbientCrySoundQueued,
    bool SamusTouchCollision,
    bool HealingCompleted,
    ushort Health,
    bool SamusRainbowActivated,
    bool SamusAnimationFrozen,
    bool SamusRainbowDisabled,
    bool HyperBeamEnabled,
    bool PhaseThreeHandoff,
    BabyMetroidDeathExplosionRequest? DeathExplosion,
    BabyMetroidPaletteTransferRequest? BabyPaletteTransfer,
    MotherBrainSpriteTileTransferRequest? AttackTileTransfer,
    MotherBrainBackgroundPaletteTransferRequest? BackgroundPaletteTransfer);
