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
/// Each move pushes Samus by its speed: a fraction past a whole pixel carries into the whole pixels, and her current
/// and previous positions both take the result.
/// </remarks>
public sealed class MotherBrainRainbowBeamSamusMovement
{
    /// <summary>The velocity the beam forces on Samus.</summary>
    private PixelVelocity<RoomPixels> CustomVelocity { get; set; }

    /// <summary>
    /// The beam angle the aim step sets; zero points down and the
    /// positive direction is anti-clockwise, matching the shared trigonometry.
    /// </summary>
    public RotationAngle RainbowBeamAngle { get; set; }

    /// <summary>
    /// Moves Samus toward the wall: right at 16 pixels per update and, until she reaches the arena's wall, up or down
    /// along the beam's current angle.
    /// </summary>
    /// <param name="samus">Samus, pinned by the beam.</param>
    public MotherBrainForcedSamusMovementResult MoveTowardWall(
        SamusState samus)
    {
        Ensure.NotNull(samus);

        FixedPosition<RoomPixels> before = Capture(samus);
        bool reachedWall = MoveHorizontallyTowardWall(samus, MotherBrainRainbowBeamPushDefinitions.PushSpeed);

        if (!reachedWall)
        {
            // The vertical part is the push's speed along the beam's angle.
            MoveVerticallyTowardCeilingOrFloor(samus, CalculateYVelocity(speed: MotherBrainRainbowBeamPushDefinitions.PushSpeed, RainbowBeamAngle));

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
    public static MotherBrainForcedSamusMovementResult MoveTowardMiddleOfWall(SamusState samus)
    {
        Ensure.NotNull(samus);

        FixedPosition<RoomPixels> before = Capture(samus);

        bool reachedVerticalBoundary = MoveVerticallyTowardCeilingOrFloor(
            samus,
            MotherBrainRainbowBeamPushDefinitions.MiddleOfWallSpeed.Toward(
                samus.Position.Y.Whole <= MotherBrainRainbowBeamPushDefinitions.MiddleOfWallY ? VerticalDirection.Down : VerticalDirection.Up));

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
    /// Steps Samus's fall: her horizontal speed eases toward rest and her fall speeds up, as the fall's definitions
    /// give, and she moves within the arena's bounds.
    /// </summary>
    /// <param name="samus">Samus, falling from the beam.</param>
    public MotherBrainForcedSamusMovementResult StepFallingAfterRainbowBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        FixedPosition<RoomPixels> before = Capture(samus);

        // The leftward speed slows by two units each update; as soon as it reaches zero or above it is
        // clamped to exact zero.
        CustomVelocity = CustomVelocity.WithX(SignedEightEight.Min(CustomVelocity.X + MotherBrainRainbowBeamPushDefinitions.FallingPushSlowing, SignedEightEight.Zero));
        MoveHorizontallyTowardWall(samus, CustomVelocity.X);

        CustomVelocity += MotherBrainRainbowBeamPushDefinitions.FallGravity;
        bool reachedVerticalBoundary =
            MoveVerticallyTowardCeilingOrFloor(samus, CustomVelocity.Y);

        // The wait step ends when the vertical move reaches the ceiling or floor. Whether the
        // horizontal move met a wall is intentionally ignored.
        return CreateResult(
            before,
            reachedBoundary: reachedVerticalBoundary);
    }

    /// <summary>The beam's speed along its angle, vertically.</summary>
    /// <param name="speed">The beam's speed, whose size the sine scales.</param>
    /// <param name="angle">The live beam angle.</param>
    private static SignedEightEight CalculateYVelocity(
        SignedEightEight speed,
        RotationAngle angle)
    {
        // The vertical speed is the push's speed along the beam's angle.
        return SpeedAlongAngle.Along(
            speed, (angle + RotationAngle.QuarterTurn).ToTrigonometryAngle());
    }

    /// <summary>Applies the vertical movement, copies the result into the previous position, and clamps to arena Y=48..192.</summary>
    private static bool MoveVerticallyTowardCeilingOrFloor(
        SamusState samus,
        SignedEightEight velocity)
    {
        FixedCoordinate moved = MoveByPushVelocity(samus.Position.Y, velocity);
        // The push hides the coarse part of its movement within a pixel from the camera, and every
        // exit records the new whole Y as where Samus was: the camera sees no whole-pixel movement.
        samus.PreviousPositionCorrections.RainbowPush.HideCoarseVerticalMovement(moved.FractionOfPixel);

        // The push moves her a pixel or two an update between the ceiling and the floor, so she meets either bound long
        // before the room's top.
        bool aboveCeiling = moved.Whole < MotherBrainRainbowBeamPushDefinitions.Ceiling;
        if (aboveCeiling || moved.Whole >= MotherBrainRainbowBeamPushDefinitions.Floor)
        {
            int boundary = aboveCeiling ? MotherBrainRainbowBeamPushDefinitions.Ceiling : MotherBrainRainbowBeamPushDefinitions.Floor;
            samus.Position = samus.Position.WithY(FixedCoordinate.FromWhole(boundary));
            samus.WritePreviousYPosition(boundary);
            samus.PreviousPositionCorrections.RainbowPush.HideVerticalMovement(PixelFraction.None);
            return true;
        }

        samus.Position = samus.Position.WithY(moved);
        samus.WritePreviousYPosition(moved.Whole);
        return false;
    }

    /// <summary>Applies the sideways movement, copies the result into the previous position, and clamps at the right wall X=235.</summary>
    private static bool MoveHorizontallyTowardWall(SamusState samus, SignedEightEight velocity)
    {
        FixedCoordinate moved = MoveByPushVelocity(samus.Position.X, velocity);
        // The push's movement is hidden from the camera.
        samus.PreviousPositionCorrections.RainbowPush.HideCoarseHorizontalMovement(moved.FractionOfPixel);

        if (moved.Whole >= MotherBrainRainbowBeamPushDefinitions.WallX)
        {
            samus.Position = samus.Position.WithX(FixedCoordinate.FromWhole(MotherBrainRainbowBeamPushDefinitions.WallX));
            samus.WritePreviousXPosition(MotherBrainRainbowBeamPushDefinitions.WallX);
            samus.PreviousPositionCorrections.RainbowPush.HideHorizontalMovement(PixelFraction.None);
            return true;
        }

        samus.Position = samus.Position.WithX(moved);
        samus.WritePreviousXPosition(moved.Whole);
        return false;
    }

    /// <summary>Moves the position by the velocity.</summary>
    private static FixedCoordinate MoveByPushVelocity(FixedCoordinate position, SignedEightEight velocity) =>
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
