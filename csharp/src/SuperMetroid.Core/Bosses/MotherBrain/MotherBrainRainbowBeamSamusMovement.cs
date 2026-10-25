using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

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
    /// <summary>Mother Brain body extra words <c>$0FB2/$0FB4</c>: Samus's forced signed 8.8 velocity.</summary>
    public EightEightVector<RoomPixels> CustomVelocity { get; private set; }

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
        SamusState samus)
    {
        Ensure.NotNull(samus);

        FixedPosition<RoomPixels> before = Capture(samus);
        bool reachedWall = MoveHorizontallyTowardWall(samus, velocity: 4096);
        ushort yVelocity;

        if (!reachedWall)
        {
            // `$86:C272` adds $40 before indexing the sine table. Its multiplication returns
            // product bits 8..23, i.e. a signed 8.8 component for the supplied 8.8 speed.
            yVelocity = CalculateYVelocity(speed: 4096, RainbowBeamAngle);
            MoveVerticallyTowardCeilingOrFloor(samus, yVelocity);

            // `$A9:BBCD` unconditionally clears carry after the vertical helper. The caller
            // can therefore observe only the wall result; keep the clamp witness separately
            // for debugging without pretending it controls the native state transition.
        }

        return CreateResult(
            before,
            reachedBoundary: reachedWall);
    }

    /// <summary>
    /// Ports <c>$A9:BBCF</c>: move down by <c>$00.40</c> below/equal to Y <c>$7C</c>, or
    /// up by <c>-$00.40</c> above it. Native does not snap to the target and can oscillate.
    /// </summary>
    public static MotherBrainForcedSamusMovementResult MoveTowardMiddleOfWall(SamusState samus)
    {
        Ensure.NotNull(samus);

        FixedPosition<RoomPixels> before = Capture(samus);

        // `CPY SamusYPosition / BPL` tests signed(YTarget-currentY). For the fight's normal
        // coordinate range this is exactly currentY <= $7C, but preserve the 16-bit branch
        // semantics instead of introducing a host integer comparison with different wraps.
        ushort difference = unchecked((ushort)(124 - samus.Position.Y.Whole));
        ushort yVelocity = unchecked((short)difference >= 0 ? (ushort)64 : (ushort)-64);
        bool reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(samus, yVelocity);

        return CreateResult(
            before,
            reachedBoundary: reachedVerticalBoundary);
    }

    /// <summary>
    /// Ports <c>$A9:BA82-$BA88</c>, executed when the rainbow beam has narrowed below its
    /// finishing threshold: initial left speed <c>-$01.00</c>, zero vertical speed.
    /// </summary>
    public void BeginFallingAfterRainbowBeam()
    {
        CustomVelocity = new(-256, 0);
    }

    /// <summary>
    /// Ports <c>$A9:BBE1</c>: ease horizontal speed toward zero by <c>$00.02</c>, add
    /// <c>$00.18</c> vertical speed, then move through the two hardcoded arena clamps.
    /// </summary>
    public MotherBrainForcedSamusMovementResult StepFallingAfterRainbowBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        FixedPosition<RoomPixels> before = Capture(samus);

        // Native ADC wraps to 16 bits and BMI examines the resulting sign bit. As soon as
        // the negative speed crosses into $0000..$7FFF it is clamped to exact zero.
        short acceleratedX = unchecked((short)(CustomVelocity.X.SignedRawValue + 2));
        CustomVelocity = CustomVelocity.WithX(SnesSignedEightEight.FromSignedRaw(acceleratedX < 0 ? acceleratedX : (short)0));
        MoveHorizontallyTowardWall(samus, unchecked((ushort)CustomVelocity.X.SignedRawValue));

        CustomVelocity += new EightEightVector<RoomPixels>(0, 24);
        bool reachedVerticalBoundary =
            MoveVerticallyTowardCeilingOrFloor(samus, unchecked((ushort)CustomVelocity.Y.SignedRawValue));

        // `$A9:BAD1` branches on the carry returned by the vertical helper. Horizontal wall
        // carry is intentionally discarded by the following LDA/ADC sequence.
        return CreateResult(
            before,
            reachedBoundary: reachedVerticalBoundary);
    }

    /// <summary>Projects an 8.8 beam speed onto the vertical axis using the native quarter-turn sine lookup.</summary>
    internal static ushort CalculateYVelocity(
        ushort speed,
        SnesAngle angle)
    {
        // `$86:C27A` multiplies unsigned speed by the magnitude, selects product bits 8..23,
        // then reapplies the table entry's sign with 16-bit two's-complement negation.
        return EnemyTrigonometryTables.MultiplySignedSine(
            speed, angle.AddRaw(SnesAngle.QuarterTurn.RawValue).ToByteAngle());
    }

    /// <summary>Applies native 8.8 Y movement, mirrors previous-position words, and clamps to arena Y=$30..$C0.</summary>
    private static bool MoveVerticallyTowardCeilingOrFloor(
        SamusState samus,
        ushort velocity)
    {
        ushort candidate = AddVelocityToHighSubpixel(
            samus.Position.Y.Whole,
            samus.Kinematics.Position.Y.Fraction,
            velocity,
            out ushort newSubposition);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(newSubposition);
        // `$A9:BC06` stores the same fraction byte into SamusPreviousYSubPosition+1, and every
        // exit stores the new Y into SamusPreviousYPosition: the camera sees no movement.
        samus.WritePreviousYSubposition(SnesLittleEndian.Word(0, byte.MaxValue), newSubposition);

        // BMI/BPL are signed tests of the subtraction result, not unsigned C# comparisons.
        // This distinction is observable if a malformed/debug position wraps around zero.
        ushort? boundary = unchecked((short)(candidate - 48)) < 0 ? 48
            : unchecked((short)(candidate - 192)) >= 0 ? 192
            : null;
        if (boundary is ushort clamped)
        {
            samus.Position = samus.Position.WithWholeY(clamped);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
            samus.WritePreviousYPosition(clamped);
            samus.WritePreviousYSubposition(ushort.MaxValue, 0);
            return true;
        }

        samus.Position = samus.Position.WithWholeY(candidate);
        samus.WritePreviousYPosition(candidate);
        return false;
    }

    /// <summary>Applies native 8.8 X movement, mirrors previous-position words, and clamps at the right wall X=$EB.</summary>
    private static bool MoveHorizontallyTowardWall(SamusState samus, ushort velocity)
    {
        ushort candidate = AddVelocityToHighSubpixel(
            samus.Position.X.Whole,
            samus.Kinematics.Position.X.Fraction,
            velocity,
            out ushort newSubposition);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(newSubposition);
        // `$A9:BC48/$BC61/$BC6B/$BC71` mirror every store into the previous-X words.
        samus.WritePreviousXSubposition(SnesLittleEndian.Word(0, byte.MaxValue), newSubposition);

        if (unchecked((short)(candidate - 235)) >= 0)
        {
            samus.Position = samus.Position.WithWholeX(235);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0);
            samus.WritePreviousXPosition(235);
            samus.WritePreviousXSubposition(ushort.MaxValue, 0);
            return true;
        }

        samus.Position = samus.Position.WithWholeX(candidate);
        samus.WritePreviousXPosition(candidate);
        return false;
    }

    /// <summary>Adds velocity to only the high subposition byte and carries into the whole pixel word, retaining the low byte.</summary>
    private static ushort AddVelocityToHighSubpixel(
        ushort wholePosition,
        ushort subposition,
        ushort velocity,
        out ushort newSubposition)
    {
        // `$A9:BBFD/BC3F` add only to WRAM subposition byte +1; byte +0 is never read or written.
        SnesFixedPosition moved = new SnesFixedPosition(wholePosition, subposition).AddEightEightToHighFraction(velocity);
        newSubposition = moved.Fraction;
        return moved.Whole;
    }

    /// <summary>Records Samus's position before a forced movement helper mutates it.</summary>
    private static FixedPosition<RoomPixels> Capture(SamusState samus) => samus.Position;

    /// <summary>Packages the previous position with whether the active arena clamp reached its boundary.</summary>
    private static MotherBrainForcedSamusMovementResult CreateResult(
        FixedPosition<RoomPixels> before,
        bool reachedBoundary) => new(
            before,
            reachedBoundary);
}
