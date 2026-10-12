using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{

    private static void VerifyCompiledZoaSpeeds(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        ushort Word(int address) => CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
        for (ushort offset = 0; offset <= 16; offset++)
        {
            int address = ZoaSpeedDefinitions.ReferenceAddress + offset;
            int expected = unchecked((Word(address) << 16) | Word(address + 2));
            AssertEqual(expected, ZoaSpeedDefinitions.Displacement(offset), "Zoa byte-indexed native split velocity");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => ZoaSpeedDefinitions.Displacement(17), "Zoa incomplete record rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => ZoaSpeedDefinitions.Displacement(ushort.MaxValue), "Zoa invalid index rejected");
        if (definitionsOnly)
        {
            Console.WriteLine("Zoa speed cases: all 17 original byte windows and invalid bounds pass.");
            return;
        }
        var enemies = new RoomEnemySystem();
        var shoot = typeof(RoomEnemySystem).GetMethod(
                "RunZoaShooting", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<RoomEnemySlot, ZoaEnemyState, Position16<RoomPixels>>>();
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
                state.InstructionListTableIndex = state.PreviousInstructionListTableIndex =
                    (ZoaAnimationSelector)direction;
                int displacement = direction == 0 ? -magnitude : magnitude;
                for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
                {
                    actor.Position = new(new(128, (ushort)fraction), new(128, 123));
                    shoot(actor, state, Position16<RoomPixels>.Zero);
                    uint expected = unchecked(0x00800000u + (uint)fraction + (uint)displacement);
                    AssertEqual(SnesFixedPosition.WholeOf(unchecked((int)expected)), actor.Position.X.Whole, "Zoa real signed whole displacement");
                    AssertEqual((ushort)expected, actor.Position.X.Fraction, "Zoa real fractional carry/borrow");
                    AssertEqual((ushort)128, actor.Position.Y.Whole, "Zoa shooting preserves Y");
                    AssertEqual((ushort)123, actor.Position.Y.Fraction, "Zoa shooting preserves sub-Y");
                }
            }
        }

        // #1269: leaving the camera square snaps Zoa to its spawn coordinate. $A3:B51E-$B527
        // writes only the whole words, so both subpixel fractions carry into the next flight.
        state.XSpeedTableIndex = 0;
        state.InstructionListTableIndex = state.PreviousInstructionListTableIndex =
            ZoaAnimationSelector.None;
        state.SpawnPosition = new(0x560, 0xd8);
        actor.Position = new(new(128, 0x8000), new(128, 0x4000));
        int resetDisplacement = -unchecked((Word(0xa3b415) << 16) | Word(0xa3b417));
        uint movedX = unchecked(0x00808000u + (uint)resetDisplacement);
        shoot(actor, state, new(0x1000, 0x1000));
        AssertEqual((ushort)0x560, actor.Position.X.Whole, "off-screen Zoa returns to its spawn X");
        AssertEqual((ushort)0xd8, actor.Position.Y.Whole, "off-screen Zoa returns to its spawn Y");
        AssertEqual((ushort)movedX, actor.Position.X.Fraction, "the spawn reset keeps the moved sub-X");
        AssertEqual((ushort)0x4000, actor.Position.Y.Fraction, "the spawn reset keeps sub-Y");
        Console.WriteLine("Zoa compiled speeds: all 17 native byte windows and 655360 real signed/subpixel integrations match without a speed-table bus.");
    }
}
