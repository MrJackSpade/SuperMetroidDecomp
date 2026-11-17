using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How Ridley's body flies in both encounters (#2229): his 1/256-pixel movement within his arena, his swoop's heading and
/// speed, the box tests his phases hand off on, his wing beat and starting his body's animations.
/// </summary>
[RoomLifetime]
internal sealed class RidleyFlight
{
    /// <summary>The room's earthquake, which a fast Ceres Ridley striking his chamber's wall starts.</summary>
    private readonly RoomEarthquake _earthquake;

    /// <summary>Builds the flight over the room's earthquake.</summary>
    internal RidleyFlight(RoomEarthquake earthquake)
    {
        _earthquake = earthquake;
    }

    /// <summary>The heading a swoop climbs toward, turned the swooping way: fifteen thirty-seconds of a turn.</summary>
    internal static readonly RotationAngle SwoopClimbTarget = RotationAngle.FromSteps(120);

    /// <summary>Applies Ridley's 1/256-pixel motion and arena clamps, invoking impact only at the strict left bound.</summary>
    internal void IntegrateMovement(
        RoomEnemyPlace place,
        RidleyFlightState flight,
        RidleyWallImpact wallImpact = RidleyWallImpact.None)
    {
        // The bounds clamp runs the impact only when proposed X is strictly below the
        // left bound, before zeroing X velocity. Vertical speed can meet its threshold
        // even when horizontal motion is slow; the right and vertical clamps do not call it.
        int proposedX = SignedPlace(place.Position.X.MovedBy(flight.Velocity.X));
        int proposedY = SignedPlace(place.Position.Y.MovedBy(flight.Velocity.Y));
        flight.HitRoomBoundary = proposedX < flight.MovementMinimum.X ||
            proposedX >= flight.MovementMaximum.X ||
            proposedY < flight.MovementMinimum.Y ||
            proposedY >= flight.MovementMaximum.Y;
        if (wallImpact == RidleyWallImpact.CeresWall && proposedX < flight.MovementMinimum.X &&
            SignedEightEight.Max(flight.Velocity.X.Magnitude(), flight.Velocity.Y.Magnitude()) >= RidleyWallImpactDefinitions.MinimumSpeed)
        {
            _earthquake.Start(RidleyWallImpactDefinitions.Earthquake, RidleyWallImpactDefinitions.Duration);
        }
        (FixedCoordinate x, SignedEightEight xVelocity) = IntegrateAxis(
            place.Position.X,
            flight.Velocity.X,
            flight.MovementMinimum.X,
            flight.MovementMaximum.X);
        flight.Velocity = flight.Velocity.WithX(xVelocity);
        place.Position = place.Position.WithX(x);
        (FixedCoordinate y, SignedEightEight yVelocity) = IntegrateAxis(
            place.Position.Y,
            flight.Velocity.Y,
            flight.MovementMinimum.Y,
            flight.MovementMaximum.Y);
        flight.Velocity = flight.Velocity.WithY(yVelocity);
        place.Position = place.Position.WithY(y);
    }

    /// <summary>Integrates one 1/256-pixel velocity into a position's fraction, in 256ths of a pixel, and clips to an arena bound.</summary>
    private static RidleyAxisMotion IntegrateAxis(
        FixedCoordinate position,
        SignedEightEight velocity,
        int minimum,
        int maximum)
    {
        // Ridley's velocities are signed 1/256-pixel values that the game adds, in 256ths of a pixel, to the
        // ordinary 1/65536-pixel position's fraction.
        // A bound above the room's top is stored as positions are, continuing from the far end of the coordinate range.
        FixedCoordinate moved = position.MovedBy(velocity);

        if (SignedPlace(moved) < minimum)
            return new RidleyAxisMotion(moved.WithWrappedWhole(minimum), SignedEightEight.Zero);
        if (SignedPlace(moved) >= maximum)
            return new RidleyAxisMotion(moved.WithWrappedWhole(maximum), SignedEightEight.Zero);
        return new RidleyAxisMotion(moved, velocity);
    }

    /// <summary>
    /// Where <paramref name="place"/>'s body lies from the room's origin: Ceres Ridley's retreat carries him above his
    /// chamber, where he lies at a negative height. Every comparison of a Ridley place against a line or bound reads this.
    /// </summary>
    internal static Offset16<RoomPixels> SignedPlace(RoomEnemyPlace place) => SignedPlace(place.Position.Whole);

    /// <summary>Where <paramref name="position"/> lies from the room's origin: one above or left of the room lies at a negative height or column.</summary>
    internal static Offset16<RoomPixels> SignedPlace(Position16<RoomPixels> position) =>
        new(CoordinateAxis.AsSigned(position.X), CoordinateAxis.AsSigned(position.Y));

