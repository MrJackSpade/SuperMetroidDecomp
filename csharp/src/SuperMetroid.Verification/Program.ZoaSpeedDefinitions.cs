using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledZoaSpeeds(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort offset = 0; offset <= 16; offset++)
        {
            int address = 0xa3b415 + offset;
            int expected = unchecked((Word(address) << 16) | Word(address + 2));
            AssertEqual(expected, ZoaSpeedDefinitions.Displacement(offset), "Zoa byte-indexed native split velocity");
        }
        var enemies = new RoomEnemySystem();
        var shoot = typeof(RoomEnemySystem).GetMethod("RunZoaShooting", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<RoomEnemySlot, ZoaEnemyState, ushort, ushort>>(enemies);
        RoomEnemySlot actor = enemies.Slots[0];
        var state = new ZoaEnemyState(actor);
        for (ushort offset = 0; offset <= 16; offset += 4)
        {
            int address = 0xa3b415 + offset;
            int magnitude = unchecked((Word(address) << 16) | Word(address + 2));
            state.XSpeedTableIndex = offset;
            for (ushort direction = 0; direction <= 2; direction += 2)
            {
                // Keep the same animation installed: this exercises movement without an art-data bus.
                state.InstructionListTableIndex = state.PreviousInstructionListTableIndex = direction;
                int displacement = direction == 0 ? -magnitude : magnitude;
                for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
                {
                    actor.XPosition = 128;
                    actor.XSubposition = (ushort)fraction;
                    actor.YPosition = 128;
                    actor.YSubposition = 123;
                    shoot(actor, state, 0, 0);
                    uint expected = unchecked(0x00800000u + (uint)fraction + (uint)displacement);
                    AssertEqual((ushort)(expected >> 16), actor.XPosition, "Zoa real signed whole displacement");
                    AssertEqual((ushort)expected, actor.XSubposition, "Zoa real fractional carry/borrow");
                    AssertEqual((ushort)128, actor.YPosition, "Zoa shooting preserves Y");
                    AssertEqual((ushort)123, actor.YSubposition, "Zoa shooting preserves sub-Y");
                }
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => ZoaSpeedDefinitions.Displacement(17), "Zoa incomplete record rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => ZoaSpeedDefinitions.Displacement(ushort.MaxValue), "Zoa invalid index rejected");
        Console.WriteLine("Zoa compiled speeds: all 17 native byte windows and 655360 real signed/subpixel integrations match without a speed-table bus.");
    }
}
