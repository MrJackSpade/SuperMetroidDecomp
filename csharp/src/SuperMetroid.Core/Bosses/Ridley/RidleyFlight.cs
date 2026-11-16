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
        FixedCoordinate proposedX = place.Position.X.MovedBy(flight.Velocity.X);
        FixedCoordinate proposedY = place.Position.Y.MovedBy(flight.Velocity.Y);
        flight.HitRoomBoundary = CoordinateAxis.Difference(proposedX.Whole, flight.MovementMinimum.X) < 0 ||
            CoordinateAxis.Difference(proposedX.Whole, flight.MovementMaximum.X) >= 0 ||
            CoordinateAxis.Difference(proposedY.Whole, flight.MovementMinimum.Y) < 0 ||
            CoordinateAxis.Difference(proposedY.Whole, flight.MovementMaximum.Y) >= 0;
        if (wallImpact == RidleyWallImpact.CeresWall && CoordinateAxis.Difference(proposedX.Whole, flight.MovementMinimum.X) < 0 &&
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
        FixedCoordinate moved = position.MovedBy(velocity);

        if (CoordinateAxis.Difference(moved.Whole, minimum) < 0)
            return new RidleyAxisMotion(moved.WithWhole(minimum), SignedEightEight.Zero);
        if (CoordinateAxis.Difference(moved.Whole, maximum) >= 0)
            return new RidleyAxisMotion(moved.WithWhole(maximum), SignedEightEight.Zero);
        return new RidleyAxisMotion(moved, velocity);
    }

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
        Extent16<RoomPixels> radius) =>
        // The hover check tests overlap with the actor's rectangle, including its boundary.
        // A center-only test delays flight and attack handoffs until Ridley gets too close.
        CoordinateAxis.Distance(place.Position.X.Whole, center.X) < CoordinateAxis.Wrap(place.Radius.X + radius.X + 1) &&
            CoordinateAxis.Distance(place.Position.Y.Whole, center.Y) < CoordinateAxis.Wrap(place.Radius.Y + radius.Y + 1);

    /// <summary>Restarts Ridley's body animation and frame timer at a selected segment.</summary>
    internal static void StartAnimation(RidleyFlightState flight, RoomEnemyPlace place, RidleyBodySegment segment) =>
        flight.BodyPlayback = EnemyAnimationPlayback.Start(place, RidleyAnimations.StartingWith(segment));

    /// <summary>Restarts Ridley's body animation and frame timer at <paramref name="animation"/> for the way he faces (#2175).</summary>
    internal static void StartAnimation(RidleyFlightState flight, RoomEnemyPlace place, RidleyFacedAnimation animation) =>
        StartAnimation(flight, place, animation.For(Ensure.NotNull(flight).FacingDirection));

    /// <summary>Animates Ridley's wings after his movement pass.</summary>
    internal static void TickWingAnimation(RidleyFlightState flight)
    {
        // Every acceleration clamps Ridley to five pixels an update, so his speed on each axis has a magnitude.
        SignedEightEight largestAxisVelocity = SignedEightEight.Max(flight.Velocity.X.Magnitude(), flight.Velocity.Y.Magnitude());
        if (largestAxisVelocity.IsZero)
        {
            flight.WingAnimationTimerDelta = 0;
        }
        else
        {
            // The wing beat quickens with each quarter pixel of speed up to seven quarters (#1935).
            int wingStep = Math.Min(RidleyWingDefinitions.AnimationTimerSteps.Length - 1, largestAxisVelocity.WholeQuarterPixels);
            int delta = RidleyWingDefinitions.AnimationTimerSteps[wingStep];
            if (!flight.Velocity.Y.IsNegative)
                delta >>= 1;
            flight.WingAnimationTimerDelta = delta;
        }

        flight.WingAnimationTimer -= flight.WingAnimationTimerDelta;
        if (flight.WingAnimationTimer >= 0)
            return;

        flight.WingAnimationTimer = 32;
        flight.WingBeatStep = (flight.WingBeatStep + 1) % RidleySupplementalVisualDefinitions.WingBeatSteps;
    }
}