    /// <summary>Where one axis of a moving place lies from the room's origin, negative above or left of the room.</summary>
    private static int SignedPlace(FixedCoordinate coordinate) => CoordinateAxis.AsSigned(coordinate.Whole);

    /// <summary>
    /// Moves Ridley's swoop speed toward <paramref name="targetSpeed"/>, turns his heading by <paramref name="turn"/>
    /// toward <paramref name="targetAngle"/>, stopping at it by whether their wrapped difference is negative, and derives
    /// his velocity from the two.
    /// </summary>
    internal static void UpdateSwoopVelocity(
        RidleyFlightState flight,
        RidleySwoopTurn turn,
        RotationAngle targetAngle,
        SignedEightEight targetSpeed)
    {
        SignedEightEight speed = flight.SwoopSpeed;
        if (speed < targetSpeed)
            speed = SignedEightEight.Min(targetSpeed, speed + RidleyFlightDefinitions.SwoopSpeedStep);
        else if (speed > targetSpeed)
            speed = SignedEightEight.Max(targetSpeed, speed - RidleyFlightDefinitions.SwoopSpeedStep);
        flight.SwoopSpeed = speed;

        RotationAngle angle = turn.Turn(flight.SwoopAngleAccumulator);
        // The comparison checks whether the difference is negative, not signed less-than. The heading wraps around the
        // turn, particularly onto the half-turn recovery target after a rightward swoop.
        bool negativeDifference = angle.TurnFrom(targetAngle) < 0;
        if (turn.Direction == RotationDirection.Clockwise ? negativeDifference : !negativeDifference)
            angle = targetAngle;
        flight.SwoopAngleAccumulator = angle;
        TrigonometryAngle phase = angle.ToTrigonometryAngle();
        flight.Velocity = flight.Velocity.WithX(Trigonometry.MultiplySignedSine(speed, phase));
        flight.Velocity = flight.Velocity.WithY(Trigonometry.MultiplySignedSine(speed, phase.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Tests an inclusive actor rectangle for hover, lunge or retrieval handoffs.</summary>
    internal static bool IsWithinRectangle(
        RoomEnemyPlace place,
        Position16<RoomPixels> center,
        Extent16<RoomPixels> radius)
    {
        // The hover check tests overlap with the actor's rectangle, including its boundary.
        // A center-only test delays flight and attack handoffs until Ridley gets too close.
        Offset16<RoomPixels> fromCenter = SignedPlace(place) - SignedPlace(center);
        return Math.Abs(fromCenter.X) < place.Radius.X + radius.X + 1 &&
            Math.Abs(fromCenter.Y) < place.Radius.Y + radius.Y + 1;
    }

    /// <summary>Restarts Ridley's body animation and frame timer at a selected segment.</summary>
    internal static void StartAnimation(RidleyFlightState flight, RoomEnemyPlace place, RidleyBodySegment segment) =>
        flight.BodyPlayback = EnemyAnimationPlayback.Start(place, RidleyAnimations.StartingWith(segment));

    /// <summary>Restarts Ridley's body animation and frame timer at <paramref name="animation"/> for the way he faces (#2175).</summary>
    internal static void StartAnimation(RidleyFlightState flight, RoomEnemyPlace place, RidleyFacedAnimation animation) =>
        StartAnimation(flight, place, animation.For(Ensure.NotNull(flight).FacingDirection));

    /// <summary>Beats Ridley's wings after his movement pass, at the pace his flight speed sets (#2329).</summary>
    internal static void TickWingAnimation(RidleyFlightState flight) => flight.Wings.Beat(WingPace(flight.Velocity));

    /// <summary>
    /// The countdown Ridley's wing beat takes off in an update he flies at <paramref name="velocity"/>: none while he is
    /// still, otherwise the pace of his faster axis's speed, halved unless he climbs.
    /// </summary>
    private static int WingPace(EightEightVector<RoomPixels> velocity)
    {
        // Every acceleration clamps Ridley to five pixels an update, so his speed on each axis has a magnitude.
        SignedEightEight largestAxisVelocity = SignedEightEight.Max(velocity.X.Magnitude(), velocity.Y.Magnitude());
        if (largestAxisVelocity.IsZero)
            return 0;
        // The wing beat quickens with each quarter pixel of speed up to seven quarters (#1935).
        int pace = RidleyWingDefinitions.BeatPaces[Math.Min(RidleyWingDefinitions.BeatPaces.Length - 1, largestAxisVelocity.WholeQuarterPixels)];
        // A climb beats the wings at the full pace; level and descending flight beat them at half of it.
        return velocity.Y.IsNegative ? pace : pace / 2;
    }
}
