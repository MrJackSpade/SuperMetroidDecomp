using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledLinearEnemySpeeds(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var shared = typeof(RoomEnemySystem).GetMethod("ReadLinearEnemySpeed",
            BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<ushort, (short Whole, ushort Fraction)>>();
        // Every possible four-byte window, including odd offsets and windows
        // crossing record boundaries, matches independent cartridge bytes.
        for (int offset = 0; offset <= 516; offset++)
        {
            var expected = (unchecked((short)Word(0xa08187 + offset)), Word(0xa08189 + offset));
            AssertEqual(expected, shared((ushort)offset), "compiled linear native byte window");
            AssertEqual(expected, EnemyLinearSpeedDefinitions.Read(offset), "linear catalog window");
            AssertEqual(Word(0xa08187 + offset), Word(0xa28187 + offset), "Ripper bank mirror whole");
            AssertEqual(Word(0xa08189 + offset), Word(0xa28189 + offset), "Ripper bank mirror fraction");
        }
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedDefinitions.Read(-1), "negative linear offset");
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedDefinitions.Read(517), "partial linear pair");
        AssertThrows<InvalidDataException>(() => shared(ushort.MaxValue), "unclassified linear overread");

        var enemies = new RoomEnemySystem();
        // Only family-specific distance/timer data can be read. No speed bytes
        // are present; Stoke and Ripper do not need any bus for their speed setup.
        typeof(RoomEnemySystem).GetField("_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(enemies, new TestAddressSpace());
        Action<RoomEnemySlot> Initializer(string name) => typeof(RoomEnemySystem)
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var cacatac = Initializer("InitializeCacatac");
        var owtch = Initializer("InitializeOwtch");
        var stoke = Initializer("InitializeStoke");
        var ripper = typeof(RoomEnemySystem).GetMethod("LoadRipperVelocity",
            BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Action<RoomEnemySlot, bool>>();
        var slot = enemies.Slots[0];
        for (int index = 0; index < 65; index++)
        {
            ushort right = Word(0xa08187 + index * 8), rightFraction = Word(0xa08189 + index * 8);
            ushort left = Word(0xa0818b + index * 8), leftFraction = Word(0xa0818d + index * 8);
            var expected = (right, rightFraction, left, leftFraction);
            slot.Parameter1 = 0;
            slot.Parameter2 = (ushort)(index << 8);
            cacatac(slot);
            var c = enemies.CacatacStates[0]!;
            AssertEqual(expected, (c.RightVelocity, c.RightSubvelocity, c.LeftVelocity, c.LeftSubvelocity), "Cacatac native speed setup");
            slot.Parameter2 = (ushort)index;
            owtch(slot);
            var o = enemies.OwtchStates[0]!;
            AssertEqual(expected, (o.RightVelocity, o.RightSubvelocity, o.LeftVelocity, o.LeftSubvelocity), "Owtch native speed setup");
            stoke(slot);
            var s = enemies.StokeStates[0]!;
            AssertEqual(expected, (s.RightVelocity, s.RightSubvelocity, s.LeftVelocity, s.LeftSubvelocity), "Stoke native speed setup");
            slot.VariableE = (ushort)(index * 8);
            ripper(slot, true);
            AssertEqual((right, rightFraction), (slot.VariableD, slot.VariableC), "Ripper right speed");
            ripper(slot, false);
            AssertEqual((left, leftFraction), (slot.VariableD, slot.VariableC), "Ripper reversal speed");
        }
        Console.WriteLine("Compiled linear speeds: 517 byte windows, bank mirror and all 65 speeds in four direct family callers match the cartridge without speed-table ROM reads.");
    }
}
