using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and runs the Etecoon instruction-program verification suite.</summary>
    private static void VerifyEtecoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics words and executes reachable Etecoon programs with mechanics reads guarded.</summary>
    /// <param name="rom">Retail address space used to compare instruction and presentation words.</param>
    private static void VerifyEtecoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < EtecoonInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EtecoonInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadEtecoonInstructionWord(rom, definition.Address),
                $"Etecoon mechanics word $A7:{definition.Address:X4}");
        }

        (ushort Program, int Calls, ushort Terminal, string Name)[] programs =
        [
            (EtecoonInstructionProgramDefinitions.LookRightAtSamusAndRunLeft, 2, 0xe822,
                "look right"),
            (EtecoonInstructionProgramDefinitions.BeginRunningLeft, 1, 0xe828,
                "begin running left"),
            (EtecoonInstructionProgramDefinitions.RunningLeft, 5, 0xe82c,
                "running left"),
            (EtecoonInstructionProgramDefinitions.WallJumpLeft, 6, 0xe844,
                "wall jump left"),
            (EtecoonInstructionProgramDefinitions.WallJumpLeftLoop, 5, 0xe844,
                "wall jump left loop"),
            (EtecoonInstructionProgramDefinitions.HoppingFacingLeft, 2, 0xe858,
                "left hop wait"),
            (EtecoonInstructionProgramDefinitions.ContinueHoppingFacingLeft, 6, 0xe86e,
                "left hop continuation"),
            (EtecoonInstructionProgramDefinitions.HitCeiling, 4, 0xe86e,
                "ceiling hit"),
            (EtecoonInstructionProgramDefinitions.WallJumpLeftEligible, 2, 0xe874,
                "left wall-jump eligible"),
            (EtecoonInstructionProgramDefinitions.LookLeftAtSamusAndRunRight, 2, 0xe87a,
                "look left"),
            (EtecoonInstructionProgramDefinitions.BeginRunningRight, 1, 0xe880,
                "begin running right"),
            (EtecoonInstructionProgramDefinitions.RunningRight, 5, 0xe884,
                "running right"),
            (EtecoonInstructionProgramDefinitions.WallJumpRight, 6, 0xe89c,
                "wall jump right"),
            (EtecoonInstructionProgramDefinitions.JumpingRight, 5, 0xe89c,
                "jumping right"),
            (EtecoonInstructionProgramDefinitions.HoppingFacingRight, 2, 0xe8b0,
                "right hop wait"),
            (EtecoonInstructionProgramDefinitions.ContinueHoppingFacingRight, 6, 0xe8c6,
                "right hop continuation"),
            (EtecoonInstructionProgramDefinitions.WallJumpRightEligible, 2, 0xe8cc,
                "right wall-jump eligible"),
            (EtecoonInstructionProgramDefinitions.Initial, 2, 0xe8d2,
                "initial loop"),
            (EtecoonInstructionProgramDefinitions.Flexing, 27, 0xe8fe,
                "four-cycle flex"),
        ];

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var executedOperands = new HashSet<ushort>();
        var guard = new EtecoonInstructionReadGuard(rom);
        foreach (var program in programs)
        {
            var enemies = new RoomEnemySystem();
            Type type = typeof(RoomEnemySystem);
            type.GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = type.GetMethod("InitializeEtecoon", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;
            RoomEnemySlot etecoon = enemies.Slots[0];
            etecoon.EnemyDefinitionPointer = RoomEnemySystem.EtecoonDefinition;
            etecoon.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            initialize(etecoon);
            AssertEqual(EtecoonInstructionProgramDefinitions.Initial,
                etecoon.CurrentInstruction,
                $"real Etecoon initializer before {program.Name}");

            ExecuteEtecoonProgram(rom, executedOperands,
                enemies, process, etecoon, program.Program, program.Calls);
            AssertEqual(program.Terminal, etecoon.CurrentInstruction,
                $"Etecoon {program.Name} terminal instruction");
            if (program.Program == EtecoonInstructionProgramDefinitions.Flexing)
                AssertEqual((ushort)0, etecoon.Timer, "Etecoon flex loop count expires");
        }

        AssertEqual(EtecoonInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int operandIndex = 0; operandIndex < EtecoonInstructionProgramDefinitions.PresentationWordCount; operandIndex++)
            AssertTrue(executedOperands.Contains(EtecoonInstructionProgramDefinitions.PresentationWordAddress(operandIndex)),
                "execution covers each authored presentation operand");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Etecoon mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EtecoonInstructionProgramDefinitions.ReadMechanicsWord(
                EtecoonInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Etecoon spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EtecoonInstructionProgramDefinitions.ReadMechanicsWord(
                EtecoonInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Etecoon movement data is rejected as instruction mechanics");

        _ = ProbeEtecoonInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEtecoonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Etecoon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Etecoon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"Etecoon instruction mechanics: " +
            $"{EtecoonInstructionProgramDefinitionsTooling.MechanicsWordCount} compiled words, " +
            "all overlapping entries and the four-cycle flex program, and forty-five executed " +
            "spritemap selectors match cartridge data with runtime reads forbidden.");
    }

    /// <summary>Starts one Etecoon instruction list and advances it for the requested number of calls.</summary>
    /// <param name="rom">Address space used to verify the presentation selector installed after each call.</param>
    /// <param name="executedOperands">Set receiving each selector observed during execution.</param>
    /// <param name="enemies">Room system whose instruction processor is invoked.</param>
    /// <param name="process">Reflected production instruction-processing method.</param>
    /// <param name="etecoon">Initialized Etecoon slot to run.</param>
    /// <param name="program">Native instruction-list entry point for this run.</param>
    /// <param name="callCount">Number of instruction-processing calls to perform.</param>
    private static void ExecuteEtecoonProgram(
        ISnesAddressSpace rom,
        HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot etecoon,
        ushort program,
        int callCount)
    {
        etecoon.CurrentInstruction = program;
        object?[] arguments =
            [etecoon, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            etecoon.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            VerifyExecutedEnemySelector(rom, etecoon, executedOperands);
        }
    }

    /// <summary>Repeats compiled mechanics lookups so the caller can measure warmed lookup allocations.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbeEtecoonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EtecoonInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EtecoonInstructionProgramDefinitions.Initial
                    : (ushort)0xe8fe);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from bank $A7.</summary>
    /// <param name="source">Address space supplying the two instruction bytes.</param>
    /// <param name="address">Bank-relative address of the word's low byte.</param>
    /// <returns>The adjacent bytes combined into a 16-bit word.</returns>
    private static ushort ReadEtecoonInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects reads from compiled Etecoon mechanics while recording native presentation-word accesses.</summary>
    /// <param name="source">Underlying address space used for permitted cartridge reads and writes.</param>
    private sealed class EtecoonInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation-word base addresses observed through this guarded address space.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempts to read mechanics bytes that production should obtain from compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the same mechanics guard and presentation observer.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the guarded read is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation operands, and forwards other addresses.</summary>
        /// <param name="address">Absolute cartridge address to read.</param>
        /// <returns>The source byte for a permitted address.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Etecoon mechanics data.</exception>
        public byte ReadByte(int address)
        {
            if (EtecoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Etecoon mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EtecoonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        EtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
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
