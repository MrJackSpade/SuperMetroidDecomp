using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Loads the retail ROM and checks compiled Waver mechanics and instruction programs
    /// against cartridge behavior.
    /// </summary>
    private static void VerifyWaverInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWaverInstructionProgramDefinitions), () => VerifyWaverInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Verifies the compiled Waver instruction words, runs each steady and spinning animation
    /// through its completion callback and terminal sleep, and guards against runtime ROM reads.
    /// </summary>
    /// <param name="rom">The retail address space used to compare compiled words and visual selectors.</param>
    private static void VerifyWaverInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < WaverInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                WaverInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadWaverInstructionWord(rom, 0xa30000 | definition.Address),
                $"Waver instruction mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new WaverInstructionProgramReadGuard(rom);
        (WaverAnimationSelector Selector, ushort Terminal, int Frames)[] programs =
        [
            (WaverAnimationSelector.None, 0x86ab, 2),
            (WaverAnimationSelector.FacingRight, 0x86b1, 2),
            (WaverAnimationSelector.Spinning, 0x86c5, 34),
            (WaverAnimationSelector.Spinning | WaverAnimationSelector.FacingRight,
                0x86d9, 34),
        ];

        foreach ((WaverAnimationSelector selector, ushort terminal, int frames) in programs)
        {
            RoomEnemySystem enemies =
                CreateWaverProgramSystem(guard, selector, out RoomEnemySlot slot);
            WaverEnemyState state = enemies.WaverStates[0]!;
            AssertEqual(WaverAnimationDefinitions.InstructionList(selector),
                slot.CurrentInstruction, $"Waver installs {selector} program");
            RunWaverProgram(enemies, slot, frames);
            AssertEqual(terminal, slot.CurrentInstruction,
                $"Waver {selector} program sleeps at native terminal command");
            AssertEqual(selector.HasFlag(WaverAnimationSelector.Spinning),
                state.SpinFinished, $"Waver {selector} spin-completion callback");
        }

        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Waver production programs never read installed visual selectors");
        for (int index = 0;
             index < WaverInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = WaverInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(ReadWaverInstructionWord(rom, 0xa30000 | address),
                EnemySpritemapDefinitions.WaverFrameAt(address),
                $"compiled Waver frame selection $A3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.WaverFrameAt(0x86db),
            "unlisted Waver frame selector fails loudly");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Waver mechanics byte");

        AssertThrows<InvalidDataException>(
            () => WaverInstructionProgramDefinitions.ReadMechanicsWord(0x86a9),
            "interleaved Waver spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WaverInstructionProgramDefinitions.ReadMechanicsWord(0x86db),
            "adjacent Waver selector table is rejected as mechanics");

        _ = ProbeWaverInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWaverInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Waver allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Waver mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Waver instruction mechanics: sixteen compiled words, both steady and both " +
            "spinning programs, completion callbacks, and sleeps pass with all " +
            "ten visual-selector and mechanics source words forbidden.");

        static RoomEnemySystem CreateWaverProgramSystem(
            WaverInstructionProgramReadGuard guard,
            WaverAnimationSelector selector,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeWaver", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var install = typeof(RoomEnemySystem).GetMethod("SetWaverInstructionList", flags)!
                .CreateDelegate<Action<RoomEnemySlot, WaverEnemyState>>();

            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.WaverDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            initialize(slot);
            WaverEnemyState state = enemies.WaverStates[0]!;
            state.CurrentInstructionListIndex = selector == WaverAnimationSelector.None
                ? WaverAnimationSelector.FacingRight
                : WaverAnimationSelector.None;
            state.RequestedInstructionListIndex = selector;
            state.SpinFinished = false;
            install(slot, state);
            return enemies;
        }

        static void RunWaverProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    /// <summary>
    /// Repeatedly resolves the steady-left mechanics word to measure warmed lookup allocations.
    /// </summary>
    /// <returns>A checksum that consumes the values returned by the mechanics lookup.</returns>
    private static int ProbeWaverInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WaverInstructionProgramDefinitions.ReadMechanicsWord(
                WaverInstructionProgramDefinitions.SteadyFacingLeft);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian word from two adjacent cartridge bytes.</summary>
    /// <param name="bus">The address space containing the instruction data.</param>
    /// <param name="address">The absolute address of the word's low byte.</param>
    /// <returns>The word formed from the addressed byte and its successor.</returns>
    private static ushort ReadWaverInstructionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>
    /// Wraps the cartridge bus to reject runtime reads of compiled Waver mechanics and visual
    /// selectors while the real enemy instruction programs execute.
    /// </summary>
    /// <param name="source">The underlying address space used for reads outside the guarded data and for writes.</param>
    private sealed class WaverInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads of installed Waver visual-selector words.</summary>
        internal int ForbiddenPresentationReadAttempts { get; private set; }

        /// <summary>Number of attempted reads of compiled Waver mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte request through the guard's forbidden-read checks.</summary>
        /// <param name="address">The absolute cartridge address to read.</param>
        /// <returns>The underlying byte if the address is not guarded.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics or visual selectors and forwards other reads.</summary>
        /// <param name="address">The absolute address to read.</param>
        /// <returns>The byte supplied by the wrapped address space.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled mechanics or a visual selector.</exception>
        public byte ReadByte(int address)
        {
            if (WaverInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Waver mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < WaverInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        WaverInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenPresentationReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read installed Waver selector ${address:X6}.");
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards the write unchanged to the wrapped address space.</summary>
        /// <param name="address">The absolute address receiving the write.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
