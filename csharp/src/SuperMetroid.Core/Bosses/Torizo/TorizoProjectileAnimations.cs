using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's projectile animations (#641, #1788, #2011): each animation's steps, each frame the installed
/// <see cref="EnemyProjectileFrameId"/> it shows and each cue a <see cref="TorizoProjectileCue"/> with the values it acts on; the phases it plays them in, each a run of its steps played a number of times; and whether it
/// then loops or holds. The runs several animations play alike (the drool's release and fall, the egg's hatch, the statue
/// fragments' crack) are one array of steps each reuses.
/// </summary>
internal static class TorizoProjectileAnimations
{
    /// <summary>
    /// A drool drop's release: it starts falling, harmless and in front of Samus, then moves behind her. The shared runs are
    /// declared before the steps that spread them in, so static initialization builds them first.
    /// </summary>
    private static readonly AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] DroolRelease =
    [
        Cue(new TorizoProjectileCue.Signal(K.StartDroolFall)),
        Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
        Show(EnemyProjectileFrameId.TorizoDroolDrop, 5),
        Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus)),
    ];

    /// <summary>A drool drop's falling pose, which its animation loops on while it falls.</summary>
    private static readonly AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] DroolFalling =
    [
        Show(EnemyProjectileFrameId.TorizoDroolDropTwin, 64),
    ];

    /// <summary>A cracked egg's hatch: it turns harmful and shot-blocking, then continues in its hatched animation.</summary>
    private static readonly AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] EggHatch =
    [
        Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
        Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.BlocksShots)),
        Cue(new TorizoProjectileCue.Signal(K.Hatch)),
    ];

    /// <summary>A statue fragment cracking off the hand: the impact sound, then its fall.</summary>
    private static readonly AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] StatueFragmentCracking =
    [
        Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
        Cue(new TorizoProjectileCue.Signal(K.StartFragmentFall)),
    ];

    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<TorizoProjectileSegment, EnemyProjectileFrameId, TorizoProjectileCue> Segments { get; } = new(
        new Dictionary<TorizoProjectileSegment, AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[]>
    {
        [P.LowHealthExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 0)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoExplosionSpark, 2),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareBall, 2),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRingCored, 3),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRing, 3),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRingFading, 2),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DeathExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.Signal(K.MayTurnToSmoke)),
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 16)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoExplosionSparkTwin, 4),
            Show(EnemyProjectileFrameId.TorizoExplosionFireballSmall, 6),
            Show(EnemyProjectileFrameId.TorizoExplosionFireballLarge, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionFlamingRing, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionStarburst, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionEmbers, 6),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DeathSmoke] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 4)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoExplosionFireCloud, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionFireCloudSpreading, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionFireCloudBreaking, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionFireCloudEmbers, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DroolFourFrameDelay] = [Show(EnemyProjectileFrameId.TorizoDroolBlank, 2), Show(EnemyProjectileFrameId.TorizoDroolBlankTwin, 2), .. DroolRelease, .. DroolFalling],
        [P.DroolTwoFrameDelay] = [Show(EnemyProjectileFrameId.TorizoDroolBlankTwin, 2), .. DroolRelease, .. DroolFalling],
        [P.DroolNoDelay] = [.. DroolRelease, .. DroolFalling],
        [P.DroolWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DroolFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.TorizoDroolSplashRising, 8),
            Show(EnemyProjectileFrameId.TorizoDroolSplashSpreading, 8),
            Show(EnemyProjectileFrameId.TorizoDroolSplashFlat, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ExplosiveSwipe] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BombTorizoSwipe, Priority: SoundRequestPriority.WhenIdle)),
            Show(EnemyProjectileFrameId.TorizoSwipeSpark, 5),
            Show(EnemyProjectileFrameId.TorizoSwipeFlareBall, 5),
            Show(EnemyProjectileFrameId.TorizoSwipeFlareRingCored, 5),
            Show(EnemyProjectileFrameId.TorizoSwipeFlareRing, 5),
            Show(EnemyProjectileFrameId.TorizoSwipeFlareRingFading, 5),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.FirstStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoStatueMossClump, 128), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoStatueMossClumpTwin, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SecondStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoStatueRubbleLitRight, 120), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoStatueRubbleLitRightTwin, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.ThirdStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoStatueRubbleLitCenter, 112), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoStatueRubbleLitCenterTwin, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.FourthStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoStatueRubbleLitSpot, 104), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoStatueRubbleLitSpotTwin, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.FifthStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoStatueRubbleMossyTop, 96), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoStatueRubbleMossyTopTwin, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SixthStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoStatueRubbleMossSpeck, 88), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoStatueRubbleMossSpeckTwin, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SeventhStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoStatueRubbleLitBase, 80), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoStatueRubbleLitBaseTwin, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.EighthStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoStatueSlab, 72), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoStatueSlabTwin, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.ChozoOrbMovingLeft] = [Show(EnemyProjectileFrameId.TorizoChozoOrbFacingLeft, 85)],
        [P.ChozoOrbMovingRight] = [Show(EnemyProjectileFrameId.TorizoChozoOrbFacingRight, 85)],
        [P.ChozoOrbWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(EnemyProjectileFrameId.TorizoChozoOrbSpark, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareBall, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingCored, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRing, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingFading, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ChozoOrbFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameBurst, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameDome, 5),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameColumnShort, 6),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameColumnTall, 7),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameBurstAboveDome, 8),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameEmbersAboveDome, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ChozoOrbShot] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(EnemyProjectileFrameId.TorizoChozoOrbSparkTwin, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareBallTwin, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingCoredTwin, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingTwin, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingFadingTwin, 4),
            Cue(new TorizoProjectileCue.Signal(K.SpawnOrbDrops)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.SonicBoomFiredLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoSonicBoomLeftSmall, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomLeftMedium, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomLeftLarge, 80),
        ],
        [P.SonicBoomFiredRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoSonicBoomRightSmall, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomRightMedium, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomRightLarge, 80),
        ],
        [P.SonicBoomWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.StopBlockingShots)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamus)),
            Cue(new TorizoProjectileCue.Signal(K.DrawInFront)),
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 5), Center: new(0, 0)))),
            Show(EnemyProjectileFrameId.TorizoSonicBoomSpark, 2),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareBall, 2),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRingCored, 3),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRing, 3),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRingFading, 2),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.LandingDustRightFoot] =
        [
            Show(EnemyProjectileFrameId.TorizoLandingDustRightDustPuff, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustRightDustPuffSmall, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustRightDustPuffThin, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustRightDustPuffFaint, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.LandingDustLeftFoot] =
        [
            Show(EnemyProjectileFrameId.TorizoLandingDustLeftDustPuff, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustLeftDustPuffSmall, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustLeftDustPuffThin, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustLeftDustPuffFaint, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EggBouncingLeft] = [Show(EnemyProjectileFrameId.GoldenTorizoEggWholeLeft, 48)],
        [P.EggHatchingLeft] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggCrackedLeft, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggSplittingLeft, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchingLeft, 4),
            .. EggHatch,
        ],
        [P.EggBouncingRight] = [Show(EnemyProjectileFrameId.GoldenTorizoEggWholeRight, 48)],
        [P.EggHatchingRight] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggCrackedRight, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggSplittingRight, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchingRight, 4),
            .. EggHatch,
        ],
        [P.EggHatchedLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.RisingSqueaks, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.Signal(K.StartEggCharge)),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftClawsRaised, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftClawsLowered, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftHunched, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftClawsLoweredTwin, 6),
        ],
        [P.EggHatchedRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.RisingSqueaks, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.Signal(K.StartEggCharge)),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightClawsRaised, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightClawsLowered, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightHunched, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightClawsLoweredTwin, 6),
        ],
        [P.EggBreakLeft] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftCrumbling, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftCollapsing, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftSlumped, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftRemains, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftRemainsFlat, 10),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EggBreakRight] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightCrumbling, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightCollapsing, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightSlumped, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightRemains, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightRemainsFlat, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.SuperMissileRight] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown, 48),
            Cue(new TorizoProjectileCue.Signal(K.AimSuperMissileTowardSamus)),
            Cue(new TorizoProjectileCue.Signal(K.ThrowSuperMissile)),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUp, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownTwin, 2),
        ],
        [P.SuperMissileLeft] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownTwinB, 48),
            Cue(new TorizoProjectileCue.Signal(K.AimSuperMissileAwayFromSamus)),
            Cue(new TorizoProjectileCue.Signal(K.ThrowSuperMissile)),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownLeftTwin, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingLeftTwin, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpLeftTwin, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpTwin, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpRightTwin, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingRightTwin, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownRightTwin, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownTwinC, 2),
        ],
        [P.SuperMissileImpact] =
        [
            Cue(new TorizoProjectileCue.SetRadius(Radius: new(16, 16))),
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballSmall, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballLarge, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileFlamingRing, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballHuge, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileWideRing, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileWideRingFading, 5),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeamWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamSpark, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareBall, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingCored, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRing, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingFading, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeamFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamBlank, 8),
            Cue(new TorizoProjectileCue.Signal(K.ReplayWhileEyeBeamExplosionsDisabled)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameBurst, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameDome, 5),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameColumnShort, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameColumnTall, 7),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameBurstAboveDome, 8),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameEmbersAboveDome, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeam] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingFadingTwin, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingTwin, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingCoredTwin, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareBallTwin, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamSparkTwin, 1),
        ],
    });

    /// <summary>How each animation plays its own steps.</summary>
    internal static IReadOnlyDictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>> StartingAt { get; } = new Dictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>>
    {
        [P.LowHealthExplosion] = new([new(P.LowHealthExplosion, 0, 2, 1), new(P.LowHealthExplosion, 2, 8, 3), new(P.LowHealthExplosion, 10, 1, 1)], AnimationEnd.Holds),
        [P.DeathExplosion] = new([new(P.DeathExplosion, 0, 3, 1), new(P.DeathExplosion, 3, 9, 2), new(P.DeathExplosion, 12, 1, 1)], AnimationEnd.Holds),
        [P.DeathSmoke] = new([new(P.DeathSmoke, 0, 7, 2), new(P.DeathSmoke, 7, 1, 1)], AnimationEnd.Holds),
        [P.DroolFourFrameDelay] = new([new(P.DroolFourFrameDelay, 0, 1, 1), new(P.DroolFourFrameDelay, 1, 1, 1), new(P.DroolFourFrameDelay, 2, 4, 1), new(P.DroolFourFrameDelay, 6, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [P.DroolTwoFrameDelay] = new([new(P.DroolTwoFrameDelay, 0, 1, 1), new(P.DroolTwoFrameDelay, 1, 4, 1), new(P.DroolTwoFrameDelay, 5, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.DroolNoDelay] = new([new(P.DroolNoDelay, 0, 4, 1), new(P.DroolNoDelay, 4, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DroolWallImpact] = new([new(P.DroolWallImpact, 0, 2, 1)], AnimationEnd.Holds),
        [P.DroolFloorImpact] = new([new(P.DroolFloorImpact, 0, 5, 1)], AnimationEnd.Holds),
        [P.ExplosiveSwipe] = new([new(P.ExplosiveSwipe, 0, 7, 1)], AnimationEnd.Holds),
        [P.FirstStatueFragment] = new([new(P.FirstStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.SecondStatueFragment] = new([new(P.SecondStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.ThirdStatueFragment] = new([new(P.ThirdStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.FourthStatueFragment] = new([new(P.FourthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.FifthStatueFragment] = new([new(P.FifthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.SixthStatueFragment] = new([new(P.SixthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.SeventhStatueFragment] = new([new(P.SeventhStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.EighthStatueFragment] = new([new(P.EighthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.ChozoOrbMovingLeft] = new([new(P.ChozoOrbMovingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChozoOrbMovingRight] = new([new(P.ChozoOrbMovingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChozoOrbWallImpact] = new([new(P.ChozoOrbWallImpact, 0, 8, 1)], AnimationEnd.Holds),
        [P.ChozoOrbFloorImpact] = new([new(P.ChozoOrbFloorImpact, 0, 11, 1)], AnimationEnd.Holds),
        [P.ChozoOrbShot] = new([new(P.ChozoOrbShot, 0, 9, 1)], AnimationEnd.Holds),
        [P.SonicBoomFiredLeft] = new([new(P.SonicBoomFiredLeft, 0, 3, 1), new(P.SonicBoomFiredLeft, 3, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SonicBoomFiredRight] = new([new(P.SonicBoomFiredRight, 0, 3, 1), new(P.SonicBoomFiredRight, 3, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SonicBoomWallImpact] = new([new(P.SonicBoomWallImpact, 0, 4, 1), new(P.SonicBoomWallImpact, 4, 7, 5), new(P.SonicBoomWallImpact, 11, 1, 1)], AnimationEnd.Holds),
        [P.LandingDustRightFoot] = new([new(P.LandingDustRightFoot, 0, 8, 1)], AnimationEnd.Holds),
        [P.LandingDustLeftFoot] = new([new(P.LandingDustLeftFoot, 0, 8, 1)], AnimationEnd.Holds),
        [P.EggBouncingLeft] = new([new(P.EggBouncingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.EggHatchingLeft] = new([new(P.EggHatchingLeft, 0, 4, 1), new(P.EggHatchingLeft, 4, 3, 1)], AnimationEnd.Holds),
        [P.EggBouncingRight] = new([new(P.EggBouncingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EggHatchingRight] = new([new(P.EggHatchingRight, 0, 4, 1), new(P.EggHatchingRight, 4, 3, 1)], AnimationEnd.Holds),
        [P.EggHatchedLeft] = new([new(P.EggHatchedLeft, 0, 2, 1), new(P.EggHatchedLeft, 2, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.EggHatchedRight] = new([new(P.EggHatchedRight, 0, 2, 1), new(P.EggHatchedRight, 2, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.EggBreakLeft] = new([new(P.EggBreakLeft, 0, 7, 1)], AnimationEnd.Holds),
        [P.EggBreakRight] = new([new(P.EggBreakRight, 0, 7, 1)], AnimationEnd.Holds),
        [P.SuperMissileRight] = new([new(P.SuperMissileRight, 0, 3, 1), new(P.SuperMissileRight, 3, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SuperMissileLeft] = new([new(P.SuperMissileLeft, 0, 3, 1), new(P.SuperMissileLeft, 3, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SuperMissileImpact] = new([new(P.SuperMissileImpact, 0, 12, 1)], AnimationEnd.Holds),
        [P.EyeBeamWallImpact] = new([new(P.EyeBeamWallImpact, 0, 7, 1)], AnimationEnd.Holds),
        [P.EyeBeamFloorImpact] = new([new(P.EyeBeamFloorImpact, 0, 1, 1), new(P.EyeBeamFloorImpact, 1, 11, 1)], AnimationEnd.Holds),
        [P.EyeBeam] = new([new(P.EyeBeam, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<TorizoProjectileSegment> StartingWith(TorizoProjectileSegment animation) => StartingAt[animation];

    /// <summary>A cue's continuation into <paramref name="animation"/>, from its start.</summary>
    internal static AnimationContinuation<TorizoProjectileSegment> ContinueWith(TorizoProjectileSegment animation) =>
        new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>
    /// A replay cue's continuation (<see cref="TorizoProjectileCueKind.ReplayWhileEyeBeamExplosionsDisabled"/>): the start of
    /// the phase <paramref name="pastCue"/>, the place just past the cue, stands in, so the run the cue is in plays again from
    /// its first step.
    /// </summary>
    internal static AnimationContinuation<TorizoProjectileSegment> ReplayPhase(AnimationPlayback<TorizoProjectileSegment> pastCue) =>
        new(Ensure.NotNull(pastCue.Animation), AnimationPlace.Before(pastCue.Place.Phase));

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>.Event(cue);
}
