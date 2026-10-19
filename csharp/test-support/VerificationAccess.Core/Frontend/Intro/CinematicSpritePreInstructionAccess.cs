namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The oracles' map between cinematic pre-instructions and their bank-$8B routine addresses.</summary>
internal static class CinematicSpritePreInstructionAccess
{
    /// <summary>Each pre-instruction's bank-$8B address.</summary>
    private static readonly Dictionary<CinematicSpritePreInstruction, ushort> Addresses = new()
    {
        [CinematicSpritePreInstruction.NoOp] = 0x93d9,
        [CinematicSpritePreInstruction.MetroidEgg] = 0xa8e8,
        [CinematicSpritePreInstruction.MetroidEggDeleteAfterCrossFade] = 0xa903,
        [CinematicSpritePreInstruction.MetroidEggParticle] = 0xa994,
        [CinematicSpritePreInstruction.MetroidEggSlimeDrops] = 0xaab3,
        [CinematicSpritePreInstruction.BabyMetroidBeingDelivered] = 0xad68,
        [CinematicSpritePreInstruction.BabyMetroidBeingExamined] = 0xada6,
        [CinematicSpritePreInstruction.IntroMotherBrainCrossFading] = 0xb82e,
        [CinematicSpritePreInstruction.IntroRinkaMovingHitsSamus] = 0xb8d8,
        [CinematicSpritePreInstruction.IntroRinkaMovingMissesSamus] = 0xb93b,
        [CinematicSpritePreInstruction.ConfusedBabyWaitingForHatch] = 0xba5e,
        [CinematicSpritePreInstruction.ConfusedBabyHatched] = 0xba73,
        [CinematicSpritePreInstruction.ConfusedBabyIdling] = 0xbb0d,
        [CinematicSpritePreInstruction.ConfusedBabyDancing] = 0xbb24,
        [CinematicSpritePreInstruction.CeresStars] = 0xbeb5,
        [CinematicSpritePreInstruction.CeresLargeAsteroids] = 0xbf35,
        [CinematicSpritePreInstruction.CeresUnderAttack] = 0xbf5f,
        [CinematicSpritePreInstruction.CeresSmallAsteroid] = 0xbf89,
        [CinematicSpritePreInstruction.CeresPurpleSpaceVortex] = 0xbfc6,
        [CinematicSpritePreInstruction.CeresStationaryVortexNoOp] = 0xbfd9,
        [CinematicSpritePreInstruction.CeresExplosionSpawnerRepeating] = 0xc489,
        [CinematicSpritePreInstruction.CeresExplosion] = 0xc582,
        [CinematicSpritePreInstruction.Zebes] = 0xc84e,
        [CinematicSpritePreInstruction.ZebesSlideSceneAway] = 0xc85d,
        [CinematicSpritePreInstruction.ZebesStarsCompletionWait] = 0xc8aa,
        [CinematicSpritePreInstruction.ZebesStarsCompletionSlideSceneAway] = 0xc8b9,
        [CinematicSpritePreInstruction.ZebesStarsWait] = 0xc8f9,
        [CinematicSpritePreInstruction.ZebesStarsSlideSceneAway] = 0xc908,
        [CinematicSpritePreInstruction.SuperMetroidIconTopHalf] = 0xf1e7,
        [CinematicSpritePreInstruction.SuperMetroidIconBottomHalf] = 0xf227,
        [CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway] = 0xf35a,
        [CinematicSpritePreInstruction.ZebesExplosionStarsMoving] = 0xf375,
        [CinematicSpritePreInstruction.RewardSuitlessUpperNoOp] = 0xf51c,
        [CinematicSpritePreInstruction.RewardBodyFlight] = 0xf528,
        [CinematicSpritePreInstruction.RewardHeadFlight] = 0xf57f,
        [CinematicSpritePreInstruction.RewardLanding] = 0xf5dd,
    };

    /// <summary>Every mapped bank-$8B pre-instruction address.</summary>
    extension(CinematicSpritePreInstruction)
    {
        /// <summary>The bank-$8B address of every pre-instruction.</summary>
        internal static IEnumerable<ushort> NativeAddresses => Addresses.Values;
    }

    /// <summary>A pre-instruction's native address.</summary>
    extension(CinematicSpritePreInstruction self)
    {
        /// <summary>The bank-$8B address of this pre-instruction.</summary>
        internal ushort NativeAddress => Addresses[self];
    }

    /// <summary>An optional pre-instruction's native word.</summary>
    extension(CinematicSpritePreInstruction? self)
    {
        /// <summary>The bank-$8B address of this pre-instruction, or zero for none.</summary>
        internal ushort NativeAddress => self is { } preInstruction ? Addresses[preInstruction] : (ushort)0;
    }

    /// <summary>A sprite's pre-instruction word.</summary>
    extension(IntroDiscoverySprite self)
    {
        /// <summary>The bank-$8B address of the pre-instruction the sprite runs, or zero for none.</summary>
        internal ushort PreInstructionPointer => self.PreInstruction.NativeAddress;
    }
}
