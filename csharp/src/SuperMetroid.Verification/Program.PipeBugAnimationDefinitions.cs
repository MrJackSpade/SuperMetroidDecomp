using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPipeBugAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        const int normalTable = 0xb3882b;
        const int strongTable = 0xb38833;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeBrinstarPipeBug", instanceFlags)!;
        MethodInfo select = typeof(RoomEnemySystem).GetMethod(
            "SelectBrinstarPipeBugAnimation",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var guarded = new PipeBugAnimationReadGuard(rom);

        foreach (bool strong in new[] { false, true })
        {
            var initializedEnemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
                .SetValue(initializedEnemies, guarded);
            RoomEnemySlot initializedSlot = initializedEnemies.Slots[0];
            initializedSlot.EnemyDefinitionPointer = strong
                ? PipeBugDefinitions.StrongBrinstarEnemyDefinition
                : PipeBugDefinitions.BrinstarEnemyDefinition;
            initializedSlot.Parameter1 = strong ? (ushort)1 : (ushort)0;
            initializedSlot.XPosition = 0x0100;
            initializedSlot.YPosition = 0x0080;
            initialize.Invoke(initializedEnemies, [initializedSlot]);
            AssertEqual(PipeBugDefinitions.BrinstarInstructionList(
                    strong,
                    PipeBugAnimationSelector.None),
                initializedSlot.CurrentInstruction,
                $"production {(strong ? "strong" : "normal")} Pipe Bug initialization");

            for (int index = 0; index < 4; index++)
            {
                var selector = (PipeBugAnimationSelector)index;
                ushort expected = ReadPipeBugAnimationWord(
                    rom,
                    (strong ? strongTable : normalTable) + index * 2);
                AssertEqual(expected,
                    PipeBugDefinitions.BrinstarInstructionList(strong, selector),
                    $"{(strong ? "strong" : "normal")} Pipe Bug {selector}");

                var enemies = new RoomEnemySystem();
                typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
                    .SetValue(enemies, guarded);
                var slot = new RoomEnemySlot(0)
                {
                    Parameter1 = strong ? (ushort)1 : (ushort)0,
                    CurrentInstruction = 0x7777,
                    InstructionTimer = 0x2222,
                    Timer = 0x3333,
                };
                var state = new PipeBugEnemyState(slot)
                {
                    AnimationState = selector,
                    InstalledAnimationState = selector == PipeBugAnimationSelector.None
                        ? PipeBugAnimationSelector.Shooting
                        : PipeBugAnimationSelector.None,
                };
                select.Invoke(null, [slot, state]);
                AssertEqual(selector, state.InstalledAnimationState,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug selector {selector}");
                AssertEqual(expected, slot.CurrentInstruction,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug instruction {selector}");
                AssertEqual(1, slot.InstructionTimer,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug timer {selector}");
                AssertEqual(0, slot.Timer,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug loop {selector}");
            }
        }

        AssertThrows<InvalidDataException>(
            () => PipeBugDefinitions.BrinstarInstructionList(
                strong: false,
                (PipeBugAnimationSelector)4),
            "Pipe Bug animation selector past table");
        Console.WriteLine(
            "Pipe Bug animation definitions: eight native selectors and all eight production handoffs pass with both pointer tables forbidden.");
    }

    private static ushort ReadPipeBugAnimationWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class PipeBugAnimationReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xb3882b and < 0xb3883b
                ? throw new InvalidOperationException(
                    $"Pipe Bug attempted migrated animation read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
