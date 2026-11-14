using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Sbug;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail-word and guarded-execution checks for compiled Sbug movement programs.</summary>
internal static class SbugMovementDefinitionsVerification
{
    /// <summary>Checks compiled Sbug facing, activation, and instruction data against retail words, then runs every facing loop with migrated reads forbidden.</summary>
    internal static void VerifySbugMovementDefinitions(CartridgeImportAddressSpace rom)
    {
        const int instructionTable = 0xa3a111;
        const int activationTable = 0xa3a121;

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = SbugBehavior.InitializeSbug;
        Action<RoomEnemySlot, SamusState, RoomLevelData?> runMain = enemies.SbugBehavior.RunSbugMain;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Sbug, slot.Definition);

        for (int facing = 0; facing < 8; facing++)
        {
            int address = instructionTable + facing * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, NativeEnemyAnimationOracle.EntryOf(SbugNativeListAddresses.Entries,
                    SbugMovement.FacingAnimation(SbugMovementDefinitions.Facings.InTurnOrder[facing])),
                $"compiled Sbug facing selector {facing}");
        }

        for (int behavior = 0; behavior < SbugActivationBehaviorCount; behavior++)
        {
            int address = activationTable + behavior * 2;
            var native = SbugNativeWords.SbugActions.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), "Sbug activation function");
            var placed = (SbugSpawnSettings)SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(
                EnemyDefinitionId.Sbug, SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(0, behavior << 8));
            AssertEqual(native, SbugMovement.ActivationAction(placed.Behavior),
                $"compiled Sbug activation selector {behavior}");
        }
        AssertThrows<InvalidDataException>(
            () => SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(
                EnemyDefinitionId.Sbug, SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(0, SbugActivationBehaviorCount << 8)),
            "a placement selecting past the seven activation behaviours fails to decode");

        for (ushort selector = 0; selector < 8; selector++)
        {
            byte angle = unchecked((byte)(0x30 + selector * 0x20));
            slot.SetSpawnParameter1(EnemyDefinitionId.Sbug, (ushort)(angle << 8 | 1));
            slot.SetSpawnParameter2(EnemyDefinitionId.Sbug, 1);
            initialize(slot);

            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(instructionTable + selector * 2);
            SbugEnemyState state = (enemies.SbugStates[0] ?? throw new InvalidOperationException("Verification requires enemies.SbugStates[0]."));
            AssertEqual(SbugMovementDefinitions.Facings.InTurnOrder[selector], state.ForwardFacing,
                $"Sbug production facing {selector}");
            AssertEqual(new AnimationPlayback<SbugSegment>(SbugAnimations.StartingWith(SbugMovement.FacingAnimation(state.ForwardFacing)), AnimationPlace.StartOfAnimation), state.Playback,
                $"Sbug production facing animation {selector}");
            AssertEqual(native, NativeEnemyAnimationOracle.EntryOf(SbugNativeListAddresses.Entries, state.Playback.Animation.Phases[0].Segment),
                $"Sbug production facing list {selector}");
            // Each loop shows its four frames five updates apiece, then its goto shows the first again.
            ushort[] loop = SbugAnimationsConstants.Spritemaps(state.Playback.Animation.Phases[0].Segment).ToArray();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. loop, loop[0]], [5, 5, 5, 5, 5]),
                EnemyInstructionRunner.ShownFrames(slot, 21), $"Sbug facing loop {selector}");
        }

        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x1234, 0x5678)),
        };
        slot.Position = slot.Position.WithWhole(samus.Position.Whole);
        for (int behavior = 0; behavior < SbugActivationBehaviorCount; behavior++)
        {
            SbugEnemyState state = (enemies.SbugStates[0] ?? throw new InvalidOperationException("Verification requires enemies.SbugStates[0]."));
            state.Action = SbugAction.WaitForSamus;
            slot.SetSpawnParameter2(EnemyDefinitionId.Sbug, (ushort)(behavior << 8 | 1));
            runMain(slot, samus, null);

            int address = activationTable + behavior * 2;
            var native = SbugNativeWords.SbugActions.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), "Sbug activation function");
            AssertEqual(native, state.Action, $"Sbug production activation {behavior}");
        }

        AssertThrows<InvalidDataException>(
            () => SbugMovement.ActivationAction((SbugActivationBehavior)7),
            "Sbug activation selector beyond authored table");

        Console.WriteLine(
            "Sbug movement definitions: eight facing loops, seven activation callbacks and every loop's run pass " +
            "with the selector tables forbidden.");
    }

    /// <summary>
    /// The production random-direction chooser adds an unsigned byte turn to the custom heading: from heading 240,
    /// random 96 turns by 32, so the heading wraps around the turn to 16, facing down-right. A later reversal, with Samus at least six blocks away on
    /// both axes, turns that facing a quarter turn counter-clockwise and folds it into the four facings from right to
    /// up-left: down-right becomes up-right.
    /// </summary>
    internal static void VerifySbugCustomHeadingCarriesAndFolds()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 96;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Sbug, slot.Definition);
        slot.SetSpawnParameter1(EnemyDefinitionId.Sbug, 1);
        slot.SetSpawnParameter2(EnemyDefinitionId.Sbug, 1);
        slot.Position = slot.Position.WithWhole(new(512, 512));
        SbugBehavior.InitializeSbug(slot);
        SbugEnemyState state = slot.RequireSpeciesState<SbugEnemyState>();
        state.FaceCustomHeading(RotationAngle.FromSteps(240));
        state.Action = SbugAction.ChooseRandomDirectionAndReverseWhenFar;
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(Position16<RoomPixels>.Zero) };

        enemies.SbugBehavior.RunSbugMain(slot, samus, null);
        AssertEqual(RotationAngle.FromSteps(16), state.CustomHeading, "the turn wraps the heading around the turn");
        AssertEqual(CompassDirection.DownRight, state.CustomFacing, "heading 16 faces down-right");
        AssertEqual(SbugAction.MoveStraightAndReverseWhenFar, state.Action, "the chooser starts a reversal-aware leg");

        state.StraightMovementTimer = WrappingCountdown.Start(512);
        enemies.SbugBehavior.RunSbugMain(slot, samus, null);
        AssertEqual(CompassDirection.UpRight, state.CustomFacing, "the reversal folds down-right into up-right");
        AssertEqual(new AnimationPlayback<SbugSegment>(SbugAnimations.StartingWith(SbugMovement.FacingAnimation(CompassDirection.UpRight)), AnimationPlace.StartOfAnimation), state.Playback,
            "the folded facing installs its loop");

        // Every random turn, less a quarter turn, carries around the turn and keeps the heading's fraction.
        RotationAngle start = RotationAngle.FromSteps(240, 77);
        for (int random = 0; random <= byte.MaxValue; random++)
        {
            ushort turn = checked((ushort)random);
            enemies.VerificationServices().NextRandom = () => turn;
            state.FaceCustomHeading(start);
            state.Action = SbugAction.ChooseRandomDirectionAndReverseWhenFar;
            enemies.SbugBehavior.RunSbugMain(slot, samus, null);
            AssertEqual(RotationAngle.FromRaw(unchecked((ushort)(start.RawValue + (random - 64) * 256))), state.CustomHeading,
                $"Sbug random turn {random}");
        }
        Console.WriteLine("  Sbug custom heading: turns carry into the heading word and a reversal folds the facing.");
    }

    /// <summary>
    /// Each facing's reversal is the facing two eighths of a turn counter-clockwise, folded into the four facings from
    /// right to up-left: the game advances the facing's place in the counter-clockwise order by two and keeps its
    /// place within the first four.
    /// </summary>
    internal static void VerifySbugReversedFacings()
    {
        for (int place = 0; place < 8; place++)
        {
            CompassDirection facing = SbugMovementDefinitions.Facings.InTurnOrder[place];
            AssertEqual(SbugMovementDefinitions.Facings.InTurnOrder[(place + 2) % 4], SbugMovement.ReversedFacing(facing),
                $"Sbug reversal of {facing}");
        }
        Console.WriteLine("  Sbug reversed facings: all eight fold as the game's reversal does.");
    }


    /// <summary>The number of activation behaviours a placement can select.</summary>
    private const int SbugActivationBehaviorCount = 7;

    /// <summary>Runs the retail-backed Sbug movement checks and program oracle.</summary>
    [VerificationCommand("--sbug-instruction-mechanics")]
    internal static int SbugInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(rom));
        Suite(nameof(VerifySbugCustomHeadingCarriesAndFolds), () => VerifySbugCustomHeadingCarriesAndFolds());
        Suite(nameof(VerifySbugReversedFacings), VerifySbugReversedFacings);
        Suite(nameof(SbugAnimationsVerification.VerifySbugAnimations), () => SbugAnimationsVerification.VerifySbugAnimations(rom));
        return 0;
    }
}
