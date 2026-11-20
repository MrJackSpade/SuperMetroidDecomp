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
        var body = new RoomEnemyPlace(0, StandaloneFixtureSpecies.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>());
        var brain = new RoomEnemyPlace(1, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>()) { Health = 3000 };
        var encounter = new MotherBrainEnemyState(body.BoundFor(EnemyDefinitionId.MotherBrainBody), brain, new MotherBrainWalkCounter(), bursts, new MotherBrainBrainAnimation()) { RainbowPhase = MotherBrainRainbowPhase.StartCharging, DroolGenerationEnabled = true, SmallPurpleBreathGenerationEnabled = true, HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck };
        return new MotherBrainRainbowBeamAttackSequence(encounter);
    }

    /// <summary>
    /// Gives <paramref name="baby"/> a standalone place that holds <paramref name="leftInPlace"/> from its previous occupant,
    /// as the final beam's spawn does, and initializes the Baby there (#2572): its position and health live in the place.
    /// </summary>
    internal static RoomEnemyPlace Arrive(BabyMetroidCutsceneState baby, FixedPosition<RoomPixels> leftInPlace)
    {
        var place = new RoomEnemyPlace(2, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>()) { Position = leftInPlace };
        Ensure.NotNull(baby).Arrive(place);
        return place;
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
        AlternateEscapeText = false,
        TypewriterFinished = false,
        RoomEarthquakeStopped = false,
    };
}
