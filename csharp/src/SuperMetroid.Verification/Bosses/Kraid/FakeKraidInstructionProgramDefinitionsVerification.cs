using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Mini-Kraid's typed programs and their cues through its real initializer, walks and spits.</summary>
internal static class FakeKraidInstructionProgramDefinitionsVerification
{
    /// <summary>Enough walk decisions that the spit decision cannot expire during a sixty-update run.</summary>
    private const int UnexpiredSpitDecisions = 256;

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
        EnemyDefinitionId populationDefinition = EnemyDefinitionIds.FromHeaderPointer(LittleEndianBytes.Word(FakeKraidInstructionProgramDefinitionsRomByte(rom, population), FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 1)));
        ushort extraProperties = LittleEndianBytes.Word(FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 10), FakeKraidInstructionProgramDefinitionsRomByte(rom, population + 11));
        AssertEqual(EnemyDefinitionId.MiniKraid, populationDefinition,
            "retail Fake Kraid population matches the installed visual family");
        AssertTrue((extraProperties &
                (ushort)EnemyExtraProperties.UsesExtendedSpritemap) == 0,
            "retail Fake Kraid uses ordinary OAM composition");

        Suite(nameof(FakeKraidAnimationsVerification.VerifyFakeKraidAnimations), () => FakeKraidAnimationsVerification.VerifyFakeKraidAnimations(rom));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]));

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
        ExerciseWalkTimerExpiry(
            FakeKraidProgram.StepForwardsFacingLeft,
            movingRight: false,
            walkDelta: -4);
        ExerciseWalkTimerExpiry(
            FakeKraidProgram.StepBackwardsFacingRight,
            movingRight: true,
            walkDelta: -4);
        ExerciseWalkTimerWrap(
            FakeKraidProgram.StepForwardsFacingLeft,
            movingRight: false,
            walkDelta: -4);
        ExerciseWalkTimerWrap(
            FakeKraidProgram.StepBackwardsFacingRight,
            movingRight: true,
            walkDelta: -4);

        Console.WriteLine("Fake Kraid: both initial facings, four walk and two spit programs with their cues, and the walk timer's expiry at zero and wrap from zero pass.");

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
            slot.InstructionTimer = WrappingCountdown.Start(1);
            state.WalkDelta = walkDelta;
            state.FacingDelta = movingRight ? (short)4 : (short)-4;
            state.WalkStepTimer = WrappingCountdown.Start(0x0100);
            state.SpitDecisionTimer = WrappingCountdown.Start(0x0100);

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual((ushort)0x00ff, state.WalkStepTimer.RawValue,
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
            slot.InstructionTimer = WrappingCountdown.Start(1);
            state.WalkStepTimer = WrappingCountdown.Start(0x0100);
            state.SpitDecisionTimer = WrappingCountdown.Start(0x0100);

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual(2, state.SpawnedSpitCount,
                $"Fake Kraid {program} spawns its projectile pair");
            AssertEqual((SoundEffectId?)KraidSoundDefinitions.MiniKraidCry, enemies.FakeKraidBehavior.LastFakeKraidSoundEffect,
                $"Fake Kraid {program} publishes its cry");
        }

        void ExerciseWalkTimerExpiry(FakeKraidProgram program, bool movingRight, short walkDelta)
        {
            // A timer at one reaches zero on the walk callback: the actor reverses without stepping and reloads
            // seven through ten (seven for the fixture's zero random word).
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out SamusState samus,
                out FakeKraidEnemyState state);
            state.Program = AnimationProgramCursor<FakeKraidProgram>.Start(program);
            slot.InstructionTimer = WrappingCountdown.Start(1);
            state.WalkDelta = walkDelta;
            state.FacingDelta = movingRight ? (short)4 : (short)-4;
            state.WalkStepTimer = WrappingCountdown.Start(1);
            state.SpitDecisionTimer = WrappingCountdown.Start(UnexpiredSpitDecisions);
            ushort startX = slot.Position.X.Whole;

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual((short)-walkDelta, state.WalkDelta,
                $"Fake Kraid {program} reverses when its walk timer reaches zero");
            AssertEqual(startX, slot.Position.X.Whole,
                $"Fake Kraid {program} does not step on the reversing callback");
            AssertEqual((ushort)7, state.WalkStepTimer.RawValue,
                $"Fake Kraid {program} reloads its walk timer from the random word");
        }

        void ExerciseWalkTimerWrap(FakeKraidProgram program, bool movingRight, short walkDelta)
        {
            // A cleared timer does not expire: the decrement wraps to the full word and the actor still steps.
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out SamusState samus,
                out FakeKraidEnemyState state);
            state.Program = AnimationProgramCursor<FakeKraidProgram>.Start(program);
            slot.InstructionTimer = WrappingCountdown.Start(1);
            state.WalkDelta = walkDelta;
            state.FacingDelta = movingRight ? (short)4 : (short)-4;
            state.WalkStepTimer = WrappingCountdown.Start(0);
            state.SpitDecisionTimer = WrappingCountdown.Start(UnexpiredSpitDecisions);
            ushort startX = slot.Position.X.Whole;

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual(ushort.MaxValue, state.WalkStepTimer.RawValue,
                $"Fake Kraid {program} wraps a cleared walk timer");
            AssertEqual(walkDelta, state.WalkDelta,
                $"Fake Kraid {program} keeps its walk direction when the cleared timer wraps");
            AssertEqual(unchecked((ushort)(startX + walkDelta)), slot.Position.X.Whole,
                $"Fake Kraid {program} still steps when the cleared timer wraps");
        }

        RoomEnemySystem CreateSystem(
            bool movingRight,
            out RoomEnemySlot slot,
            out SamusState samus,
            out FakeKraidEnemyState state)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().ReadRandom = () => 0;
            Action<RoomEnemySlot, SamusState?> initialize = enemies.FakeKraidBehavior.InitializeFakeKraid;

            slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.MiniKraid, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            // The header's collision half-extents: the terrain movers scan the rows the body spans.
            slot.Radius = CompiledRoomEnemyDefinitions.Instance.ReadEnemyDefinition(EnemyDefinitionId.MiniKraid).Radius;
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
                [slot, samus, level, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Reads native bank-$A6 bytes only for the independent instruction comparison.</summary>
    private static byte FakeKraidInstructionProgramDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the Fake Kraid instruction check against the local retail cartridge.</summary>
    [VerificationCommand("--fake-kraid-instruction-mechanics")]
    internal static int FakeKraidInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions());
        return 0;
    }
}
