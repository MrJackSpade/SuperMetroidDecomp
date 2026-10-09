using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks both Pipe Bug animation pointer tables against the cartridge and verifies normal/strong initialization and selector handoffs without runtime table reads.</summary>
    /// <param name="rom">Address space containing the native animation pointer tables.</param>
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

    /// <summary>Reads one little-endian instruction-list pointer from a Pipe Bug animation table.</summary>
    /// <param name="bus">Address space containing the table.</param>
    /// <param name="address">Address of the pointer's low byte.</param>
    /// <returns>The adjacent bytes combined into a 16-bit pointer.</returns>
    private static ushort ReadPipeBugAnimationWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Address-space wrapper that rejects gameplay reads from the migrated normal and strong Pipe Bug animation tables.</summary>
    /// <param name="source">Underlying address space used for reads outside the tables and for all writes.</param>
    private sealed class PipeBugAnimationReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge reads through the animation-table guard.</summary>
        /// <param name="address">Full SNES address requested by the caller.</param>
        /// <returns>The wrapped byte when the address is outside the protected tables.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects normal or strong animation-table reads and forwards other byte reads to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to read.</param>
        /// <returns>The wrapped byte when the address is outside the protected tables.</returns>
        public byte ReadByte(int address) =>
            address is >= 0xb3882b and < 0xb3883b
                ? throw new InvalidOperationException(
                    $"Pipe Bug attempted migrated animation read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
