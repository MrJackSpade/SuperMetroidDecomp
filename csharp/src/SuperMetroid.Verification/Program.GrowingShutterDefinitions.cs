using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledGrowingShutters(SuperMetroidAddressSpace rom)
    {
        VerifyGrowingShutterInitialSelection(rom);
        VerifyGrowingShutterWholeSpeed(rom);
        VerifyGrowingShutterFractionalSpeed(rom);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeGrowingShutter", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];
        for (ushort selector = 0; selector < 4; selector++)
        {
            for (int high = 0; high < 256; high++)
            {
                for (int speed = 0; speed < 24; speed++)
                {
                    slot.ExtraProperties = (ushort)(selector >> 1);
                    slot.CurrentInstruction = (ushort)(selector & 1);
                    slot.Parameter2 = (ushort)((high << 8) | speed);
                    slot.YPosition = selector < 2 ? (ushort)65530 : (ushort)5;
                    initialize(slot);
                    GrowingShutterEnemyState state = enemies.GrowingShutterStates[0]!;
                    AssertEqual(Word(0xa2ea4e + selector * 2), (ushort)state.Function, "Shutter real dispatch without bus");
                    AssertEqual(unchecked((short)Word(0xa2ea56 + speed * 4)), state.GrowthVelocity, "Shutter native whole speed");
                    AssertEqual(Word(0xa2ea58 + speed * 4), state.GrowthSubvelocity, "Shutter native fractional speed");
                    int direction = selector < 2 ? 1 : -1;
                    AssertEqual(slot.YPosition, state.GrowthLevel0OriginY, "Shutter base origin");
                    AssertEqual(unchecked((ushort)(slot.YPosition + direction * 8)), state.GrowthLevel1OriginY, "Shutter wrapped origin one");
                    AssertEqual(unchecked((ushort)(slot.YPosition + direction * 16)), state.GrowthLevel2OriginY, "Shutter wrapped origin two");
                    AssertEqual(unchecked((ushort)(slot.YPosition + direction * 24)), state.GrowthLevel3OriginY, "Shutter wrapped origin three");
                    AssertEqual((ushort)0, slot.ExtraProperties, "Shutter consumes initial direction word");
                }
            }
        }
        AssertThrows<InvalidDataException>(() => GrowingShutterDefinitions.InitialFunction(4), "Shutter invalid initial selector");
        AssertThrows<InvalidDataException>(() => GrowingShutterDefinitions.Speed(24), "Shutter speed outside authored records");
        Console.WriteLine("Growing shutter definitions: 52 native words, 24576 real initializers and wrapped section origins match without a bus.");
    }
    private static void VerifyGrowingShutterInitialSelection(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            int address = 0xa2ea4e + 2 * index;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, (ushort)GrowingShutterDefinitions.InitialFunction(index),
                "growing shutter named initial case matches original pointer");
        }
        foreach (int index in new[] { int.MinValue, -1, 4, 5, 65536, int.MaxValue })
            AssertThrows<InvalidDataException>(() => GrowingShutterDefinitions.InitialFunction(index),
                "growing shutter initial selector rejects invalid full-width inputs");
    }

    private static void VerifyGrowingShutterWholeSpeed(SuperMetroidAddressSpace rom) =>
        VerifyGrowingShutterSpeedField(rom, fractional: false);

    private static void VerifyGrowingShutterFractionalSpeed(SuperMetroidAddressSpace rom) =>
        VerifyGrowingShutterSpeedField(rom, fractional: true);

    private static void VerifyGrowingShutterSpeedField(SuperMetroidAddressSpace rom, bool fractional)
    {
        for (int index = 0; index < 24; index++)
        {
            int address = 0xa2ea56 + 4 * index + (fractional ? 2 : 0);
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            for (int high = 0; high < 256; high++)
            {
                var speed = GrowingShutterDefinitions.Speed((ushort)((high << 8) | index));
                if (fractional)
                    AssertEqual(native, speed.Fraction, "growing shutter native unsigned fraction and high-byte aliases");
                else
                    AssertEqual(unchecked((short)native), speed.Whole, "growing shutter native signed whole word and high-byte aliases");
            }
        }
        for (int high = 0; high < 256; high++)
            foreach (int low in new[] { 24, 25, 127, 255 })
                AssertThrows<InvalidDataException>(() => GrowingShutterDefinitions.Speed((ushort)((high << 8) | low)),
                    "growing shutter speed rejects out-of-domain low bytes regardless of high byte");
    }
}
