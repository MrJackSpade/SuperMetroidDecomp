using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mini-Kraid's typed programs and their cues through its real initializer, walks and spits.</summary>
internal static class FakeKraidInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge oracle for Fake Kraid instruction verification.</summary>
    internal static void VerifyFakeKraidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the typed programs against the cartridge and exercises both initial facings, four walk programs and two spit programs.</summary>
    internal static void VerifyFakeKraidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {

        int population = EnemyRomTablePointers.Kraid.FakeKraidPopulationRecord;
        EnemyDefinitionId populationDefinition = EnemyDefinitionIds.FromHeaderPointer((ushort)(SnesLittleEndian.Word(FakeKraidInstructionProgramDefinitionsRomByte(rom, population), FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 1))));
        ushort extraProperties = (ushort)(SnesLittleEndian.Word(FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 10), FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 11)));
        AssertEqual(EnemyDefinitionId.MiniKraid, populationDefinition,
            "retail Fake Kraid population matches the installed visual family");
        AssertTrue((extraProperties &
                (ushort)EnemyExtraProperties.UsesExtendedSpritemap) == 0,
            "retail Fake Kraid uses ordinary OAM composition");

        Suite(nameof(FakeKraidAnimationsVerification.VerifyFakeKraidAnimations), () => FakeKraidAnimationsVerification.VerifyFakeKraidAnimations(rom));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]), new byte[8]);

        Suite(nameof(VerifyInitializer), () => VerifyInitializer(movingRight: false));
        Suite(nameof(VerifyInitializer), () => VerifyInitializer(movingRight: true));

        ExerciseWalk(
            FakeKraidProgram.StepForwardsFacingLeft,
            movingRight: false,
            walkDelta: -4);
        ExerciseWalk(
            FakeKraidProgram.StepBackwardsFacingLeft,
            movingRight: false,
            walkDelta: 4);
        ExerciseWalk(
            FakeKraidProgram.StepForwardsFacingRight,
            movingRight: true,
            walkDelta: 4);
        ExerciseWalk(
            FakeKraidProgram.StepBackwardsFacingRight,
            movingRight: true,
            walkDelta: -4);
        ExerciseSpit(
            FakeKraidProgram.FireSpitFacingLeft,
            movingRight: false);
        ExerciseSpit(
            FakeKraidProgram.FireSpitFacingRight,
            movingRight: true);

        Console.WriteLine("Fake Kraid: both initial facings, four walk and two spit programs with their cues pass.");

        void VerifyInitializer(bool movingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out _,
                out FakeKraidEnemyState state);
            AssertEqual(
                AnimationProgramCursor<FakeKraidProgram>.Start(movingRight
                    ? FakeKraidProgram.StepForwardsFacingRight
                    : FakeKraidProgram.StepForwardsFacingLeft),
                state.Program,
                $"Fake Kraid initializer selects {(movingRight ? "right" : "left")} program");
        }

        void ExerciseWalk(FakeKraidProgram program, bool movingRight, short walkDelta)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out SamusState samus,
                out FakeKraidEnemyState state);
            state.Program = AnimationProgramCursor<FakeKraidProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            state.WalkDelta = walkDelta;
            state.FacingDelta = movingRight ? (short)4 : (short)-4;
            state.WalkStepTimer = 0x0100;
            state.SpitDecisionTimer = NativeCountdown.Start(0x0100);

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual((ushort)0x00ff, state.WalkStepTimer,
                $"Fake Kraid {program} executes its walk callback");
            AssertEqual((ushort)0x00ff, state.SpitDecisionTimer.RawValue,
                $"Fake Kraid {program} executes its action callback");
        }

        void ExerciseSpit(FakeKraidProgram program, bool movingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out SamusState samus,
                out FakeKraidEnemyState state);
            state.Program = AnimationProgramCursor<FakeKraidProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            state.WalkStepTimer = 0x0100;
            state.SpitDecisionTimer = NativeCountdown.Start(0x0100);

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual(2, state.SpawnedSpitCount,
                $"Fake Kraid {program} spawns its projectile pair");
            AssertEqual((ushort?)0x0016, enemies.FakeKraidBehavior.LastFakeKraidSoundEffect,
                $"Fake Kraid {program} publishes its cry");
        }

        RoomEnemySystem CreateSystem(
            bool movingRight,
            out RoomEnemySlot slot,
            out SamusState samus,
            out FakeKraidEnemyState state)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(rom);
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
            state = (enemies.FakeKraidStates[0] ?? throw new InvalidOperationException("Verification requires enemies.FakeKraidStates[0]."));
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

    /// <summary>Reads native bank-$A6 bytes only for the independent instruction comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for fake Kraid instruction program definitions")]
    private static byte FakeKraidInstructionProgramDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the Fake Kraid instruction check against the local retail cartridge.</summary>
    [VerificationCommand("--fake-kraid-instruction-mechanics")]
    internal static int FakeKraidInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions());
        return 0;
    }
}
