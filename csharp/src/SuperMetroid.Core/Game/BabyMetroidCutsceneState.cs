using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal stateful translation of the Baby Metroid cutscene entrance, Mother Brain drain,
/// release, and ceiling retreat at <c>$A9:C710-$A9:C98B</c> and its shared movement helpers.
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

    // `$A9:93BB-$93CA` is shared by Mother Brain's brain shake and the latched Baby.
    // `Enemy.frameCounter & 6` is a byte offset into these four 16-bit entries.
    private static ReadOnlySpan<short> ShakingXOffsets => [0, -1, 0, 1];
    private static ReadOnlySpan<short> ShakingYOffsets => [0, 1, -1, 1];

    // Enemy header `$A0:ECBF` declares width/height `$24`. Generic enemy initialization
    // stores half of those values in the slot's collision-radius words.
    public const ushort XHitboxRadius = 0x0012;
    public const ushort YHitboxRadius = 0x0012;

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
        ushort enemyFrameCounter = 0)
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
                // `$C9C3+` consumes the route table beginning at `$CA24`; that multi-leg
                // flight and subsequent Samus latch/heal are the next explicit actor seam.
                break;

            default:
                throw new InvalidOperationException($"Unsupported Baby Metroid phase {Phase}.");
        }

        // Main AI calls this even on phase transitions and target snaps. In the snap case
        // both velocities were explicitly zeroed, so the pin remains exact.
        MoveAccordingToVelocity();

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
            MovementTablePointer);
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
}

/// <summary>Whole/subpixel coordinates before or after one cutscene-enemy main-AI call.</summary>
public readonly record struct BabyMetroidCutscenePoint(
    ushort XPosition,
    ushort XSubposition,
    ushort YPosition,
    ushort YSubposition);

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
    ushort MovementTablePointer);
