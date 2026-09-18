using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresDoorInitializationDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa6f52c;
        const int functionTable = 0xa6f72b;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeCeresDoor", flags)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        FieldInfo vramField = typeof(RoomEnemySystem).GetField("_vram", flags)!;
        FieldInfo cgramField = typeof(RoomEnemySystem).GetField("_cgram", flags)!;
        var guarded = new CeresDoorInitializationReadGuard(rom);

        for (ushort variant = 0; variant < 7; variant++)
        {
            CeresDoorInitializationDefinition definition =
                CeresDoorInitializationDefinitions.For(variant);
            AssertEqual(ReadCeresDoorInitializationWord(rom, functionTable + variant * 2),
                definition.MainFunction,
                $"Ceres door function selector {variant}");
            AssertEqual(ReadCeresDoorInitializationWord(rom, instructionTable + variant * 2),
                definition.InstructionList,
                $"Ceres door instruction selector {variant}");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            vramField.SetValue(enemies, new SnesVram());
            cgramField.SetValue(enemies, new SnesCgram());
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = variant;

            initialize.Invoke(enemies, [slot]);

            AssertEqual(definition.MainFunction, slot.VariableA,
                $"production Ceres door function selector {variant}");
            AssertEqual(definition.InstructionList, slot.CurrentInstruction,
                $"production Ceres door instruction selector {variant}");
            AssertEqual((ushort)1, slot.InstructionTimer,
                $"production Ceres door instruction timer {variant}");
            AssertEqual((ushort)0, slot.Timer,
                $"production Ceres door general timer {variant}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresDoorInitializationDefinitions.For(7),
            "Ceres door selector past definitions");
        Console.WriteLine(
            "Ceres door initialization definitions: all fourteen native selector words and seven production initializers pass with both source tables forbidden.");
    }

    private static ushort ReadCeresDoorInitializationWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class CeresDoorInitializationReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa6f52c and < 0xa6f53a or >= 0xa6f72b and < 0xa6f739
                ? throw new InvalidOperationException(
                    $"Ceres door attempted migrated initialization-table read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
