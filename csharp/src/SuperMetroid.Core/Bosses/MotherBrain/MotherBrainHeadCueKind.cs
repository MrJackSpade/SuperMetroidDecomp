namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a cue every Mother Brain brain animation raises to the same effect on her encounter does (#2554).</summary>
internal enum MotherBrainHeadCueKind
{
    /// <summary>Shakes the brain for <see cref="MotherBrainFightDefinitions.BrainShakeUpdates"/>.</summary>
    ShakeBrain,
    /// <summary>Spawns a drool drop.</summary>
    SpawnDrool,
    /// <summary>Spawns a big purple breath cloud.</summary>
    SpawnBigPurpleBreath,
    /// <summary>Freezes the neck.</summary>
    DisableNeckMovement,
    /// <summary>Lets the neck move again.</summary>
    EnableNeckMovement,
    /// <summary>Aims the onion rings at Samus.</summary>
    AimOnionRingsAtSamus,
    /// <summary>Fires an onion ring along the aimed angle.</summary>
    SpawnOnionRing,
    /// <summary>Fires the mouth laser.</summary>
    SpawnLaser,
    /// <summary>Sets up the colours, breath and sound for the rainbow-beam charge.</summary>
    PrepareRainbowBeamCharge,
    /// <summary>Spawns a rainbow-beam charge particle.</summary>
    SpawnRainbowBeamChargeParticle,
}
