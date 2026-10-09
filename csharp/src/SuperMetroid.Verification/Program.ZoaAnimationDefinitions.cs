using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares the four compiled Zoa animation selectors with ROM and optionally runs handoff checks.</summary>
    /// <param name="rom">Retail address space containing the native selector pointer table.</param>
    /// <param name="definitionsOnly">When true, checks selector definitions without registering runtime handoff cases.</param>
    private static void VerifyZoaAnimationDefinitions(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        for (int index = 0; index < 4; index++)
        {
            var selector = (ZoaAnimationSelector)index;
            AssertEqual(ReadZoaAnimationWord(rom, ZoaAnimationDefinitions.ReferenceAddress + index * 2),
                ZoaAnimationDefinitions.InstructionList(selector),
                $"Zoa animation selector {selector}");
        }

        foreach (ushort invalid in new ushort[] { 4, 5, 0x8000, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() =>
                ZoaAnimationDefinitions.InstructionList((ZoaAnimationSelector)invalid),
                "Zoa animation rejects unsupported flag combinations");
        if (definitionsOnly)
        {
            Console.WriteLine("Zoa animation cases: four original pointers and unsupported flag bounds pass.");
            return;
        }
        Suite(nameof(VerifyAllZoaAnimationHandoffs), () => VerifyAllZoaAnimationHandoffs());
        Suite(nameof(VerifyLiveZoaAnimationHandoffs), () => VerifyLiveZoaAnimationHandoffs(rom, facingRight: false));
        Suite(nameof(VerifyLiveZoaAnimationHandoffs), () => VerifyLiveZoaAnimationHandoffs(rom, facingRight: true));

        AssertThrows<InvalidDataException>(
            () => ZoaAnimationDefinitions.InstructionList((ZoaAnimationSelector)4),
            "Zoa animation selector past table");
        Console.WriteLine(
            "Zoa animation definitions: four native selectors, all four installs, and both complete live facing/rise/shoot handoffs pass with the pointer table forbidden.");
    }

    /// <summary>Checks that each selector, including the no-animation state, installs its matching instruction list.</summary>
    private static void VerifyAllZoaAnimationHandoffs()
    {
        MethodInfo setInstruction = typeof(RoomEnemySystem).GetMethod(
            "SetZoaInstructionList", BindingFlags.Static | BindingFlags.NonPublic)!;
        for (int index = 0; index < 4; index++)
        {
            var selector = (ZoaAnimationSelector)index;
            var slot = NewZoaAnimationSlot();
            var state = new ZoaEnemyState(slot)
            {
                InstructionListTableIndex = selector,
                PreviousInstructionListTableIndex = selector == ZoaAnimationSelector.None
                    ? ZoaAnimationSelector.Rising
                    : ZoaAnimationSelector.None,
            };
            setInstruction.Invoke(null, [slot, state]);
            AssertZoaAnimationHandoff(slot, state, selector, $"Zoa {selector}");
        }
    }

    /// <summary>Exercises the production wait-to-rise and rise-to-shoot transitions for one facing direction.</summary>
    /// <param name="rom">Retail address space wrapped to detect animation-pointer reads during execution.</param>
    /// <param name="facingRight">Whether Samus is placed to the right of Zoa before the live wait callback runs.</param>
    private static void VerifyLiveZoaAnimationHandoffs(
        SuperMetroidAddressSpace rom,
        bool facingRight)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(enemies, new ZoaAnimationReadGuard(rom));
        RoomEnemySlot slot = enemies.Slots[0];
        slot.XPosition = 0x0100;
        slot.YPosition = 0x0080;

        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeZoa", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo wait = typeof(RoomEnemySystem).GetMethod(
            "RunZoaWait", BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo rise = typeof(RoomEnemySystem).GetMethod(
            "RunZoaRising", BindingFlags.Static | BindingFlags.NonPublic)!;
        initialize.Invoke(enemies, [slot]);
        ZoaEnemyState state = enemies.ZoaStates[0]!;

        var samus = new SamusState
        {
            XPosition = unchecked((ushort)(slot.XPosition + (facingRight ? 0x10 : -0x10))),
            YPosition = slot.YPosition,
        };
        wait.Invoke(null, [slot, state, samus]);
        ZoaAnimationSelector direction = facingRight
            ? ZoaAnimationSelector.FacingRight
            : ZoaAnimationSelector.None;
        ZoaAnimationSelector rising = direction | ZoaAnimationSelector.Rising;
        AssertZoaAnimationHandoff(
            slot,
            state,
            rising,
            facingRight ? "live right-facing Zoa rise" : "live left-facing Zoa rise");

        rise.Invoke(null, [slot, state, samus]);
        AssertZoaAnimationHandoff(
            slot,
            state,
            direction,
            facingRight ? "live right-facing Zoa shoot" : "live left-facing Zoa shoot");
    }

    /// <summary>Creates a Zoa slot with nondefault instruction and loop timers for handoff-reset checks.</summary>
    /// <returns>A slot whose timers reveal whether selector installation resets them.</returns>
    private static RoomEnemySlot NewZoaAnimationSlot() => new(0)
    {
        CurrentInstruction = 0x7777,
        InstructionTimer = 0x2222,
        Timer = 0x3333,
    };

    /// <summary>Checks that the requested selector is recorded and its instruction list and timers are installed.</summary>
    /// <param name="slot">Enemy slot whose current instruction and timers are inspected.</param>
    /// <param name="state">Zoa state containing the current and previous selector indices.</param>
    /// <param name="expected">Selector expected to be active after the handoff.</param>
    /// <param name="context">Label included in assertion messages to identify the transition being checked.</param>
    private static void AssertZoaAnimationHandoff(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        ZoaAnimationSelector expected,
        string context)
    {
        AssertEqual(expected, state.InstructionListTableIndex,
            $"{context} requested selector");
        AssertEqual(expected, state.PreviousInstructionListTableIndex,
            $"{context} installed selector");
        AssertEqual(ZoaAnimationDefinitions.InstructionList(expected),
            slot.CurrentInstruction,
            $"{context} instruction");
        AssertEqual(1, slot.InstructionTimer, $"{context} instruction timer");
        AssertEqual(0, slot.Timer, $"{context} loop timer");
    }

    /// <summary>Reads one little-endian selector pointer from the supplied address space.</summary>
    /// <param name="bus">Address space containing the pointer bytes.</param>
    /// <param name="address">Address of the pointer's low byte.</param>
    /// <returns>The low byte followed by the high byte as a 16-bit value.</returns>
    private static ushort ReadZoaAnimationWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>
    /// Wraps cartridge access to reject reads from the migrated Zoa animation-selector
    /// pointer table while forwarding all other memory operations.
    /// </summary>
    /// <param name="source">Underlying address space for reads outside the selector table and for writes.</param>
    private sealed class ZoaAnimationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the same animation-table guard.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The wrapped address space's byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to the migrated Zoa selector table.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects migrated selector-table reads and forwards every other address.</summary>
        /// <param name="address">Address requested by the running game logic.</param>
        /// <returns>The byte returned by the wrapped address space for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address is within the Zoa animation-selector pointer table.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa3b40d and < 0xa3b415
                ? throw new InvalidOperationException(
                    $"Zoa attempted migrated animation-selector read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Address that receives the write.</param>
        /// <param name="value">Byte written at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
