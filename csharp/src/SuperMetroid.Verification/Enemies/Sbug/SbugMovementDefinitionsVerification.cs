using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Animation;
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

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Func<RoomEnemyPlace, SbugEnemyState> initialize = fixturePlace => SbugBehavior.InitializeSbug(fixturePlace.HeldOccupant().SpawnSettings<SbugSpawnSettings>(), fixturePlace);
        Action<RoomEnemyPlace, SbugEnemyState, SamusState> runMain = (fixturePlace, fixtureState, fixtureSamus) =>
            SbugBehavior.RunSbugMainAi(fixturePlace.HeldOccupant().SpawnSettings<SbugSpawnSettings>(), fixturePlace, fixtureState, PlacedEnemyAccess.AiFrame(fixtureSamus));
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Sbug, place.KeptDefinition);

        for (int facing = 0; facing < 8; facing++)
        {
            int address = instructionTable + facing * 2;
            ushort native = rom.ReadCartridgeWord(address);
            AssertEqual(native, NativeEnemyAnimationOracle.EntryOf(SbugNativeListAddresses.Entries,
                    SbugMovementDefinitions.Facings.InTurnOrder[facing]),
                $"compiled Sbug facing selector {facing}");
        }

        foreach (int behavior in AuthoredActivationSelectors)
        {
            int address = activationTable + behavior * 2;
            var native = SbugNativeWords.SbugActions.Decode(rom.ReadCartridgeWord(address), "Sbug activation function");
            var placed = (SbugSpawnSettings)SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(
                EnemyDefinitionId.Sbug, SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(0, behavior << 8));
            AssertEqual(native, SbugMovement.ActivationAction(placed.Behavior),
                $"compiled Sbug activation selector {behavior}");
        }
        // No room places an Sbug with any other selector, so none decodes (#2454).
        foreach (int behavior in Enumerable.Range(0, 8).Except(AuthoredActivationSelectors))
        {
            AssertThrows<InvalidDataException>(
                () => SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(
                    EnemyDefinitionId.Sbug, SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(0, behavior << 8)),
                $"a placement selecting unplaced activation behaviour {behavior} fails to decode");
        }

        for (ushort selector = 0; selector < 8; selector++)
        {
            byte angle = unchecked((byte)(0x30 + selector * 0x20));
            place.SetSpawnParameter1(EnemyDefinitionId.Sbug, (ushort)(angle << 8 | 1));
            place.SetSpawnParameter2(EnemyDefinitionId.Sbug, 1);
            SbugEnemyState state = initialize(place);

            ushort native = rom.ReadCartridgeWord(instructionTable + selector * 2);
            CompassDirection forward = SbugBehavior.ForwardLeg(place.HeldOccupant()).Facing;
            AssertEqual(SbugMovementDefinitions.Facings.InTurnOrder[selector], forward,
                $"Sbug production facing {selector}");
            AssertEqual(new AnimationPlayback<CompassDirection>(SbugAnimations.StartingWith(forward), AnimationPlace.StartOfAnimation), state.Playback,
                $"Sbug production facing animation {selector}");
            AssertEqual(native, NativeEnemyAnimationOracle.EntryOf(SbugNativeListAddresses.Entries, state.Playback.Animation.Phases[0].Segment),
                $"Sbug production facing list {selector}");
            // Each loop shows its four frames five updates apiece, then its goto shows the first again.
            ushort[] loop = SbugAnimationsConstants.Spritemaps(state.Playback.Animation.Phases[0].Segment).ToArray();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. loop, loop[0]], [5, 5, 5, 5, 5]),
                EnemyInstructionRunner.ShownFrames(place, 21), $"Sbug facing loop {selector}");
        }

        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x1234, 0x5678)),
        };
        place.Position = place.Position.WithWhole(samus.Position.Whole);
        foreach (int behavior in AuthoredActivationSelectors)
        {
            // The Sbug takes its activation behaviour from its placement when it is placed (#2063).
            place.SetSpawnParameter2(EnemyDefinitionId.Sbug, (ushort)(behavior << 8 | 1));
            SbugEnemyState state = initialize(place);
            place.Position = place.Position.WithWhole(samus.Position.Whole);
            state.Action = SbugAction.WaitForSamus;
            runMain(place, state, samus);

            int address = activationTable + behavior * 2;
            var native = SbugNativeWords.SbugActions.Decode(rom.ReadCartridgeWord(address), "Sbug activation function");
            AssertEqual(native, state.Action, $"Sbug production activation {behavior}");
        }

        Console.WriteLine(
            "Sbug movement definitions: eight facing loops, the three placed activation callbacks and every loop's run pass " +
            "with the selector tables forbidden.");
    }


    /// <summary>
    /// The activation selectors the game's rooms place Sbugs with: forward, zigzag and forward-then-wait, the first, second
    /// and sixth of the cartridge's seven.
    /// </summary>
    private static readonly int[] AuthoredActivationSelectors = [0, 1, 5];

    /// <summary>Runs the retail-backed Sbug movement checks and program oracle.</summary>
    [VerificationCommand("--sbug-instruction-mechanics")]
    internal static int SbugInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(rom));
        Suite(nameof(SbugAnimationsVerification.VerifySbugAnimations), () => SbugAnimationsVerification.VerifySbugAnimations(rom));
        return 0;
    }
}
