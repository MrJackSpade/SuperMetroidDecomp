using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal translation of the four Samus-position helpers at <c>$A9:BBB5-$BC75</c>
/// used by Mother Brain's phase-two rainbow beam.
/// </summary>
/// <remarks>
/// This state belongs to Mother Brain, not to Samus's movement-type dispatcher. Pose type
/// <c>$1B</c> deliberately returns without moving at <c>$90:A7DA</c>; the enemy AI calls
/// these helpers separately after it has locked Samus. Keeping that ownership explicit
/// prevents a future runtime from accidentally applying both ordinary and forced movement.
///
/// The native helpers use a peculiar 8.8 velocity against only byte <c>+1</c> of Samus's
/// 16-bit subposition. The low subposition byte is untouched, the 8-bit addition's carry
/// enters the whole-pixel addition, and both current and previous positions are overwritten
/// with the result. This implementation spells out those byte operations instead of using
/// floating point or the ordinary 16.16 movement helpers.
/// </remarks>
public sealed class MotherBrainRainbowBeamSamusMovement
{
    // `$A0:B443` is the signed 8-bit sine table consumed by the shared bank-$86 velocity
    // component routine. Entries are 16-bit sign-extended values in the range -256..256.

    /// <summary>Mother Brain body extra word <c>$0FB2</c>, interpreted as signed 8.8.</summary>
    public ushort CustomXVelocity { get; private set; }

    /// <summary>Mother Brain body extra word <c>$0FB4</c>, interpreted as signed 8.8.</summary>
    public ushort CustomYVelocity { get; private set; }

    /// <summary>
    /// The low-byte beam angle stored by <c>$A9:BBA8-$BBAB</c>; zero points down and the
    /// positive direction is anti-clockwise, matching the shared bank-$86 trig routines.
    /// </summary>
    public SnesAngle RainbowBeamAngle { get; set; }

    /// <summary>
    /// Ports <c>$A9:BBB5</c>: move right at <c>$10.00</c> pixels per call and, until the
    /// hardcoded wall is reached, derive the vertical component from the live beam angle.
    /// </summary>
    public MotherBrainForcedSamusMovementResult MoveTowardWall(
        ISnesAddressSpace bus,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        SamusCameraPoint before = Capture(samus);
        bool reachedWall = MoveHorizontallyTowardWall(samus, velocity: 0x1000);
        ushort yVelocity = 0;
        bool reachedVerticalBoundary = false;

        if (!reachedWall)
        {
            // `$86:C272` adds $40 before indexing the sine table. Its multiplication returns
            // product bits 8..23, i.e. a signed 8.8 component for the supplied 8.8 speed.
            yVelocity = CalculateYVelocity(speed: 0x1000, RainbowBeamAngle);
            reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(samus, yVelocity);

            // `$A9:BBCD` unconditionally clears carry after the vertical helper. The caller
            // can therefore observe only the wall result; keep the clamp witness separately
            // for debugging without pretending it controls the native state transition.
        }

        return CreateResult(
            before,
            samus,
            xVelocity: 0x1000,
            yVelocity,
            reachedWall,
            reachedVerticalBoundary,
            nativeCarry: reachedWall);
    }

    /// <summary>
    /// Ports <c>$A9:BBCF</c>: move down by <c>$00.40</c> below/equal to Y <c>$7C</c>, or
    /// up by <c>-$00.40</c> above it. Native does not snap to the target and can oscillate.
    /// </summary>
    public static MotherBrainForcedSamusMovementResult MoveTowardMiddleOfWall(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);

        SamusCameraPoint before = Capture(samus);

        // `CPY SamusYPosition / BPL` tests signed(YTarget-currentY). For the fight's normal
        // coordinate range this is exactly currentY <= $7C, but preserve the 16-bit branch
        // semantics instead of introducing a host integer comparison with different wraps.
        ushort difference = unchecked((ushort)(0x007c - samus.YPosition));
        ushort yVelocity = unchecked((short)difference >= 0 ? (ushort)0x0040 : (ushort)0xffc0);
        bool reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(samus, yVelocity);

