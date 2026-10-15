using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileProgram;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

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
        // $86:A3CB
        [P.ExplosionLowHealthInitial] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.SetProperties, Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus)),
            SetCounter(3),
            FallInto(P.ExplosionLowHealthInitialLoop),
        ],
        // $86:A3D5
        [P.ExplosionLowHealthInitialLoop] =
        [
            Cue(new(K.ReturnToOrigin)),
            Cue(new(K.Scatter, Scatter: new(Range: new(15, 15), Center: new(0, 0)))),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.SmallExplosion, MaximumQueued: 6)),
            Show(P.ExplosionLowHealthInitialLoop, 0, 2),
            Show(P.ExplosionLowHealthInitialLoop, 1, 2),
            Show(P.ExplosionLowHealthInitialLoop, 2, 3),
            Show(P.ExplosionLowHealthInitialLoop, 3, 3),
            Show(P.ExplosionLowHealthInitialLoop, 4, 2),
            RepeatWhileCounting(P.ExplosionLowHealthInitialLoop),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A3FA
        [P.ExplosionDeathInitial] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.SetProperties, Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new(K.GoToSometimes, Target: P.ExplosionDeathSmokeSetup)),
            SetCounter(2),
            FallInto(P.ExplosionDeathInitialLoop),
        ],
        // $86:A408
        [P.ExplosionDeathInitialLoop] =
        [
            Cue(new(K.ReturnToOrigin)),
            Cue(new(K.Scatter, Scatter: new(Range: new(31, 63), Center: new(0, 16)))),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.SmallExplosion, MaximumQueued: 6)),
            Show(P.ExplosionDeathInitialLoop, 0, 4),
            Show(P.ExplosionDeathInitialLoop, 1, 6),
            Show(P.ExplosionDeathInitialLoop, 2, 5),
            Show(P.ExplosionDeathInitialLoop, 3, 5),
            Show(P.ExplosionDeathInitialLoop, 4, 5),
            Show(P.ExplosionDeathInitialLoop, 5, 6),
            RepeatWhileCounting(P.ExplosionDeathInitialLoop),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A431
        [P.ExplosionDeathSmokeSetup] =
        [
            SetCounter(2),
            FallInto(P.ExplosionDeathSmokeSetupLoop),
        ],
        // $86:A435
        [P.ExplosionDeathSmokeSetupLoop] =
        [
            Cue(new(K.ReturnToOrigin)),
            Cue(new(K.Scatter, Scatter: new(Range: new(31, 63), Center: new(0, 4)))),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.SmallExplosion, MaximumQueued: 6)),
            Show(P.ExplosionDeathSmokeSetupLoop, 0, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 1, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 2, 8),
            Show(P.ExplosionDeathSmokeSetupLoop, 3, 8),
            RepeatWhileCounting(P.ExplosionDeathSmokeSetupLoop),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A46A
        [P.DroolFourFrameDelay] =
        [
            Show(P.DroolFourFrameDelay, 0, 2),
            FallInto(P.DroolTwoFrameDelay),
        ],
        // $86:A46E
        [P.DroolTwoFrameDelay] =
        [
            Show(P.DroolTwoFrameDelay, 0, 2),
            FallInto(P.DroolNoDelay),
        ],
        // $86:A472
        [P.DroolNoDelay] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoLowHealthDrool_Falling)),
            Cue(new(K.SetProperties, Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Show(P.DroolNoDelay, 0, 5),
            Cue(new(K.ClearProperties, Properties: EnemyProjectilePropertyFlags.HighPriority)),
            FallInto(P.DroolFallingLoop),
        ],
        // $86:A482
        [P.DroolFallingLoop] =
        [
            Show(P.DroolFallingLoop, 0, 64),
            GoTo(P.DroolFallingLoop),
        ],
        // $86:A48A
        [P.DroolWallImpact] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A48E
        [P.DroolFloorImpact] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(P.DroolFloorImpact, 0, 8),
            Show(P.DroolFloorImpact, 1, 8),
            Show(P.DroolFloorImpact, 2, 8),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A4AA
        [P.SwipeInitial] =
        [
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BombTorizoExplosiveSwipe, MaximumQueued: 1)),
            Show(P.SwipeInitial, 0, 5),
            Show(P.SwipeInitial, 1, 5),
            Show(P.SwipeInitial, 2, 5),
            Show(P.SwipeInitial, 3, 5),
            Show(P.SwipeInitial, 4, 5),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A4C3
        [P.StatueFragment0] =
        [
            Show(P.StatueFragment0, 0, 128),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment0, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A4D4
        [P.StatueFragment1] =
        [
            Show(P.StatueFragment1, 0, 120),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment1, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A4E5
        [P.StatueFragment2] =
        [
            Show(P.StatueFragment2, 0, 112),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment2, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A4F6
        [P.StatueFragment3] =
        [
            Show(P.StatueFragment3, 0, 104),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment3, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A507
        [P.StatueFragment4] =
        [
            Show(P.StatueFragment4, 0, 96),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment4, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A518
        [P.StatueFragment5] =
        [
            Show(P.StatueFragment5, 0, 88),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment5, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A529
        [P.StatueFragment6] =
        [
            Show(P.StatueFragment6, 0, 80),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment6, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A53A
        [P.StatueFragment7] =
        [
            Show(P.StatueFragment7, 0, 72),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment7, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A54B
        [P.StatueFragment8] =
        [
            Show(P.StatueFragment8, 0, 64),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment8, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A55C
        [P.StatueFragment9] =
        [
            Show(P.StatueFragment9, 0, 64),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment9, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A56D
        [P.StatueFragment10] =
        [
            Show(P.StatueFragment10, 0, 64),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment10, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A57E
        [P.StatueFragment11] =
        [
            Show(P.StatueFragment11, 0, 64),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment11, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A58F
        [P.StatueFragment12] =
        [
            Show(P.StatueFragment12, 0, 64),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment12, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A5A0
        [P.StatueFragment13] =
        [
            Show(P.StatueFragment13, 0, 64),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment13, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A5B1
        [P.StatueFragment14] =
        [
            Show(P.StatueFragment14, 0, 64),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment14, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:A5C2
        [P.StatueFragment15] =
        [
            Show(P.StatueFragment15, 0, 64),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.BeamImpact, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling)),
            Show(P.StatueFragment15, 1, 112),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:AB15
        [P.ChozoOrbMovingLeft] =
        [
            Show(P.ChozoOrbMovingLeft, 0, 85),
            GoTo(P.ChozoOrbMovingLeft),
        ],
        // $86:AB1D
        [P.ChozoOrbMovingRight] =
        [
            Show(P.ChozoOrbMovingRight, 0, 85),
            GoTo(P.ChozoOrbMovingRight),
        ],
        // $86:AB25
        [P.ChozoOrbWallImpact] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.SetProperties, Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact)),
            Show(P.ChozoOrbWallImpact, 0, 4),
            Show(P.ChozoOrbWallImpact, 1, 4),
            Show(P.ChozoOrbWallImpact, 2, 4),
            Show(P.ChozoOrbWallImpact, 3, 4),
            Show(P.ChozoOrbWallImpact, 4, 4),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:AB41
        [P.ChozoOrbFloorImpact] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.ClearProperties, Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new(K.SetProperties, Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact)),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary3Sounds.TorizoProjectileFloorImpact, MaximumQueued: 6)),
            Show(P.ChozoOrbFloorImpact, 0, 4),
            Show(P.ChozoOrbFloorImpact, 1, 5),
            Show(P.ChozoOrbFloorImpact, 2, 6),
            Show(P.ChozoOrbFloorImpact, 3, 7),
            Show(P.ChozoOrbFloorImpact, 4, 8),
            Show(P.ChozoOrbFloorImpact, 5, 9),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:AB68
        [P.ChozoOrbShot] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.SetProperties, Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact)),
            Show(P.ChozoOrbShot, 0, 4),
            Show(P.ChozoOrbShot, 1, 4),
            Show(P.ChozoOrbShot, 2, 4),
            Show(P.ChozoOrbShot, 3, 4),
            Show(P.ChozoOrbShot, 4, 4),
            Cue(new(K.SpawnOrbDrops)),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:ADBF
        [P.SonicBoomFiredLeft] =
        [
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.TorizoSonicBoom, MaximumQueued: 6)),
            Show(P.SonicBoomFiredLeft, 0, 6),
            Show(P.SonicBoomFiredLeft, 1, 6),
            FallInto(P.SonicBoomMovingLeft),
        ],
        // $86:ADCA
        [P.SonicBoomMovingLeft] =
        [
            Show(P.SonicBoomMovingLeft, 0, 80),
            GoTo(P.SonicBoomMovingLeft),
        ],
        // $86:ADD2
        [P.SonicBoomFiredRight] =
        [
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.TorizoSonicBoom, MaximumQueued: 6)),
            Show(P.SonicBoomFiredRight, 0, 6),
            Show(P.SonicBoomFiredRight, 1, 6),
            FallInto(P.SonicBoomMovingRight),
        ],
        // $86:ADDD
        [P.SonicBoomMovingRight] =
        [
            Show(P.SonicBoomMovingRight, 0, 80),
            GoTo(P.SonicBoomMovingRight),
        ],
        // $86:ADE5
        [P.SonicBoomWallImpact] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.StopBlockingShots)),
            Cue(new(K.StopHarmingSamus)),
            Cue(new(K.DrawInFront)),
            SetCounter(5),
            FallInto(P.SonicBoomWallImpactLoop),
        ],
        // $86:ADF1
        [P.SonicBoomWallImpactLoop] =
        [
            Cue(new(K.ReturnToOrigin)),
            Cue(new(K.Scatter, Scatter: new(Range: new(15, 31), Center: new(0, 0)))),
            Show(P.SonicBoomWallImpactLoop, 0, 2),
            Show(P.SonicBoomWallImpactLoop, 1, 2),
            Show(P.SonicBoomWallImpactLoop, 2, 3),
            Show(P.SonicBoomWallImpactLoop, 3, 3),
            Show(P.SonicBoomWallImpactLoop, 4, 2),
            Cue(new(K.NoOperation)),
            RepeatWhileCounting(P.SonicBoomWallImpactLoop),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:AF9D
        [P.LandingDustRightFoot] =
        [
            Show(P.LandingDustRightFoot, 0, 4),
            Cue(new(K.RaiseDust)),
            Show(P.LandingDustRightFoot, 1, 4),
            Cue(new(K.RaiseDust)),
            Show(P.LandingDustRightFoot, 2, 4),
            Cue(new(K.RaiseDust)),
            Show(P.LandingDustRightFoot, 3, 4),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:AFB5
        [P.LandingDustLeftFoot] =
        [
            Show(P.LandingDustLeftFoot, 0, 4),
            Cue(new(K.RaiseDust)),
            Show(P.LandingDustLeftFoot, 1, 4),
            Cue(new(K.RaiseDust)),
            Show(P.LandingDustLeftFoot, 2, 4),
            Cue(new(K.RaiseDust)),
            Show(P.LandingDustLeftFoot, 3, 4),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:B104
        [P.EggBouncingLeft] =
        [
            Show(P.EggBouncingLeft, 0, 48),
            Hold,
        ],
        // $86:B10A
        [P.EggBouncingLeftHatching] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(P.EggBouncingLeftHatching, 0, 4),
            Show(P.EggBouncingLeftHatching, 1, 4),
            Show(P.EggBouncingLeftHatching, 2, 4),
            GoTo(P.EggHatch),
        ],
        // $86:B11C
        [P.EggBouncingRight] =
        [
            Show(P.EggBouncingRight, 0, 48),
            Hold,
        ],
        // $86:B122
        [P.EggBouncingRightHatching] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(P.EggBouncingRightHatching, 0, 4),
            Show(P.EggBouncingRightHatching, 1, 4),
            Show(P.EggBouncingRightHatching, 2, 4),
            GoTo(P.EggHatch),
        ],
        // $86:B134
        [P.EggHatch] =
        [
            Cue(new(K.ClearProperties, Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new(K.SetProperties, Properties: EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new(K.Hatch)),
            Hold,
        ],
        // $86:B14B
        [P.EggHatchedLeft] =
        [
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.GoldenTorizoEggHatch, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.GoldenTorizoEgg_Hatched)),
            FallInto(P.EggHatchedLeftLoop),
        ],
        // $86:B152
        [P.EggHatchedLeftLoop] =
        [
            Show(P.EggHatchedLeftLoop, 0, 6),
            Show(P.EggHatchedLeftLoop, 1, 6),
            Show(P.EggHatchedLeftLoop, 2, 6),
            Show(P.EggHatchedLeftLoop, 3, 6),
            GoTo(P.EggHatchedLeftLoop),
        ],
        // $86:B166
        [P.EggHatchedRight] =
        [
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.GoldenTorizoEggHatch, MaximumQueued: 6)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.GoldenTorizoEgg_Hatched)),
            FallInto(P.EggHatchedRightLoop),
        ],
        // $86:B16D
        [P.EggHatchedRightLoop] =
        [
            Show(P.EggHatchedRightLoop, 0, 6),
            Show(P.EggHatchedRightLoop, 1, 6),
            Show(P.EggHatchedRightLoop, 2, 6),
            Show(P.EggHatchedRightLoop, 3, 6),
            GoTo(P.EggHatchedRightLoop),
        ],
        // $86:B190
        [P.EggBreakLeft] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(P.EggBreakLeft, 0, 4),
            Show(P.EggBreakLeft, 1, 4),
            Show(P.EggBreakLeft, 2, 4),
            Show(P.EggBreakLeft, 3, 4),
            Show(P.EggBreakLeft, 4, 10),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:B1A8
        [P.EggBreakRight] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(P.EggBreakRight, 0, 4),
            Show(P.EggBreakRight, 1, 4),
            Show(P.EggBreakRight, 2, 4),
            Show(P.EggBreakRight, 3, 4),
            Show(P.EggBreakRight, 4, 8),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:B293
        [P.SuperMissileRightInitial] =
        [
            Show(P.SuperMissileRightInitial, 0, 48),
            Cue(new(K.AimSuperMissileTowardSamus)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown)),
            FallInto(P.SuperMissileRightLoop),
        ],
        // $86:B29D
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
        // $86:B2C1
        [P.SuperMissileLeftInitial] =
        [
            Show(P.SuperMissileLeftInitial, 0, 48),
            Cue(new(K.AimSuperMissileAwayFromSamus)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown)),
            FallInto(P.SuperMissileLeftLoop),
        ],
        // $86:B2CB
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
        // $86:B2EF
        [P.SuperMissileImpact] =
        [
            Cue(new(K.SetRadius, Radius: new(16, 16))),
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.SetProperties, Properties: EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact)),
            Cue(new(K.ClearProperties, Properties: EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary2Sounds.SmallExplosion, MaximumQueued: 6)),
            Show(P.SuperMissileImpact, 0, 5),
            Show(P.SuperMissileImpact, 1, 5),
            Show(P.SuperMissileImpact, 2, 5),
            Show(P.SuperMissileImpact, 3, 5),
            Show(P.SuperMissileImpact, 4, 5),
            Show(P.SuperMissileImpact, 5, 5),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:B3CD
        [P.EyeBeamWallImpact] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(P.EyeBeamWallImpact, 0, 4),
            Show(P.EyeBeamWallImpact, 1, 4),
            Show(P.EyeBeamWallImpact, 2, 4),
            Show(P.EyeBeamWallImpact, 3, 4),
            Show(P.EyeBeamWallImpact, 4, 4),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:B3E5
        [P.EyeBeamFloorImpact] =
        [
            Cue(new(K.ClearPreInstruction)),
            FallInto(P.EyeBeamFloorImpactLoop),
        ],
        // $86:B3E7
        [P.EyeBeamFloorImpactLoop] =
        [
            Show(P.EyeBeamFloorImpactLoop, 0, 8),
            Cue(new(K.GoToIfEyeBeamExplosionsDisabled, Target: P.EyeBeamFloorImpactLoop)),
            Cue(new(K.PlaySound, Sound: SoundEffectLibrary3Sounds.TorizoProjectileFloorImpact, MaximumQueued: 6)),
            Show(P.EyeBeamFloorImpactLoop, 1, 4),
            Show(P.EyeBeamFloorImpactLoop, 2, 5),
            Cue(new(K.ClearProperties, Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Show(P.EyeBeamFloorImpactLoop, 3, 6),
            Show(P.EyeBeamFloorImpactLoop, 4, 7),
            Show(P.EyeBeamFloorImpactLoop, 5, 8),
            Show(P.EyeBeamFloorImpactLoop, 6, 9),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:B410
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
    internal static TorizoProjectileProgram StatueFragment(int index) => index switch
    {
        0 => P.StatueFragment0,
        1 => P.StatueFragment1,
        2 => P.StatueFragment2,
        3 => P.StatueFragment3,
        4 => P.StatueFragment4,
        5 => P.StatueFragment5,
        6 => P.StatueFragment6,
        7 => P.StatueFragment7,
        8 => P.StatueFragment8,
        9 => P.StatueFragment9,
        10 => P.StatueFragment10,
        11 => P.StatueFragment11,
        12 => P.StatueFragment12,
        13 => P.StatueFragment13,
        14 => P.StatueFragment14,
        15 => P.StatueFragment15,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Bomb Torizo's statue hand emits sixteen fragments."),
    };

    /// <summary>The program a sleeping egg resumes into when its hatch timer steps the list past the sleep.</summary>
    internal static TorizoProjectileProgram AfterSleep(TorizoProjectileProgram program) => program switch
    {
        P.EggBouncingLeft => P.EggBouncingLeftHatching,
        P.EggBouncingRight => P.EggBouncingRightHatching,
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
