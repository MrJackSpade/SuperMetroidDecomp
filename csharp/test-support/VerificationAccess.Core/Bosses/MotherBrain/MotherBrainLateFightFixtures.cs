using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Late-fight sequences over standalone encounters, for checks that step her late fight without her room.</summary>
internal static class MotherBrainLateFightFixtures
{
    /// <summary>
    /// A late-fight sequence over a fresh encounter whose body and brain hold two standalone places, as phase two leaves
    /// her when her health first runs out: her brain at its initial health, drooling and breathing, every part's hitbox
    /// on, at the rainbow attack's charge.
    /// </summary>
    internal static MotherBrainRainbowBeamAttackSequence NewSequence() =>
        NewSequence(new MotherBrainBurstCycle());

    /// <summary>A late-fight sequence as <see cref="NewSequence()"/> builds, whose encounter's bursts are <paramref name="bursts"/>.</summary>
    internal static MotherBrainRainbowBeamAttackSequence NewSequence(MotherBrainBurstCycle bursts)
    {
<<<<<<< ours
        var body = new RoomEnemyPlace(0, StandaloneFixtureSpecies.Registry.Common, new RemovedEnemyLeftovers<IEnemyEntity>());
        var brain = new RoomEnemyPlace(1, EnemyPlaceFixtures.Registry.Common, new RemovedEnemyLeftovers<IEnemyEntity>()) { Health = 3000 };
        var encounter = new MotherBrainEnemyState(body.BoundFor(EnemyDefinitionId.MotherBrainBody), brain, new MotherBrainWalkCounter(), bursts, new MotherBrainBrainAnimation(), RoomWarningFixtures.Zebes()) { RainbowPhase = MotherBrainRainbowPhase.StartCharging, DroolGenerationEnabled = true, SmallPurpleBreathGenerationEnabled = true, HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck };
        return new MotherBrainRainbowBeamAttackSequence(encounter);
||||||| base
        var body = new RoomEnemyPlace(0, StandaloneFixtureSpecies.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>());
        var brain = new RoomEnemyPlace(1, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>()) { Health = 3000 };
        var encounter = new MotherBrainEnemyState(body.BoundFor(EnemyDefinitionId.MotherBrainBody), brain, new MotherBrainWalkCounter(), bursts, new MotherBrainBrainAnimation()) { RainbowPhase = MotherBrainRainbowPhase.StartCharging, DroolGenerationEnabled = true, SmallPurpleBreathGenerationEnabled = true, HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck };
        return new MotherBrainRainbowBeamAttackSequence(encounter);
=======
        var body = new RoomEnemyPlace(0, StandaloneFixtureSpecies.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>());
        var brain = new RoomEnemyPlace(1, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>()) { Health = 3000 };
        var encounter = new MotherBrainEnemyState(body.BoundFor(EnemyDefinitionId.MotherBrainBody), brain, new MotherBrainWalkCounter(), bursts, new MotherBrainBrainAnimation()) { DroolGenerationEnabled = true, SmallPurpleBreathGenerationEnabled = true, HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck };
        var sequence = new MotherBrainRainbowBeamAttackSequence(encounter);
        sequence.State.RainbowPhase = MotherBrainRainbowPhase.StartCharging;
        return sequence;
>>>>>>> theirs
    }

    /// <summary>
    /// Makes <paramref name="lateFight"/>'s Baby in a standalone place that holds <paramref name="leftInPlace"/> from its
    /// previous occupant, as the final beam's spawn does, and initializes the Baby there (#2572): its position and health
    /// live in the place.
    /// </summary>
    internal static BabyMetroidCutsceneState Arrive(MotherBrainRainbowBeamAttackSequence lateFight, FixedPosition<RoomPixels> leftInPlace)
    {
        var place = new RoomEnemyPlace(2, EnemyPlaceFixtures.Registry.Common, new RemovedEnemyLeftovers<IEnemyEntity>()) { Position = leftInPlace };
        BabyMetroidCutsceneState baby = Ensure.NotNull(lateFight).MakeBaby(place);
        baby.Arrive();
        return baby;
    }

    /// <summary>
    /// One late-fight update with nothing else under way (#2261): no Power Bomb, the room's random number zero and a fresh
    /// shared random sequence for the draws, the ordinary escape text not yet typed, and the room's earthquake not stopped.
    /// </summary>
    /// <param name="enemyFrameCounter">The enemy frame counter's count.</param>
    /// <param name="enemyPassCount">The main enemy pass counter's count.</param>
    internal static RainbowBeamAttackFrame QuietFrame(
        int enemyFrameCounter,
        int enemyPassCount) => new()
    {
        EnemyFrameCounter = enemyFrameCounter,
        EnemyPassCount = enemyPassCount,
        PowerBombActive = false,
        RandomNumberSeed = MotherBrainRandomSample.FromGenerator(0),
        NextRandomNumber = new SuperMetroid.Core.Compatibility.SharedRandom().Generate,
        Language = SuperMetroid.Core.Progression.TextLanguage.English,
        TypewriterFinished = false,
        RoomEarthquakeStopped = false,
    };
}
