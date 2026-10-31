namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The oracles' map between cinematic pre-instructions and their bank-$8B routine addresses.</summary>
internal static class CinematicSpriteFrameBehaviorAccess
{
    /// <summary>Each pre-instruction's bank-$8B address.</summary>
    private static readonly Dictionary<CinematicSpriteFrameBehavior, ushort> Addresses = new()
    {
        [CinematicSpriteFrameBehavior.NoOp] = 0x93d9,
        [CinematicSpriteFrameBehavior.MetroidEgg] = 0xa8e8,
        [CinematicSpriteFrameBehavior.MetroidEggDeleteAfterCrossFade] = 0xa903,
        [CinematicSpriteFrameBehavior.MetroidEggParticle] = 0xa994,
        [CinematicSpriteFrameBehavior.MetroidEggSlimeDrops] = 0xaab3,
        [CinematicSpriteFrameBehavior.BabyMetroidBeingDelivered] = 0xad68,
        [CinematicSpriteFrameBehavior.BabyMetroidBeingExamined] = 0xada6,
        [CinematicSpriteFrameBehavior.IntroMotherBrainCrossFading] = 0xb82e,
        [CinematicSpriteFrameBehavior.IntroRinkaMovingHitsSamus] = 0xb8d8,
        [CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus] = 0xb93b,
        [CinematicSpriteFrameBehavior.ConfusedBabyWaitingForHatch] = 0xba5e,
        [CinematicSpriteFrameBehavior.ConfusedBabyHatched] = 0xba73,
        [CinematicSpriteFrameBehavior.ConfusedBabyIdling] = 0xbb0d,
        [CinematicSpriteFrameBehavior.ConfusedBabyDancing] = 0xbb24,
        [CinematicSpriteFrameBehavior.CeresStars] = 0xbeb5,
        [CinematicSpriteFrameBehavior.CeresLargeAsteroids] = 0xbf35,
        [CinematicSpriteFrameBehavior.CeresUnderAttack] = 0xbf5f,
        [CinematicSpriteFrameBehavior.CeresSmallAsteroid] = 0xbf89,
        [CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex] = 0xbfc6,
        [CinematicSpriteFrameBehavior.CeresStationaryVortexNoOp] = 0xbfd9,
        [CinematicSpriteFrameBehavior.CeresExplosionSpawnerRepeating] = 0xc489,
        [CinematicSpriteFrameBehavior.CeresExplosion] = 0xc582,
        [CinematicSpriteFrameBehavior.Zebes] = 0xc84e,
        [CinematicSpriteFrameBehavior.ZebesSlideSceneAway] = 0xc85d,
        [CinematicSpriteFrameBehavior.ZebesStarsCompletionWait] = 0xc8aa,
        [CinematicSpriteFrameBehavior.ZebesStarsCompletionSlideSceneAway] = 0xc8b9,
        [CinematicSpriteFrameBehavior.ZebesStarsWait] = 0xc8f9,
        [CinematicSpriteFrameBehavior.ZebesStarsSlideSceneAway] = 0xc908,
        [CinematicSpriteFrameBehavior.SuperMetroidIconTopHalf] = 0xf1e7,
        [CinematicSpriteFrameBehavior.SuperMetroidIconBottomHalf] = 0xf227,
        [CinematicSpriteFrameBehavior.ZebesExplosionStarsWaitForFlyaway] = 0xf35a,
        [CinematicSpriteFrameBehavior.ZebesExplosionStarsMoving] = 0xf375,
        [CinematicSpriteFrameBehavior.RewardSuitlessUpperNoOp] = 0xf51c,
        [CinematicSpriteFrameBehavior.RewardBodyFlight] = 0xf528,
        [CinematicSpriteFrameBehavior.RewardHeadFlight] = 0xf57f,
        [CinematicSpriteFrameBehavior.RewardLanding] = 0xf5dd,
    };

    /// <summary>Every mapped bank-$8B pre-instruction address.</summary>
    extension(CinematicSpriteFrameBehavior)
    {
        /// <summary>The bank-$8B address of every pre-instruction.</summary>
        internal static IEnumerable<ushort> NativeAddresses => Addresses.Values;
    }

    /// <summary>A pre-instruction's native address.</summary>
    extension(CinematicSpriteFrameBehavior self)
    {
        /// <summary>The bank-$8B address of this pre-instruction.</summary>
        internal ushort NativeAddress => Addresses[self];
    }

    /// <summary>An optional pre-instruction's native word.</summary>
    extension(CinematicSpriteFrameBehavior? self)
    {
        /// <summary>The bank-$8B address of this pre-instruction, or zero for none.</summary>
        internal ushort NativeAddress => self is { } frameBehavior ? Addresses[frameBehavior] : (ushort)0;
    }

    /// <summary>A sprite's pre-instruction word.</summary>
    extension(IntroDiscoverySprite self)
    {
        /// <summary>The bank-$8B address of the pre-instruction the sprite runs, or zero for none.</summary>
        internal ushort FrameBehaviorPointer => self.FrameBehavior.NativeAddress;
    }
}
