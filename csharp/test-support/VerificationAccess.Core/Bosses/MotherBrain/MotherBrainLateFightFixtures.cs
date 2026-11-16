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
        var body = new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>());
        var brain = new RoomEnemyPlace(1, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>()) { Health = 3000 };
        var encounter = new MotherBrainEnemyState(body, new MotherBrainWalkCounter(), bursts)
        {
            Head = brain,
            RainbowPhase = MotherBrainRainbowPhase.StartCharging,
            DroolGenerationEnabled = true,
            SmallPurpleBreathGenerationEnabled = true,
            HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck,
        };
        return new MotherBrainRainbowBeamAttackSequence(encounter);
    }
}
