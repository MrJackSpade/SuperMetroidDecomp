using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledGrowingShutters(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeGrowingShutter", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];
        for (ushort selector = 0; selector < 4; selector++)
        {
            AssertEqual(Word(0xa2ea4e + selector * 2), (ushort)GrowingShutterDefinitions.InitialFunction(selector), "Shutter native function ordering");
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
}
