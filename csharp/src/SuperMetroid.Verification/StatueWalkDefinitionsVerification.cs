using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

/// <summary>Checks compiled Golden Torizo walking and Chozo statue carry motion against native tables.</summary>
internal static class StatueWalkDefinitionsVerification
{
    /// <summary>Compares overlapping Torizo velocity windows and Chozo carry words, then checks production movement and Samus offsets without a bus.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for statue walking")]
    internal static void VerifyCompiledStatueWalking(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0; offset <= 38; offset++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaad59a + offset)), GoldenTorizoWalkDefinitions.Velocity(offset), "Torizo all complete native byte windows");
        var enemies = new RoomEnemySystem();
        Func<RoomEnemySlot, TorizoEnemyState, SamusState?, RoomLevelData, ushort, ushort, ushort> walk = enemies.BombTorizoBehavior.ProcessGoldenTorizoWalkInstruction;
        Action<RoomEnemySlot, ChozoStatueState, SamusState, RoomLevelData, ushort> carry = enemies.ChozoStatueBehavior.ProcessChozoStatueMovement;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]), new byte[8]);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Radius = new(8, 8);
        var torizo = new TorizoEnemyState(slot, true);
        var statue = new ChozoStatueState(slot);
        var samus = new SamusState();
        for (ushort offset = 0; offset < 40; offset += 2)
        {
            slot.Position = slot.Position.WithWholeY(256).WithWholeX(256).WithFractionX(123);
            ushort next = walk(slot, torizo, null, level, 0x1000, offset);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaad59a + offset)), torizo.Velocity.X.RawValue, "Torizo actual walking velocity without bus");
            AssertEqual(unchecked((ushort)(256 + (short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaad59a + offset)))), slot.Position.X.Whole, "Torizo real whole-pixel movement");
            AssertEqual((ushort)123, slot.Position.X.Fraction, "Torizo preserves subpixel state");
            AssertEqual((ushort)0x1004, next, "Torizo instruction operand handoff");
        }
        for (ushort offset = 0; offset < 64; offset += 2)
        {
            var data = ChozoCarryMotionDefinitions.Read(offset);
            short velocity = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae630 + offset)));
            short xOffset = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae670 + offset)));
            short yOffset = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae6b0 + offset)));
            AssertEqual(velocity, data.Velocity, "Chozo native movement velocity");
            AssertEqual(xOffset, data.SamusX, "Chozo native hand X");
            AssertEqual(yOffset, data.SamusY, "Chozo native hand Y");
            slot.Position = new(new(256, 0x8123), new(256, 0x8123));
            uint start = 0x01008123;
            carry(slot, statue, samus, level, offset);
            uint x = unchecked(start + (uint)(SnesSixteenSixteen.FromEightEight(velocity)));
            uint y = start + (uint)(SnesSixteenSixteen.FromEightEight(Math.Abs((int)velocity)));
            AssertEqual(SnesFixedPosition.WholeOf(unchecked((int)x)), slot.Position.X.Whole, "Chozo real signed X movement");
            AssertEqual((ushort)x, slot.Position.X.Fraction, "Chozo real fractional X");
            AssertEqual(SnesFixedPosition.WholeOf(unchecked((int)y)), slot.Position.Y.Whole, "Chozo real absolute Y movement");
            AssertEqual((ushort)y, slot.Position.Y.Fraction, "Chozo real fractional Y");
            AssertEqual(unchecked((ushort)(slot.Position.X.Whole + xOffset)), samus.Position.X.Whole, "Chozo actual carried Samus X");
            AssertEqual(unchecked((ushort)(slot.Position.Y.Whole + yOffset)), samus.Position.Y.Whole, "Chozo actual carried Samus Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => GoldenTorizoWalkDefinitions.Velocity(39), "Torizo incomplete byte window");
        AssertThrows<InvalidDataException>(() => ChozoCarryMotionDefinitions.Read(1), "Chozo odd selector remains invalid");
        Console.WriteLine("Statue walking: 39 Torizo byte windows, 96 Chozo words and 52 real movement/carry calls match without a bus.");
    }
}
