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
        ushort reversalBoost = 0)
    {
        ushort divisor = RidleyInertiaDefinitions.Divisor(divisorIndex);

        state.Velocity = new EightEightVector<RoomPixels>(
            new SnesSignedEightEight(AccelerateNorfairRidleyAxis(
                state.Velocity.X.RawValue,
                slot.Position.X.Whole, target.X,
                divisor,
                reversalBoost)),
            new SnesSignedEightEight(AccelerateNorfairRidleyAxis(
                state.Velocity.Y.RawValue,
                slot.Position.Y.Whole, target.Y,
                divisor,
                reversalBoost)));
    }

    /// <summary>Preserves ADC/SBC carry across reversal and normal acceleration steps on one axis.</summary>
    private static ushort AccelerateNorfairRidleyAxis(
        ushort velocityWord,
        ushort position,
        ushort target,
        ushort divisor,
        ushort reversalBoost)
    {
        short distance = unchecked((short)(position - target));
        if (distance == 0) return velocityWord;
        ushort step = (ushort)Math.Max(1, Math.Abs((int)distance) / divisor);
        bool carry = SnesCarry.AfterCompare(position, target);
        ushort velocity = velocityWord;

        // $A6:D559..D5A5 and D5CF..D61B retain carry/borrow across the chained
        // ADC/SBC instructions. Crossing zero can add one more acceleration unit;
        // combining these operations into integer arithmetic changes the trajectory.
        if (distance > 0)
        {
            if ((short)velocity >= 0)
            {
                carry = true;
                Subtract(reversalBoost);
                carry = true;
                Subtract(RidleyInertiaDefinitions.ReversalAcceleration);
                Subtract(step);
            }
            Subtract(step);
            if (unchecked((short)(velocity - RidleyInertiaDefinitions.MinimumVelocity)) < 0) velocity = RidleyInertiaDefinitions.MinimumVelocity;
        }
        else
        {
            if ((short)velocity < 0)
            {
                carry = false;
                Add(reversalBoost);
                carry = false;
                Add(RidleyInertiaDefinitions.ReversalAcceleration);
                Add(step);
            }
            Add(step);
            if (unchecked((short)(velocity - RidleyInertiaDefinitions.MaximumVelocity)) >= 0) velocity = RidleyInertiaDefinitions.MaximumVelocity;
        }
        return velocity;

        void Add(ushort operand) => velocity = SnesCarry.AddWithCarry(velocity, operand, ref carry);
        void Subtract(ushort operand) => velocity = SnesCarry.SubtractWithBorrow(velocity, operand, ref carry);
    }
}
