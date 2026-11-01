using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Literal translation of the four Samus-position helpers
/// used by Mother Brain's phase-two rainbow beam.
/// </summary>
/// <remarks>
/// This state belongs to Mother Brain, not to Samus's movement-type dispatcher. Pose type
/// <c>$1B</c> deliberately returns without moving; the enemy AI calls
/// these helpers separately after it has locked Samus. Keeping that ownership explicit
/// prevents a future runtime from accidentally applying both ordinary and forced movement.
///
/// The helpers use a peculiar 8.8 velocity against only byte <c>+1</c> of Samus's
/// 16-bit subposition. The low subposition byte is untouched, the 8-bit addition's carry
/// enters the whole-pixel addition, and both current and previous positions are overwritten
/// with the result. This implementation spells out those byte operations instead of using
/// floating point or the ordinary 16.16 movement helpers.
/// </remarks>
public sealed class MotherBrainRainbowBeamSamusMovement
{
    /// <summary>Samus's forced signed 8.8 velocity.</summary>
    public EightEightVector<RoomPixels> CustomVelocity { get; private set; }

    /// <summary>
    /// The low-byte beam angle stored by the aim step; zero points down and the
    /// positive direction is anti-clockwise, matching the shared trig routines.
    /// </summary>
    public RotationAngle RainbowBeamAngle { get; set; }

    /// <summary>
    /// Moves Samus toward the wall: right at 16 pixels per call and, until the
    /// hardcoded wall is reached, derive the vertical component from the live beam angle.
    /// </summary>
    /// <param name="samus">Samus, pinned by the beam.</param>
    /// <param name="cameraCheckpoint">The session's record of what the push hides from the camera.</param>
    public MotherBrainForcedSamusMovementResult MoveTowardWall(
        SamusState samus,
        RainbowBeamCameraCheckpoint cameraCheckpoint)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(cameraCheckpoint);

        FixedPosition<RoomPixels> before = Capture(samus);
        bool reachedWall = MoveHorizontallyTowardWall(samus, cameraCheckpoint, SignedEightEight.FromSignedRaw(4096));

        if (!reachedWall)
        {
            // The sine multiply adds a quarter turn before indexing the sine table. Its multiplication returns
            // product bits 8..23, i.e. a signed 8.8 component for the supplied 8.8 speed.
            MoveVerticallyTowardCeilingOrFloor(samus, cameraCheckpoint, CalculateYVelocity(speed: 4096, RainbowBeamAngle));

            // The wall step unconditionally clears carry after the vertical helper. The caller
            // can therefore observe only the wall result; keep the clamp witness separately
            // for debugging without pretending it controls the state transition.
        }

