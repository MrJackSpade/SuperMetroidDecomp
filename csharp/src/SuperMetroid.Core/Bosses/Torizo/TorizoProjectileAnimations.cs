using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed projectile programs (#641), in place of their lists. Generated from the
/// verified lists: each frame is a <see cref="TorizoProjectileFrame"/> pose of its program, and each command
/// a <see cref="TorizoProjectileCue"/> with its typed operands.
/// </summary>
internal static class TorizoProjectileAnimations
{
    /// <summary>Every projectile program.</summary>
    internal static AnimationSegments<TorizoProjectileSegment, TorizoProjectileFrame, TorizoProjectileCue> Segments { get; } = new(
        new Dictionary<TorizoProjectileSegment, AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>[]>
    {
        [P.ExplosionLowHealthInitial] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
        ],
        [P.ExplosionLowHealthInitialLoop] =
        [
            Cue(new TorizoProjectileCue.Command(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 0)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(P.ExplosionLowHealthInitialLoop, 0, 2),
            Show(P.ExplosionLowHealthInitialLoop, 1, 2),
            Show(P.ExplosionLowHealthInitialLoop, 2, 3),
            Show(P.ExplosionLowHealthInitialLoop, 3, 3),
            Show(P.ExplosionLowHealthInitialLoop, 4, 2),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.ExplosionDeathInitial] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.GoToSometimes(Target: P.ExplosionDeathSmokeSetup)),
        ],
        [P.ExplosionDeathInitialLoop] =
        [
            Cue(new TorizoProjectileCue.Command(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 16)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(P.ExplosionDeathInitialLoop, 0, 4),
            Show(P.ExplosionDeathInitialLoop, 1, 6),
            Show(P.ExplosionDeathInitialLoop, 2, 5),
            Show(P.ExplosionDeathInitialLoop, 3, 5),
            Show(P.ExplosionDeathInitialLoop, 4, 5),
            Show(P.ExplosionDeathInitialLoop, 5, 6),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.ExplosionDeathSmokeSetup] =
        [
        ],
        [P.ExplosionDeathSmokeSetupLoop] =
        [
            Cue(new TorizoProjectileCue.Command(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 4)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(P.ExplosionDeathSmokeSetupLoop, 0, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 1, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 2, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 3, 8),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.DroolFourFrameDelay] =
        [
            Show(P.DroolFourFrameDelay, 0, 2),
        ],
        [P.DroolTwoFrameDelay] =
        [
            Show(P.DroolTwoFrameDelay, 0, 2),
        ],
        [P.DroolNoDelay] =
        [
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoDroolFall)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(P.DroolNoDelay, 0, 5),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus)),
        ],
        [P.DroolFallingLoop] =
        [
            Show(P.DroolFallingLoop, 0, 64),
        ],
        [P.DroolWallImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.DroolFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Show(P.DroolFloorImpact, 0, 8),
            Show(P.DroolFloorImpact, 1, 8),
            Show(P.DroolFloorImpact, 2, 8),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.SwipeInitial] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BombTorizoSwipe, Priority: SoundRequestPriority.WhenIdle)),
            Show(P.SwipeInitial, 0, 5),
            Show(P.SwipeInitial, 1, 5),
            Show(P.SwipeInitial, 2, 5),
            Show(P.SwipeInitial, 3, 5),
            Show(P.SwipeInitial, 4, 5),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment0] =
        [
            Show(P.StatueFragment0, 0, 128),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment0, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment1] =
        [
            Show(P.StatueFragment1, 0, 120),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment1, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment2] =
        [
            Show(P.StatueFragment2, 0, 112),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment2, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment3] =
        [
            Show(P.StatueFragment3, 0, 104),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment3, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment4] =
        [
            Show(P.StatueFragment4, 0, 96),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment4, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment5] =
        [
            Show(P.StatueFragment5, 0, 88),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment5, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment6] =
        [
            Show(P.StatueFragment6, 0, 80),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment6, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment7] =
        [
            Show(P.StatueFragment7, 0, 72),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment7, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment8] =
        [
            Show(P.StatueFragment8, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment8, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment9] =
        [
            Show(P.StatueFragment9, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment9, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment10] =
        [
            Show(P.StatueFragment10, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment10, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment11] =
        [
            Show(P.StatueFragment11, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment11, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment12] =
        [
            Show(P.StatueFragment12, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment12, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment13] =
        [
            Show(P.StatueFragment13, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment13, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment14] =
        [
            Show(P.StatueFragment14, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment14, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.StatueFragment15] =
        [
            Show(P.StatueFragment15, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
            Show(P.StatueFragment15, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.ChozoOrbMovingLeft] =
        [
            Show(P.ChozoOrbMovingLeft, 0, 85),
        ],
        [P.ChozoOrbMovingRight] =
        [
            Show(P.ChozoOrbMovingRight, 0, 85),
        ],
        [P.ChozoOrbWallImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(P.ChozoOrbWallImpact, 0, 4),
            Show(P.ChozoOrbWallImpact, 1, 4),
            Show(P.ChozoOrbWallImpact, 2, 4),
            Show(P.ChozoOrbWallImpact, 3, 4),
            Show(P.ChozoOrbWallImpact, 4, 4),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.ChozoOrbFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(P.ChozoOrbFloorImpact, 0, 4),
            Show(P.ChozoOrbFloorImpact, 1, 5),
            Show(P.ChozoOrbFloorImpact, 2, 6),
            Show(P.ChozoOrbFloorImpact, 3, 7),
            Show(P.ChozoOrbFloorImpact, 4, 8),
            Show(P.ChozoOrbFloorImpact, 5, 9),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.ChozoOrbShot] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(P.ChozoOrbShot, 0, 4),
            Show(P.ChozoOrbShot, 1, 4),
            Show(P.ChozoOrbShot, 2, 4),
            Show(P.ChozoOrbShot, 3, 4),
            Show(P.ChozoOrbShot, 4, 4),
            Cue(new TorizoProjectileCue.Command(K.SpawnOrbDrops)),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.SonicBoomFiredLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(P.SonicBoomFiredLeft, 0, 6),
            Show(P.SonicBoomFiredLeft, 1, 6),
        ],
        [P.SonicBoomMovingLeft] =
        [
            Show(P.SonicBoomMovingLeft, 0, 80),
        ],
        [P.SonicBoomFiredRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(P.SonicBoomFiredRight, 0, 6),
            Show(P.SonicBoomFiredRight, 1, 6),
        ],
        [P.SonicBoomMovingRight] =
        [
            Show(P.SonicBoomMovingRight, 0, 80),
        ],
        [P.SonicBoomWallImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.Command(K.StopBlockingShots)),
            Cue(new TorizoProjectileCue.Command(K.StopHarmingSamus)),
            Cue(new TorizoProjectileCue.Command(K.DrawInFront)),
        ],
        [P.SonicBoomWallImpactLoop] =
        [
            Cue(new TorizoProjectileCue.Command(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 5), Center: new(0, 0)))),
            Show(P.SonicBoomWallImpactLoop, 0, 2),
            Show(P.SonicBoomWallImpactLoop, 1, 2),
            Show(P.SonicBoomWallImpactLoop, 2, 3),
            Show(P.SonicBoomWallImpactLoop, 3, 3),
            Show(P.SonicBoomWallImpactLoop, 4, 2),
            Cue(new TorizoProjectileCue.Command(K.NoOperation)),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.LandingDustRightFoot] =
        [
            Show(P.LandingDustRightFoot, 0, 4),
            Cue(new TorizoProjectileCue.Command(K.RaiseDust)),
            Show(P.LandingDustRightFoot, 1, 4),
            Cue(new TorizoProjectileCue.Command(K.RaiseDust)),
            Show(P.LandingDustRightFoot, 2, 4),
            Cue(new TorizoProjectileCue.Command(K.RaiseDust)),
            Show(P.LandingDustRightFoot, 3, 4),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.LandingDustLeftFoot] =
        [
            Show(P.LandingDustLeftFoot, 0, 4),
            Cue(new TorizoProjectileCue.Command(K.RaiseDust)),
            Show(P.LandingDustLeftFoot, 1, 4),
            Cue(new TorizoProjectileCue.Command(K.RaiseDust)),
            Show(P.LandingDustLeftFoot, 2, 4),
            Cue(new TorizoProjectileCue.Command(K.RaiseDust)),
            Show(P.LandingDustLeftFoot, 3, 4),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.EggBouncingLeft] =
        [
            Show(P.EggBouncingLeft, 0, 48),
        ],
        [P.EggBouncingLeftHatching] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Show(P.EggBouncingLeftHatching, 0, 4),
            Show(P.EggBouncingLeftHatching, 1, 4),
            Show(P.EggBouncingLeftHatching, 2, 4),
        ],
        [P.EggBouncingRight] =
        [
            Show(P.EggBouncingRight, 0, 48),
        ],
        [P.EggBouncingRightHatching] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Show(P.EggBouncingRightHatching, 0, 4),
            Show(P.EggBouncingRightHatching, 1, 4),
            Show(P.EggBouncingRightHatching, 2, 4),
        ],
        [P.EggHatch] =
        [
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new TorizoProjectileCue.Command(K.Hatch)),
        ],
        [P.EggHatchedLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.ShotPipeBug, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.GoldenTorizoEggCharge)),
        ],
        [P.EggHatchedLeftLoop] =
        [
            Show(P.EggHatchedLeftLoop, 0, 6),
            Show(P.EggHatchedLeftLoop, 1, 6),
            Show(P.EggHatchedLeftLoop, 2, 6),
            Show(P.EggHatchedLeftLoop, 3, 6),
        ],
        [P.EggHatchedRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.ShotPipeBug, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.GoldenTorizoEggCharge)),
        ],
        [P.EggHatchedRightLoop] =
        [
            Show(P.EggHatchedRightLoop, 0, 6),
            Show(P.EggHatchedRightLoop, 1, 6),
            Show(P.EggHatchedRightLoop, 2, 6),
            Show(P.EggHatchedRightLoop, 3, 6),
        ],
        [P.EggBreakLeft] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Show(P.EggBreakLeft, 0, 4),
            Show(P.EggBreakLeft, 1, 4),
            Show(P.EggBreakLeft, 2, 4),
            Show(P.EggBreakLeft, 3, 4),
            Show(P.EggBreakLeft, 4, 10),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.EggBreakRight] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Show(P.EggBreakRight, 0, 4),
            Show(P.EggBreakRight, 1, 4),
            Show(P.EggBreakRight, 2, 4),
            Show(P.EggBreakRight, 3, 4),
            Show(P.EggBreakRight, 4, 8),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.SuperMissileRightInitial] =
        [
            Show(P.SuperMissileRightInitial, 0, 48),
            Cue(new TorizoProjectileCue.Command(K.AimSuperMissileTowardSamus)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileThrown)),
        ],
        [P.SuperMissileRightLoop] =
        [
            Show(P.SuperMissileRightLoop, 0, 2),
            Show(P.SuperMissileRightLoop, 1, 2),
            Show(P.SuperMissileRightLoop, 2, 2),
            Show(P.SuperMissileRightLoop, 3, 2),
            Show(P.SuperMissileRightLoop, 4, 2),
            Show(P.SuperMissileRightLoop, 5, 2),
            Show(P.SuperMissileRightLoop, 6, 2),
            Show(P.SuperMissileRightLoop, 7, 2),
        ],
        [P.SuperMissileLeftInitial] =
        [
            Show(P.SuperMissileLeftInitial, 0, 48),
            Cue(new TorizoProjectileCue.Command(K.AimSuperMissileAwayFromSamus)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileThrown)),
        ],
        [P.SuperMissileLeftLoop] =
        [
            Show(P.SuperMissileLeftLoop, 0, 2),
            Show(P.SuperMissileLeftLoop, 1, 2),
            Show(P.SuperMissileLeftLoop, 2, 2),
            Show(P.SuperMissileLeftLoop, 3, 2),
            Show(P.SuperMissileLeftLoop, 4, 2),
            Show(P.SuperMissileLeftLoop, 5, 2),
            Show(P.SuperMissileLeftLoop, 6, 2),
            Show(P.SuperMissileLeftLoop, 7, 2),
        ],
        [P.SuperMissileImpact] =
        [
            Cue(new TorizoProjectileCue.SetRadius(Radius: new(16, 16))),
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(P.SuperMissileImpact, 0, 5),
            Show(P.SuperMissileImpact, 1, 5),
            Show(P.SuperMissileImpact, 2, 5),
            Show(P.SuperMissileImpact, 3, 5),
            Show(P.SuperMissileImpact, 4, 5),
            Show(P.SuperMissileImpact, 5, 5),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.EyeBeamWallImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
            Show(P.EyeBeamWallImpact, 0, 4),
            Show(P.EyeBeamWallImpact, 1, 4),
            Show(P.EyeBeamWallImpact, 2, 4),
            Show(P.EyeBeamWallImpact, 3, 4),
            Show(P.EyeBeamWallImpact, 4, 4),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.EyeBeamFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearFrameBehavior)),
        ],
        [P.EyeBeamFloorImpactLoop] =
        [
            Show(P.EyeBeamFloorImpactLoop, 0, 8),
            Cue(new TorizoProjectileCue.GoToIfEyeBeamExplosionsDisabled(Target: P.EyeBeamFloorImpactLoop)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(P.EyeBeamFloorImpactLoop, 1, 4),
            Show(P.EyeBeamFloorImpactLoop, 2, 5),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Show(P.EyeBeamFloorImpactLoop, 3, 6),
            Show(P.EyeBeamFloorImpactLoop, 4, 7),
            Show(P.EyeBeamFloorImpactLoop, 5, 8),
            Show(P.EyeBeamFloorImpactLoop, 6, 9),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
        ],
        [P.EyeBeamNormal] =
        [
            Show(P.EyeBeamNormal, 0, 1),
            Show(P.EyeBeamNormal, 1, 1),
            Show(P.EyeBeamNormal, 2, 1),
            Show(P.EyeBeamNormal, 3, 1),
            Show(P.EyeBeamNormal, 4, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>> StartingAt { get; } = new Dictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>>
    {
        [P.ExplosionLowHealthInitial] = new([new(P.ExplosionLowHealthInitial, 0, 2, 1), new(P.ExplosionLowHealthInitialLoop, 0, 8, 3), new(P.ExplosionLowHealthInitialLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.ExplosionDeathInitial] = new([new(P.ExplosionDeathInitial, 0, 3, 1), new(P.ExplosionDeathInitialLoop, 0, 9, 2), new(P.ExplosionDeathInitialLoop, 9, 1, 1)], AnimationEnd.Holds),
        [P.ExplosionDeathSmokeSetup] = new([new(P.ExplosionDeathSmokeSetupLoop, 0, 7, 2), new(P.ExplosionDeathSmokeSetupLoop, 7, 1, 1)], AnimationEnd.Holds),
        [P.DroolFourFrameDelay] = new([new(P.DroolFourFrameDelay, 0, 1, 1), new(P.DroolTwoFrameDelay, 0, 1, 1), new(P.DroolNoDelay, 0, 4, 1), new(P.DroolFallingLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [P.DroolTwoFrameDelay] = new([new(P.DroolTwoFrameDelay, 0, 1, 1), new(P.DroolNoDelay, 0, 4, 1), new(P.DroolFallingLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.DroolNoDelay] = new([new(P.DroolNoDelay, 0, 4, 1), new(P.DroolFallingLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DroolFallingLoop] = new([new(P.DroolFallingLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DroolWallImpact] = new([new(P.DroolWallImpact, 0, 2, 1)], AnimationEnd.Holds),
        [P.DroolFloorImpact] = new([new(P.DroolFloorImpact, 0, 5, 1)], AnimationEnd.Holds),
        [P.SwipeInitial] = new([new(P.SwipeInitial, 0, 7, 1)], AnimationEnd.Holds),
        [P.StatueFragment0] = new([new(P.StatueFragment0, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment1] = new([new(P.StatueFragment1, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment2] = new([new(P.StatueFragment2, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment3] = new([new(P.StatueFragment3, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment4] = new([new(P.StatueFragment4, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment5] = new([new(P.StatueFragment5, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment6] = new([new(P.StatueFragment6, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment7] = new([new(P.StatueFragment7, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment8] = new([new(P.StatueFragment8, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment9] = new([new(P.StatueFragment9, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment10] = new([new(P.StatueFragment10, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment11] = new([new(P.StatueFragment11, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment12] = new([new(P.StatueFragment12, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment13] = new([new(P.StatueFragment13, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment14] = new([new(P.StatueFragment14, 0, 5, 1)], AnimationEnd.Holds),
        [P.StatueFragment15] = new([new(P.StatueFragment15, 0, 5, 1)], AnimationEnd.Holds),
        [P.ChozoOrbMovingLeft] = new([new(P.ChozoOrbMovingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChozoOrbMovingRight] = new([new(P.ChozoOrbMovingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChozoOrbWallImpact] = new([new(P.ChozoOrbWallImpact, 0, 8, 1)], AnimationEnd.Holds),
        [P.ChozoOrbFloorImpact] = new([new(P.ChozoOrbFloorImpact, 0, 11, 1)], AnimationEnd.Holds),
        [P.ChozoOrbShot] = new([new(P.ChozoOrbShot, 0, 9, 1)], AnimationEnd.Holds),
        [P.SonicBoomFiredLeft] = new([new(P.SonicBoomFiredLeft, 0, 3, 1), new(P.SonicBoomMovingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SonicBoomMovingLeft] = new([new(P.SonicBoomMovingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SonicBoomFiredRight] = new([new(P.SonicBoomFiredRight, 0, 3, 1), new(P.SonicBoomMovingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SonicBoomMovingRight] = new([new(P.SonicBoomMovingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SonicBoomWallImpact] = new([new(P.SonicBoomWallImpact, 0, 4, 1), new(P.SonicBoomWallImpactLoop, 0, 8, 5), new(P.SonicBoomWallImpactLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.LandingDustRightFoot] = new([new(P.LandingDustRightFoot, 0, 8, 1)], AnimationEnd.Holds),
        [P.LandingDustLeftFoot] = new([new(P.LandingDustLeftFoot, 0, 8, 1)], AnimationEnd.Holds),
        [P.EggBouncingLeft] = new([new(P.EggBouncingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.EggBouncingLeftHatching] = new([new(P.EggBouncingLeftHatching, 0, 4, 1), new(P.EggHatch, 0, 3, 1)], AnimationEnd.Holds),
        [P.EggBouncingRight] = new([new(P.EggBouncingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EggBouncingRightHatching] = new([new(P.EggBouncingRightHatching, 0, 4, 1), new(P.EggHatch, 0, 3, 1)], AnimationEnd.Holds),
        [P.EggHatch] = new([new(P.EggHatch, 0, 3, 1)], AnimationEnd.Holds),
        [P.EggHatchedLeft] = new([new(P.EggHatchedLeft, 0, 2, 1), new(P.EggHatchedLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.EggHatchedLeftLoop] = new([new(P.EggHatchedLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EggHatchedRight] = new([new(P.EggHatchedRight, 0, 2, 1), new(P.EggHatchedRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.EggHatchedRightLoop] = new([new(P.EggHatchedRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EggBreakLeft] = new([new(P.EggBreakLeft, 0, 7, 1)], AnimationEnd.Holds),
        [P.EggBreakRight] = new([new(P.EggBreakRight, 0, 7, 1)], AnimationEnd.Holds),
        [P.SuperMissileRightInitial] = new([new(P.SuperMissileRightInitial, 0, 3, 1), new(P.SuperMissileRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SuperMissileRightLoop] = new([new(P.SuperMissileRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileLeftInitial] = new([new(P.SuperMissileLeftInitial, 0, 3, 1), new(P.SuperMissileLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SuperMissileLeftLoop] = new([new(P.SuperMissileLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileImpact] = new([new(P.SuperMissileImpact, 0, 12, 1)], AnimationEnd.Holds),
        [P.EyeBeamWallImpact] = new([new(P.EyeBeamWallImpact, 0, 7, 1)], AnimationEnd.Holds),
        [P.EyeBeamFloorImpact] = new([new(P.EyeBeamFloorImpact, 0, 1, 1), new(P.EyeBeamFloorImpactLoop, 0, 11, 1)], AnimationEnd.Holds),
        [P.EyeBeamFloorImpactLoop] = new([new(P.EyeBeamFloorImpactLoop, 0, 11, 1)], AnimationEnd.Holds),
        [P.EyeBeamNormal] = new([new(P.EyeBeamNormal, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<TorizoProjectileSegment> StartingWith(TorizoProjectileSegment segment) => StartingAt[segment];

    /// <summary>The program of statue fragment <paramref name="index"/>, of the sixteen the breaking statue hand emits.</summary>
    internal static TorizoProjectileSegment StatueFragment(int index) => (uint)index < (uint)StatueFragments.Length
        ? StatueFragments[index]
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Bomb Torizo's statue hand emits sixteen fragments.");

    /// <summary>The sixteen statue fragments' programs, indexed by fragment.</summary>
    private static readonly TorizoProjectileSegment[] StatueFragments =
    [
        P.StatueFragment0, P.StatueFragment1, P.StatueFragment2, P.StatueFragment3,
        P.StatueFragment4, P.StatueFragment5, P.StatueFragment6, P.StatueFragment7,
        P.StatueFragment8, P.StatueFragment9, P.StatueFragment10, P.StatueFragment11,
        P.StatueFragment12, P.StatueFragment13, P.StatueFragment14, P.StatueFragment15,
    ];

    /// <summary>The program a sleeping egg resumes into when its hatch timer steps the list past the sleep.</summary>
    internal static TorizoProjectileSegment AfterSleep(TorizoProjectileSegment program) => program switch
    {
        P.EggBouncingLeft
            or P.EggBouncingRight => EnumOrder.Next(program),
        P.ExplosionLowHealthInitial or
            P.ExplosionLowHealthInitialLoop or
            P.ExplosionDeathInitial or
            P.ExplosionDeathInitialLoop or
            P.ExplosionDeathSmokeSetup or
            P.ExplosionDeathSmokeSetupLoop or
            P.DroolFourFrameDelay or
            P.DroolTwoFrameDelay or
            P.DroolNoDelay or
            P.DroolFallingLoop or
            P.DroolWallImpact or
            P.DroolFloorImpact or
            P.SwipeInitial or
            P.StatueFragment0 or
            P.StatueFragment1 or
            P.StatueFragment2 or
            P.StatueFragment3 or
            P.StatueFragment4 or
            P.StatueFragment5 or
            P.StatueFragment6 or
            P.StatueFragment7 or
            P.StatueFragment8 or
            P.StatueFragment9 or
            P.StatueFragment10 or
            P.StatueFragment11 or
            P.StatueFragment12 or
            P.StatueFragment13 or
            P.StatueFragment14 or
            P.StatueFragment15 or
            P.ChozoOrbMovingLeft or
            P.ChozoOrbMovingRight or
            P.ChozoOrbWallImpact or
            P.ChozoOrbFloorImpact or
            P.ChozoOrbShot or
            P.SonicBoomFiredLeft or
            P.SonicBoomMovingLeft or
            P.SonicBoomFiredRight or
            P.SonicBoomMovingRight or
            P.SonicBoomWallImpact or
            P.SonicBoomWallImpactLoop or
            P.LandingDustRightFoot or
            P.LandingDustLeftFoot or
            P.EggBouncingLeftHatching or
            P.EggBouncingRightHatching or
            P.EggHatch or
            P.EggHatchedLeft or
            P.EggHatchedLeftLoop or
            P.EggHatchedRight or
            P.EggHatchedRightLoop or
            P.EggBreakLeft or
            P.EggBreakRight or
            P.SuperMissileRightInitial or
            P.SuperMissileRightLoop or
            P.SuperMissileLeftInitial or
            P.SuperMissileLeftLoop or
            P.SuperMissileImpact or
            P.EyeBeamWallImpact or
            P.EyeBeamFloorImpact or
            P.EyeBeamFloorImpactLoop or
            P.EyeBeamNormal => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
    };

    /// <summary>Shows pose <paramref name="phase"/> of <paramref name="program"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue> Show(
        TorizoProjectileSegment program, int phase, int duration) =>
        new AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>.Frame(new(program, phase), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>.Event(cue);
}
