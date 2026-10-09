using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the compiled Maridia Large Snail selectors, mechanics words, and complete instruction programs against retail data and production execution.</summary>
    /// <param name="rom">Retail address space used for native-word comparisons and executed-selector verification.</param>
    private static void VerifyMaridiaLargeSnailInstructionDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo install = typeof(RoomEnemySystem).GetMethod(
            "InstallMaridiaLargeSnailInstructionList",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", instance)!;
        var executedOperands = new HashSet<ushort>();
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

        for (int index = 0;
             index < MaridiaLargeSnailInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MaridiaLargeSnailInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMaridiaLargeSnailInstructionWord(
                    rom, 0xa20000 | definition.Address),
                $"Maridia Large Snail mechanics word $A2:{definition.Address:X4}");
        }

        (ushort Program, int Calls, ushort Terminal, bool FinishesAttack, bool PlaysSplash,
            string Name)[] programs =
        [
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle, 2, 0xca4f,
                false, false, "left idle"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftAttacking, 14, 0xca89,
                true, true, "left attack"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingForwards, 9,
                0xca8f, false, false, "left forward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingBackwards, 9,
                0xcab7, false, false, "left backward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightIdle, 2, 0xcadf,
                false, false, "right idle"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightAttacking, 14, 0xcb19,
                true, true, "right attack"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightRollingForwards, 9,
                0xcb1f, false, false, "right forward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightRollingBackwards, 9,
                0xcb47, false, false, "right backward roll"),
        ];

        foreach (var program in programs)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeMaridiaLargeSnail", instance)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", instance)!;
            RoomEnemySlot snail = enemies.Slots[0];
            snail.EnemyDefinitionPointer = RoomEnemySystem.MaridiaLargeSnailDefinition;
            snail.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(snail);

            AssertEqual(MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle,
                snail.CurrentInstruction,
                $"real Maridia Large Snail initializer before {program.Name}");
            ExecuteMaridiaLargeSnailProgram(rom, executedOperands,
                enemies, process, snail, program.Program, program.Calls);
            AssertEqual(program.Terminal, snail.CurrentInstruction,
                $"Maridia Large Snail {program.Name} terminal instruction");

            MaridiaLargeSnailEnemyState state = enemies.MaridiaLargeSnailStates[0]!;
            AssertEqual(program.FinishesAttack, state.AttackAnimationFinished,
                $"Maridia Large Snail {program.Name} completion flag");
            AssertEqual(program.PlaysSplash ? (ushort?)0x000e : null,
                enemies.LastMaridiaLargeSnailSoundEffect,
                $"Maridia Large Snail {program.Name} splash sound");
            AssertEqual(false, state.AttackAllowsRotation,
                $"Maridia Large Snail {program.Name} leaves rotation disallowed");
        }

        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeMaridiaLargeSnail", instance)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", instance)!;
            RoomEnemySlot snail = enemies.Slots[0];
            snail.EnemyDefinitionPointer = RoomEnemySystem.MaridiaLargeSnailDefinition;
            snail.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(snail);
            ExecuteMaridiaLargeSnailProgram(rom, executedOperands,
                enemies,
                process,
                snail,
                MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingForwards,
                2);
            AssertTrue(enemies.MaridiaLargeSnailStates[0]!.AttackAllowsRotation,
                "Maridia Large Snail forward roll enables its attack window");
            ExecuteMaridiaLargeSnailProgram(rom, executedOperands,
                enemies,
                process,
                snail,
                snail.CurrentInstruction,
                1);
            AssertEqual(false, enemies.MaridiaLargeSnailStates[0]!.AttackAllowsRotation,
                "Maridia Large Snail forward roll closes its attack window");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MaridiaLargeSnailInstructionDefinitions.InstructionPointer(8),
            "Maridia Large Snail selector past table");
        AssertEqual(MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int operandIndex = 0; operandIndex < MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordCount; operandIndex++)
            AssertTrue(executedOperands.Contains(MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordAddress(operandIndex)),
                "execution covers each authored presentation operand");
        AssertEqual(0, guarded.ObservedPresentationWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production execution avoids compiled Maridia Large Snail mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Maridia Large Snail presentation pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                MaridiaLargeSnailInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Maridia Large Snail selector data is rejected as mechanics");

        _ = ProbeMaridiaLargeSnailInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMaridiaLargeSnailInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Maridia Large Snail allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Maridia Large Snail mechanics lookups allocate no per-frame storage");
        Console.WriteLine(
            "Maridia Large Snail instruction mechanics: all eight native selectors, " +
            "eighty-four compiled mechanics words, all eight programs and callback side " +
            "effects, and sixty executed selectors match cartridge data with runtime reads forbidden.");
    }

    /// <summary>Runs the selected Large Snail program through the production instruction processor and records each executed visual selector.</summary>
    /// <param name="rom">Address space used to verify selectors against their native operands.</param>
    /// <param name="executedOperands">Set collecting verified presentation operand addresses.</param>
    /// <param name="enemies">Enemy system whose production processor executes the list.</param>
    /// <param name="process">Reflected production instruction-processing method.</param>
    /// <param name="snail">Large Snail slot whose instruction pointer and timer are advanced.</param>
    /// <param name="program">Instruction-list entry selected for this run.</param>
    /// <param name="callCount">Number of eligible instruction calls to make.</param>
    private static void ExecuteMaridiaLargeSnailProgram(
        ISnesAddressSpace rom,
        HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot snail,
        ushort program,
        int callCount)
    {
        snail.CurrentInstruction = program;
        object?[] arguments =
            [snail, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            snail.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            VerifyExecutedEnemySelector(rom, snail, executedOperands);
        }
    }

    /// <summary>Warms and repeatedly reads compiled Large Snail mechanics words so the caller can measure steady-state lookup allocations.</summary>
    /// <returns>A checksum that keeps the repeated reads observable.</returns>
    private static int ProbeMaridiaLargeSnailInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle
                    : unchecked((ushort)(
                        MaridiaLargeSnailInstructionProgramDefinitions.FacingRightAttacking +
                        0x38)));
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from the supplied cartridge address space.</summary>
    /// <param name="bus">Retail address space containing the original Large Snail instruction data.</param>
    /// <param name="address">Address of the low byte; the high byte follows immediately.</param>
    /// <returns>The adjacent bytes combined as an unsigned 16-bit word.</returns>
    private static ushort ReadMaridiaLargeSnailInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects reads of compiled Large Snail mechanics and selectors while tracking which installed presentation operands production execution requests.</summary>
    /// <param name="source">Underlying address space for reads outside guarded instruction data and for writes.</param>
    private sealed class MaridiaLargeSnailInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Unique presentation operand addresses observed through this guarded address space.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted reads rejected because they target migrated selector or mechanics data.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes import-source reads through the same guard and presentation tracking as ordinary byte reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address is outside guarded Large Snail data.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects migrated mechanics reads, records selector reads, and forwards other byte requests.</summary>
        /// <param name="address">Address-space location requested by production code.</param>
        /// <returns>The underlying byte when the address is outside the guarded instruction ranges.</returns>
        /// <exception cref="InvalidOperationException">The address identifies compiled selector or mechanics data.</exception>
        public byte ReadByte(int address)
        {
            if (address is >= 0xa2cb77 and < 0xa2cb87 ||
                MaridiaLargeSnailInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Maridia Large Snail attempted migrated mechanics read ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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

        /// <summary>Forwards writes unchanged; this wrapper guards cartridge reads only.</summary>
        /// <param name="address">Address-space location to write.</param>
        /// <param name="value">Byte passed through to the underlying source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
