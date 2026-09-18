using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySparkMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa8e682;
        const int functionTable = 0xa8e688;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new SparkMovementReadGuard(new TestAddressSpace()));
        typeof(RoomEnemySystem).GetField("_isAreaBossDefeated", flags)!.SetValue(
            enemies,
            (Func<bool>)(() => true));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeSpark", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        for (ushort selector = 0; selector < 4; selector++)
        {
            int offset = selector * 2;
            ushort nativeInstruction = (ushort)(rom.ReadByte(instructionTable + offset) |
                rom.ReadByte(instructionTable + offset + 1) << 8);
            var nativeFunction = (SparkEnemyFunction)(rom.ReadByte(functionTable + offset) |
                rom.ReadByte(functionTable + offset + 1) << 8);
            SparkMovementDefinition definition = SparkMovementDefinitions.InitialState(selector);
            AssertEqual(nativeInstruction, definition.InstructionList,
                $"compiled Spark instruction selector {selector}");
            AssertEqual(nativeFunction, definition.Function,
                $"compiled Spark function selector {selector}");

            slot.Parameter1 = selector;
            slot.Parameter2 = 5;
            initialize(slot);
            AssertEqual(nativeInstruction, slot.CurrentInstruction,
                $"Spark production instruction selector {selector}");
            AssertEqual(nativeFunction, enemies.SparkStates[0]!.Function,
                $"Spark production function selector {selector}");
        }

        foreach (ushort highBits in new ushort[] { 4, 0x0100, 0xffff })
        {
            SparkMovementDefinition expected = SparkMovementDefinitions.InitialState(
                (ushort)(highBits & 3));
            AssertEqual(expected, SparkMovementDefinitions.InitialState(highBits),
                $"Spark selector mask {highBits:X4}");
        }

        Console.WriteLine(
            "Spark movement definitions: three authored pairs, both selector-three overreads and production initialization pass with table reads forbidden.");
    }

    private sealed class SparkMovementReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa8e682 and < 0xa8e690
            ? throw new InvalidOperationException(
                $"Spark initializer attempted migrated selector read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
