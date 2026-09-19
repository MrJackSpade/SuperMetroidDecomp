using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
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
            SparkInstructionMechanicsWord definition =
                SparkInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadSparkProgramWord(rom, definition.Address),
                $"Spark mechanics word $A8:{definition.Address:X4}");
        }

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
                : (ushort)EnemyProperties.None;
            object?[] arguments =
                [programSlot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

            for (int frame = 0; frame < frames; frame++)
                process.Invoke(programSystem, arguments);

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
            programGuard.ObservedPresentationWords.Count,
            "all live Spark spritemap words remain cartridge reads");
        for (int index = 0;
             index < SparkInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                SparkInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(programGuard.ObservedPresentationWords.Contains(address),
                $"production execution reads Spark presentation word $A8:{address:X4}");
        }
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
            "pass with mechanics reads forbidden; 26 spritemap words remain live.");
    }

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

    private static ushort ReadSparkProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class SparkMovementReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa8e682 and < 0xa8e690
            ? throw new InvalidOperationException(
                $"Spark initializer attempted migrated selector read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class SparkProgramReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (SparkInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
