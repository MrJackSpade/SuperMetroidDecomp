using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How Norfair Ridley accelerates and moves toward a target, and the function timer its steps count down.</summary>
[RoomLifetime]
internal sealed class NorfairRidleyMovement
{

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal NorfairRidleyMovement()
    {
    }

    /// <summary>Counts the function timer down and reports whether it ran past zero.</summary>
    internal static bool TickRidleyFunctionTimer(RidleyEnemyState state) => --state.FunctionTimer < 0;

    /// <summary>Accelerates both 8.8 velocity axes toward a target using inertia divisors.</summary>
    internal static void MoveNorfairRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        Position16<RoomPixels> target,
        int divisor,
        int reversalBoost = 0)
    {
        divisor = RidleyInertia.Divisor(divisor);

        state.Velocity = new EightEightVector<RoomPixels>(
            AccelerateNorfairRidleyAxis(
                state.Velocity.X,
                slot.Position.X.Whole, target.X,
                divisor,
                reversalBoost),
            AccelerateNorfairRidleyAxis(
                state.Velocity.Y,
                slot.Position.Y.Whole, target.Y,
                divisor,
                reversalBoost));
    }

    /// <summary>
    /// Accelerates one axis toward the target: while reversing, the reversal boost and then the reversal acceleration
    /// each start a fresh run of steps, the second continuing into two distance steps; otherwise one distance step.
    /// Within a run a step that crosses zero makes the next one a unit larger (<see cref="SeekingVelocityRun"/>). The
    /// velocity stays within a few thousand units of rest (each call clamps it to five pixels and a step is at most the
    /// arena's height), so the clamps compare signed velocities.
    /// </summary>
    /// <param name="axisVelocity">The axis velocity.</param>
    /// <param name="position">Ridley's whole coordinate on the axis, as his position holds it.</param>
    /// <param name="target">The target's whole coordinate on the axis, as its position holds it.</param>
    /// <param name="divisor">The inertia divisor, at least one.</param>
    /// <param name="reversalBoost">The extra speed a reversal subtracts or adds first, never negative.</param>
    private static SignedEightEight AccelerateNorfairRidleyAxis(
        SignedEightEight axisVelocity,
        int position,
        int target,
        int divisor,
        int reversalBoost)
    {
        int distance = CoordinateAxis.Difference(position, target);
        if (distance == 0) return axisVelocity;
        int step = Math.Max(1, Math.Abs(distance) / divisor);
        var run = new SeekingVelocityRun(axisVelocity, position, target);

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
