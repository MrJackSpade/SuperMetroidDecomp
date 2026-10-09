using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the retail-ROM-backed checks for compiled normal and strong Brinstar Pipe Bug instruction programs.</summary>
    private static void VerifyBrinstarPipeBugInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBrinstarPipeBugInstructionProgramDefinitions), () => VerifyBrinstarPipeBugInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with cartridge words and drives each normal/strong animation loop through production initialization.</summary>
    /// <param name="rom">Retail address space used to check instruction and visual operand values.</param>
    private static void VerifyBrinstarPipeBugInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < BrinstarPipeBugInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                BrinstarPipeBugInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadBrinstarPipeBugInstructionWord(rom, 0xb30000 | definition.Address),
                $"Brinstar Pipe Bug instruction mechanics word $B3:{definition.Address:X4}");
        }

        var guard = new BrinstarPipeBugInstructionProgramReadGuard(rom);
        (PipeBugAnimationSelector Selector, ushort NormalCursor, int NormalFrames,
            ushort StrongCursor, int StrongFrames)[] programs =
        [
            (PipeBugAnimationSelector.None, 0x87af, 17, 0x8a21, 7),
            (PipeBugAnimationSelector.Shooting, 0x87d3, 7, 0x8a35, 13),
            (PipeBugAnimationSelector.FacingRight, 0x87ef, 17, 0x8a49, 7),
            (PipeBugAnimationSelector.FacingRight | PipeBugAnimationSelector.Shooting,
                0x8813, 7, 0x8a5d, 13),
        ];

        foreach (var program in programs)
        {
            VerifyProgram(strong: false, program.Selector, program.NormalCursor,
                program.NormalFrames);
            VerifyProgram(strong: true, program.Selector, program.StrongCursor,
                program.StrongFrames);
        }

        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Brinstar Pipe Bug programs never read installed visual selectors");
        for (int index = 0;
             index < BrinstarPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                BrinstarPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            ushort definition = address < 0x8a00
                ? PipeBugDefinitions.BrinstarEnemyDefinition
                : PipeBugDefinitions.StrongBrinstarEnemyDefinition;
            AssertEqual(ReadBrinstarPipeBugInstructionWord(rom, 0xb30000 | address),
                PipeBugVisualDefinitions.FrameAt(definition, address),
                $"compiled Brinstar Pipe Bug frame $B3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => PipeBugVisualDefinitions.FrameAt(
                PipeBugDefinitions.BrinstarEnemyDefinition, 0x882b),
            "unlisted Brinstar Pipe Bug visual operand fails loudly");
        AssertThrows<InvalidDataException>(
            () => PipeBugVisualDefinitions.FrameAt(
                PipeBugDefinitions.BrinstarEnemyDefinition, 0x8a1f),
            "normal Brinstar Pipe Bug rejects a strong visual operand");
        AssertThrows<InvalidDataException>(
            () => PipeBugVisualDefinitions.FrameAt(
                PipeBugDefinitions.StrongBrinstarEnemyDefinition, 0x87ad),
            "strong Brinstar Pipe Bug rejects a normal visual operand");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Brinstar Pipe Bug mechanics byte");

        AssertThrows<InvalidDataException>(
            () => BrinstarPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x87ad),
            "interleaved Pipe Bug spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BrinstarPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x882b),
            "adjacent Pipe Bug selector table is rejected as mechanics");

        _ = ProbeBrinstarPipeBugInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBrinstarPipeBugInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Pipe Bug allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Pipe Bug mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Brinstar Pipe Bug instruction mechanics: sixty compiled words, all eight " +
            "normal/strong programs and complete loops pass with forty-four visual " +
            "selectors and mechanics source words forbidden.");

        void VerifyProgram(
            bool strong,
            PipeBugAnimationSelector selector,
            ushort expectedCursor,
            int frames)
        {
            RoomEnemySystem enemies = CreateProgramSystem(strong, selector, out RoomEnemySlot slot);
            AssertEqual(PipeBugDefinitions.BrinstarInstructionList(strong, selector),
                slot.CurrentInstruction,
                $"{(strong ? "strong" : "normal")} Pipe Bug installs {selector}");
            RunProgram(enemies, slot, frames);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"{(strong ? "strong" : "normal")} Pipe Bug {selector} loops");
        }

        RoomEnemySystem CreateProgramSystem(
            bool strong,
            PipeBugAnimationSelector selector,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeBrinstarPipeBug", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo install = typeof(RoomEnemySystem).GetMethod(
                "SelectBrinstarPipeBugAnimation", flags)!;
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = strong
                ? PipeBugDefinitions.StrongBrinstarEnemyDefinition
                : PipeBugDefinitions.BrinstarEnemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
            slot.Parameter1 = strong ? (ushort)1 : (ushort)0;
            initialize(slot);
            PipeBugEnemyState state = enemies.PipeBugStates[0]!;
            state.InstalledAnimationState = selector == PipeBugAnimationSelector.None
                ? PipeBugAnimationSelector.Shooting
                : PipeBugAnimationSelector.None;
            state.AnimationState = selector;
            install.Invoke(null, [slot, state]);
            return enemies;
        }

        static void RunProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Repeats a normal Pipe Bug mechanics lookup for the warmed allocation check.</summary>
    /// <returns>A checksum that consumes the resolved word values.</returns>
    private static int ProbeBrinstarPipeBugInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BrinstarPipeBugInstructionProgramDefinitions.ReadMechanicsWord(
                BrinstarPipeBugInstructionProgramDefinitions.NormalRisingLeft);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from the supplied retail address space.</summary>
    /// <param name="bus">Retail address space containing the reference bytes.</param>
    /// <param name="address">SNES address of the word's low byte.</param>
    /// <returns>The two adjacent bytes combined into a 16-bit value.</returns>
    private static ushort ReadBrinstarPipeBugInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects production reads from compiled Pipe Bug mechanics and installed presentation selectors.</summary>
    /// <param name="source">Underlying address space used for permitted reads and forwarded writes.</param>
    private sealed class BrinstarPipeBugInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads rejected for targeting a compiled presentation word.</summary>
        internal int ForbiddenPresentationReadAttempts { get; private set; }

        /// <summary>Number of attempted reads rejected for targeting a compiled mechanics byte.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge reads through the mechanics and presentation checks.</summary>
        /// <param name="address">SNES cartridge address requested by the caller.</param>
        /// <returns>The underlying byte when the address is outside both compiled regions.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from compiled mechanics and presentation data before delegating other reads.</summary>
        /// <param name="address">SNES address requested by the caller.</param>
        /// <returns>The wrapped address-space byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a compiled mechanics or presentation word.</exception>
        public byte ReadByte(int address)
        {
            if (BrinstarPipeBugInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Pipe Bug mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BrinstarPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BrinstarPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenPresentationReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read installed Brinstar Pipe Bug selector ${address:X6}.");
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged because this verification guard restricts reads only.</summary>
        /// <param name="address">SNES address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
