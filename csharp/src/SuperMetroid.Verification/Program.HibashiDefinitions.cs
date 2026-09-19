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
            new HibashiDefinitionReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeHibashi", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var applyFrame = typeof(RoomEnemySystem).GetMethod("ApplyHibashiActivityFrame", flags)!
            .CreateDelegate<Action<RoomEnemySlot, int>>(enemies);

        RoomEnemySlot graphics = enemies.Slots[0];
        graphics.Parameter2 = 0;
        graphics.YPosition = 0x0200;
        initialize(graphics);
        RoomEnemySlot hitbox = enemies.Slots[1];
        hitbox.Parameter2 = 1;
        hitbox.YPosition = graphics.YPosition;
        initialize(hitbox);

        for (int frameIndex = 0; frameIndex < 22; frameIndex++)
        {
            HibashiActivityDefinition frame = HibashiDefinitions.ActivityFrame(frameIndex);
            AssertEqual(ReadHibashiWord(rom, yOffsetTable + frameIndex * 2), frame.YOffset,
                $"Hibashi Y offset {frameIndex}");
            AssertEqual(ReadHibashiWord(rom, yRadiusTable + frameIndex * 2), frame.YRadius,
                $"Hibashi Y radius {frameIndex}");

            hitbox.XRadius = 0;
            applyFrame(graphics, frameIndex);
            AssertEqual(unchecked((ushort)(0x0200 - frame.YOffset)), hitbox.YPosition,
                $"Hibashi production Y position {frameIndex}");
            AssertEqual(frame.YRadius, hitbox.YRadius,
                $"Hibashi production Y radius {frameIndex}");
            AssertEqual(frameIndex, enemies.LastHibashiActivityFrameIndex!.Value,
                $"Hibashi production frame publication {frameIndex}");
            AssertEqual(frameIndex == 0 ? (ushort)8 : (ushort)0, hitbox.XRadius,
                $"Hibashi production X radius {frameIndex}");
        }

        AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(-1),
            "Hibashi negative activity index");
        AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(22),
            "Hibashi activity index beyond authored table");
        AssertThrows<ArgumentOutOfRangeException>(() => applyFrame(graphics, 22),
            "Hibashi production activity index beyond authored table");

        for (int index = 0;
             index < HibashiInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            HibashiInstructionMechanicsWord definition =
                HibashiInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadHibashiWord(rom, 0xa60000 | definition.Address),
                $"Hibashi instruction mechanics word $A6:{definition.Address:X4}");
        }

        var programGuard = new HibashiProgramReadGuard(rom);
        var programSystem = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            programSystem,
            programGuard);
        var initializeProgram = typeof(RoomEnemySystem).GetMethod("InitializeHibashi", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(programSystem);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot programGraphics = programSystem.Slots[0];
        programGraphics.EnemyDefinitionPointer = HibashiDefinitions.EnemyDefinition;
        programGraphics.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        programGraphics.Parameter2 = 0;
        programGraphics.XPosition = 0x0180;
        programGraphics.YPosition = 0x0200;
        initializeProgram(programGraphics);

        RoomEnemySlot programHitbox = programSystem.Slots[1];
        programHitbox.EnemyDefinitionPointer = HibashiDefinitions.EnemyDefinition;
        programHitbox.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        programHitbox.Parameter2 = 1;
        programHitbox.XPosition = programGraphics.XPosition;
        programHitbox.YPosition = programGraphics.YPosition;
        initializeProgram(programHitbox);

        object?[] graphicsArguments =
            [programGraphics, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        object?[] hitboxArguments =
            [programHitbox, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        // The visible stream needs 59 actor frames through its final callback. The margin
        // proves both actors remain parked on their terminal sleep commands afterward.
        for (int frame = 0; frame < 70; frame++)
        {
            process.Invoke(programSystem, graphicsArguments);
            process.Invoke(programSystem, hitboxArguments);
        }

        AssertEqual(
            HibashiInstructionProgramDefinitions.PresentationWordCount,
            programGuard.ObservedPresentationWords.Count,
            "all live Hibashi spritemap words remain cartridge reads");
        for (int index = 0;
             index < HibashiInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                HibashiInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(programGuard.ObservedPresentationWords.Contains(address),
                $"production execution reads Hibashi presentation word $A6:{address:X4}");
        }
        AssertEqual(0, programGuard.ForbiddenReadAttempts,
            "production execution avoids every compiled Hibashi mechanics byte");
        AssertEqual(HibashiDefinitions.EruptionSoundEffect,
            programSystem.LastHibashiSoundEffect!.Value,
            "Hibashi graphics program queues eruption sound");
        AssertEqual(21, programSystem.LastHibashiActivityFrameIndex!.Value,
            "Hibashi graphics program executes final activity callback");
        AssertEqual((ushort)1, programSystem.HibashiStates[0]!.FinishedActivityFlag,
            "Hibashi graphics program publishes finished activity");
        AssertEqual((ushort)0, programHitbox.XRadius,
            "Hibashi finished program clears hitbox X radius");
        AssertEqual((ushort)0, programHitbox.YRadius,
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
            "pass with mechanics reads forbidden; 24 spritemap words remain live.");
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

    private sealed class HibashiDefinitionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa68dbb and < 0xa68e13
            ? throw new InvalidOperationException(
                $"Hibashi attempted migrated hitbox read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class HibashiProgramReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (HibashiInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Hibashi mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < HibashiInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        HibashiInstructionProgramDefinitions.PresentationWordAddress(index);
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
