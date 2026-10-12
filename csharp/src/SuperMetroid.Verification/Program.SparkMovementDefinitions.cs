using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for spark movement definitions")]
    private static void VerifySparkMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa8e682;
        const int functionTable = 0xa8e688;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            NewSparkMovementReadGuard(new TestAddressSpace()));
        enemies.VerificationServices().IsAreaBossDefeated = (Func<bool>)(() => true);
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeSpark", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        for (ushort selector = 0; selector < 4; selector++)
        {
            int offset = selector * 2;
            ushort nativeInstruction = CartridgeImportSource.Require(rom).ReadCartridgeWord(instructionTable + offset);
            var nativeFunction = (SparkEnemyFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord(functionTable + offset);
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
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | definition.Address),
                $"Spark mechanics word $A8:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var programGuard = NewSparkProgramReadGuard(rom);
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
            programSlot.EnemyDefinitionPointer = EnemyDefinitionId.Spark;
            programSlot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            programSlot.CurrentInstruction = entry;
            programSlot.InstructionTimer = 1;
            programSlot.Properties = entry == SparkInstructionProgramDefinitions.FlickerOn
                ? (ushort)EnemyProperties.IgnoreSamusCollision
                : (ushort)0;
            object?[] arguments =
                [programSlot, null, null, Position16<RoomPixels>.Zero, (ushort)0];

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
        AssertEqual(0, programGuard.ObservedWords.Count,
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

    private static CartridgeReadGuard NewSparkMovementReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa8e682 and < 0xa8e690,
            address => $"Spark initializer attempted migrated selector read ${address:X6}.");

    private static CartridgeReadGuard NewSparkProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(SparkInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Spark mechanics byte ${address:X6}.")
            .ObserveWords(0xa80000, SparkInstructionProgramDefinitions.PresentationWordCount, SparkInstructionProgramDefinitions.PresentationWordAddress);
}
