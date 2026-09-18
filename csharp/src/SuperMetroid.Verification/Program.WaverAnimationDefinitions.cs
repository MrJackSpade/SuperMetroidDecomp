using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWaverAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            var selector = (WaverAnimationSelector)index;
            AssertEqual(ReadWaverAnimationWord(rom, 0xa386db + index * 2),
                WaverAnimationDefinitions.InstructionList(selector),
                $"Waver animation selector {selector}");
        }

        MethodInfo setInstruction = typeof(RoomEnemySystem).GetMethod(
            "SetWaverInstructionList",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        for (int index = 0; index < 4; index++)
        {
            var selector = (WaverAnimationSelector)index;
            var slot = new RoomEnemySlot(0)
            {
                CurrentInstruction = 0x7777,
                InstructionTimer = 0x2222,
                Timer = 0x3333,
            };
            var state = new WaverEnemyState(slot)
            {
                CurrentInstructionListIndex = selector == WaverAnimationSelector.None
                    ? WaverAnimationSelector.FacingRight
                    : WaverAnimationSelector.None,
                RequestedInstructionListIndex = selector,
            };
            setInstruction.Invoke(null, [slot, state]);
            AssertEqual(selector, state.CurrentInstructionListIndex,
                $"production Waver current selector {selector}");
            AssertEqual(WaverAnimationDefinitions.InstructionList(selector),
                slot.CurrentInstruction,
                $"production Waver instruction {selector}");
            AssertEqual(1, slot.InstructionTimer,
                $"production Waver instruction timer {selector}");
            AssertEqual(0, slot.Timer,
                $"production Waver loop timer {selector}");
        }

        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeWaver", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new WaverAnimationReadGuard(rom);
        for (ushort parameter = 0; parameter < 2; parameter++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = parameter;
            initialize.Invoke(enemies, [slot]);
            var expected = (WaverAnimationSelector)parameter;
            AssertEqual(expected, enemies.WaverStates[0]!.RequestedInstructionListIndex,
                $"production Waver initial requested selector {parameter}");
            AssertEqual(expected, enemies.WaverStates[0]!.CurrentInstructionListIndex,
                $"production Waver initial current selector {parameter}");
            AssertEqual(WaverAnimationDefinitions.InstructionList(expected), slot.CurrentInstruction,
                $"production Waver initial instruction {parameter}");
        }

        AssertThrows<InvalidDataException>(
            () => WaverAnimationDefinitions.InstructionList((WaverAnimationSelector)4),
            "Waver animation selector past table");
        Console.WriteLine(
            "Waver animation definitions: four native selectors, all four real handoffs, and both production initial facing paths pass with the pointer table forbidden.");
    }

    private static ushort ReadWaverAnimationWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class WaverAnimationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa386db and < 0xa386e3
                ? throw new InvalidOperationException(
                    $"Waver attempted migrated animation-selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
