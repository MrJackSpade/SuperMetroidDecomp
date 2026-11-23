using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Metroid's initial animation and both pulse loops with their closing sounds.</summary>
internal static class MetroidBehaviorVerification
{
    /// <summary>
    /// The real initializer starts the chasing pulse; the chasing loop's cue advances the RNG once for a random cry and
    /// the draining loop's cue plays the draining sound without touching the RNG; both loops run on time.
    /// </summary>
    internal static void VerifyMetroidBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Metroid, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        // The room spawn primes the instruction timer before the species initializer runs.
        place.AnimationClock.StepOnNextUpdate();
        place.SetSpawnWords(EnemyDefinitionId.Metroid, words => words);
        // The Metroid's setup builds the state its animation, which the check plays, runs on.
        MetroidEnemyState state = enemies.MetroidBehavior.InitializeMetroid(place.HeldOccupant(), place.HeldOccupant().SpawnSettings<MetroidSpawnSettings>(), place);
        AssertEqual(new AnimationPlayback<MetroidSegment>(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus), AnimationPlace.StartOfAnimation),
            state.Playback, "the Metroid starts chasing");

        foreach ((MetroidSegment loop, int cries) in new[] { new VerifyMetroidBehaviorCase(MetroidSegment.ChasingSamus, 1), new VerifyMetroidBehaviorCase(MetroidSegment.DrainingSamus, 0) })
        {
            int randomCalls = 0;
            enemies.VerificationServices().NextRandom = () => { randomCalls++; return 0; };
            state.Playback = new(MetroidAnimations.StartingWith(loop), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            // The chasing loop pulses four times before its cry (#2337), the draining loop once before its sound.
            AssertEqual(EnemyInstructionRunner.ExpectedAnimationLoop(MetroidAnimations.Segments, MetroidAnimations.StartingWith(loop), MetroidFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.AnimationLoopUpdates(MetroidAnimations.Segments, MetroidAnimations.StartingWith(loop)),
                    () => enemies.MetroidBehavior.AdvanceMetroidTypedAnimation(place, state)),
                $"Metroid {loop} loop");
            AssertEqual(cries, randomCalls, $"Metroid {loop} loop's RNG use");
            AssertTrue(enemies.Audio.LatestSound is not null, $"Metroid {loop} loop plays its sound");
        }
        Console.WriteLine("  Metroid behavior: the chasing start and both pulse loops with their sound and RNG cues pass.");
    }

    /// <summary>Runs the Metroid behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--metroid-instruction-mechanics")]
    internal static int MetroidBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMetroidBehavior), () => VerifyMetroidBehavior());
        Suite(nameof(MetroidAnimationsVerification.VerifyMetroidAnimations), () => MetroidAnimationsVerification.VerifyMetroidAnimations(rom));
        return 0;
    }
}
