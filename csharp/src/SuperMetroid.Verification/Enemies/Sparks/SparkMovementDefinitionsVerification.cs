using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Sparks.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Spark's initial animation/function selections and how its animations run.</summary>
internal static class SparkMovementDefinitionsVerification
{
    /// <summary>
    /// The three authored selectors start the native list and function, and no other initial state is accepted.
    /// Flicker-on makes the Spark tangible and runs into the active loop; flicker-out makes it intangible and holds; both
    /// loops run on time.
    /// </summary>
    internal static void VerifySparkMovementDefinitions(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: new TestAddressSpace(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().IsAreaBossDefeated = () => true;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Spark, slot.Definition);
        for (ushort selector = 0; selector < 3; selector++)
        {
            ushort nativeList = CartridgeImportSource.Require(rom).ReadCartridgeWord(SparkNativeValues.AnimationScriptTable + selector * 2);
            var nativeFunction = SparksNativeWords.SparkEnemyFunctions.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(SparkNativeValues.FunctionTable + selector * 2),
                $"Spark selector {selector}'s function");
            SparkMovementDefinition definition = SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.SparkStart(selector);
            AssertEqual(nativeList, NativeEnemyAnimationOracle.EntryOf(SparkNativeListAddresses.Entries, definition.Program),
                $"Spark list selector {selector}");
            AssertEqual(nativeFunction, definition.Function, $"Spark function selector {selector}");

            slot.SetSpawnParameter1(EnemyDefinitionId.Spark, selector);
            slot.SetSpawnParameter2(EnemyDefinitionId.Spark, 5);
            SparkBehavior.InitializeSpark(slot);
            SparkEnemyState state = enemies.SparkStates[0] ?? throw new InvalidOperationException("Spark has no state.");
            AssertEqual(new AnimationPlayback<SparkProgram>(SparkAnimations.StartingWith(definition.Program), AnimationPlace.StartOfAnimation),
                state.Playback, $"Spark production animation selector {selector}");
            AssertEqual(nativeFunction, state.Function, $"Spark production function selector {selector}");
        }
        foreach (int unauthored in new[] { 3, 4, 0xffff })
            AssertThrows<InvalidDataException>(
                () => SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.SparkStart(unauthored),
                $"Spark unauthored initial state {unauthored}");

        foreach (SparkProgram loop in new[] { SparkProgram.Active, SparkProgram.Emitter })
        {
            (RoomEnemySystem loopSystem, RoomEnemySlot loopSlot, _) = Start(loop, intangible: false);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(SparkAnimations.Segments, loop, SparkFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(loopSlot, EnemyInstructionRunner.LoopUpdates(SparkAnimations.Segments, loop)), $"Spark {loop} loop");
        }
        (RoomEnemySystem onSystem, RoomEnemySlot on, SparkEnemyState onState) = Start(SparkProgram.FlickerOn, intangible: true);
        EnemyInstructionRunner.ShownFrames(on, 1);
        AssertTrue(!on.Contact.HasAny(EnemyContact.IgnoresSamus), "Spark flicker-on makes the actor tangible before its first flash");
        EnemyInstructionRunner.ShownFrames(on, 40);
        AssertEqual<SparkProgram?>(SparkProgram.Active, StandsIn(onState.Playback), "Spark flicker-on runs into the active loop");
        (RoomEnemySystem outSystem, RoomEnemySlot off, SparkEnemyState offState) = Start(SparkProgram.FlickerOut, intangible: false);
        EnemyInstructionRunner.ShownFrames(off, 40);
        AssertTrue(off.Contact.HasAny(EnemyContact.IgnoresSamus), "Spark flicker-out makes the actor intangible");
        AssertEqual<SparkProgram?>(SparkProgram.FlickerOut, StandsIn(offState.Playback), "Spark flicker-out holds");
        Console.WriteLine("  Spark: three native selections, the unauthored-state rejection, both loops and both flickers with their collision cues pass.");

        // The segment a playback stands in: the segment of the phase it plays.
        static SparkProgram? StandsIn(AnimationPlayback<SparkProgram>? playback) =>
            playback is { } playing ? playing.Animation.Phases[playing.Place.Phase].Segment : null;
    }

    /// <summary>A Spark running <paramref name="program"/> from its first step.</summary>
    private static SparkMovementDefinitionsStartResult Start(SparkProgram program, bool intangible)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().IsAreaBossDefeated = () => true;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Spark, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnSettings(new SparkSpawnSettings(Start: SparkMovementDefinitions.AlwaysActive, BaseFunctionTime: 0));
        SparkBehavior.InitializeSpark(slot);
        SparkEnemyState state = enemies.SparkStates[0] ?? throw new InvalidOperationException("Spark has no state.");
        state.Playback = new(SparkAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
        slot.Contact = intangible ? EnemyContact.IgnoresSamus : EnemyContact.None;
        slot.Processing = EnemyProcessing.None;
        slot.Presence = EnemyPresence.None;
        return new SparkMovementDefinitionsStartResult(enemies, slot, state);
    }

    /// <summary>Runs the Spark checks and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--spark-instruction-mechanics")]
    internal static int SparkInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySparkMovementDefinitions), () => VerifySparkMovementDefinitions(rom));
        Suite(nameof(SparkAnimationsVerification.VerifySparkAnimations), () => SparkAnimationsVerification.VerifySparkAnimations(rom));
        return 0;
    }
}
