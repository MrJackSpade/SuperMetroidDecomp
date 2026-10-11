using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHibashiDefinitions(SuperMetroidAddressSpace rom)
    {
        const int yOffsetTable = 0xa68dbb;
        const int yRadiusTable = 0xa68de7;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            NewHibashiDefinitionGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeHibashi", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var applyFrame = typeof(RoomEnemySystem).GetMethod("ApplyHibashiActivityFrame", flags)!
            .CreateDelegate<Action<RoomEnemySlot, int>>(enemies);

        RoomEnemySlot graphics = enemies.Slots[0];
        graphics.Parameter2 = 0;
        graphics.Position = graphics.Position.WithWholeY(0x0200);
        initialize(graphics);
        RoomEnemySlot hitbox = enemies.Slots[1];
        hitbox.Parameter2 = 1;
        hitbox.Position = hitbox.Position.WithWholeY(graphics.Position.Y.Whole);
        initialize(hitbox);

        for (int frameIndex = 0; frameIndex < 22; frameIndex++)
        {
            HibashiActivityDefinition frame = HibashiDefinitions.ActivityFrame(frameIndex);
            AssertEqual(ReadHibashiWord(rom, yOffsetTable + frameIndex * 2), frame.YOffset,
                $"Hibashi Y offset {frameIndex}");
            AssertEqual(ReadHibashiWord(rom, yRadiusTable + frameIndex * 2), frame.YRadius,
                $"Hibashi Y radius {frameIndex}");

            hitbox.Radius = hitbox.Radius.WithX(0);
            applyFrame(graphics, frameIndex);
            AssertEqual(unchecked((ushort)(0x0200 - frame.YOffset)), hitbox.Position.Y.Whole,
                $"Hibashi production Y position {frameIndex}");
            AssertEqual(frame.YRadius, hitbox.Radius.Y,
                $"Hibashi production Y radius {frameIndex}");
            AssertEqual(frameIndex, enemies.LastHibashiActivityFrameIndex!.Value,
                $"Hibashi production frame publication {frameIndex}");
            AssertEqual(frameIndex == 0 ? (ushort)8 : (ushort)0, hitbox.Radius.X,
                $"Hibashi production X radius {frameIndex}");
        }

        AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(-1),
            "Hibashi negative activity index");
        AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(22),
            "Hibashi activity index beyond authored table");
        AssertThrows<ArgumentOutOfRangeException>(() => applyFrame(graphics, 22),
            "Hibashi production activity index beyond authored table");

        for (int index = 0;
             index < HibashiInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                HibashiInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadHibashiWord(rom, 0xa60000 | definition.Address),
                $"Hibashi instruction mechanics word $A6:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var programGuard = NewHibashiProgramGuard(rom);
        var programSystem = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            programSystem,
            programGuard);
        var initializeProgram = typeof(RoomEnemySystem).GetMethod("InitializeHibashi", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(programSystem);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot programGraphics = programSystem.Slots[0];
        programGraphics.EnemyDefinitionPointer = EnemyDefinitionId.Hibashi;
        programGraphics.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        programGraphics.Parameter2 = 0;
        programGraphics.Position = programGraphics.Position.WithWhole(new(0x0180, 0x0200));
        initializeProgram(programGraphics);

        RoomEnemySlot programHitbox = programSystem.Slots[1];
        programHitbox.EnemyDefinitionPointer = EnemyDefinitionId.Hibashi;
        programHitbox.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        programHitbox.Parameter2 = 1;
        programHitbox.Position = programHitbox.Position.WithWhole(programGraphics.Position.Whole);
        initializeProgram(programHitbox);

        object?[] graphicsArguments =
            [programGraphics, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        object?[] hitboxArguments =
            [programHitbox, null, null, Position16<RoomPixels>.Zero, (ushort)0];

        // The visible stream needs 59 actor frames through its final callback. The margin
        // proves both actors remain parked on their terminal sleep commands afterward.
        for (int frame = 0; frame < 70; frame++)
        {
            ushort previousGraphicsSprite = programGraphics.SpritemapPointer;
            process.Invoke(programSystem, graphicsArguments);
            if (programGraphics.CurrentInstruction == 0x8da7)
                AssertEqual(previousGraphicsSprite, programGraphics.SpritemapPointer,
                    "Hibashi terminal callback retains the last graphics sprite");
            else
                VerifyExecutedEnemySelector(rom, programGraphics, executedOperands);
            process.Invoke(programSystem, hitboxArguments);
            VerifyExecutedEnemySelector(rom, programHitbox, executedOperands);
        }

        AssertEqual(
            HibashiInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "all Hibashi selectors execute and match cartridge operands");
        for (int index = 0;
             index < HibashiInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                HibashiInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution covers Hibashi presentation word $A6:{address:X4}");
        }
        AssertEqual(0, programGuard.ObservedWords.Count,
            "compiled presentation selectors require no runtime ROM reads");
        AssertEqual(0, programGuard.ForbiddenReadAttempts,
            "production execution avoids every compiled Hibashi mechanics byte");
        AssertEqual(HibashiDefinitions.EruptionSoundEffect,
            programSystem.LastHibashiSoundEffect!.Value,
            "Hibashi graphics program queues eruption sound");
        AssertEqual(21, programSystem.LastHibashiActivityFrameIndex!.Value,
            "Hibashi graphics program executes final activity callback");
        AssertEqual((ushort)1, programSystem.HibashiStates[0]!.FinishedActivityFlag,
            "Hibashi graphics program publishes finished activity");
        AssertEqual((ushort)0, programHitbox.Radius.X,
            "Hibashi finished program clears hitbox X radius");
        AssertEqual((ushort)0, programHitbox.Radius.Y,
            "Hibashi finished program clears hitbox Y radius");
        AssertTrue(programGraphics.Properties.HasAny(EnemyProperties.Invisible),
            "Hibashi finished program hides graphics actor");
        AssertTrue(programHitbox.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
            "Hibashi finished program disables collision actor");
        AssertEqual((ushort)0x8da7, programGraphics.CurrentInstruction,
            "Hibashi graphics program sleeps at terminal command");
        AssertEqual((ushort)0x8dad, programHitbox.CurrentInstruction,
            "Hibashi hitbox program sleeps at terminal command");

        AssertThrows<InvalidDataException>(
            () => HibashiInstructionProgramDefinitions.ReadMechanicsWord(0x8d1f),
            "interleaved Hibashi spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => HibashiInstructionProgramDefinitions.ReadMechanicsWord(0x8e13),
            "Hibashi callback code is rejected as instruction-stream mechanics");

        _ = ProbeHibashiInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeHibashiInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Hibashi allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Hibashi mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Hibashi definitions: 44 physical words, all 22 real activity-frame " +
            "hitboxes, 50 compiled instruction words, and both production programs " +
            "pass with runtime ROM reads forbidden; 24 executed selectors match the cartridge.");
    }

    private static int ProbeHibashiInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += HibashiInstructionProgramDefinitions.ReadMechanicsWord(
                HibashiInstructionProgramDefinitions.GraphicsProgram);
        }
        return checksum;
    }

    private static ushort ReadHibashiWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static CartridgeReadGuard NewHibashiDefinitionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(0xa68dbb, 0xa68e13 - 0xa68dbb,
            address => $"Hibashi attempted migrated hitbox read ${address:X6}.");

    private static CartridgeReadGuard NewHibashiProgramGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(HibashiInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Hibashi mechanics byte ${address:X6}.")
            .ObserveWords(0xa60000, HibashiInstructionProgramDefinitions.PresentationWordCount, HibashiInstructionProgramDefinitions.PresentationWordAddress);
}
