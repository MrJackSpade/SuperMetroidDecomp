using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledStatueWalking(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort offset = 0; offset <= 38; offset++)
            AssertEqual(Word(0xaad59a + offset), GoldenTorizoWalkDefinitions.Velocity(offset), "Torizo all complete native byte windows");
        var enemies = new RoomEnemySystem();
        var walk = typeof(RoomEnemySystem).GetMethod("ProcessGoldenTorizoWalkInstruction", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Func<RoomEnemySlot, TorizoEnemyState, SamusState?, RoomLevelData, ushort, ushort, ushort>>(enemies);
        var carry = typeof(RoomEnemySystem).GetMethod("ProcessChozoStatueMovement", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<RoomEnemySlot, ChozoStatueState, SamusState, RoomLevelData, ushort>>(enemies);
        var level = new RoomLevelData(32, 32, new ushort[1024], new byte[1024], new ushort[1024], new byte[8]);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.XRadius = slot.YRadius = 8;
        var torizo = new TorizoEnemyState(slot, true);
        var statue = new ChozoStatueState(slot);
        var samus = new SamusState();
        for (ushort offset = 0; offset < 40; offset += 2)
        {
            slot.XPosition = slot.YPosition = 256;
            slot.XSubposition = 123;
            ushort next = walk(slot, torizo, null, level, 0x1000, offset);
            AssertEqual(Word(0xaad59a + offset), torizo.HorizontalVelocity, "Torizo actual walking velocity without bus");
            AssertEqual(unchecked((ushort)(256 + (short)Word(0xaad59a + offset))), slot.XPosition, "Torizo real whole-pixel movement");
            AssertEqual((ushort)123, slot.XSubposition, "Torizo preserves subpixel state");
            AssertEqual((ushort)0x1004, next, "Torizo instruction operand handoff");
        }
        for (ushort offset = 0; offset < 64; offset += 2)
        {
            var data = ChozoCarryMotionDefinitions.Read(offset);
            short velocity = unchecked((short)Word(0xaae630 + offset));
            short xOffset = unchecked((short)Word(0xaae670 + offset));
            short yOffset = unchecked((short)Word(0xaae6b0 + offset));
            AssertEqual(velocity, data.Velocity, "Chozo native movement velocity");
            AssertEqual(xOffset, data.SamusX, "Chozo native hand X");
            AssertEqual(yOffset, data.SamusY, "Chozo native hand Y");
            slot.XPosition = slot.YPosition = 256;
            slot.XSubposition = slot.YSubposition = 0x8123;
            uint start = 0x01008123;
            carry(slot, statue, samus, level, offset);
            uint x = unchecked(start + (uint)(velocity << 8));
            uint y = start + (uint)(Math.Abs((int)velocity) << 8);
            AssertEqual((ushort)(x >> 16), slot.XPosition, "Chozo real signed X movement");
            AssertEqual((ushort)x, slot.XSubposition, "Chozo real fractional X");
            AssertEqual((ushort)(y >> 16), slot.YPosition, "Chozo real absolute Y movement");
            AssertEqual((ushort)y, slot.YSubposition, "Chozo real fractional Y");
            AssertEqual(unchecked((ushort)(slot.XPosition + xOffset)), samus.XPosition, "Chozo actual carried Samus X");
            AssertEqual(unchecked((ushort)(slot.YPosition + yOffset)), samus.YPosition, "Chozo actual carried Samus Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => GoldenTorizoWalkDefinitions.Velocity(39), "Torizo incomplete byte window");
        AssertThrows<InvalidDataException>(() => ChozoCarryMotionDefinitions.Read(1), "Chozo odd selector remains invalid");
        Console.WriteLine("Statue walking: 39 Torizo byte windows, 96 Chozo words and 52 real movement/carry calls match without a bus.");
    }
}
