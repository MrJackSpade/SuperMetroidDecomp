namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The per-frame behaviours cinematic sprites run, whether their animations or their owners install them.</summary>
internal enum CinematicSpriteFrameBehavior
{
    /// <summary>The shared behaviour that does nothing.</summary>
    NoOp = 1,
    /// <summary>The Metroid egg's behaviour.</summary>
    MetroidEgg,
    /// <summary>Deletes the Metroid egg once its cross-fade completes.</summary>
    MetroidEggDeleteAfterCrossFade,
    /// <summary>A Metroid egg particle's flight.</summary>
    MetroidEggParticle,
    /// <summary>The Metroid egg's slime drops.</summary>
    MetroidEggSlimeDrops,
    /// <summary>The Baby Metroid being delivered.</summary>
    BabyMetroidBeingDelivered,
    /// <summary>The Baby Metroid being examined.</summary>
    BabyMetroidBeingExamined,
    /// <summary>The intro Mother Brain during the page cross-fade.</summary>
    IntroMotherBrainCrossFading,
    /// <summary>An intro Rinka moving on a course that hits Samus.</summary>
    IntroRinkaMovingHitsSamus,
    /// <summary>An intro Rinka moving on a course that misses Samus.</summary>
    IntroRinkaMovingMissesSamus,
    /// <summary>The confused Baby Metroid waiting for the egg to hatch.</summary>
    ConfusedBabyWaitingForHatch,
    /// <summary>The confused Baby Metroid once the egg has hatched.</summary>
    ConfusedBabyHatched,
    /// <summary>The confused Baby Metroid idling.</summary>
    ConfusedBabyIdling,
    /// <summary>The confused Baby Metroid dancing.</summary>
    ConfusedBabyDancing,
    /// <summary>The Ceres star field's motion.</summary>
    CeresStars,
    /// <summary>The Ceres destruction's large asteroids drifting.</summary>
    CeresLargeAsteroids,
    /// <summary>Ceres station under attack drifting.</summary>
    CeresUnderAttack,
    /// <summary>The Ceres small asteroids drifting.</summary>
    CeresSmallAsteroid,
    /// <summary>The purple space vortex drifting.</summary>
    CeresPurpleSpaceVortex,
    /// <summary>The behaviour the stationary vortex's initializer installs, which does nothing.</summary>
    CeresStationaryVortexNoOp,
    /// <summary>The Ceres explosion spawner's repeating explosions.</summary>
    CeresExplosionSpawnerRepeating,
    /// <summary>A Ceres explosion's motion with the fleeing gunship.</summary>
    CeresExplosion,
    /// <summary>Planet Zebes waiting for the scene to slide.</summary>
    Zebes,
    /// <summary>Planet Zebes sliding the scene away.</summary>
    ZebesSlideSceneAway,
    /// <summary>The completion owner's star sheet waiting for scene sliding.</summary>
    ZebesStarsCompletionWait,
    /// <summary>The completion owner's star sheet sliding the scene away, which additionally hands the cinematic back to game load.</summary>
    ZebesStarsCompletionSlideSceneAway,
    /// <summary>The other star sheets waiting for scene sliding.</summary>
    ZebesStarsWait,
    /// <summary>The other star sheets sliding the scene away.</summary>
    ZebesStarsSlideSceneAway,
    /// <summary>The Super Metroid icon's top half.</summary>
    SuperMetroidIconTopHalf,
    /// <summary>The Super Metroid icon's bottom half.</summary>
    SuperMetroidIconBottomHalf,
    /// <summary>The left stars' wait for the flyaway.</summary>
    ZebesExplosionStarsWaitForFlyaway,
    /// <summary>The left stars' horizontal acceleration, falling through to the text-handoff deletion check.</summary>
    ZebesExplosionStarsMoving,
    /// <summary>The suitless upper body's behaviour, which does nothing.</summary>
    RewardSuitlessUpperNoOp,
    /// <summary>The reward body's flight, which switches to the falling sheet above Y=-80.</summary>
    RewardBodyFlight,
    /// <summary>The reward head's flight, which deletes the old head above Y=-80.</summary>
    RewardHeadFlight,
    /// <summary>The returning reward body, which queues graphics and lands at Y=136.</summary>
    RewardLanding,
}
