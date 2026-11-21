using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>Checks the four native animation selectors and live instruction handoffs for both Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitionsVerification
{
    /// <summary>Compares both compiled phase tables with ROM and exercises all install and attack transitions with table reads forbidden.</summary>
    internal static void VerifySkreeMetareeAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        // The tables' fourth entry is the stop animation the AI never selects, which the port does not keep (#1838).
        // Each phase number names the animation in its species' phase table, which Core holds directly (#1810).
        for (int index = 0; index < 3; index++)
        {
            SkreeMetareeSegment metaree = SkreeNativeWords.MetareeAnimationPhases.Decode(index, "Metaree animation phase");
            AssertEqual(rom.ReadCartridgeWord((0xa3894e + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(SkreeMetareeNativeListAddresses.Entries, metaree),
                $"Metaree animation phase {index}");
            SkreeMetareeSegment skree = SkreeNativeWords.SkreeAnimationPhases.Decode(index, "Skree animation phase");
            AssertEqual(rom.ReadCartridgeWord((0xa3c69c + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(SkreeMetareeNativeListAddresses.Entries, skree),
                $"Skree animation phase {index}");
        }

        Suite(nameof(VerifyAllSkreeMetareeInstallHandoffs), () => VerifyAllSkreeMetareeInstallHandoffs());
        Suite(nameof(VerifyLiveMetareeAnimationHandoffs), () => VerifyLiveMetareeAnimationHandoffs());
        Suite(nameof(VerifyLiveSkreeAnimationHandoffs), () => VerifyLiveSkreeAnimationHandoffs());

        AssertThrows<InvalidDataException>(
            () => SkreeNativeWords.SkreeAnimationPhases.Decode(3, "Skree animation phase"),
            "Skree animation phase past table");
        AssertThrows<InvalidDataException>(
            () => SkreeNativeWords.MetareeAnimationPhases.Decode(3, "Metaree animation phase"),
            "Metaree animation phase past table");
        Console.WriteLine(
            "Skree/Metaree animation definitions: six native selectors, all eight install handoffs, and both live attack transitions per family pass with pointer tables forbidden.");
    }

    /// <summary>
    /// Starts every animation the AI starts from a different playing one and checks each slot's animation and timer
    /// reset; asking for the playing animation again leaves it playing (#2145).
    /// </summary>
    private static void VerifyAllSkreeMetareeInstallHandoffs()
    {
        SkreeMetareeSegment[] metareeAnimations = [SkreeMetareeSegment.MetareeIdling, SkreeMetareeSegment.MetareePreparingAttack, SkreeMetareeSegment.MetareeDiving];
        foreach (SkreeMetareeSegment animation in metareeAnimations)
        {
            var metareePlace = NewSkreeMetareeAnimationPlace();
            var metareeState = new MetareeEnemyState(MetareeAction.Idling) { Playback = new(SkreeMetareeAnimations.StartingWith(Different(metareeAnimations, animation)), AnimationPlace.StartOfAnimation) };
            MetareeBehavior.StartMetareeAnimation(metareePlace, metareeState, animation);
            AssertSkreeMetareeHandoff(metareePlace, animation, metareeState.Playback, $"Metaree {animation}");

            AnimationPlayback<SkreeMetareeSegment> playing = metareeState.Playback;
            MetareeBehavior.StartMetareeAnimation(metareePlace, metareeState, animation);
            AssertEqual(playing, metareeState.Playback, $"Metaree {animation} asked for again keeps playing");
        }

        SkreeMetareeSegment[] skreeAnimations = [SkreeMetareeSegment.SkreeIdling, SkreeMetareeSegment.SkreePreparingAttack, SkreeMetareeSegment.SkreeDiving];
        foreach (SkreeMetareeSegment animation in skreeAnimations)
        {
            var skreePlace = NewSkreeMetareeAnimationPlace();
            var skreeState = new SkreeEnemyState(SkreeAction.Idling) { Playback = new(SkreeMetareeAnimations.StartingWith(Different(skreeAnimations, animation)), AnimationPlace.StartOfAnimation) };
            SkreeBehavior.StartSkreeAnimation(skreePlace, skreeState, animation);
            AssertSkreeMetareeHandoff(skreePlace, animation, skreeState.Playback, $"Skree {animation}");

            AnimationPlayback<SkreeMetareeSegment> playing = skreeState.Playback;
            SkreeBehavior.StartSkreeAnimation(skreePlace, skreeState, animation);
            AssertEqual(playing, skreeState.Playback, $"Skree {animation} asked for again keeps playing");
        }
    }

    /// <summary>Drives a live Metaree from idle through attack preparation into its diving animation.</summary>
    private static void VerifyLiveMetareeAnimationHandoffs()
    {
        var enemies = NewSkreeMetareeGuardedSystem();
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Position = place.Position.WithWhole(new(0x0100, 0x0080));

        place.Bind(EnemyDefinitionId.Metaree, place.KeptDefinition);
        MetareeBehavior.InitializeMetaree();
        MetareeEnemyState state = (enemies.MetareeStates[0] ?? throw new InvalidOperationException("Verification requires enemies.MetareeStates[0]."));
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(place.Position.X.Whole, unchecked((ushort)(place.Position.Y.Whole + 0x60)))),
        };

        MetareeBehavior.RunMetareeIdle(place, (state), samus);
        AssertSkreeMetareeHandoff(place, SkreeMetareeSegment.MetareePreparingAttack, state.Playback, "live Metaree preparation");

        state.Action = MetareeAction.AttackReady;
        enemies.MetareeBehavior.LaunchMetareeAttack(place, state);
        AssertSkreeMetareeHandoff(place, SkreeMetareeSegment.MetareeDiving, state.Playback, "live Metaree dive");
    }

    /// <summary>Drives a live Skree from idle through attack preparation into its diving animation.</summary>
    private static void VerifyLiveSkreeAnimationHandoffs()
    {
        var enemies = NewSkreeMetareeGuardedSystem();
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Position = place.Position.WithWhole(new(0x0100, 0x0080));

        place.Bind(EnemyDefinitionId.Skree, place.KeptDefinition);
        SkreeBehavior.InitializeSkree();
        SkreeEnemyState state = (enemies.SkreeStates[0] ?? throw new InvalidOperationException("Verification requires enemies.SkreeStates[0]."));
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(place.Position.X.Whole, unchecked((ushort)(place.Position.Y.Whole + 0x60)))),
        };

        enemies.SkreeBehavior.RunSkreeMain(place.HeldOccupant(), place, samus, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertSkreeMetareeHandoff(place, SkreeMetareeSegment.SkreePreparingAttack, state.Playback, "live Skree preparation");

        state.Action = SkreeAction.AttackReady;
        enemies.SkreeBehavior.RunSkreeMain(place.HeldOccupant(), place, samus, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        AssertSkreeMetareeHandoff(place, SkreeMetareeSegment.SkreeDiving, state.Playback, "live Skree dive");
    }

    /// <summary>Creates an enemy system that rejects reads from the two migrated animation selector tables.</summary>
    private static RoomEnemySystem NewSkreeMetareeGuardedSystem(
)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        return enemies;
    }

    /// <summary>Seeds nondefault instruction and timer values so installation must visibly replace them.</summary>
    private static RoomEnemyPlace NewSkreeMetareeAnimationPlace() =>
        new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry.Common, new RemovedEnemyLeftovers<IEnemyEntity>()).WithAnimationClock(hold: 0x2222);

    /// <summary>One of <paramref name="animations"/> other than <paramref name="animation"/>, playing before each handoff.</summary>
    private static SkreeMetareeSegment Different(SkreeMetareeSegment[] animations, SkreeMetareeSegment animation) =>
        animations.First(candidate => candidate != animation);

    /// <summary>Checks the freshly started animation and its timer reset.</summary>
    private static void AssertSkreeMetareeHandoff(
        RoomEnemyPlace place,
        SkreeMetareeSegment expected,
        AnimationPlayback<SkreeMetareeSegment> playback,
        string context)
    {
        AssertTrue(ReferenceEquals(SkreeMetareeAnimations.StartingWith(expected), playback.Animation) &&
            playback.Place == AnimationPlace.StartOfAnimation, $"{context} animation");
        AssertEqual(1, place.AnimationClock.Hold, $"{context} instruction timer");
    }

}
