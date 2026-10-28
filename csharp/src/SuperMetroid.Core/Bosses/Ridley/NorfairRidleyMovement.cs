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

    /// <summary>Preserves ADC/SBC carry across reversal and normal acceleration steps on one axis.</summary>
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
        bool carry = WordCarry.AfterCompare(checked((ushort)position), checked((ushort)target));
        int velocity = axisVelocity.RawValue;

        // $A6:D559..D5A5 and D5CF..D61B retain carry/borrow across the chained
        // ADC/SBC instructions. Crossing zero can add one more acceleration unit;
        // combining these operations into integer arithmetic changes the trajectory.
        if (distance > 0)
        {
            if (unchecked((short)velocity) >= 0)
            {
                carry = true;
                Subtract(reversalBoost);
                carry = true;
                Subtract(RidleyInertiaDefinitions.ReversalAcceleration);
                Subtract(step);
            }
            Subtract(step);
            if (unchecked((short)(velocity - RidleyInertiaDefinitions.MinimumVelocity.RawValue)) < 0) velocity = RidleyInertiaDefinitions.MinimumVelocity.RawValue;
        }
        else
        {
            if (unchecked((short)velocity) < 0)
            {
                carry = false;
                Add(reversalBoost);
                carry = false;
                Add(RidleyInertiaDefinitions.ReversalAcceleration);
                Add(step);
            }
            Add(step);
            if (unchecked((short)(velocity - RidleyInertiaDefinitions.MaximumVelocity.RawValue)) >= 0) velocity = RidleyInertiaDefinitions.MaximumVelocity.RawValue;
        }
        return new SignedEightEight(checked((ushort)velocity));

        void Add(int operand) => velocity = WordCarry.AddWithCarry(checked((ushort)velocity), checked((ushort)operand), ref carry);
        void Subtract(int operand) => velocity = WordCarry.SubtractWithBorrow(checked((ushort)velocity), checked((ushort)operand), ref carry);
    }
}
