using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks compiled Spark initialization values, instruction mechanics, and live presentation against the cartridge.</summary>
    /// <param name="rom">Retail address space used as the reference for native table and program words.</param>
    private static void VerifySparkMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa8e682;
        const int functionTable = 0xa8e688;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new SparkMovementReadGuard(new TestAddressSpace()));
        typeof(RoomEnemySystem).GetField("_isAreaBossDefeated", flags)!.SetValue(
            enemies,
            (Func<bool>)(() => true));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeSpark", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        for (ushort selector = 0; selector < 4; selector++)
        {
            int offset = selector * 2;
            ushort nativeInstruction = (ushort)(rom.ReadByte(instructionTable + offset) |
                rom.ReadByte(instructionTable + offset + 1) << 8);
            var nativeFunction = (SparkEnemyFunction)(rom.ReadByte(functionTable + offset) |
                rom.ReadByte(functionTable + offset + 1) << 8);
            SparkMovementDefinition definition = SparkMovementDefinitions.InitialState(selector);
            AssertEqual(nativeInstruction, definition.InstructionList,
                $"compiled Spark instruction selector {selector}");
            AssertEqual(nativeFunction, definition.Function,
                $"compiled Spark function selector {selector}");

            slot.Parameter1 = selector;
            slot.Parameter2 = 5;
            initialize(slot);
            AssertEqual(nativeInstruction, slot.CurrentInstruction,
                $"Spark production instruction selector {selector}");
            AssertEqual(nativeFunction, enemies.SparkStates[0]!.Function,
                $"Spark production function selector {selector}");
        }

        foreach (ushort highBits in new ushort[] { 4, 0x0100, 0xffff })
        {
            SparkMovementDefinition expected = SparkMovementDefinitions.InitialState(
                (ushort)(highBits & 3));
            AssertEqual(expected, SparkMovementDefinitions.InitialState(highBits),
                $"Spark selector mask {highBits:X4}");
        }

        for (int index = 0;
             index < SparkInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SparkInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadSparkProgramWord(rom, definition.Address),
                $"Spark mechanics word $A8:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var programGuard = new SparkProgramReadGuard(rom);
        (ushort Entry, int Frames)[] programs =
        [
            (SparkInstructionProgramDefinitions.FlickerOn, 35),
            (SparkInstructionProgramDefinitions.Active, 16),
            (SparkInstructionProgramDefinitions.FlickerOut, 12),
            (SparkInstructionProgramDefinitions.Emitter, 16),
        ];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach ((ushort entry, int frames) in programs)
        {
            var programSystem = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                programSystem,
                programGuard);
            RoomEnemySlot programSlot = programSystem.Slots[0];
            programSlot.EnemyDefinitionPointer = RoomEnemySystem.SparkDefinition;
            programSlot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            programSlot.CurrentInstruction = entry;
            programSlot.InstructionTimer = 1;
            programSlot.Properties = entry == SparkInstructionProgramDefinitions.FlickerOn
                ? (ushort)EnemyProperties.IgnoreSamusCollision
                : (ushort)0;
            object?[] arguments =
                [programSlot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

            for (int frame = 0; frame < frames; frame++)
            {
                ushort previousSprite = programSlot.SpritemapPointer;
                process.Invoke(programSystem, arguments);
                if (programSlot.CurrentInstruction == 0xe607)
                    AssertEqual(previousSprite, programSlot.SpritemapPointer,
                        "Spark terminal callback retains the last sprite");
                else
                    VerifyExecutedEnemySelector(rom, programSlot, executedOperands);
            }

            if (entry == SparkInstructionProgramDefinitions.FlickerOn)
            {
                AssertTrue(
                    !programSlot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
                    "Spark flicker-on callback makes actor tangible");
            }
            else if (entry == SparkInstructionProgramDefinitions.FlickerOut)
            {
                AssertTrue(
                    programSlot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
                    "Spark flicker-out callback makes actor intangible");
                AssertEqual((ushort)0xe607, programSlot.CurrentInstruction,
                    "Spark flicker-out sleeps on its terminal command");
            }
        }

        AssertEqual(
            SparkInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "all Spark selectors execute and match cartridge operands");
        for (int index = 0;
             index < SparkInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                SparkInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution covers Spark presentation word $A8:{address:X4}");
        }
        AssertEqual(0, programGuard.ObservedPresentationWords.Count,
            "compiled presentation selectors require no runtime ROM reads");
        AssertEqual(0, programGuard.ForbiddenReadAttempts,
            "production execution avoids every compiled Spark mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SparkInstructionProgramDefinitions.ReadMechanicsWord(0xe5ab),
            "interleaved Spark spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SparkInstructionProgramDefinitions.ReadMechanicsWord(0xe694),
            "selector-three adjacent code word is not treated as an animation program");

        _ = ProbeSparkInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSparkInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Spark allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Spark mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Spark movement definitions: three authored pairs, both selector-three " +
            "overreads, 33 compiled instruction words, and four production programs " +
            "pass with runtime ROM reads forbidden; 26 executed selectors match the cartridge.");
    }

    /// <summary>Repeats a compiled mechanics lookup so the caller can measure warmed allocation behavior.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbeSparkInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SparkInstructionProgramDefinitions.ReadMechanicsWord(
                SparkInstructionProgramDefinitions.Active);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from bank $A8.</summary>
    /// <param name="source">Address space supplying the two instruction bytes.</param>
    /// <param name="address">Bank-relative address of the word's low byte.</param>
    /// <returns>The adjacent bytes combined into a 16-bit word.</returns>
    private static ushort ReadSparkProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Prevents Spark initialization from reading selector values that have been moved into compiled definitions.</summary>
    /// <param name="source">Address space used for all reads outside the migrated selector table.</param>
    private sealed class SparkMovementReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the guard's selector-table check.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The source byte when the address is outside the forbidden selector table.</returns>
        /// <exception cref="InvalidOperationException">The initializer attempts to read a migrated selector address.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated selector table and forwards other addresses.</summary>
        /// <param name="address">Absolute cartridge address to read.</param>
        /// <returns>The underlying source byte for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to the migrated Spark selector table.</exception>
        public byte ReadByte(int address) => address is >= 0xa8e682 and < 0xa8e690
            ? throw new InvalidOperationException(
                $"Spark initializer attempted migrated selector read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards a write to the wrapped address space.</summary>
        /// <param name="address">Absolute cartridge address to update.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>Tracks presentation-word reads and rejects any production read of compiled Spark mechanics bytes.</summary>
    /// <param name="source">Underlying address space for permitted cartridge accesses.</param>
    private sealed class SparkProgramReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation-word base addresses observed during production instruction execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempts to read mechanics bytes that should come from compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the same monitoring and rejection logic.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the guarded read is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects mechanics reads, records presentation operands, and forwards other addresses.</summary>
        /// <param name="address">Absolute cartridge address to read.</param>
        /// <returns>The underlying source byte for a permitted address.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Spark mechanics data.</exception>
        public byte ReadByte(int address)
        {
            if (SparkInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Spark mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SparkInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        SparkInstructionProgramDefinitions.PresentationWordAddress(index);
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

        /// <summary>Forwards a write to the wrapped address space.</summary>
        /// <param name="address">Absolute cartridge address to update.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
