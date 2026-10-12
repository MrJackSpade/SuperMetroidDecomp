using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static RidleyInertiaVerification;

internal static class RidleyDeathAccelerationVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ridley death acceleration")]
    internal static void VerifyRidleyDeathAcceleration(SuperMetroidAddressSpace rom)
    {
        // $A6:C60E LDY #0, $C611 LDA #16: index and extra deceleration are
        // distinct native arguments, not an out-of-table index of sixteen.
        AssertEqual((byte)0xa0, rom.ReadCartridgeByte(0xa6c60e), "Death movement LDY immediate opcode");
        AssertEqual((byte)0, rom.ReadCartridgeByte(0xa6c60f), "Death movement native inertia index");
        AssertEqual((byte)0xa9, rom.ReadCartridgeByte(0xa6c611), "Death movement LDA immediate opcode");
        AssertEqual((byte)16, rom.ReadCartridgeByte(0xa6c612), "Death movement native extra deceleration");
        var enemies = new RoomEnemySystem();
        Action<RoomEnemySlot, RidleyEnemyState> approaching = RidleyBehavior.TickNorfairRidleyMoveToDeathSpot;
        Action<RoomEnemySlot, RidleyEnemyState> roaring = enemies.RidleyBehavior.TickNorfairRidleyDeathRoar;
        var slot = enemies.SlotTable.Slots[0];
        var state = new RidleyEnemyState();
        int[] distances = [-256, -64, -16, -1, 0, 1, 16, 64, 256];
        int[] velocities = [-1280, -256, -1, 0, 1, 256, 1280];
        foreach (var step in new[] { approaching, roaring })
        foreach (int distance in distances)
        foreach (int velocity in velocities)
        {
            slot.Position = new(new(unchecked((ushort)(128 + distance)), 0x8123), new(unchecked((ushort)(328 - distance)), 0x8123));
            state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(unchecked((ushort)velocity)), new SnesSignedEightEight(unchecked((ushort)velocity)));
            state.FunctionTimer = 20;
            step(slot, state);
            AssertEqual(ExpectedRidleyNativeAcceleration(unchecked((ushort)velocity), slot.Position.X.Whole, 128, 16, 16), state.Velocity.X.RawValue, "Death phase native X acceleration");
            AssertEqual(ExpectedRidleyNativeAcceleration(unchecked((ushort)velocity), slot.Position.Y.Whole, 328, 16, 16), state.Velocity.Y.RawValue, "Death phase native Y acceleration");
            AssertEqual(unchecked((ushort)(128 + distance)), slot.Position.X.Whole, "Death phase leaves integration to shared movement");
            AssertEqual((ushort)0x8123, slot.Position.X.Fraction, "Death phase preserves fractional position");
        }
        Console.WriteLine("Ridley death acceleration: both phase callers match native index 0 / reversal boost 16 across 126 cases.");

    }
}
