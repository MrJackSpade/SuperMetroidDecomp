using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How Norfair Ridley accelerates and moves toward a target.</summary>
internal static class NorfairRidleyMovement
{
    /// <summary>Accelerates both axes of his velocity toward a target using inertia divisors.</summary>
    internal static void MoveNorfairRidleyToward(
        RoomEnemyPlace place,
        RidleyFlightState flight,
        Position16<RoomPixels> target,
        int divisor,
        int reversalBoost = 0)
    {
        divisor = RidleyInertia.Divisor(divisor);

        Position16<RoomPixels> at = place.Position.Whole;
        Offset16<RoomPixels> fromTarget = at - target;
        flight.Velocity = new PixelVelocity<RoomPixels>(
            AccelerateNorfairRidleyAxis(
                new SeekingVelocityRun(flight.Velocity.X, at.X, target.X),
                fromTarget.X,
                divisor,
                reversalBoost),
            AccelerateNorfairRidleyAxis(
                new SeekingVelocityRun(flight.Velocity.Y, at.Y, target.Y),
                fromTarget.Y,
                divisor,
                reversalBoost));
    }

    /// <summary>
    /// Accelerates one axis toward the target: while reversing, the reversal boost and then the reversal acceleration
    /// each start a fresh run of steps, the second continuing into two distance steps; otherwise one distance step.
    /// Within a run a step that crosses zero makes the next one a unit larger (<see cref="SeekingVelocityRun"/>). The
    /// velocity stays within a few thousand units of rest (each update clamps it to five pixels and a step is at most the
    /// arena's height), so the clamps compare signed velocities.
    /// </summary>
    /// <param name="run">The run of steps from the axis velocity toward the target from Ridley's place on the axis.</param>
    /// <param name="distance">Ridley's displacement from the target on the axis.</param>
    /// <param name="divisor">The inertia divisor, at least one.</param>
    /// <param name="reversalBoost">The extra speed a reversal subtracts or adds first, never negative.</param>
    private static SignedEightEight AccelerateNorfairRidleyAxis(
        SeekingVelocityRun run,
        int distance,
        int divisor,
        int reversalBoost)
    {
        SignedEightEight axisVelocity = run.Velocity;
        if (distance == 0) return axisVelocity;
        int step = Math.Max(1, Math.Abs(distance) / divisor);

        if (distance > 0)
        {
            if (!axisVelocity.IsNegative)
            {
                run.StartFresh();
                run.Lower(reversalBoost);
                run.StartFresh();
                run.Lower(RidleyInertiaDefinitions.ReversalAcceleration);
                run.Lower(step);
            }
            run.Lower(step);
            SignedEightEight lowered = run.Velocity;
            return lowered < RidleyInertiaDefinitions.MinimumVelocity
                ? RidleyInertiaDefinitions.MinimumVelocity
                : lowered;
        }
        if (axisVelocity.IsNegative)
        {
            run.StartFresh();
            run.Raise(reversalBoost);
            run.StartFresh();
            run.Raise(RidleyInertiaDefinitions.ReversalAcceleration);
            run.Raise(step);
        }
        run.Raise(step);
        SignedEightEight raised = run.Velocity;
        return raised >= RidleyInertiaDefinitions.MaximumVelocity
            ? RidleyInertiaDefinitions.MaximumVelocity
            : raised;
    }
}
