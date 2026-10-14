namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The pre-instructions the cinematic sprite lists install, by their bank-$8B routines.</summary>
internal enum CinematicSpritePreInstruction : ushort
{
    /// <summary>No pre-instruction installed by a list.</summary>
    None = 0,
    /// <summary><c>PreInstruction_MetroidEgg_DeleteAfterCrossFade</c> at $8B:A903.</summary>
    MetroidEggDeleteAfterCrossFade = 0xa903,
    /// <summary><c>PreInstruction_IntroMotherBrain_CrossFading</c> at $8B:B82E.</summary>
    IntroMotherBrainCrossFading = 0xb82e,
    /// <summary>The Ceres explosion spawner's repeating explosions at $8B:C489.</summary>
    CeresExplosionSpawnerRepeating = 0xc489,
    /// <summary>The left stars' wait for the flyaway at $8B:F35A.</summary>
    ZebesExplosionStarsWaitForFlyaway = 0xf35a,
    /// <summary>The reward body's flight at $8B:F528.</summary>
    RewardBodyFlight = 0xf528,
    /// <summary>The reward head's flight at $8B:F57F.</summary>
    RewardHeadFlight = 0xf57f,
}
