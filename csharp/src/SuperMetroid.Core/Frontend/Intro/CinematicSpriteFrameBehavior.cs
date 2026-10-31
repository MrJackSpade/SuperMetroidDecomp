namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The bank-$8B pre-instructions cinematic sprites run, whether their lists or their owners install them.</summary>
internal enum CinematicSpriteFrameBehavior
{
    /// <summary><c>RTS_8B93D9</c>, the shared no-op pre-instruction.</summary>
    NoOp = 1,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_MetroidEgg</c> at $8B:A8E8.</summary>
    MetroidEgg,
    /// <summary><c>FrameBehavior_MetroidEgg_DeleteAfterCrossFade</c> at $8B:A903.</summary>
    MetroidEggDeleteAfterCrossFade,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_MetroidEggParticle</c> at $8B:A994.</summary>
    MetroidEggParticle,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_MetroidEggSlimeDrops</c> at $8B:AAB3.</summary>
    MetroidEggSlimeDrops,
    /// <summary><c>PreInst_CinematicSpriteObject_BabyMetroidBeingDelivered</c> at $8B:AD68.</summary>
    BabyMetroidBeingDelivered,
    /// <summary><c>PreInst_CinematicSpriteObject_BabyMetroidBeingExamined</c> at $8B:ADA6.</summary>
    BabyMetroidBeingExamined,
    /// <summary><c>FrameBehavior_IntroMotherBrain_CrossFading</c> at $8B:B82E.</summary>
    IntroMotherBrainCrossFading,
    /// <summary><c>FrameBehavior_IntroRinka_Moving_HitsSamus</c> at $8B:B8D8.</summary>
    IntroRinkaMovingHitsSamus,
    /// <summary><c>FrameBehavior_IntroRinka_Moving_MissesSamus</c> at $8B:B93B.</summary>
    IntroRinkaMovingMissesSamus,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_ConfusedBabyMetroid</c> at $8B:BA5E: waiting for the egg to hatch.</summary>
    ConfusedBabyWaitingForHatch,
    /// <summary><c>FrameBehavior_ConfusedBabyMetroid_Hatched</c> at $8B:BA73.</summary>
    ConfusedBabyHatched,
    /// <summary><c>FrameBehavior_ConfusedBabyMetroid_Idling</c> at $8B:BB0D.</summary>
    ConfusedBabyIdling,
    /// <summary><c>FrameBehavior_ConfusedBabyMetroid_Dancing</c> at $8B:BB24.</summary>
    ConfusedBabyDancing,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_CeresStars</c> at $8B:BEB5.</summary>
    CeresStars,
    /// <summary><c>PreInst_CinematicSpriteObject_CeresExplosionLargeAsteroids</c> at $8B:BF35.</summary>
    CeresLargeAsteroids,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_CeresUnderAttack</c> at $8B:BF5F.</summary>
    CeresUnderAttack,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_CeresSmallAsteroid</c> at $8B:BF89.</summary>
    CeresSmallAsteroid,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_CeresPurpleSpaceVortex</c> at $8B:BFC6.</summary>
    CeresPurpleSpaceVortex,
    /// <summary><c>RTS_8BBFD9</c>, the no-op the stationary vortex's initializer installs.</summary>
    CeresStationaryVortexNoOp,
    /// <summary>The Ceres explosion spawner's repeating explosions at $8B:C489.</summary>
    CeresExplosionSpawnerRepeating,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_CeresExplosion</c> at $8B:C582.</summary>
    CeresExplosion,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_Zebes</c> at $8B:C84E.</summary>
    Zebes,
    /// <summary><c>FrameBehavior_Zebes_SlideSceneAway</c> at $8B:C85D.</summary>
    ZebesSlideSceneAway,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_ZebesStars5</c> at $8B:C8AA: the completion owner waits for scene sliding.</summary>
    ZebesStarsCompletionWait,
    /// <summary><c>FrameBehavior_ZebesStars5_SlideSceneAway</c> at $8B:C8B9, which additionally installs game load at C8F2.</summary>
    ZebesStarsCompletionSlideSceneAway,
    /// <summary><c>FrameBehavior_CinematicSpriteObject_ZebesStars_2_3_4</c> at $8B:C8F9: waits for scene sliding.</summary>
    ZebesStarsWait,
    /// <summary><c>FrameBehavior_ZebesStars_2_3_4_SlideSceneAway</c> at $8B:C908.</summary>
    ZebesStarsSlideSceneAway,
    /// <summary>The Super Metroid icon's top half at $8B:F1E7.</summary>
    SuperMetroidIconTopHalf,
    /// <summary>The Super Metroid icon's bottom half at $8B:F227.</summary>
    SuperMetroidIconBottomHalf,
    /// <summary>The left stars' wait for the flyaway at $8B:F35A.</summary>
    ZebesExplosionStarsWaitForFlyaway,
    /// <summary>The left stars' horizontal acceleration at $8B:F375, falling through to the text-handoff deletion check.</summary>
    ZebesExplosionStarsMoving,
    /// <summary><c>RTS_8BF51C</c>, the suitless upper body's no-op.</summary>
    RewardSuitlessUpperNoOp,
    /// <summary>The reward body's flight at $8B:F528, which switches to the falling sheet above Y=-80.</summary>
    RewardBodyFlight,
    /// <summary>The reward head's flight at $8B:F57F, which deletes the old head above Y=-80.</summary>
    RewardHeadFlight,
    /// <summary>The returning reward body at $8B:F5DD, which queues graphics and lands at Y=136.</summary>
    RewardLanding,
}
