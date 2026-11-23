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
            RoomEnemyPlace place = PlaceSbug(angle << 8 | 1, 1, Position16<RoomPixels>.Zero);
            SbugEnemyState state = place.RequireSpeciesState<SbugEnemyState>();

            ushort native = rom.ReadCartridgeWord(instructionTable + selector * 2);
            CompassDirection forward = SbugBehavior.ForwardLeg(place.HeldOccupant().SpawnSettings<SbugSpawnSettings>()).Facing;
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
        foreach (int behavior in AuthoredActivationSelectors)
        {
            // The Sbug takes its activation behaviour from its placement when it is placed (#2063), and waits for Samus,
            // who stands where it is placed.
            RoomEnemyPlace place = PlaceSbug(0x10 << 8 | 1, behavior << 8 | 1, samus.Position.Whole);
            SbugEnemyState state = place.RequireSpeciesState<SbugEnemyState>();
            AssertEqual(SbugAction.WaitForSamus, state.Action, $"Sbug placed with activation {behavior} waits for Samus");
            place.RunMainAi(samus);

            int address = activationTable + behavior * 2;
            var native = SbugNativeWords.SbugActions.Decode(rom.ReadCartridgeWord(address), "Sbug activation function");
            AssertEqual(native, state.Action, $"Sbug production activation {behavior}");
        }

        Console.WriteLine(
            "Sbug movement definitions: eight facing loops, the three placed activation callbacks and every loop's run pass " +
            "with the selector tables forbidden.");
    }


    /// <summary>
    /// An Sbug whose population entry's parameters are <paramref name="parameter1"/> and <paramref name="parameter2"/>,
    /// placed at <paramref name="at"/> as its room's population sets it up (#2554), each in a room of its own.
    /// </summary>
    private static RoomEnemyPlace PlaceSbug(int parameter1, int parameter2, Position16<RoomPixels> at)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        return enemies.LoadPopulation(PopulationPlacementAccess.Entry(EnemyDefinitionId.Sbug,
            SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Sbug,
                SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(parameter1, parameter2)), at))[0];
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