        return CreateResult(
            before,
            samus,
            xVelocity: 0,
            yVelocity,
            reachedWall: false,
            reachedVerticalBoundary,
            nativeCarry: reachedVerticalBoundary);
    }

    /// <summary>
    /// Ports <c>$A9:BA82-$BA88</c>, executed when the rainbow beam has narrowed below its
    /// finishing threshold: initial left speed <c>-$01.00</c>, zero vertical speed.
    /// </summary>
    public void BeginFallingAfterRainbowBeam()
    {
        CustomXVelocity = 0xff00;
        CustomYVelocity = 0;
    }

    /// <summary>
    /// Ports <c>$A9:BBE1</c>: ease horizontal speed toward zero by <c>$00.02</c>, add
    /// <c>$00.18</c> vertical speed, then move through the two hardcoded arena clamps.
    /// </summary>
    public MotherBrainForcedSamusMovementResult StepFallingAfterRainbowBeam(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);

        SamusCameraPoint before = Capture(samus);

        // Native ADC wraps to 16 bits and BMI examines the resulting sign bit. As soon as
        // the negative speed crosses into $0000..$7FFF it is clamped to exact zero.
        ushort acceleratedX = unchecked((ushort)(CustomXVelocity + 0x0002));
        CustomXVelocity = unchecked((short)acceleratedX < 0 ? acceleratedX : (ushort)0);
        bool reachedWall = MoveHorizontallyTowardWall(samus, CustomXVelocity);

        CustomYVelocity = unchecked((ushort)(CustomYVelocity + 0x0018));
        bool reachedVerticalBoundary =
            MoveVerticallyTowardCeilingOrFloor(samus, CustomYVelocity);

        // `$A9:BAD1` branches on the carry returned by the vertical helper. Horizontal wall
        // carry is intentionally discarded by the following LDA/ADC sequence.
        return CreateResult(
            before,
            samus,
            CustomXVelocity,
            CustomYVelocity,
            reachedWall,
            reachedVerticalBoundary,
            nativeCarry: reachedVerticalBoundary);
    }

    private static ushort CalculateYVelocity(
        ushort speed,
        SnesAngle angle)
    {
        // `$86:C27A` multiplies unsigned speed by the magnitude, selects product bits 8..23,
        // then reapplies the table entry's sign with 16-bit two's-complement negation.
        return EnemyTrigonometryTables.MultiplySignedSine(
            speed, angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
    }

    private static bool MoveVerticallyTowardCeilingOrFloor(
        SamusState samus,
        ushort velocity)
    {
        ushort candidate = AddNativeEightEightVelocity(
            samus.YPosition,
            samus.Kinematics.YSubposition,
            velocity,
            out ushort newSubposition);
        samus.Kinematics.YSubposition = newSubposition;

        // BMI/BPL are signed tests of the subtraction result, not unsigned C# comparisons.
        // This distinction is observable if a malformed/debug position wraps around zero.
        if (unchecked((short)(candidate - 0x0030)) < 0)
        {
            samus.YPosition = 0x0030;
            samus.Kinematics.YSubposition = 0;
            return true;
        }

        if (unchecked((short)(candidate - 0x00c0)) >= 0)
        {
            samus.YPosition = 0x00c0;
            samus.Kinematics.YSubposition = 0;
            return true;
        }

        samus.YPosition = candidate;
        return false;
    }

    private static bool MoveHorizontallyTowardWall(SamusState samus, ushort velocity)
    {
        ushort candidate = AddNativeEightEightVelocity(
            samus.XPosition,
            samus.Kinematics.XSubposition,
            velocity,
            out ushort newSubposition);
        samus.Kinematics.XSubposition = newSubposition;

        if (unchecked((short)(candidate - 0x00eb)) >= 0)
        {
            samus.XPosition = 0x00eb;
            samus.Kinematics.XSubposition = 0;
            return true;
        }

        samus.XPosition = candidate;
        return false;
    }

    private static ushort AddNativeEightEightVelocity(
        ushort wholePosition,
        ushort subposition,
        ushort velocity,
        out ushort newSubposition)
    {
        // SEP #$20 leaves the velocity's high byte in B while the low accumulator adds only
        // to WRAM subposition byte +1. Byte +0 is never read or written by `$A9:BBFD/BC3F`.
        int fractionalSum = (subposition >> 8) + (velocity & 0x00ff);
        byte newHighFraction = unchecked((byte)fractionalSum);
        newSubposition = unchecked((ushort)((newHighFraction << 8) | (subposition & 0x00ff)));

        // REP #$20, AND #$FF00, XBA, sign extension reconstruct the signed whole byte. The
        // carry left by the 8-bit ADC is still set and participates in the final ADC.
        int wholeDelta = unchecked((sbyte)(velocity >> 8)) + (fractionalSum > 0xff ? 1 : 0);
        return unchecked((ushort)(wholePosition + wholeDelta));
    }

    private static SamusCameraPoint Capture(SamusState samus) => new(
        samus.XPosition,
        samus.Kinematics.XSubposition,
        samus.YPosition,
        samus.Kinematics.YSubposition);

    private static MotherBrainForcedSamusMovementResult CreateResult(
        SamusCameraPoint before,
        SamusState samus,
        ushort xVelocity,
        ushort yVelocity,
        bool reachedWall,
        bool reachedVerticalBoundary,
        bool nativeCarry) => new(
            before,
            Capture(samus),
            xVelocity,
            yVelocity,
            reachedWall,
            reachedVerticalBoundary,
            nativeCarry,
            // Both helpers write the new current values into the native previous-position
            // words. A future camera integration must use this point instead of `before`.
            Capture(samus));
}

/// <summary>One-call witness for Mother Brain's forced Samus coordinate helpers.</summary>
public readonly record struct MotherBrainForcedSamusMovementResult(
    SamusCameraPoint Before,
    SamusCameraPoint After,
    ushort XVelocity,
    ushort YVelocity,
    bool ReachedWall,
    bool ReachedVerticalBoundary,
    bool NativeCarry,
    SamusCameraPoint CameraPreviousPosition);
