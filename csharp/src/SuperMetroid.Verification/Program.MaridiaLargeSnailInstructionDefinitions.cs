using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMaridiaLargeSnailInstructionDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo install = typeof(RoomEnemySystem).GetMethod(
            "InstallMaridiaLargeSnailInstructionList",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", instance)!;
        var guarded = new MaridiaLargeSnailInstructionReadGuard(rom);

        for (ushort animationIndex = 0; animationIndex < 8; animationIndex++)
        {
            ushort expected = ReadMaridiaLargeSnailInstructionWord(
                rom, 0xa2cb77 + animationIndex * 2);
            AssertEqual(expected,
                MaridiaLargeSnailInstructionDefinitions.InstructionPointer(animationIndex),
                $"Maridia Large Snail instruction selector {animationIndex}");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            RoomEnemySlot slot = enemies.Slots[0];
            var state = new MaridiaLargeSnailEnemyState(slot)
            {
                RequestedInstructionListIndex = animationIndex,
                InstalledInstructionListIndex = ushort.MaxValue,
            };
            slot.InstructionTimer = 0x1234;
            slot.Timer = 0x5678;

            install.Invoke(null, [slot, state]);
            AssertEqual(animationIndex, state.InstalledInstructionListIndex,
                $"production Maridia Large Snail installed selector {animationIndex}");
            AssertEqual(expected, slot.CurrentInstruction,
                $"production Maridia Large Snail instruction {animationIndex}");
            AssertEqual((ushort)1, slot.InstructionTimer,
                $"production Maridia Large Snail instruction timer {animationIndex}");
            AssertEqual((ushort)0, slot.Timer,
                $"production Maridia Large Snail general timer {animationIndex}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MaridiaLargeSnailInstructionDefinitions.InstructionPointer(8),
            "Maridia Large Snail selector past table");
        Console.WriteLine(
            "Maridia Large Snail instruction definitions: all eight native selectors and production installs pass with the pointer table forbidden.");
    }

    private static ushort ReadMaridiaLargeSnailInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class MaridiaLargeSnailInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa2cb77 and < 0xa2cb87
                ? throw new InvalidOperationException(
                    $"Maridia Large Snail attempted migrated instruction-table read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
