using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Grounded loop programs exercised to confirm their compiled timing and repeat behavior.</summary>
    private static readonly ushort[] PuyoGroundedInstructionPrograms =
    [
        PuyoInstructionProgramDefinitions.GroundedFast,
        PuyoInstructionProgramDefinitions.GroundedMedium,
        PuyoInstructionProgramDefinitions.GroundedSlow,
    ];

    /// <summary>Airborne pose programs exercised to confirm each frame reaches its terminal sleep instruction.</summary>
    private static readonly ushort[] PuyoAirborneInstructionPrograms =
    [
        PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4,
        PuyoInstructionProgramDefinitions.RightFrame1LeftFrame3,
        PuyoInstructionProgramDefinitions.Frame2,
        PuyoInstructionProgramDefinitions.RightFrame3LeftFrame1,
        PuyoInstructionProgramDefinitions.RightFrame4LeftFrame0,
    ];

    /// <summary>Loads the retail ROM and verifies Puyo instruction definitions against production execution.</summary>
    private static void VerifyPuyoInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics words and runs grounded and airborne programs with source reads guarded.</summary>
    /// <param name="rom">Retail address space used to compare native instruction words.</param>
    private static void VerifyPuyoInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PuyoInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PuyoInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPuyoInstructionWord(rom, definition.Address),
                $"Puyo mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new PuyoInstructionReadGuard(rom, forbidPresentation: true);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializePuyo", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot puyo = enemies.Slots[0];
        puyo.EnemyDefinitionPointer = RoomEnemySystem.PuyoDefinition;
        puyo.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(puyo);
        AssertEqual(PuyoInstructionProgramDefinitions.GroundedFast,
            puyo.CurrentInstruction,
            "real Puyo initializer installs compiled fast grounded program");

        object?[] processArguments =
            [puyo, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        foreach (ushort program in PuyoGroundedInstructionPrograms)
        {
            puyo.CurrentInstruction = program;
            ExecutePuyoInstructionCalls(enemies, process, processArguments, puyo, 5);
            AssertEqual(unchecked((ushort)(program + 4)),
                puyo.CurrentInstruction,
                $"Puyo grounded program $A2:{program:X4} completes goto and repeats");
        }

        foreach (ushort program in PuyoAirborneInstructionPrograms)
        {
            puyo.CurrentInstruction = program;
            ExecutePuyoInstructionCalls(enemies, process, processArguments, puyo, 2);
            AssertEqual(unchecked((ushort)(program + 4)),
                puyo.CurrentInstruction,
                $"Puyo airborne pose $A2:{program:X4} reaches terminal sleep");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "all Puyo spritemap operands select compiled presentation frames");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Puyo mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                PuyoInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Puyo spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                PuyoInstructionProgramDefinitions.FirstAdjacentDefinition),
            "adjacent Puyo hop definitions are rejected as instruction mechanics");

        _ = ProbePuyoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePuyoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Puyo allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Puyo mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Puyo instruction mechanics: 28 compiled words, all three grounded loops, " +
            "all five airborne poses, and 17 compiled visual selectors pass with mechanics " +
            "bytes forbidden.");
    }

    /// <summary>Invokes the production instruction processor a fixed number of times for one slot.</summary>
    /// <param name="enemies">System instance that owns the slot and instruction processor state.</param>
    /// <param name="process">Reflected instruction-processing method to invoke.</param>
    /// <param name="arguments">Argument array passed to the reflected processor on every call.</param>
    /// <param name="slot">Slot whose instruction timer is made ready before each invocation.</param>
    /// <param name="calls">Number of processor invocations to perform.</param>
    private static void ExecutePuyoInstructionCalls(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        int calls)
    {
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Repeats mechanics lookups so the caller can check warmed lookup allocation behavior.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbePuyoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PuyoInstructionProgramDefinitions.GroundedFast
                    : PuyoInstructionProgramDefinitions.LastSleepOpcode);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from bank $A2.</summary>
    /// <param name="source">Address space supplying the two instruction bytes.</param>
    /// <param name="address">Bank-relative address of the word's low byte.</param>
    /// <returns>The adjacent bytes combined into a 16-bit instruction word.</returns>
    private static ushort ReadPuyoInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Wraps cartridge reads to detect Puyo presentation access and reject reads of compiled mechanics data.</summary>
    /// <param name="source">Underlying address space used for permitted reads and writes.</param>
    /// <param name="forbidPresentation">Whether presentation-word reads should throw instead of being recorded.</param>
    private sealed class PuyoInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Base addresses of presentation words observed by the guard when observation mode is enabled.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempts to read mechanics bytes that production should obtain from compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes import-source reads through the same checks as ordinary address-space reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying source byte when the guarded access is allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, optionally rejects presentation reads, and forwards other addresses.</summary>
        /// <param name="address">Absolute cartridge address to read.</param>
        /// <returns>The underlying source byte for a permitted address.</returns>
        /// <exception cref="InvalidOperationException">The address is compiled mechanics data or forbidden presentation data.</exception>
        public byte ReadByte(int address)
        {
            if (PuyoInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Puyo mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < PuyoInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        PuyoInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Puyo read cartridge visual selector $A2:{presentation:X4}.");
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a cartridge write to the wrapped address space.</summary>
        /// <param name="address">Absolute cartridge address to update.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
