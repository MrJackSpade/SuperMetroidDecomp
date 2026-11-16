using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The four ways Mother Brain's phase-two rainbow beam moves Samus: toward the wall, toward the middle of the wall, the
/// start of her fall and the fall itself.
/// </summary>
/// <remarks>
/// This movement belongs to Mother Brain, not to Samus's own movement: while the beam locks her, her own movement does
/// not move her, and Mother Brain's attack moves her instead.
///
/// Each move adds its velocity to Samus's position in 256ths of a pixel only: the finer part of her position is
/// untouched, a fraction past a whole pixel carries into the whole pixels, and her current and previous positions both
/// take the result.
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
    /// Moves Samus toward the wall: right at 16 pixels per update and, until she reaches the arena's wall, up or down
    /// along the beam's current angle.
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
        bool reachedWall = MoveHorizontallyTowardWall(samus, cameraCheckpoint, MotherBrainRainbowBeamPushDefinitions.PushSpeed);

        if (!reachedWall)
        {
            // The vertical part is the push's speed times the cosine of the beam's angle, in 256ths of a pixel.
            MoveVerticallyTowardCeilingOrFloor(samus, cameraCheckpoint, CalculateYVelocity(speed: MotherBrainRainbowBeamPushDefinitions.PushSpeed, RainbowBeamAngle));

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

        bool reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(
            samus, cameraCheckpoint,
            samus.Position.Y.Whole <= MotherBrainRainbowBeamPushDefinitions.MiddleOfWallY
                ? MotherBrainRainbowBeamPushDefinitions.MiddleOfWallSpeed
                : -MotherBrainRainbowBeamPushDefinitions.MiddleOfWallSpeed);

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
        CustomVelocity = new(MotherBrainRainbowBeamPushDefinitions.FallStartSpeed, SignedEightEight.Zero);
    }

    /// <summary>
    /// Steps Samus's fall: ease horizontal speed toward zero by 2/256, add 24/256 vertical speed, then move her within
    /// the arena's bounds.
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
        CustomVelocity = CustomVelocity.WithX(SignedEightEight.Min(CustomVelocity.X + MotherBrainRainbowBeamPushDefinitions.FallingPushSlowing, SignedEightEight.Zero));
        MoveHorizontallyTowardWall(samus, cameraCheckpoint, CustomVelocity.X);

        CustomVelocity += MotherBrainRainbowBeamPushDefinitions.FallGravity;
        bool reachedVerticalBoundary =
            MoveVerticallyTowardCeilingOrFloor(samus, cameraCheckpoint, CustomVelocity.Y);

        // The wait step ends when the vertical move reaches the ceiling or floor. Whether the
        // horizontal move met a wall is intentionally ignored.
        return CreateResult(
            before,
            reachedBoundary: reachedVerticalBoundary);
    }

    /// <summary>Projects a 1/256-pixel beam speed onto the vertical axis using the quarter-turn sine lookup.</summary>
    /// <param name="speed">The beam's speed, whose size the sine scales.</param>
    /// <param name="angle">The live beam angle.</param>
    internal static SignedEightEight CalculateYVelocity(
        SignedEightEight speed,
        RotationAngle angle)
    {
        // The speed's size times the sine's size in 256ths, in the sine's direction.
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

        // The bounds compare Samus's height with the ceiling and the floor around the room's wrap, so a height past
        // zero lies above the ceiling.
        bool aboveCeiling = CoordinateAxis.Difference(moved.Whole, MotherBrainRainbowBeamPushDefinitions.Ceiling) < 0;
        if (aboveCeiling || CoordinateAxis.Difference(moved.Whole, MotherBrainRainbowBeamPushDefinitions.Floor) >= 0)
        {
            int boundary = aboveCeiling ? MotherBrainRainbowBeamPushDefinitions.Ceiling : MotherBrainRainbowBeamPushDefinitions.Floor;
            samus.Position = samus.Position.WithWholeY(boundary);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
            samus.WritePreviousYPosition(boundary);
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

        if (CoordinateAxis.Difference(moved.Whole, MotherBrainRainbowBeamPushDefinitions.WallX) >= 0)
        {
            samus.Position = samus.Position.WithWholeX(MotherBrainRainbowBeamPushDefinitions.WallX);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0);
            samus.WritePreviousXPosition(MotherBrainRainbowBeamPushDefinitions.WallX);
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

    /// <summary>Records Samus's position before a forced move changes it.</summary>
    private static FixedPosition<RoomPixels> Capture(SamusState samus) => samus.Position;

    /// <summary>Packages the previous position with whether the active arena clamp reached its boundary.</summary>
    private static MotherBrainForcedSamusMovementResult CreateResult(
        FixedPosition<RoomPixels> before,
        bool reachedBoundary) => new(
            before,
            reachedBoundary);
}
