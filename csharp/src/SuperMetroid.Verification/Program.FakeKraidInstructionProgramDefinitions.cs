using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using static FakeKraidProgramMappingsVerification;
using static KraidVisualSelectorsVerification;
using static SuiteTimingVerification;

internal static class FakeKraidInstructionProgramDefinitionsVerification
{
    internal static void VerifyFakeKraidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyFakeKraidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        int population = EnemyRomTablePointers.Kraid.FakeKraidPopulationRecord;
        EnemyDefinitionId populationDefinition = EnemyDefinitionIds.FromHeaderPointer((ushort)(SnesLittleEndian.Word(FakeKraidInstructionProgramDefinitionsRomByte(rom, population), FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 1))));
        ushort extraProperties = (ushort)(SnesLittleEndian.Word(FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 10), FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 11)));
        AssertEqual(EnemyDefinitionId.MiniKraid, populationDefinition,
            "retail Fake Kraid population matches the installed visual family");
        AssertTrue((extraProperties &
                (ushort)EnemyExtraProperties.UsesExtendedSpritemap) == 0,
            "retail Fake Kraid uses ordinary OAM composition");

        Suite(nameof(VerifyFakeKraidMechanicsMapping), () => VerifyFakeKraidMechanicsMapping(rom));
        Suite(nameof(VerifyFakeKraidPresentationAddresses), () => VerifyFakeKraidPresentationAddresses());

        int forbiddenPresentationReadAttempts = 0;
        CartridgeReadGuard guard = new CartridgeReadGuard(rom)
            .Forbid(FakeKraidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Fake Kraid mechanics byte ${address:X6}.")
            .Observe(IsFakeKraidSelectorByte, _ => forbiddenPresentationReadAttempts++)
            .Forbid(IsFakeKraidSelectorByte,
                address => $"Production read installed Fake Kraid selector ${address:X6}.");
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]), new byte[8]);

        Suite(nameof(VerifyInitializer), () => VerifyInitializer(movingRight: false));
        Suite(nameof(VerifyInitializer), () => VerifyInitializer(movingRight: true));

        ExerciseWalk(
            FakeKraidInstructionProgramDefinitions.StepForwardsFacingLeft,
            movingRight: false,
            walkDelta: -4);
        ExerciseWalk(
            FakeKraidInstructionProgramDefinitions.StepBackwardsFacingLeft,
            movingRight: false,
            walkDelta: 4);
        ExerciseWalk(
            FakeKraidInstructionProgramDefinitions.StepForwardsFacingRight,
            movingRight: true,
            walkDelta: 4);
        ExerciseWalk(
            FakeKraidInstructionProgramDefinitions.StepBackwardsFacingRight,
            movingRight: true,
            walkDelta: -4);
        ExerciseSpit(
            FakeKraidInstructionProgramDefinitions.FireSpitFacingLeft,
            movingRight: false);
        ExerciseSpit(
            FakeKraidInstructionProgramDefinitions.FireSpitFacingRight,
            movingRight: true);

        AssertEqual(0, forbiddenPresentationReadAttempts,
            "Fake Kraid programs never read installed visual selectors");
        Suite(nameof(VerifyFakeKraidVisualSelectors), () => VerifyFakeKraidVisualSelectors(rom));
        AssertThrows<InvalidDataException>(
            () => KraidVisualDefinitions.FrameAt(
                EnemyDefinitionId.MiniKraid, 0x9a42),
            "unlisted Fake Kraid visual operand fails loudly");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Fake Kraid mechanics byte");

        AssertThrows<InvalidDataException>(
            () => FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(0x99b0),
            "interleaved Fake Kraid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(0x99f4),
            "retail-unused Fake Kraid standing program is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(0x9a42),
            "adjacent Fake Kraid standing program is rejected as mechanics");

        _ = ProbeFakeKraidInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFakeKraidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fake Kraid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Fake Kraid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fake Kraid instruction mechanics: forty-eight compiled words, all six live " +
            "programs and four action selectors pass with twenty-four visual selectors " +
            "and mechanics source words forbidden.");

        void VerifyInitializer(bool movingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out _,
                out _);
            AssertEqual(
                movingRight
                    ? FakeKraidInstructionProgramDefinitions.StepForwardsFacingRight
                    : FakeKraidInstructionProgramDefinitions.StepForwardsFacingLeft,
                slot.CurrentInstruction,
                $"Fake Kraid initializer selects {(movingRight ? "right" : "left")} program");
        }

        void ExerciseWalk(ushort program, bool movingRight, short walkDelta)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out SamusState samus,
                out FakeKraidEnemyState state);
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            state.WalkDelta = walkDelta;
            state.FacingDelta = movingRight ? (short)4 : (short)-4;
            state.WalkStepTimer = 0x0100;
            state.SpitDecisionTimer = 0x0100;

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual((ushort)0x00ff, state.WalkStepTimer,
                $"Fake Kraid $A6:{program:X4} executes its walk callback");
            AssertEqual((ushort)0x00ff, state.SpitDecisionTimer,
                $"Fake Kraid $A6:{program:X4} executes its action callback");
        }

        void ExerciseSpit(ushort program, bool movingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out SamusState samus,
                out FakeKraidEnemyState state);
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            state.WalkStepTimer = 0x0100;
            state.SpitDecisionTimer = 0x0100;

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual(2, state.SpawnedSpitCount,
                $"Fake Kraid $A6:{program:X4} spawns its projectile pair");
            AssertEqual((ushort?)0x0016, enemies.FakeKraidBehavior.LastFakeKraidSoundEffect,
                $"Fake Kraid $A6:{program:X4} publishes its cry");
        }

        RoomEnemySystem CreateSystem(
            bool movingRight,
            out RoomEnemySlot slot,
            out SamusState samus,
            out FakeKraidEnemyState state)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0);
            Action<RoomEnemySlot, SamusState?> initialize = enemies.FakeKraidBehavior.InitializeFakeKraid;

            slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.MiniKraid;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(movingRight ? (ushort)0x0140 : (ushort)0x00c0, slot.Position.Y.Whole)),
            };
            initialize(slot, samus);
            state = enemies.FakeKraidStates[0]!;
            return enemies;
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            RoomLevelData level,
            int frames)
        {
            object?[] arguments =
                [slot, samus, level, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for fake Kraid instruction program definitions")]
    private static byte FakeKraidInstructionProgramDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    private static int ProbeFakeKraidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(
                FakeKraidInstructionProgramDefinitions.StepForwardsFacingLeft);
        }
        return checksum;
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for fake Kraid instruction words")]
    internal static ushort ReadFakeKraidInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    private static bool IsFakeKraidSelectorByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < FakeKraidInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            ushort presentation = FakeKraidInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            if (bankAddress == presentation || bankAddress == unchecked((ushort)(presentation + 1)))
                return true;
        }
        return false;
    }

    [VerificationCommand("--fake-kraid-instruction-mechanics")]
    internal static int FakeKraidInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions());
        return 0;
    }
}
