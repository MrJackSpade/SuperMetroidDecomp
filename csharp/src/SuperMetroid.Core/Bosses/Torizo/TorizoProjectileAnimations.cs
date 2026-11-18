using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's projectile animations (#641, #1788, #2011): each animation's steps, each frame the installed
/// <see cref="EnemyProjectileFrameId"/> it shows and each cue a <see cref="TorizoProjectileCue"/> with the values it acts on;
/// the segments each animation plays whole, once or a number of times (#2235); and whether it then loops or holds. The runs
/// several animations play alike (the drool's release and fall, the egg's hatch, the explosions' deletion) are one segment
/// each plays, and a still picture is a held frame.
/// </summary>
internal static class TorizoProjectileAnimations
{
    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<TorizoProjectileSegment, EnemyProjectileFrameId, TorizoProjectileCue> Segments { get; } = new(
        new Dictionary<TorizoProjectileSegment, AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[]>
    {
        [P.LowHealthExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamusAndDrawInFront)),
        ],
        [P.LowHealthExplosionBurst] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 0)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoExplosionSpark, 2),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareBall, 2),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRingCored, 3),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRing, 3),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRingFading, 2),
        ],
        [P.DeathExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamusAndDrawInFront)),
            Cue(new TorizoProjectileCue.Signal(K.MayTurnToSmoke)),
        ],
        [P.DeathExplosionBurst] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 16)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoDeathExplosionSpark, 4),
            Show(EnemyProjectileFrameId.TorizoExplosionFireballSmall, 6),
            Show(EnemyProjectileFrameId.TorizoExplosionFireballLarge, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionFlamingRing, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionStarburst, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionEmbers, 6),
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
        ],
        [P.DroolFourFrameDelay] = [Show(EnemyProjectileFrameId.TorizoDroolBlank, 2)],
        [P.DroolTwoFrameDelay] = [Show(EnemyProjectileFrameId.TorizoDroolBlank, 2)],
        [P.DroolNoDelay] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StartDroolFall)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamusAndDrawInFront)),
            Show(EnemyProjectileFrameId.TorizoDroolDrop, 5),
            Cue(new TorizoProjectileCue.Signal(K.DrawBehindSamus)),
        ],
        [P.DroolFalling] = [Hold(EnemyProjectileFrameId.TorizoDroolDrop)],
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
        [P.StatueFragmentTopMiddle] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueMossClump, restUpdates: 128),
        [P.StatueFragmentTopRight] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleLitRight, restUpdates: 120),
        [P.StatueFragmentMiddleLeft] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleLitCenter, restUpdates: 112),
        [P.StatueFragmentCentre] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleLitSpot, restUpdates: 104),
        [P.StatueFragmentMiddleRight] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleMossyTop, restUpdates: 96),
        [P.StatueFragmentBottomLeft] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleMossSpeck, restUpdates: 88),
        [P.StatueFragmentBottomMiddle] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleLitBase, restUpdates: 80),
        [P.StatueFragmentBottomRight] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueSlab, restUpdates: 72),
        [P.ChozoOrbMovingLeft] = [Hold(EnemyProjectileFrameId.TorizoChozoOrbFacingLeft)],
        [P.ChozoOrbMovingRight] = [Hold(EnemyProjectileFrameId.TorizoChozoOrbFacingRight)],
        [P.ChozoOrbWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
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
            Cue(new TorizoProjectileCue.Signal(K.TurnHarmful)),
            Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
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
            Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
            Show(EnemyProjectileFrameId.TorizoChozoOrbSpark, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareBall, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingCored, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRing, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingFading, 4),
            Cue(new TorizoProjectileCue.Signal(K.SpawnOrbDrops)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.SonicBoomFiredLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoSonicBoomLeftSmall, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomLeftMedium, 6),
        ],
        [P.SonicBoomFlyingLeft] = [Hold(EnemyProjectileFrameId.TorizoSonicBoomLeftLarge)],
        [P.SonicBoomFiredRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoSonicBoomRightSmall, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomRightMedium, 6),
        ],
        [P.SonicBoomFlyingRight] = [Hold(EnemyProjectileFrameId.TorizoSonicBoomRightLarge)],
        [P.SonicBoomWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.StopBlockingShots)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamus)),
            Cue(new TorizoProjectileCue.Signal(K.DrawInFront)),
        ],
        [P.SonicBoomWallImpactBurst] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 5), Center: new(0, 0)))),
            Show(EnemyProjectileFrameId.TorizoSonicBoomSpark, 2),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareBall, 2),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRingCored, 3),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRing, 3),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRingFading, 2),
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
        [P.EggBouncingLeft] = [Hold(EnemyProjectileFrameId.GoldenTorizoEggWholeLeft)],
        [P.EggHatchingLeft] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggCrackedLeft, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggSplittingLeft, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchingLeft, 4),
        ],
        [P.EggBouncingRight] = [Hold(EnemyProjectileFrameId.GoldenTorizoEggWholeRight)],
        [P.EggHatchingRight] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggCrackedRight, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggSplittingRight, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchingRight, 4),
        ],
        [P.EggHatchedLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.RisingSqueaks, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.Signal(K.StartEggCharge)),
        ],
        [P.HatchlingChargingLeft] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftClawsRaised, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftClawsLowered, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftHunched, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingLeftClawsLowered, 6),
        ],
        [P.EggHatchedRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.RisingSqueaks, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.Signal(K.StartEggCharge)),
        ],
        [P.HatchlingChargingRight] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightClawsRaised, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightClawsLowered, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightHunched, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoHatchlingRightClawsLowered, 6),
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
        ],
        [P.SuperMissileSpinningRight] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUp, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown, 2),
        ],
        [P.SuperMissileLeft] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown, 48),
            Cue(new TorizoProjectileCue.Signal(K.AimSuperMissileAwayFromSamus)),
            Cue(new TorizoProjectileCue.Signal(K.ThrowSuperMissile)),
        ],
        [P.SuperMissileSpinningLeft] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpLeft, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUp, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUpRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDownRight, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown, 2),
        ],
        [P.SuperMissileImpact] =
        [
            Cue(new TorizoProjectileCue.SetRadius(Radius: new(16, 16))),
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
            Cue(new TorizoProjectileCue.Signal(K.TurnHarmfulAndStopBlockingShots)),
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
        ],
        [P.EyeBeamFloorImpactFlames] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamBlank, 8),
            Cue(new TorizoProjectileCue.Signal(K.ReplayWhileEyeBeamExplosionsDisabled)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameBurst, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameDome, 5),
            Cue(new TorizoProjectileCue.Signal(K.TurnHarmful)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameColumnShort, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameColumnTall, 7),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameBurstAboveDome, 8),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameEmbersAboveDome, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeam] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingFading, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRing, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingCored, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareBall, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamSpark, 1),
        ],
        [P.EggHatch] =
        [
            Cue(new TorizoProjectileCue.Signal(K.TurnHarmful)),
            Cue(new TorizoProjectileCue.Signal(K.BlockShots)),
            Cue(new TorizoProjectileCue.Signal(K.Hatch)),
        ],
        [P.Deletion] = [Cue(new TorizoProjectileCue.Signal(K.Delete))],
    });

    /// <summary>
    /// The animation that starts at each segment a spawn, a reaction or a cue starts (#2235): the segments it plays whole,
    /// once or a number of times, and whether it then loops or holds. An animation that reaches a still picture holds it
    /// for good (#2353).
    /// </summary>
    internal static IReadOnlyDictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>> StartingAt { get; } = new Dictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>>
    {
        [P.LowHealthExplosion] = new([Segments.Whole(P.LowHealthExplosion), Segments.Repeated(P.LowHealthExplosionBurst, 3), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [P.DeathExplosion] = new([Segments.Whole(P.DeathExplosion), Segments.Repeated(P.DeathExplosionBurst, 2), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [P.DeathSmoke] = new([Segments.Repeated(P.DeathSmoke, 2), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [P.DroolFourFrameDelay] = new([Segments.Whole(P.DroolFourFrameDelay), Segments.Whole(P.DroolTwoFrameDelay), Segments.Whole(P.DroolNoDelay), Segments.Whole(P.DroolFalling)], AnimationEnd.Holds),
        [P.DroolTwoFrameDelay] = new([Segments.Whole(P.DroolTwoFrameDelay), Segments.Whole(P.DroolNoDelay), Segments.Whole(P.DroolFalling)], AnimationEnd.Holds),
        [P.DroolNoDelay] = new([Segments.Whole(P.DroolNoDelay), Segments.Whole(P.DroolFalling)], AnimationEnd.Holds),
        [P.DroolWallImpact] = new([Segments.Whole(P.DroolWallImpact)], AnimationEnd.Holds),
        [P.DroolFloorImpact] = new([Segments.Whole(P.DroolFloorImpact)], AnimationEnd.Holds),
        [P.ExplosiveSwipe] = new([Segments.Whole(P.ExplosiveSwipe)], AnimationEnd.Holds),
        [P.StatueFragmentTopMiddle] = new([Segments.Whole(P.StatueFragmentTopMiddle)], AnimationEnd.Holds),
        [P.StatueFragmentTopRight] = new([Segments.Whole(P.StatueFragmentTopRight)], AnimationEnd.Holds),
        [P.StatueFragmentMiddleLeft] = new([Segments.Whole(P.StatueFragmentMiddleLeft)], AnimationEnd.Holds),
        [P.StatueFragmentCentre] = new([Segments.Whole(P.StatueFragmentCentre)], AnimationEnd.Holds),
        [P.StatueFragmentMiddleRight] = new([Segments.Whole(P.StatueFragmentMiddleRight)], AnimationEnd.Holds),
        [P.StatueFragmentBottomLeft] = new([Segments.Whole(P.StatueFragmentBottomLeft)], AnimationEnd.Holds),
        [P.StatueFragmentBottomMiddle] = new([Segments.Whole(P.StatueFragmentBottomMiddle)], AnimationEnd.Holds),
        [P.StatueFragmentBottomRight] = new([Segments.Whole(P.StatueFragmentBottomRight)], AnimationEnd.Holds),
        [P.ChozoOrbMovingLeft] = new([Segments.Whole(P.ChozoOrbMovingLeft)], AnimationEnd.Holds),
        [P.ChozoOrbMovingRight] = new([Segments.Whole(P.ChozoOrbMovingRight)], AnimationEnd.Holds),
        [P.ChozoOrbWallImpact] = new([Segments.Whole(P.ChozoOrbWallImpact)], AnimationEnd.Holds),
        [P.ChozoOrbFloorImpact] = new([Segments.Whole(P.ChozoOrbFloorImpact)], AnimationEnd.Holds),
        [P.ChozoOrbShot] = new([Segments.Whole(P.ChozoOrbShot)], AnimationEnd.Holds),
        [P.SonicBoomFiredLeft] = new([Segments.Whole(P.SonicBoomFiredLeft), Segments.Whole(P.SonicBoomFlyingLeft)], AnimationEnd.Holds),
        [P.SonicBoomFiredRight] = new([Segments.Whole(P.SonicBoomFiredRight), Segments.Whole(P.SonicBoomFlyingRight)], AnimationEnd.Holds),
        [P.SonicBoomWallImpact] = new([Segments.Whole(P.SonicBoomWallImpact), Segments.Repeated(P.SonicBoomWallImpactBurst, 5), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [P.LandingDustRightFoot] = new([Segments.Whole(P.LandingDustRightFoot)], AnimationEnd.Holds),
        [P.LandingDustLeftFoot] = new([Segments.Whole(P.LandingDustLeftFoot)], AnimationEnd.Holds),
        [P.EggBouncingLeft] = new([Segments.Whole(P.EggBouncingLeft)], AnimationEnd.Holds),
        [P.EggHatchingLeft] = new([Segments.Whole(P.EggHatchingLeft), Segments.Whole(P.EggHatch)], AnimationEnd.Holds),
        [P.EggBouncingRight] = new([Segments.Whole(P.EggBouncingRight)], AnimationEnd.Holds),
        [P.EggHatchingRight] = new([Segments.Whole(P.EggHatchingRight), Segments.Whole(P.EggHatch)], AnimationEnd.Holds),
        [P.EggHatchedLeft] = Animation<P>.LeadingInto([Segments.Whole(P.EggHatchedLeft)], [Segments.Whole(P.HatchlingChargingLeft)]),
        [P.EggHatchedRight] = Animation<P>.LeadingInto([Segments.Whole(P.EggHatchedRight)], [Segments.Whole(P.HatchlingChargingRight)]),
        [P.EggBreakLeft] = new([Segments.Whole(P.EggBreakLeft)], AnimationEnd.Holds),
        [P.EggBreakRight] = new([Segments.Whole(P.EggBreakRight)], AnimationEnd.Holds),
        [P.SuperMissileRight] = Animation<P>.LeadingInto([Segments.Whole(P.SuperMissileRight)], [Segments.Whole(P.SuperMissileSpinningRight)]),
        [P.SuperMissileLeft] = Animation<P>.LeadingInto([Segments.Whole(P.SuperMissileLeft)], [Segments.Whole(P.SuperMissileSpinningLeft)]),
        [P.SuperMissileImpact] = new([Segments.Whole(P.SuperMissileImpact)], AnimationEnd.Holds),
        [P.EyeBeamWallImpact] = new([Segments.Whole(P.EyeBeamWallImpact)], AnimationEnd.Holds),
        [P.EyeBeamFloorImpact] = new([Segments.Whole(P.EyeBeamFloorImpact), Segments.Whole(P.EyeBeamFloorImpactFlames)], AnimationEnd.Holds),
        [P.EyeBeam] = Animation<P>.Looping([Segments.Whole(P.EyeBeam)]),
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

    /// <summary>
    /// A piece of the breaking statue hand (#2240), every piece alike: it shows <paramref name="fragment"/>, resting for
    /// <paramref name="restUpdates"/>, cracks off with the impact sound and starts its fall, falls for
    /// <see cref="BombTorizoStatueFragmentDefinitions.FallUpdates"/>, and is deleted.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] StatueFragment(EnemyProjectileFrameId fragment, int restUpdates) =>
    [
        Show(fragment, restUpdates),
        Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
        Cue(new TorizoProjectileCue.Signal(K.StartFragmentFall)),
        Show(fragment, BombTorizoStatueFragmentDefinitions.FallUpdates),
        Cue(new TorizoProjectileCue.Signal(K.Delete)),
    ];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue> Hold(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>.Event(cue);
}
