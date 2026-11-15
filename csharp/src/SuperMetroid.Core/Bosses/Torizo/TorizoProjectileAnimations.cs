using System.Collections.Frozen;
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
        Show(EnemyProjectileFrameId.TorizoDroolNoDelay, 5),
        Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus)),
    ];

    /// <summary>A drool drop's falling pose, which its animation loops on while it falls.</summary>
    private static readonly AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>[] DroolFalling =
    [
        Show(EnemyProjectileFrameId.TorizoDroolFallingLoop, 64),
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
            Show(EnemyProjectileFrameId.TorizoExplosionLowHealthInitialLoopFirst, 2),
            Show(EnemyProjectileFrameId.TorizoExplosionLowHealthInitialLoopSecond, 2),
            Show(EnemyProjectileFrameId.TorizoExplosionLowHealthInitialLoopThird, 3),
            Show(EnemyProjectileFrameId.TorizoExplosionLowHealthInitialLoopFourth, 3),
            Show(EnemyProjectileFrameId.TorizoExplosionLowHealthInitialLoopFifth, 2),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DeathExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.GoToSometimes(Target: P.DeathSmoke)),
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 16)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathInitialLoopFirst, 4),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathInitialLoopSecond, 6),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathInitialLoopThird, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathInitialLoopFourth, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathInitialLoopFifth, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathInitialLoopSixth, 6),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DeathSmoke] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 4)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathSmokeSetupLoopFirst, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathSmokeSetupLoopSecond, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathSmokeSetupLoopThird, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionDeathSmokeSetupLoopFourth, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DroolFourFrameDelay] = [Show(EnemyProjectileFrameId.TorizoDroolFourFrameDelay, 2), Show(EnemyProjectileFrameId.TorizoDroolTwoFrameDelay, 2), .. DroolRelease, .. DroolFalling],
        [P.DroolTwoFrameDelay] = [Show(EnemyProjectileFrameId.TorizoDroolTwoFrameDelay, 2), .. DroolRelease, .. DroolFalling],
        [P.DroolNoDelay] = [.. DroolRelease, .. DroolFalling],
        [P.DroolWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DroolFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.TorizoDroolFloorImpactFirst, 8),
            Show(EnemyProjectileFrameId.TorizoDroolFloorImpactSecond, 8),
            Show(EnemyProjectileFrameId.TorizoDroolFloorImpactThird, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ExplosiveSwipe] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BombTorizoSwipe, Priority: SoundRequestPriority.WhenIdle)),
            Show(EnemyProjectileFrameId.TorizoExplosiveSwipeInitialFirst, 5),
            Show(EnemyProjectileFrameId.TorizoExplosiveSwipeInitialSecond, 5),
            Show(EnemyProjectileFrameId.TorizoExplosiveSwipeInitialThird, 5),
            Show(EnemyProjectileFrameId.TorizoExplosiveSwipeInitialFourth, 5),
            Show(EnemyProjectileFrameId.TorizoExplosiveSwipeInitialFifth, 5),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.FirstStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoFirstStatueFragmentFirst, 128), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoFirstStatueFragmentSecond, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SecondStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoSecondStatueFragmentFirst, 120), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoSecondStatueFragmentSecond, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.ThirdStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoThirdStatueFragmentFirst, 112), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoThirdStatueFragmentSecond, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.FourthStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoFourthStatueFragmentFirst, 104), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoFourthStatueFragmentSecond, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.FifthStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoFifthStatueFragmentFirst, 96), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoFifthStatueFragmentSecond, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SixthStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoSixthStatueFragmentFirst, 88), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoSixthStatueFragmentSecond, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SeventhStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoSeventhStatueFragmentFirst, 80), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoSeventhStatueFragmentSecond, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.EighthStatueFragment] = [Show(EnemyProjectileFrameId.BombTorizoEighthStatueFragmentFirst, 72), .. StatueFragmentCracking, Show(EnemyProjectileFrameId.BombTorizoEighthStatueFragmentSecond, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.ChozoOrbMovingLeft] = [Show(EnemyProjectileFrameId.TorizoChozoOrbMovingLeft, 85)],
        [P.ChozoOrbMovingRight] = [Show(EnemyProjectileFrameId.TorizoChozoOrbMovingRight, 85)],
        [P.ChozoOrbWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(EnemyProjectileFrameId.TorizoChozoOrbWallImpactFirst, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbWallImpactSecond, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbWallImpactThird, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbWallImpactFourth, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbWallImpactFifth, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ChozoOrbFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFloorImpactFirst, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFloorImpactSecond, 5),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFloorImpactThird, 6),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFloorImpactFourth, 7),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFloorImpactFifth, 8),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFloorImpactSixth, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ChozoOrbShot] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(EnemyProjectileFrameId.TorizoChozoOrbShotFirst, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbShotSecond, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbShotThird, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbShotFourth, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbShotFifth, 4),
            Cue(new TorizoProjectileCue.Signal(K.SpawnOrbDrops)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.SonicBoomFiredLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFiredLeftFirst, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFiredLeftSecond, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomMovingLeft, 80),
        ],
        [P.SonicBoomFiredRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFiredRightFirst, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFiredRightSecond, 6),
            Show(EnemyProjectileFrameId.TorizoSonicBoomMovingRight, 80),
        ],
        [P.SonicBoomWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.StopBlockingShots)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamus)),
            Cue(new TorizoProjectileCue.Signal(K.DrawInFront)),
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 5), Center: new(0, 0)))),
            Show(EnemyProjectileFrameId.TorizoSonicBoomWallImpactLoopFirst, 2),
            Show(EnemyProjectileFrameId.TorizoSonicBoomWallImpactLoopSecond, 2),
            Show(EnemyProjectileFrameId.TorizoSonicBoomWallImpactLoopThird, 3),
            Show(EnemyProjectileFrameId.TorizoSonicBoomWallImpactLoopFourth, 3),
            Show(EnemyProjectileFrameId.TorizoSonicBoomWallImpactLoopFifth, 2),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.LandingDustRightFoot] =
        [
            Show(EnemyProjectileFrameId.TorizoLandingDustRightFootFirst, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustRightFootSecond, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustRightFootThird, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustRightFootFourth, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.LandingDustLeftFoot] =
        [
            Show(EnemyProjectileFrameId.TorizoLandingDustLeftFootFirst, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustLeftFootSecond, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustLeftFootThird, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(EnemyProjectileFrameId.TorizoLandingDustLeftFootFourth, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EggBouncingLeft] = [Show(EnemyProjectileFrameId.GoldenTorizoEggBouncingLeft, 48)],
        [P.EggHatchingLeft] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBouncingLeftHatchingFirst, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBouncingLeftHatchingSecond, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBouncingLeftHatchingThird, 4),
            .. EggHatch,
        ],
        [P.EggBouncingRight] = [Show(EnemyProjectileFrameId.GoldenTorizoEggBouncingRight, 48)],
        [P.EggHatchingRight] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBouncingRightHatchingFirst, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBouncingRightHatchingSecond, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBouncingRightHatchingThird, 4),
            .. EggHatch,
        ],
        [P.EggHatchedLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.ShotPipeBug, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.Signal(K.StartEggCharge)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchedLeftLoopFirst, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchedLeftLoopSecond, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchedLeftLoopThird, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchedLeftLoopFourth, 6),
        ],
        [P.EggHatchedRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.ShotPipeBug, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.Signal(K.StartEggCharge)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchedRightLoopFirst, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchedRightLoopSecond, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchedRightLoopThird, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEggHatchedRightLoopFourth, 6),
        ],
        [P.EggBreakLeft] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakLeftFirst, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakLeftSecond, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakLeftThird, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakLeftFourth, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakLeftFifth, 10),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EggBreakRight] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakRightFirst, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakRightSecond, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakRightThird, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakRightFourth, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEggBreakRightFifth, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.SuperMissileRight] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightInitial, 48),
            Cue(new TorizoProjectileCue.Signal(K.AimSuperMissileTowardSamus)),
            Cue(new TorizoProjectileCue.Signal(K.ThrowSuperMissile)),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightLoopFirst, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightLoopSecond, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightLoopThird, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightLoopFourth, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightLoopFifth, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightLoopSixth, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightLoopSeventh, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileRightLoopEighth, 2),
        ],
        [P.SuperMissileLeft] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftInitial, 48),
            Cue(new TorizoProjectileCue.Signal(K.AimSuperMissileAwayFromSamus)),
            Cue(new TorizoProjectileCue.Signal(K.ThrowSuperMissile)),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftLoopFirst, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftLoopSecond, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftLoopThird, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftLoopFourth, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftLoopFifth, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftLoopSixth, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftLoopSeventh, 2),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileLeftLoopEighth, 2),
        ],
        [P.SuperMissileImpact] =
        [
            Cue(new TorizoProjectileCue.SetRadius(Radius: new(16, 16))),
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileImpactFirst, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileImpactSecond, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileImpactThird, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileImpactFourth, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileImpactFifth, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileImpactSixth, 5),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeamWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamWallImpactFirst, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamWallImpactSecond, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamWallImpactThird, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamWallImpactFourth, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamWallImpactFifth, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeamFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopFirst, 8),
            Cue(new TorizoProjectileCue.Signal(K.ReplayWhileEyeBeamExplosionsDisabled)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopSecond, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopThird, 5),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopFourth, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopFifth, 7),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopSixth, 8),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopSeventh, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeam] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamNormalFirst, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamNormalSecond, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamNormalThird, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamNormalFourth, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamNormalFifth, 1),
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

    /// <summary>The animation each resting egg resumes into when its hatch timer wakes it; no other animation rests.</summary>
    private static readonly FrozenDictionary<P, P> ResumesAfterSleep = new Dictionary<P, P>
    {
        [P.EggBouncingLeft] = P.EggHatchingLeft,
        [P.EggBouncingRight] = P.EggHatchingRight,
    }.ToFrozenDictionary();

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

    /// <summary>The animation a resting egg resumes into when its hatch timer wakes it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="animation"/> never rests.</exception>
    internal static TorizoProjectileSegment AfterSleep(TorizoProjectileSegment animation) =>
        ResumesAfterSleep.TryGetValue(animation, out P resumed)
            ? resumed
            : throw new ArgumentOutOfRangeException(nameof(animation), animation, "The animation never rests.");

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoProjectileCue>.Event(cue);
}
