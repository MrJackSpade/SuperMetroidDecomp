using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Boulder;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that Boulder starts and runs its typed animation (#641).</summary>
internal static class BoulderBehaviorVerification
{
    /// <summary>The real initializer starts the Left program, which the runtime then plays through once and back.</summary>
    internal static void VerifyBoulderBehavior()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Boulder, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnSettings(new BoulderSpawnSettings(TriggerHeight: 8, TriggerReach: 8, Drop: 1, Roll: BoulderRoll.Left,
            Landing: BoulderLanding.Standard));
        slot.Position = slot.Position.WithWhole(new(256, 512));
        BoulderBehavior.InitializeBoulder(slot);
        AssertEqual(AnimationProgramCursor<BoulderProgram>.Start(BoulderProgram.Left), (enemies.SlotTable.Slots[0].EntityState<BoulderEntity, BoulderEnemyState>() ?? throw new InvalidOperationException("The Boulder has no state.")).Program,
            "Boulder initializer animation");
        slot.InstructionTimer = WrappingCountdown.Start(1);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BoulderAnimations.Programs, BoulderProgram.Left, BoulderFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(BoulderAnimations.Programs, BoulderProgram.Left)),
            "Boulder runs its loop and returns through its goto");
        Console.WriteLine("  Boulder behavior: the initializer starts and runs its loop.");
    }

    /// <summary>
    /// The production rebound update slows the bounce clock by one quadratic-speed record. A clock below one record
    /// would go below zero: the cartridge stores zero and starts falling instead. A clock of exactly one record reaches
    /// zero and keeps rebounding. The clock's slot variable holds the same value.
    /// </summary>
    internal static void VerifyBoulderReboundClock()
    {
        RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(8, 8));
        foreach ((int start, int expected, BoulderAiFunction function) in new VerifyBoulderReboundClockCase[]
                 {
                     new VerifyBoulderReboundClockCase(200, 0, BoulderAiFunction.Falling),
                     new VerifyBoulderReboundClockCase(255, 0, BoulderAiFunction.Falling),
                     new VerifyBoulderReboundClockCase(256, 0, BoulderAiFunction.Rebound),
                     new VerifyBoulderReboundClockCase(8192, 7936, BoulderAiFunction.Rebound),
                 })
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Boulder, slot.Definition);
            slot.Position = slot.Position.WithWhole(new(256, 512));
            slot.SetSpawnWords(EnemyDefinitionId.Boulder, words => words);
            BoulderBehavior.InitializeBoulder(slot);
            BoulderEnemyState state = slot.RequireSpeciesState<BoulderEnemyState>();
            state.Function = BoulderAiFunction.Rebound;
            state.BounceSpeedAccumulator = start;
            var frame = new ClockedEnemyAiFrame(
                new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), Level = level },
                new FrameCounters(0));

            enemies.BoulderBehavior.RunBoulderMainAi(slot, frame);

            AssertEqual(expected, state.BounceSpeedAccumulator, $"Boulder rebound clock from {start}");
            AssertEqual(function, state.Function, $"Boulder rebound from {start} selects its next function");
        }
        Console.WriteLine("  Boulder rebound: a clock below one record clamps to zero and falls; one record reaches zero and keeps rebounding.");
    }

    /// <summary>Runs the Boulder behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--boulder-instruction-mechanics")]
    internal static int BoulderBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBoulderBehavior), () => VerifyBoulderBehavior());
        Suite(nameof(VerifyBoulderReboundClock), () => VerifyBoulderReboundClock());
        Suite(nameof(BoulderAnimationsVerification.VerifyBoulderAnimations), () => BoulderAnimationsVerification.VerifyBoulderAnimations(rom));
        return 0;
    }
}
