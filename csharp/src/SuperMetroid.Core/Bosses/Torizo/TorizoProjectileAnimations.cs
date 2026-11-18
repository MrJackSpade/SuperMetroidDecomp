using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSide;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's projectile animations (#641, #1788, #2011): each animation's steps, each frame the installed
/// <see cref="EnemyProjectileFrameId"/> it shows and each cue a <see cref="TorizoProjectileCue"/> with the values it acts on;
/// the segments each animation plays whole, once or a number of times (#2235); and whether it then loops or holds. The runs
/// several animations play alike (the drool's release and fall, the egg's hatch, the explosions' deletion) are one segment
/// each plays, and a still picture is a held frame. A sonic boom, a Chozo orb, an egg, its hatchling, a Super Missile's
/// spin and the landing dust are written once over the <see cref="TorizoProjectileSide"/> they face, fly or rise on
/// (#2472); the ways the two sides differ, a hatchling shot facing left lying flat longer and a Super Missile thrown facing
/// left flying away from Samus, are written at their segments.
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
        [P.DroolForming] = [Show(EnemyProjectileFrameId.Blank, 2)],
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
        [P.ChozoOrbMovingLeft] = ChozoOrbMoving(Left),
        [P.ChozoOrbMovingRight] = ChozoOrbMoving(Right),
        [P.ChozoOrbWallImpact] =
        [
            .. ChozoOrbBursting,
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
            .. ChozoOrbBursting,
            Cue(new TorizoProjectileCue.Signal(K.SpawnOrbDrops)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.SonicBoomFiredLeft] = SonicBoomFired(Left),
        [P.SonicBoomFlyingLeft] = SonicBoomFlying(Left),
        [P.SonicBoomFiredRight] = SonicBoomFired(Right),
        [P.SonicBoomFlyingRight] = SonicBoomFlying(Right),
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
        [P.LandingDustRightFoot] = LandingDust(Right),
        [P.LandingDustLeftFoot] = LandingDust(Left),
        [P.EggBouncingLeft] = EggBouncing(Left),
        [P.EggHatchingLeft] = EggHatching(Left),
        [P.EggBouncingRight] = EggBouncing(Right),
        [P.EggHatchingRight] = EggHatching(Right),
        [P.EggHatchedLeft] = [.. EggHatched],
        [P.HatchlingChargingLeft] = HatchlingCharging(Left),
        [P.EggHatchedRight] = [.. EggHatched],
        [P.HatchlingChargingRight] = HatchlingCharging(Right),
        [P.EggBreakLeft] = EggBreak(Left, remainsFlatUpdates: 10),
        [P.EggBreakRight] = EggBreak(Right, remainsFlatUpdates: 8),
        [P.SuperMissileRight] = SuperMissileHeld(aim: K.AimSuperMissileTowardSamus),
        [P.SuperMissileSpinningRight] = SuperMissileSpinning(Right),
        [P.SuperMissileLeft] = SuperMissileHeld(aim: K.AimSuperMissileAwayFromSamus),
        [P.SuperMissileSpinningLeft] = SuperMissileSpinning(Left),
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
        [P.EyeBeamFloorImpactWaiting] =
        [
            Show(EnemyProjectileFrameId.Blank, 8),
            Mark(K.FloorImpactWaited),
        ],
        [P.EyeBeamFloorFlames] =
        [
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
        [P.EyeBeamFloorImpact] = Animation<P>.LeadingInto([Segments.Whole(P.EyeBeamFloorImpact)], [Segments.Whole(P.EyeBeamFloorImpactWaiting)]),
        [P.EyeBeamFloorFlames] = new([Segments.Whole(P.EyeBeamFloorFlames)], AnimationEnd.Holds),
        [P.EyeBeam] = Animation<P>.Looping([Segments.Whole(P.EyeBeam)]),
    };

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<TorizoProjectileSegment> StartingWith(TorizoProjectileSegment animation) => StartingAt[animation];

    /// <summary>A cue's continuation into <paramref name="animation"/>, from its start.</summary>
    internal static AnimationContinuation<TorizoProjectileSegment> ContinueWith(TorizoProjectileSegment animation) =>
        new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>
    /// A low-health drool that forms unseen for two updates before it is released and falls, holding its falling pose: one of
    /// the drools <see cref="TorizoRandomDefinitions.LowHealthDroolAnimations"/> chooses among.
    /// </summary>
    internal static Animation<TorizoProjectileSegment> DroolFormingForTwoUpdates { get; } =
        new([Segments.Whole(P.DroolForming), Segments.Whole(P.DroolNoDelay), Segments.Whole(P.DroolFalling)], AnimationEnd.Holds);

    /// <summary>A low-health drool that forms unseen for four updates, its forming played twice, before it is released and falls.</summary>
    internal static Animation<TorizoProjectileSegment> DroolFormingForFourUpdates { get; } =
        new([Segments.Repeated(P.DroolForming, 2), Segments.Whole(P.DroolNoDelay), Segments.Whole(P.DroolFalling)], AnimationEnd.Holds);

    /// <summary>
    /// A Chozo orb bursting (#2411), on a wall or shot: it stops, survives touching Samus in front of her, and plays its five
    /// poses; a shot orb's burst then drops its pickup.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] ChozoOrbBursting =>
    [
        Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
        Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
        Show(EnemyProjectileFrameId.TorizoChozoOrbSpark, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareBall, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingCored, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRing, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingFading, 4),
    ];

    /// <summary>A sonic boom thrown toward <paramref name="side"/>: its sound and its two growing poses.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] SonicBoomFired(TorizoProjectileSide side) =>
    [
        Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
        Show(side.SonicBoomSmall, 6),
        Show(side.SonicBoomMedium, 6),
    ];

    /// <summary>A sonic boom flying toward <paramref name="side"/>, at its full size.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] SonicBoomFlying(TorizoProjectileSide side) =>
        [Hold(side.SonicBoomLarge)];

    /// <summary>A Chozo orb flying toward <paramref name="side"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] ChozoOrbMoving(TorizoProjectileSide side) =>
        [Hold(side.ChozoOrb)];

    /// <summary>
    /// The dust the Torizo's foot on <paramref name="side"/> raises on landing: its four puffs, each raising the dust a step,
    /// then its deletion.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] LandingDust(TorizoProjectileSide side) =>
    [
        Show(side.DustPuff, 4),
        Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
        Show(side.DustPuffSmall, 4),
        Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
        Show(side.DustPuffThin, 4),
        Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
        Show(side.DustPuffFaint, 4),
        Cue(new TorizoProjectileCue.Signal(K.Delete)),
    ];

    /// <summary>An egg facing <paramref name="side"/>, whole as it bounces.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] EggBouncing(TorizoProjectileSide side) =>
        [Hold(side.EggWhole)];

    /// <summary>An egg facing <paramref name="side"/> hatching: it stops, cracks, splits and opens.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] EggHatching(TorizoProjectileSide side) =>
    [
        Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
        Show(side.EggCracked, 4),
        Show(side.EggSplitting, 4),
        Show(side.EggHatching, 4),
    ];

    /// <summary>A hatched egg's creature, either way it faces: it squeaks and starts its charge.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] EggHatched =>
    [
        Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.RisingSqueaks, Priority: SoundRequestPriority.Normal)),
        Cue(new TorizoProjectileCue.Signal(K.StartEggCharge)),
    ];

    /// <summary>A hatchling charging toward <paramref name="side"/>: claws raised, lowered, hunched and lowered again.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] HatchlingCharging(TorizoProjectileSide side) =>
    [
        Show(side.HatchlingClawsRaised, 6),
        Show(side.HatchlingClawsLowered, 6),
        Show(side.HatchlingHunched, 6),
        Show(side.HatchlingClawsLowered, 6),
    ];

    /// <summary>
    /// A shot hatchling facing <paramref name="side"/> breaking: it stops, crumbles, collapses, slumps and lies in its
    /// remains, flattened for <paramref name="remainsFlatUpdates"/>, then is deleted.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] EggBreak(TorizoProjectileSide side, int remainsFlatUpdates) =>
    [
        Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
        Show(side.HatchlingCrumbling, 4),
        Show(side.HatchlingCollapsing, 4),
        Show(side.HatchlingSlumped, 4),
        Show(side.HatchlingRemains, 4),
        Show(side.HatchlingRemainsFlat, remainsFlatUpdates),
        Cue(new TorizoProjectileCue.Signal(K.Delete)),
    ];

    /// <summary>A caught Super Missile held pointing down, then aimed as <paramref name="aim"/> says and thrown.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] SuperMissileHeld(TorizoProjectileCueKind aim) =>
    [
        Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown, 48),
        Cue(new TorizoProjectileCue.Signal(aim)),
        Cue(new TorizoProjectileCue.Signal(K.ThrowSuperMissile)),
    ];

    /// <summary>
    /// A thrown Super Missile spinning from pointing down toward <paramref name="side"/>: round through that side, up and the
    /// other side, back to pointing down.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] SuperMissileSpinning(TorizoProjectileSide side) =>
    [
        Show(side.SuperMissileDownForward, 2),
        Show(side.SuperMissileForward, 2),
        Show(side.SuperMissileUpForward, 2),
        Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUp, 2),
        Show(side.SuperMissileUpBack, 2),
        Show(side.SuperMissileBack, 2),
        Show(side.SuperMissileDownBack, 2),
        Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown, 2),
    ];

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

    /// <summary>Marks the moment <paramref name="kind"/> names (#2411), which the projectile's owner reacts to and which plays nothing.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue> Mark(TorizoProjectileCueKind kind) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>.Marker(new TorizoProjectileCue.Signal(kind));
}
