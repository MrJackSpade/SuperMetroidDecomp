using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// Checks the seeking bosses' velocity run (#1620) and the Shitroid's production seek against the original's chained
/// sixteen-bit carries and borrows.
/// </summary>
internal static class SeekingVelocityRunVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// Every velocity word, raised and lowered by step sizes from zero to past a room's width, with and without a unit
    /// owed: each step lands on the chained addition's or subtraction's word, and the step after it is lengthened exactly
    /// when the chained arithmetic leaves its carry or borrow. A run toward a target the wrap reverses owes its first unit.
    /// </summary>
    internal static void VerifySeekingVelocityRun()
    {
        foreach (int amount in new[] { 0, 1, 7, 8, 9, 255, 256, 1280, 4096 })
        foreach (bool owed in new[] { false, true })
        {
            // From 0, a target at 1 lies ahead in both orders; one at 36864 lies behind by the signed distance.
            int target = owed ? 36864 : 1;
            int carryIn = owed ? 1 : 0;
            for (int word = 0; word < WordRange; word++)
            {
                var raising = new SeekingVelocityRun(new SignedEightEight(word), 0, target);
                int sum = word + amount + carryIn;
                raising.Raise(amount);
                int sumCarry = sum > ushort.MaxValue ? 1 : 0;
                raising.Raise(0);
                if (raising.Velocity.RawValue != unchecked((ushort)(sum + sumCarry)))
                    throw new InvalidOperationException($"Raising {word:X4} by {amount} owing {carryIn} reached {raising.Velocity.RawValue:X4}.");

                var lowering = new SeekingVelocityRun(new SignedEightEight(word), 0, target);
                int difference = word - amount - carryIn;
                lowering.Lower(amount);
                int borrow = difference < 0 ? 1 : 0;
                lowering.Lower(0);
                if (lowering.Velocity.RawValue != unchecked((ushort)(difference - borrow)))
                    throw new InvalidOperationException($"Lowering {word:X4} by {amount} owing {carryIn} reached {lowering.Velocity.RawValue:X4}.");
            }
        }

        var fresh = new SeekingVelocityRun(SignedEightEight.Zero, 0, 36864);
        fresh.StartFresh();
        fresh.Raise(1);
        AssertEqual((ushort)1, fresh.Velocity.RawValue, "a fresh run owes no unit for the wrapped target");
        var retreat = new SeekingVelocityRun(SignedEightEight.Zero, 100, CoordinateAxis.Wrap(-128));
        retreat.Lower(16);
        AssertEqual(unchecked((ushort)-17), retreat.Velocity.RawValue, "a seek toward a point above the room's top lowers by one unit more");
        Console.WriteLine("  Seeking velocity run: every velocity word's raises and lowers match the chained arithmetic.");
    }

    /// <summary>
    /// The Shitroid's production gradual acceleration (horizontal and vertical) for every signed distance within a room,
    /// across the coordinate wrap, from velocities within its clamps, for each divisor class, on and off screen: each
    /// result equals the original routines' chained carries and borrows (<c>$A9:F4E6</c>, <c>$A9:F46B</c>).
    /// </summary>
    internal static void VerifyShitroidSeekingAcceleration()
    {
        const int Reach = 1024;
        ushort[] velocities = [0x0000, 0x0001, 0x00ff, 0x0100, 0x04ff, 0x07ff, 0x0800, 0xffff, 0xff01, 0xff00, 0xfb00, 0xf801, 0xf800];
        foreach (int target in new[] { 0x0200, 0x0004, 0xfff0 })
        foreach (int divisor in new[] { 1, 2, 7, 16 })
        foreach (ushort velocity in velocities)
        for (int offset = -Reach; offset <= Reach; offset++)
        {
            int position = CoordinateAxis.Wrap(target + offset);
            foreach (bool offScreen in new[] { false, true })
            {
                const int wrongWaySpeed = 0x0010;
                ushort expected = ExpectedShitroidHorizontal(velocity, position, target, divisor, wrongWaySpeed, offScreen);
                SignedEightEight actual = BabyMetroidGradualAcceleration.AccelerateHorizontally(
                    position, target, new SignedEightEight(velocity), divisor, wrongWaySpeed, () => offScreen);
                if (actual.RawValue != expected)
                    throw new InvalidOperationException(
                        $"Horizontal seek from {position:X4} to {target:X4} at {velocity:X4} by {divisor}, off screen {offScreen}: {actual.RawValue:X4}, not {expected:X4}.");
            }
            ushort expectedVertical = ExpectedShitroidVertical(velocity, position, target, divisor);
            SignedEightEight vertical = BabyMetroidGradualAcceleration.AccelerateVertically(position, target, new SignedEightEight(velocity), divisor);
            if (vertical.RawValue != expectedVertical)
                throw new InvalidOperationException(
                    $"Vertical seek from {position:X4} to {target:X4} at {velocity:X4} by {divisor}: {vertical.RawValue:X4}, not {expectedVertical:X4}.");
        }
        Console.WriteLine("  Shitroid seeking acceleration: every distance, velocity and divisor matches the chained carries.");
    }

    /// <summary>The original horizontal gradual acceleration, its carries chained as the routine leaves them.</summary>
    private static ushort ExpectedShitroidHorizontal(ushort velocity, int position, int target, int divisor, int wrongWaySpeed, bool offScreen)
    {
        int distance = unchecked((short)(position - target));
        if (distance == 0)
            return velocity;
        int carry = position >= target ? 1 : 0;
        if (distance < 0)
        {
            int step = Math.Max(1, -distance / divisor);
            int value = velocity;
            if (velocity >= 0x8000)
            {
                if (offScreen)
                    value = Add(value, wrongWaySpeed, 1, out _);
                value = Add(value, 8, 0, out carry);
                value = Add(value, step, carry, out carry);
            }
            value = Add(value, step, carry, out _);
            return unchecked((short)(value - 0x0800)) < 0 ? (ushort)value : (ushort)0x0800;
        }
        else
        {
            int step = Math.Max(1, distance / divisor);
            int value = velocity;
            if (velocity < 0x8000)
            {
                if (offScreen)
                    value = Subtract(value, wrongWaySpeed, 0, out _);
                value = Subtract(value, 8, 0, out carry);
                value = Subtract(value, step, carry, out carry);
            }
            else
            {
                carry = carry == 1 ? 0 : 1;
            }
            value = Subtract(value, step, carry, out _);
            return unchecked((short)(value - 0xf800)) >= 0 ? (ushort)value : (ushort)0xf800;
        }
    }

    /// <summary>The original vertical gradual acceleration, its carries chained as the routine leaves them.</summary>
    private static ushort ExpectedShitroidVertical(ushort velocity, int position, int target, int divisor)
    {
        int distance = unchecked((short)(position - target));
        if (distance == 0)
            return velocity;
        int carry = position >= target ? 1 : 0;
        if (distance < 0)
        {
            int step = Math.Max(1, -distance / divisor);
            int value = velocity;
            if (velocity >= 0x8000)
            {
                value = Add(value, 8, 0, out carry);
                value = Add(value, step, carry, out carry);
            }
            value = Add(value, step, carry, out _);
            return unchecked((short)(value - 0x0500)) < 0 ? (ushort)value : (ushort)0x0500;
        }
        else
        {
            int step = Math.Max(1, distance / divisor);
            int value = velocity;
            if (velocity < 0x8000)
            {
                value = Subtract(value, 8, 0, out carry);
                value = Subtract(value, step, carry, out carry);
            }
            else
            {
                carry = carry == 1 ? 0 : 1;
            }
            value = Subtract(value, step, carry, out _);
            return unchecked((short)(value - 0xfb00)) >= 0 ? (ushort)value : (ushort)0xfb00;
        }
    }

    /// <summary>A sixteen-bit addition with a carry in, reporting its carry out.</summary>
    private static int Add(int word, int operand, int carryIn, out int carryOut)
    {
        int sum = word + operand + carryIn;
        carryOut = sum > ushort.MaxValue ? 1 : 0;
        return sum & ushort.MaxValue;
    }

    /// <summary>A sixteen-bit subtraction with a borrow in, reporting its borrow out.</summary>
    private static int Subtract(int word, int operand, int borrowIn, out int borrowOut)
    {
        int difference = word - operand - borrowIn;
        borrowOut = difference < 0 ? 1 : 0;
        return difference & ushort.MaxValue;
    }
}
