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
        var encounter = new MotherBrainEnemyState(body, brain, new MotherBrainWalkCounter(), bursts) { RainbowPhase = MotherBrainRainbowPhase.StartCharging, DroolGenerationEnabled = true, SmallPurpleBreathGenerationEnabled = true, HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck };
        return new MotherBrainRainbowBeamAttackSequence(encounter);
    }

    /// <summary>
    /// One late-fight update with nothing else under way (#2261): no Power Bomb, the room's random number zero and a fresh
    /// shared random sequence for the draws, the ordinary escape text not yet typed, and the room's earthquake not stopped.
    /// </summary>
    /// <param name="cameraCheckpoint">The session's record of what the beam's push hides from the camera.</param>
    /// <param name="enemyFrameCounter">The enemy frame counter's count.</param>
    /// <param name="mainEnemyExecutionCounter">The main enemy pass counter's count.</param>
    internal static RainbowBeamAttackFrame QuietFrame(
        SuperMetroid.Core.Compatibility.RainbowBeamCameraCheckpoint cameraCheckpoint,
        int enemyFrameCounter,
        int mainEnemyExecutionCounter) => new()
    {
        CameraCheckpoint = cameraCheckpoint,
        EnemyFrameCounter = enemyFrameCounter,
        MainEnemyExecutionCounter = mainEnemyExecutionCounter,
        PowerBombActive = false,
        RandomNumberSeed = MotherBrainRandomSample.FromGenerator(0),
        NextRandomNumber = new SuperMetroid.Core.Compatibility.SharedRandom().Generate,
        AlternateEscapeText = false,
        TypewriterFinished = false,
        RoomEarthquakeStopped = false,
    };
}
