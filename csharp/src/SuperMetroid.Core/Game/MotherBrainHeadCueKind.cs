namespace SuperMetroid.Core.Game;

/// <summary>What a Mother Brain brain cue does.</summary>
internal enum MotherBrainHeadCueKind
{
    /// <summary>Shakes the brain for fifty updates.</summary>
    ShakeBrain,
    /// <summary>Spawns a drool drop.</summary>
    SpawnDrool,
    /// <summary>Spawns a big purple breath cloud.</summary>
    SpawnBigPurpleBreath,
    /// <summary>Queues a library-two sound.</summary>
    QueueSound,
    /// <summary>Queues a library-three sound.</summary>
    QueueLibrary3Sound,
    /// <summary>Freezes the neck.</summary>
    DisableNeckMovement,
    /// <summary>Lets the neck move again.</summary>
    EnableNeckMovement,
    /// <summary>Aims the onion rings at Samus.</summary>
    AimOnionRingsAtSamus,
    /// <summary>Aims the onion rings at the Baby Metroid.</summary>
    AimOnionRingsAtBabyMetroid,
    /// <summary>Fires an onion ring along the aimed angle.</summary>
    SpawnOnionRing,
    /// <summary>Lobs a bomb with its afterburn count.</summary>
    SpawnBomb,
    /// <summary>Fires the mouth laser.</summary>
    SpawnLaser,
    /// <summary>Counts an attack on the Baby Metroid.</summary>
    CountBabyMetroidAttack,
    /// <summary>Resets the count of attacks on the Baby Metroid.</summary>
    ResetBabyMetroidAttacks,
    /// <summary>Queues the attack cry unless the Baby Metroid attack has run its course.</summary>
    QueueBabyMetroidAttackSound,
    /// <summary>Sets up the palette, breath and sound for the rainbow-beam charge.</summary>
    PrepareRainbowBeamCharge,
    /// <summary>Spawns a rainbow-beam charge particle.</summary>
    SpawnRainbowBeamChargeParticle,
    /// <summary>Usually holds the phase-two neutral pose another round.</summary>
    MaybeHoldNeutralPhase2,
    /// <summary>Usually holds the phase-three neutral pose another round.</summary>
    MaybeHoldNeutralPhase3,
    /// <summary>Rarely bursts into another round of dying drool.</summary>
    MaybeDroolAgain,
}
