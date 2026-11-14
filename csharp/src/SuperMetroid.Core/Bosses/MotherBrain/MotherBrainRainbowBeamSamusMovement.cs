using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The four Samus-position helpers
/// used by Mother Brain's phase-two rainbow beam.
/// </summary>
/// <remarks>
/// This state belongs to Mother Brain, not to Samus's movement-type step. Pose type
/// 27, the locked pose type, deliberately returns without moving; the enemy AI calls
/// these helpers separately after it has locked Samus. Keeping that ownership explicit
/// prevents a future runtime from accidentally applying both ordinary and forced movement.
///
/// The helpers add a peculiar 1/256-pixel velocity to only the coarse half of Samus's
/// sub-pixel position, in 256ths of a pixel. The finer part is untouched, the addition's overflow
/// enters the whole-pixel addition, and both current and previous positions are overwritten
/// with the result. This implementation spells out those steps instead of using
/// floating point or the ordinary 1/65536-pixel movement helpers.
/// </remarks>
public sealed class MotherBrainRainbowBeamSamusMovement
{
    /// <summary>Samus's forced signed 1/256-pixel velocity.</summary>
    public EightEightVector<RoomPixels> CustomVelocity { get; private set; }

    /// <summary>
    /// The beam angle, in 256ths of a turn, stored by the aim step; zero points down and the
    /// positive direction is anti-clockwise, matching the shared trigonometry.
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
        bool reachedWall = MoveHorizontallyTowardWall(samus, cameraCheckpoint, SignedEightEight.FromPixels(16));

        if (!reachedWall)
        {
            // The sine multiply adds a quarter turn before indexing the sine table. Its multiplication returns
            // product divided by 256, a signed 1/256-pixel component for the supplied 1/256-pixel speed.
            MoveVerticallyTowardCeilingOrFloor(samus, cameraCheckpoint, CalculateYVelocity(speed: RainbowBeamPushSpeed, RainbowBeamAngle));

            // The wall step reports only whether Samus reached the wall; the vertical clamp
            // is reported separately and does not end the forced movement.
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

        // The test checks whether the target Y minus the current Y is negative. For the fight's normal
        // coordinate range this is exactly Samus's Y <= 124, but preserve the wrapped comparison
        // instead of introducing a plain integer comparison with different wraps.
        int difference = CoordinateAxis.Difference(124, samus.Position.Y.Whole);
        bool reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(
            samus, cameraCheckpoint, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(difference >= 0 ? 64 : -64));

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
        CustomVelocity = new(SignedEightEight.FromPixels(-1), SignedEightEight.Zero);
    }

    /// <summary>How much the leftward push slows on each update of Samus's fall from the beam.</summary>
    private static readonly SignedEightEight FallingPushSlowing = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(2);

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

        // The leftward speed slows by two units each update; as soon as it reaches zero or above it is
        // clamped to exact zero.
        CustomVelocity = CustomVelocity.WithX(SignedEightEight.Min(CustomVelocity.X + FallingPushSlowing, SignedEightEight.Zero));
        MoveHorizontallyTowardWall(samus, cameraCheckpoint, CustomVelocity.X);

        CustomVelocity += EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 24);
        bool reachedVerticalBoundary =
            MoveVerticallyTowardCeilingOrFloor(samus, cameraCheckpoint, CustomVelocity.Y);

        // The wait step ends when the vertical move reaches the ceiling or floor. Whether the
        // horizontal move met a wall is intentionally ignored.
        return CreateResult(
            before,
            reachedBoundary: reachedVerticalBoundary);
    }

    /// <summary>The speed the rainbow beam pushes Samus along its angle at: sixteen pixels an update.</summary>
    private static readonly SignedEightEight RainbowBeamPushSpeed = SignedEightEight.FromPixels(16);

    /// <summary>Projects a 1/256-pixel beam speed onto the vertical axis using the quarter-turn sine lookup.</summary>
    /// <param name="speed">The beam's speed, read unsigned.</param>
    /// <param name="angle">The live beam angle.</param>
    internal static SignedEightEight CalculateYVelocity(
        SignedEightEight speed,
        RotationAngle angle)
    {
        // The sine multiply multiplies unsigned speed by the magnitude, divides the product by 256,
        // then reapplies the table entry's sign.
        return Trigonometry.MultiplySignedSine(
            speed, (angle + RotationAngle.QuarterTurn).ToTrigonometryAngle());
    }

    /// <summary>Applies 1/256-pixel Y movement, copies the result into the previous position, and clamps to arena Y=48..192.</summary>
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

        // The bounds read the subtraction result as signed, not as unsigned C# comparisons do.
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

    /// <summary>Applies 1/256-pixel X movement, copies the result into the previous position, and clamps at the right wall X=235.</summary>
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

    /// <summary>Moves the position by the velocity in 256ths of a pixel, carrying into whole pixels, and keeps its finer precision unchanged.</summary>
    private static FixedCoordinate AddVelocityToHighSubpixel(FixedCoordinate position, SignedEightEight velocity) =>
        // The push moves only in 256ths of a pixel; precision finer than that is never read or changed.
        position.MovedBy(velocity);

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
