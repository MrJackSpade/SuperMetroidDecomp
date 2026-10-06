using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledRidleyInertia(SuperMetroidAddressSpace rom)
    {
        var ceres = typeof(RoomEnemySystem).GetMethod("AccelerateRidleyToward", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, ushort, ushort, int>>();
        var norfair = typeof(RoomEnemySystem).GetMethod("MoveNorfairRidleyToward", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, ushort, ushort, int, ushort>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new RidleyEnemyState();
        for (int index = 0; index < 16; index++)
        {
            byte divisor = rom.ReadByte(0xa6d61f + index);
            byte ceresDivisor = rom.ReadByte(0xa6d712 + index);
            AssertEqual((ushort)rom.ReadByte(0xa6d712 + index), RidleyInertiaDefinitions.Divisor(index), "Ceres native inertia byte");
            AssertEqual((ushort)divisor, RidleyInertiaDefinitions.Divisor(index), "Norfair native inertia byte");
            // Every signed distance, with both reversal directions and zero/saturation boundaries.
            // Call the production two-axis entry points without an address space attached.
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                short distance = unchecked((short)raw);
                ushort velocity = unchecked((ushort)((raw % 7) switch { 0 => -1280, 1 => -1, 2 => 0, 3 => 1, 4 => 1280, 5 => short.MinValue, _ => short.MaxValue }));
                slot.XPosition = unchecked((ushort)(0xff00 + distance));
                slot.YPosition = unchecked((ushort)(0x100 - distance));
                state.HorizontalVelocity = state.VerticalVelocity = velocity;
                ceres(slot, state, 0xff00, 0x100, index);
                AssertEqual(ExpectedCeresNativeAcceleration(velocity, slot.XPosition, 0xff00, ceresDivisor), state.HorizontalVelocity, "Ceres actual X acceleration");
                AssertEqual(ExpectedCeresNativeAcceleration(velocity, slot.YPosition, 0x100, ceresDivisor), state.VerticalVelocity, "Ceres actual wrapped Y acceleration");
                state.HorizontalVelocity = state.VerticalVelocity = velocity;
                ushort boost = (ushort)(raw % 3 * 16);
                norfair(slot, state, 0xff00, 0x100, index, boost);
                AssertEqual(ExpectedRidleyNativeAcceleration(velocity, slot.XPosition, 0xff00, divisor, boost), state.HorizontalVelocity, "Norfair actual X acceleration");
                AssertEqual(ExpectedRidleyNativeAcceleration(velocity, slot.YPosition, 0x100, divisor, boost), state.VerticalVelocity, "Norfair actual wrapped Y acceleration");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertiaDefinitions.Divisor(-1), "Negative inertia index");
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertiaDefinitions.Divisor(16), "Inertia table boundary");
        Console.WriteLine("Ridley inertia: 32 native bytes and 2,097,152 real two-axis calls match without a bus.");
    }

    // $A6:D62F/$D6A6: the carry left by `position - target` feeds the first ADC/SBC of
    // the velocity step, and each reversal step chains its own carry into the next.
    private static ushort ExpectedCeresNativeAcceleration(ushort initial, ushort position, ushort target, int divisor)
    {
        short distance = unchecked((short)(position - target));
        if (distance == 0) return initial;
        int q = Math.Max(1, Math.Abs((int)distance) / divisor);
        int value;
        if (distance > 0)
        {
            int borrow = position < target ? 1 : 0;
            if ((short)initial >= 0)
            {
                int a = initial - q - borrow;
                int b = unchecked((ushort)a) - q - (a < 0 ? 1 : 0);
                value = unchecked((ushort)b) - q - (b < 0 ? 1 : 0);
            }
            else value = initial - q - borrow;
            ushort word = unchecked((ushort)value);
            return unchecked((short)(word - 0xfb00)) < 0 ? (ushort)0xfb00 : word;
        }
        int carry = position >= target ? 1 : 0;
        if ((short)initial < 0)
        {
            int a = initial + q + carry;
            int b = unchecked((ushort)a) + q + (a >> 16);
            value = unchecked((ushort)b) + q + (b >> 16);
        }
        else value = initial + q + carry;
        ushort result = unchecked((ushort)value);
        return unchecked((short)(result - 0x0500)) >= 0 ? (ushort)0x0500 : result;
    }
    // Model the native word arithmetic independently as wide intermediate values
    // and explicit carry terms. The movie-derived regression supplies independent
    // expected motion; this existing numeric contract covers wrapped inputs too.
    private static ushort ExpectedRidleyNativeAcceleration(ushort initial, ushort position, ushort target, int divisor, int boost)
    {
        short distance = unchecked((short)(position - target));
        if (distance == 0) return initial;
        int q = Math.Max(1, Math.Abs((int)distance) / divisor);
        int value;
        if (distance > 0)
        {
            if ((short)initial >= 0)
            {
                int a = unchecked((ushort)(initial - boost)) - 8;
                int b = unchecked((ushort)a) - q - (a < 0 ? 1 : 0);
                value = unchecked((ushort)b) - q - (b < 0 ? 1 : 0);
            }
            else value = initial - q - (position < target ? 1 : 0);
            ushort word = unchecked((ushort)value);
            return unchecked((short)(word - 0xfb00)) < 0 ? (ushort)0xfb00 : word;
        }
        if ((short)initial < 0)
        {
            int a = unchecked((ushort)(initial + boost)) + 8;
            int b = unchecked((ushort)a) + q + (a >> 16);
            value = unchecked((ushort)b) + q + (b >> 16);
        }
        else value = initial + q + (position >= target ? 1 : 0);
        ushort result = unchecked((ushort)value);
        return unchecked((short)(result - 0x0500)) >= 0 ? (ushort)0x0500 : result;
    }

}
