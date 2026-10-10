using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the native death-acceleration arguments and both death-phase callers across position and velocity cases without integrating movement.</summary>
    /// <param name="rom">Retail address space containing Ridley's native death-movement instructions.</param>
    private static void VerifyRidleyDeathAcceleration(SuperMetroidAddressSpace rom)
    {
        // $A6:C60E LDY #0, $C611 LDA #16: index and extra deceleration are
        // distinct native arguments, not an out-of-table index of sixteen.
        AssertEqual((byte)0xa0, rom.ReadByte(0xa6c60e), "Death movement LDY immediate opcode");
        AssertEqual((byte)0, rom.ReadByte(0xa6c60f), "Death movement native inertia index");
        AssertEqual((byte)0xa9, rom.ReadByte(0xa6c611), "Death movement LDA immediate opcode");
        AssertEqual((byte)16, rom.ReadByte(0xa6c612), "Death movement native extra deceleration");
        var enemies = new RoomEnemySystem();
        var approaching = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyMoveToDeathSpot", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>();
        var roaring = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyDeathRoar", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>(enemies);
        var slot = enemies.Slots[0];
        var state = new RidleyEnemyState();
        int[] distances = [-256, -64, -16, -1, 0, 1, 16, 64, 256];
        int[] velocities = [-1280, -256, -1, 0, 1, 256, 1280];
        foreach (var step in new[] { approaching, roaring })
        foreach (int distance in distances)
        foreach (int velocity in velocities)
        {
            slot.XPosition = unchecked((ushort)(128 + distance));
            slot.YPosition = unchecked((ushort)(328 - distance));
            slot.XSubposition = slot.YSubposition = 0x8123;
            state.HorizontalVelocity = state.VerticalVelocity = unchecked((ushort)velocity);
            state.FunctionTimer = 20;
            step(slot, state);
            AssertEqual(ExpectedRidleyNativeAcceleration(unchecked((ushort)velocity), slot.XPosition, 128, 16, 16), state.HorizontalVelocity, "Death phase native X acceleration");
            AssertEqual(ExpectedRidleyNativeAcceleration(unchecked((ushort)velocity), slot.YPosition, 328, 16, 16), state.VerticalVelocity, "Death phase native Y acceleration");
            AssertEqual(unchecked((ushort)(128 + distance)), slot.XPosition, "Death phase leaves integration to shared movement");
            AssertEqual((ushort)0x8123, slot.XSubposition, "Death phase preserves fractional position");
        }
        Console.WriteLine("Ridley death acceleration: both phase callers match native index 0 / reversal boost 16 across 126 cases.");

    }
}
