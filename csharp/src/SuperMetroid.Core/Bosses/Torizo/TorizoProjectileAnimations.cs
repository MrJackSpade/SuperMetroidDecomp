using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed projectile programs (#641), in place of their bank-$86 lists. Generated from the
/// verified native lists: each frame is a <see cref="TorizoProjectileFrame"/> pose of its program, and each command
/// a <see cref="TorizoProjectileCue"/> with its typed operands.
/// </summary>
internal static class TorizoProjectileAnimations
{
    /// <summary>Every projectile program.</summary>
    internal static AnimationProgramSet<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> Programs { get; } = new(
        new Dictionary<TorizoProjectileProgram, AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>[]>
    {
        [P.ExplosionLowHealthInitial] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus)),
            SetCounter(3),
            FallInto(P.ExplosionLowHealthInitialLoop),
        ],
        [P.ExplosionLowHealthInitialLoop] =
        [
            Cue(new TorizoProjectileCue.Command(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(Range: new(15, 15), Center: new(0, 0)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.SmallExplosion, MaximumQueued: SoundQueueLimit.Max6)),
            Show(P.ExplosionLowHealthInitialLoop, 0, 2),
            Show(P.ExplosionLowHealthInitialLoop, 1, 2),
            Show(P.ExplosionLowHealthInitialLoop, 2, 3),
            Show(P.ExplosionLowHealthInitialLoop, 3, 3),
            Show(P.ExplosionLowHealthInitialLoop, 4, 2),
            RepeatWhileCounting(P.ExplosionLowHealthInitialLoop),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.ExplosionDeathInitial] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new TorizoProjectileCue.GoToSometimes(Target: P.ExplosionDeathSmokeSetup)),
            SetCounter(2),
            FallInto(P.ExplosionDeathInitialLoop),
        ],
        [P.ExplosionDeathInitialLoop] =
        [
            Cue(new TorizoProjectileCue.Command(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(Range: new(31, 63), Center: new(0, 16)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.SmallExplosion, MaximumQueued: SoundQueueLimit.Max6)),
            Show(P.ExplosionDeathInitialLoop, 0, 4),
            Show(P.ExplosionDeathInitialLoop, 1, 6),
            Show(P.ExplosionDeathInitialLoop, 2, 5),
            Show(P.ExplosionDeathInitialLoop, 3, 5),
            Show(P.ExplosionDeathInitialLoop, 4, 5),
            Show(P.ExplosionDeathInitialLoop, 5, 6),
            RepeatWhileCounting(P.ExplosionDeathInitialLoop),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.ExplosionDeathSmokeSetup] =
        [
            SetCounter(2),
            FallInto(P.ExplosionDeathSmokeSetupLoop),
        ],
        [P.ExplosionDeathSmokeSetupLoop] =
        [
            Cue(new TorizoProjectileCue.Command(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(Range: new(31, 63), Center: new(0, 4)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.SmallExplosion, MaximumQueued: SoundQueueLimit.Max6)),
            Show(P.ExplosionDeathSmokeSetupLoop, 0, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 1, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 2, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 3, 8),
            RepeatWhileCounting(P.ExplosionDeathSmokeSetupLoop),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.DroolFourFrameDelay] =
        [
            Show(P.DroolFourFrameDelay, 0, 2),
            FallInto(P.DroolTwoFrameDelay),
        ],
        [P.DroolTwoFrameDelay] =
        [
            Show(P.DroolTwoFrameDelay, 0, 2),
            FallInto(P.DroolNoDelay),
        ],
        [P.DroolNoDelay] =
        [
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoLowHealthDrool_Falling)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Show(P.DroolNoDelay, 0, 5),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HighPriority)),
            FallInto(P.DroolFallingLoop),
        ],
        [P.DroolFallingLoop] =
        [
            Show(P.DroolFallingLoop, 0, 64),
            GoTo(P.DroolFallingLoop),
        ],
        [P.DroolWallImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.DroolFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Show(P.DroolFloorImpact, 0, 8),
            Show(P.DroolFloorImpact, 1, 8),
            Show(P.DroolFloorImpact, 2, 8),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.SwipeInitial] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BombTorizoSwipe, MaximumQueued: SoundQueueLimit.Max1)),
            Show(P.SwipeInitial, 0, 5),
            Show(P.SwipeInitial, 1, 5),
            Show(P.SwipeInitial, 2, 5),
            Show(P.SwipeInitial, 3, 5),
            Show(P.SwipeInitial, 4, 5),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment0] =
        [
            Show(P.StatueFragment0, 0, 128),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment0, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment1] =
        [
            Show(P.StatueFragment1, 0, 120),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment1, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment2] =
        [
            Show(P.StatueFragment2, 0, 112),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment2, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment3] =
        [
            Show(P.StatueFragment3, 0, 104),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment3, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment4] =
        [
            Show(P.StatueFragment4, 0, 96),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment4, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment5] =
        [
            Show(P.StatueFragment5, 0, 88),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment5, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment6] =
        [
            Show(P.StatueFragment6, 0, 80),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment6, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment7] =
        [
            Show(P.StatueFragment7, 0, 72),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment7, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment8] =
        [
            Show(P.StatueFragment8, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment8, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment9] =
        [
            Show(P.StatueFragment9, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment9, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment10] =
        [
            Show(P.StatueFragment10, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment10, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment11] =
        [
            Show(P.StatueFragment11, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment11, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment12] =
        [
            Show(P.StatueFragment12, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment12, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment13] =
        [
            Show(P.StatueFragment13, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment13, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment14] =
        [
            Show(P.StatueFragment14, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment14, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.StatueFragment15] =
        [
            Show(P.StatueFragment15, 0, 64),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.BeamImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment15, 1, 112),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.ChozoOrbMovingLeft] =
        [
            Show(P.ChozoOrbMovingLeft, 0, 85),
            GoTo(P.ChozoOrbMovingLeft),
        ],
        [P.ChozoOrbMovingRight] =
        [
            Show(P.ChozoOrbMovingRight, 0, 85),
            GoTo(P.ChozoOrbMovingRight),
        ],
        [P.ChozoOrbWallImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact)),
            Show(P.ChozoOrbWallImpact, 0, 4),
            Show(P.ChozoOrbWallImpact, 1, 4),
            Show(P.ChozoOrbWallImpact, 2, 4),
            Show(P.ChozoOrbWallImpact, 3, 4),
            Show(P.ChozoOrbWallImpact, 4, 4),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.ChozoOrbFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library3Sound.ProjectileImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Show(P.ChozoOrbFloorImpact, 0, 4),
            Show(P.ChozoOrbFloorImpact, 1, 5),
            Show(P.ChozoOrbFloorImpact, 2, 6),
            Show(P.ChozoOrbFloorImpact, 3, 7),
            Show(P.ChozoOrbFloorImpact, 4, 8),
            Show(P.ChozoOrbFloorImpact, 5, 9),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.ChozoOrbShot] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact)),
            Show(P.ChozoOrbShot, 0, 4),
            Show(P.ChozoOrbShot, 1, 4),
            Show(P.ChozoOrbShot, 2, 4),
            Show(P.ChozoOrbShot, 3, 4),
            Show(P.ChozoOrbShot, 4, 4),
            Cue(new TorizoProjectileCue.Command(K.SpawnOrbDrops)),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.SonicBoomFiredLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.TorizoSonicBoom, MaximumQueued: SoundQueueLimit.Max6)),
            Show(P.SonicBoomFiredLeft, 0, 6),
            Show(P.SonicBoomFiredLeft, 1, 6),
            FallInto(P.SonicBoomMovingLeft),
        ],
        [P.SonicBoomMovingLeft] =
        [
            Show(P.SonicBoomMovingLeft, 0, 80),
            GoTo(P.SonicBoomMovingLeft),
        ],
        [P.SonicBoomFiredRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.TorizoSonicBoom, MaximumQueued: SoundQueueLimit.Max6)),
            Show(P.SonicBoomFiredRight, 0, 6),
            Show(P.SonicBoomFiredRight, 1, 6),
            FallInto(P.SonicBoomMovingRight),
        ],
        [P.SonicBoomMovingRight] =
        [
            Show(P.SonicBoomMovingRight, 0, 80),
            GoTo(P.SonicBoomMovingRight),
        ],
        [P.SonicBoomWallImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Cue(new TorizoProjectileCue.Command(K.StopBlockingShots)),
            Cue(new TorizoProjectileCue.Command(K.StopHarmingSamus)),
            Cue(new TorizoProjectileCue.Command(K.DrawInFront)),
            SetCounter(5),
            FallInto(P.SonicBoomWallImpactLoop),
        ],
        [P.SonicBoomWallImpactLoop] =
        [
            Cue(new TorizoProjectileCue.Command(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(Range: new(15, 31), Center: new(0, 0)))),
            Show(P.SonicBoomWallImpactLoop, 0, 2),
            Show(P.SonicBoomWallImpactLoop, 1, 2),
            Show(P.SonicBoomWallImpactLoop, 2, 3),
            Show(P.SonicBoomWallImpactLoop, 3, 3),
            Show(P.SonicBoomWallImpactLoop, 4, 2),
            Cue(new TorizoProjectileCue.Command(K.NoOperation)),
            RepeatWhileCounting(P.SonicBoomWallImpactLoop),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
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
            Hold,
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
            Hold,
        ],
        [P.EggBouncingLeft] =
        [
            Show(P.EggBouncingLeft, 0, 48),
            Hold,
        ],
        [P.EggBouncingLeftHatching] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Show(P.EggBouncingLeftHatching, 0, 4),
            Show(P.EggBouncingLeftHatching, 1, 4),
            Show(P.EggBouncingLeftHatching, 2, 4),
            GoTo(P.EggHatch),
        ],
        [P.EggBouncingRight] =
        [
            Show(P.EggBouncingRight, 0, 48),
            Hold,
        ],
        [P.EggBouncingRightHatching] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Show(P.EggBouncingRightHatching, 0, 4),
            Show(P.EggBouncingRightHatching, 1, 4),
            Show(P.EggBouncingRightHatching, 2, 4),
            GoTo(P.EggHatch),
        ],
        [P.EggHatch] =
        [
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new TorizoProjectileCue.Command(K.Hatch)),
            Hold,
        ],
        [P.EggHatchedLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.ShotPipeBug, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.GoldenTorizoEgg_Hatched)),
            FallInto(P.EggHatchedLeftLoop),
        ],
        [P.EggHatchedLeftLoop] =
        [
            Show(P.EggHatchedLeftLoop, 0, 6),
            Show(P.EggHatchedLeftLoop, 1, 6),
            Show(P.EggHatchedLeftLoop, 2, 6),
            Show(P.EggHatchedLeftLoop, 3, 6),
            GoTo(P.EggHatchedLeftLoop),
        ],
        [P.EggHatchedRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.ShotPipeBug, MaximumQueued: SoundQueueLimit.Max6)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.GoldenTorizoEgg_Hatched)),
            FallInto(P.EggHatchedRightLoop),
        ],
        [P.EggHatchedRightLoop] =
        [
            Show(P.EggHatchedRightLoop, 0, 6),
            Show(P.EggHatchedRightLoop, 1, 6),
            Show(P.EggHatchedRightLoop, 2, 6),
            Show(P.EggHatchedRightLoop, 3, 6),
            GoTo(P.EggHatchedRightLoop),
        ],
        [P.EggBreakLeft] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Show(P.EggBreakLeft, 0, 4),
            Show(P.EggBreakLeft, 1, 4),
            Show(P.EggBreakLeft, 2, 4),
            Show(P.EggBreakLeft, 3, 4),
            Show(P.EggBreakLeft, 4, 10),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.EggBreakRight] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Show(P.EggBreakRight, 0, 4),
            Show(P.EggBreakRight, 1, 4),
            Show(P.EggBreakRight, 2, 4),
            Show(P.EggBreakRight, 3, 4),
            Show(P.EggBreakRight, 4, 8),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.SuperMissileRightInitial] =
        [
            Show(P.SuperMissileRightInitial, 0, 48),
            Cue(new TorizoProjectileCue.Command(K.AimSuperMissileTowardSamus)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown)),
            FallInto(P.SuperMissileRightLoop),
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
            GoTo(P.SuperMissileRightLoop),
        ],
        [P.SuperMissileLeftInitial] =
        [
            Show(P.SuperMissileLeftInitial, 0, 48),
            Cue(new TorizoProjectileCue.Command(K.AimSuperMissileAwayFromSamus)),
            Cue(new TorizoProjectileCue.SetPreInstruction(PreInstruction: EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown)),
            FallInto(P.SuperMissileLeftLoop),
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
            GoTo(P.SuperMissileLeftLoop),
        ],
        [P.SuperMissileImpact] =
        [
            Cue(new TorizoProjectileCue.SetRadius(Radius: new(16, 16))),
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library2Sound.SmallExplosion, MaximumQueued: SoundQueueLimit.Max6)),
            Show(P.SuperMissileImpact, 0, 5),
            Show(P.SuperMissileImpact, 1, 5),
            Show(P.SuperMissileImpact, 2, 5),
            Show(P.SuperMissileImpact, 3, 5),
            Show(P.SuperMissileImpact, 4, 5),
            Show(P.SuperMissileImpact, 5, 5),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.EyeBeamWallImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            Show(P.EyeBeamWallImpact, 0, 4),
            Show(P.EyeBeamWallImpact, 1, 4),
            Show(P.EyeBeamWallImpact, 2, 4),
            Show(P.EyeBeamWallImpact, 3, 4),
            Show(P.EyeBeamWallImpact, 4, 4),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.EyeBeamFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Command(K.ClearPreInstruction)),
            FallInto(P.EyeBeamFloorImpactLoop),
        ],
        [P.EyeBeamFloorImpactLoop] =
        [
            Show(P.EyeBeamFloorImpactLoop, 0, 8),
            Cue(new TorizoProjectileCue.GoToIfEyeBeamExplosionsDisabled(Target: P.EyeBeamFloorImpactLoop)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: Library3Sound.ProjectileImpact, MaximumQueued: SoundQueueLimit.Max6)),
            Show(P.EyeBeamFloorImpactLoop, 1, 4),
            Show(P.EyeBeamFloorImpactLoop, 2, 5),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Show(P.EyeBeamFloorImpactLoop, 3, 6),
            Show(P.EyeBeamFloorImpactLoop, 4, 7),
            Show(P.EyeBeamFloorImpactLoop, 5, 8),
            Show(P.EyeBeamFloorImpactLoop, 6, 9),
            Cue(new TorizoProjectileCue.Command(K.Delete)),
            Hold,
        ],
        [P.EyeBeamNormal] =
        [
            Show(P.EyeBeamNormal, 0, 1),
            Show(P.EyeBeamNormal, 1, 1),
            Show(P.EyeBeamNormal, 2, 1),
            Show(P.EyeBeamNormal, 3, 1),
            Show(P.EyeBeamNormal, 4, 1),
            GoTo(P.EyeBeamNormal),
        ],
    });

    /// <summary>The program of statue fragment <paramref name="index"/>, of the sixteen the breaking statue hand emits.</summary>
    internal static TorizoProjectileProgram StatueFragment(int index) => (uint)index < (uint)StatueFragments.Length
        ? StatueFragments[index]
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Bomb Torizo's statue hand emits sixteen fragments.");

    /// <summary>The sixteen statue fragments' programs, indexed by fragment.</summary>
    private static readonly TorizoProjectileProgram[] StatueFragments =
    [
        P.StatueFragment0, P.StatueFragment1, P.StatueFragment2, P.StatueFragment3,
        P.StatueFragment4, P.StatueFragment5, P.StatueFragment6, P.StatueFragment7,
        P.StatueFragment8, P.StatueFragment9, P.StatueFragment10, P.StatueFragment11,
        P.StatueFragment12, P.StatueFragment13, P.StatueFragment14, P.StatueFragment15,
    ];

    /// <summary>The program a sleeping egg resumes into when its hatch timer steps the list past the sleep.</summary>
    internal static TorizoProjectileProgram AfterSleep(TorizoProjectileProgram program) => program switch
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
    private static AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> Show(
        TorizoProjectileProgram program, int phase, int duration) =>
        AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.Show(new(program, phase), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> Cue(TorizoProjectileCue cue) =>
        AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.Publish(cue);

    /// <summary>Loads the projectile's general timer with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> SetCounter(int count) =>
        AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the general timer counts down.</summary>
    private static AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> RepeatWhileCounting(TorizoProjectileProgram program) =>
        AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> GoTo(TorizoProjectileProgram program) =>
        AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> FallInto(TorizoProjectileProgram program) =>
        AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep and deletion do.</summary>
    private static AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> Hold =>
        AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.Hold;
}