        return CreateResult(
            before,
            reachedBoundary: reachedWall);
    }

    /// <summary>
    /// Moves Samus toward the middle of the wall: down by a quarter pixel below/equal to Y 124, or
    /// up by a quarter pixel above it. Does not snap to the target and can oscillate.
    /// </summary>
    /// <param name="samus">Samus, pinned by the beam.</param>
    /// <param name="cameraCheckpoint">The session's record of what the push hides from the camera.</param>
    public static MotherBrainForcedSamusMovementResult MoveTowardMiddleOfWall(SamusState samus, RainbowBeamCameraCheckpoint cameraCheckpoint)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(cameraCheckpoint);

        FixedPosition<RoomPixels> before = Capture(samus);

        // `CPY SamusYPosition / BPL` tests signed(YTarget-currentY). For the fight's normal
        // coordinate range this is exactly currentY <= $7C, but preserve the 16-bit branch
        // semantics instead of introducing a host integer comparison with different wraps.
        int difference = CoordinateAxis.Difference(124, samus.Position.Y.Whole);
        bool reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(
            samus, cameraCheckpoint, SignedEightEight.FromSignedRaw(WordWrap.Signed(difference >= 0 ? 64 : -64)));

        return CreateResult(
            before,
            reachedBoundary: reachedVerticalBoundary);
    }

    /// <summary>
    /// Begins Samus's fall, executed when the rainbow beam has narrowed below its
    /// finishing threshold: initial left speed of one pixel, zero vertical speed.
    /// </summary>
    public void BeginFallingAfterRainbowBeam()
    {
        CustomVelocity = new(-256, 0);
    }

    /// <summary>
    /// Steps Samus's fall: ease horizontal speed toward zero by 2/256, add
    /// 24/256 vertical speed, then move through the two hardcoded arena clamps.
    /// </summary>
    /// <param name="samus">Samus, falling from the beam.</param>
    /// <param name="cameraCheckpoint">The session's record of what the push hides from the camera.</param>
    public MotherBrainForcedSamusMovementResult StepFallingAfterRainbowBeam(SamusState samus, RainbowBeamCameraCheckpoint cameraCheckpoint)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(cameraCheckpoint);

        FixedPosition<RoomPixels> before = Capture(samus);

        // ADC wraps to 16 bits and BMI examines the resulting sign bit. As soon as
        // the negative speed crosses into zero or above it is clamped to exact zero.
        int acceleratedX = WordWrap.Signed(CustomVelocity.X.SignedRawValue + 2);
        CustomVelocity = CustomVelocity.WithX(SignedEightEight.FromSignedRaw(CoordinateAxis.AsSigned(Math.Min(acceleratedX, 0))));
        MoveHorizontallyTowardWall(samus, cameraCheckpoint, CustomVelocity.X);

        CustomVelocity += new EightEightVector<RoomPixels>(0, 24);
        bool reachedVerticalBoundary =
            MoveVerticallyTowardCeilingOrFloor(samus, cameraCheckpoint, CustomVelocity.Y);

        // The wait step branches on the carry returned by the vertical helper. Horizontal wall
        // carry is intentionally discarded by the following LDA/ADC sequence.
        return CreateResult(
            before,
            reachedBoundary: reachedVerticalBoundary);
    }

    /// <summary>Projects an 8.8 beam speed onto the vertical axis using the quarter-turn sine lookup.</summary>
    /// <param name="speed">The unsigned 8.8 beam speed, 0 through 65535.</param>
    /// <param name="angle">The live beam angle.</param>
    internal static SignedEightEight CalculateYVelocity(
        int speed,
        RotationAngle angle)
    {
        // The sine multiply multiplies unsigned speed by the magnitude, selects product bits 8..23,
        // then reapplies the table entry's sign with 16-bit two's-complement negation.
        return Trigonometry.MultiplySignedSine(
            speed, angle.AddRaw(RotationAngle.QuarterTurn.RawValue).ToByteAngle());
    }

    /// <summary>Applies 8.8 Y movement, mirrors previous-position words, and clamps to arena Y=$30..$C0.</summary>
    private static bool MoveVerticallyTowardCeilingOrFloor(
        SamusState samus,
        RainbowBeamCameraCheckpoint cameraCheckpoint,
        SignedEightEight velocity)
    {
        FixedCoordinate moved = AddVelocityToHighSubpixel(
            new FixedCoordinate(samus.Position.Y.Whole, samus.Kinematics.Position.Y.Fraction),
            velocity);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(moved.Fraction);
        // The push hides its coarse sub-pixel movement from the camera, and every
        // exit records the new whole Y as where Samus was: the camera sees no whole-pixel movement.
        cameraCheckpoint.HideCoarseVerticalMovement(moved.Fraction);

        // BMI/BPL are signed tests of the subtraction result, not unsigned C# comparisons.
        // This distinction is observable if a malformed/debug position wraps around zero.
        bool aboveCeiling = CoordinateAxis.Difference(moved.Whole, 48) < 0;
        if (aboveCeiling || CoordinateAxis.Difference(moved.Whole, 192) >= 0)
        {
            samus.Position = samus.Position.WithWholeY(aboveCeiling ? Ceiling : Floor);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
            samus.WritePreviousYPosition(aboveCeiling ? Ceiling : Floor);
            cameraCheckpoint.HideVerticalMovement(0);
            return true;
        }

        samus.Position = samus.Position.WithWholeY(moved.Whole);
        samus.WritePreviousYPosition(moved.Whole);
        return false;
    }

    /// <summary>Applies 8.8 X movement, mirrors previous-position words, and clamps at the right wall X=$EB.</summary>
    private static bool MoveHorizontallyTowardWall(SamusState samus, RainbowBeamCameraCheckpoint cameraCheckpoint, SignedEightEight velocity)
    {
        FixedCoordinate moved = AddVelocityToHighSubpixel(
            new FixedCoordinate(samus.Position.X.Whole, samus.Kinematics.Position.X.Fraction),
            velocity);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(moved.Fraction);
        // Every store is hidden from the camera's horizontal record.
        cameraCheckpoint.HideCoarseHorizontalMovement(moved.Fraction);

        if (CoordinateAxis.Difference(moved.Whole, 235) >= 0)
        {
            samus.Position = samus.Position.WithWholeX(235);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0);
            samus.WritePreviousXPosition(235);
            cameraCheckpoint.HideHorizontalMovement(0);
            return true;
        }

        samus.Position = samus.Position.WithWholeX(moved.Whole);
        samus.WritePreviousXPosition(moved.Whole);
        return false;
    }

    /// <summary>Adds velocity to only the high subposition byte and carries into the whole pixel word, retaining the low byte.</summary>
    private static FixedCoordinate AddVelocityToHighSubpixel(FixedCoordinate position, SignedEightEight velocity) =>
        // The push adds only to the subposition's high byte; the low byte is never read or written.
        position.AddEightEightToHighFraction(velocity.RawValue);

    /// <summary>The arena's ceiling: the least whole Y the forced movement leaves Samus at.</summary>
    private const int Ceiling = 48;

    /// <summary>The arena's floor: the greatest whole Y the forced movement leaves Samus at.</summary>
    private const int Floor = 192;

    /// <summary>Records Samus's position before a forced movement helper mutates it.</summary>
    private static FixedPosition<RoomPixels> Capture(SamusState samus) => samus.Position;

    /// <summary>Packages the previous position with whether the active arena clamp reached its boundary.</summary>
    private static MotherBrainForcedSamusMovementResult CreateResult(
        FixedPosition<RoomPixels> before,
        bool reachedBoundary) => new(
            before,
            reachedBoundary);
}
