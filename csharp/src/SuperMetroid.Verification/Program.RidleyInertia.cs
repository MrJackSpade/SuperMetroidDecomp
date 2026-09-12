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
        for (int index = 0; index <= 16; index++)
        {
            byte divisor = rom.ReadByte(0xa6d61f + index);
            if (index < 16)
                AssertEqual((ushort)rom.ReadByte(0xa6d712 + index), RidleyInertiaDefinitions.Divisor(index), "Ceres native inertia byte");
            AssertEqual((ushort)divisor, RidleyInertiaDefinitions.NorfairDivisor(index), "Norfair native inertia/adjacent byte");
            // Every signed distance, with both reversal directions and zero/saturation boundaries.
            // Call the production two-axis entry points without an address space attached.
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                short distance = unchecked((short)raw);
                ushort velocity = unchecked((ushort)((raw % 7) switch { 0 => -1280, 1 => -1, 2 => 0, 3 => 1, 4 => 1280, 5 => short.MinValue, _ => short.MaxValue }));
                slot.XPosition = unchecked((ushort)(0xff00 + distance));
                slot.YPosition = unchecked((ushort)(0x100 - distance));
                short yDistance = unchecked((short)-distance);
                state.HorizontalVelocity = state.VerticalVelocity = velocity;
                if (index < 16)
                {
                    ceres(slot, state, 0xff00, 0x100, index);
                    AssertEqual(Expected(velocity, distance, divisor, true, 0), state.HorizontalVelocity, "Ceres actual X acceleration");
                    AssertEqual(Expected(velocity, yDistance, divisor, true, 0), state.VerticalVelocity, "Ceres actual wrapped Y acceleration");
                }
                state.HorizontalVelocity = state.VerticalVelocity = velocity;
                ushort boost = (ushort)(raw % 3 * 16);
                norfair(slot, state, 0xff00, 0x100, index, boost);
                AssertEqual(Expected(velocity, distance, divisor, false, boost), state.HorizontalVelocity, "Norfair actual X acceleration");
                AssertEqual(Expected(velocity, yDistance, divisor, false, boost), state.VerticalVelocity, "Norfair actual wrapped Y acceleration");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertiaDefinitions.Divisor(-1), "Negative inertia index");
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertiaDefinitions.Divisor(16), "Inertia table boundary");
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertiaDefinitions.NorfairDivisor(17), "Norfair adjacent-byte boundary");
        Console.WriteLine("Ridley inertia: 33 native bytes and 2,162,688 real two-axis calls match without a bus.");

        // Independent signed direction formulation of the existing acceleration rules.
        static ushort Expected(ushort initial, short distance, int divisor, bool isCeres, int boost)
        {
            if (distance == 0) return initial;
            int velocity = unchecked((short)initial);
            int direction = distance < 0 ? 1 : -1;
            int quotient = Math.Max(1, Math.Abs((int)distance) / divisor);
            bool reversing = direction > 0 ? velocity < 0 : velocity >= 0;
            int change = quotient + (reversing ? isCeres ? quotient * 2 : quotient + 8 + boost : 0);
            return unchecked((ushort)Math.Clamp(velocity + direction * change, -1280, 1280));
        }
    }
}
