using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How Norfair Ridley accelerates and moves toward a target, and the function timer its routines count down.</summary>
[RoomLifetime]
internal sealed class NorfairRidleyMovement
{

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal NorfairRidleyMovement()
    {
    }

    /// <summary>Decrements a native word timer and reports signed underflow.</summary>
    internal static bool TickRidleyFunctionTimer(RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        return state.FunctionTimer.IsNegative;
    }

    /// <summary>Accelerates both 8.8 velocity axes toward a target using native inertia divisors.</summary>
    internal static void MoveNorfairRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        Position16<RoomPixels> target,
        int divisorIndex,
        int reversalBoost = 0)
    {
        int divisor = RidleyInertia.Divisor(divisorIndex);

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
    /// Within a run a step that crosses zero makes the next one a unit larger (<see cref="SeekingVelocityRun"/>).
    /// </summary>
    /// <param name="axisVelocity">The axis velocity.</param>
    /// <param name="position">Ridley's whole coordinate on the axis, 0 through 65535.</param>
    /// <param name="target">The target's whole coordinate on the axis, 0 through 65535.</param>
    /// <param name="divisor">The inertia divisor, at least one.</param>
    /// <param name="reversalBoost">The extra speed a reversal subtracts or adds first, 0 through 65535.</param>
    private static SignedEightEight AccelerateNorfairRidleyAxis(
        SignedEightEight axisVelocity,
        int position,
        int target,
        int divisor,
        int reversalBoost)
    {
        int distance = unchecked((short)(position - target));
        if (distance == 0) return axisVelocity;
        int step = Math.Max(1, Math.Abs(distance) / divisor);
        var run = new SeekingVelocityRun(axisVelocity, position, target);

        if (distance > 0)
        {
            if (axisVelocity.SignedRawValue >= 0)
            {
                run.StartFresh();
                run.Lower(reversalBoost);
                run.StartFresh();
                run.Lower(RidleyInertiaDefinitions.ReversalAcceleration);
                run.Lower(step);
            }
            run.Lower(step);
            SignedEightEight lowered = run.Velocity;
            return unchecked((short)(lowered.RawValue - RidleyInertiaDefinitions.MinimumVelocity.RawValue)) < 0
                ? RidleyInertiaDefinitions.MinimumVelocity
                : lowered;
        }
        if (axisVelocity.SignedRawValue < 0)
        {
            run.StartFresh();
            run.Raise(reversalBoost);
            run.StartFresh();
            run.Raise(RidleyInertiaDefinitions.ReversalAcceleration);
            run.Raise(step);
        }
        run.Raise(step);
        SignedEightEight raised = run.Velocity;
        return unchecked((short)(raised.RawValue - RidleyInertiaDefinitions.MaximumVelocity.RawValue)) >= 0
            ? RidleyInertiaDefinitions.MaximumVelocity
            : raised;
    }
}
