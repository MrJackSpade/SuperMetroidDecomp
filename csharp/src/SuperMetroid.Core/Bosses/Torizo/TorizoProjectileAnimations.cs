using System.Collections.Frozen;
using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileFrameKind;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's projectile animations (#641, #1788): each animation's steps, each frame a
/// <see cref="TorizoProjectileFrame"/> pose of the run of poses it belongs to and each cue a <see cref="TorizoProjectileCue"/>
/// with the values it acts on; the phases it plays them in, each a run of its steps played a number of times; and whether it
/// then loops or holds. The runs several animations play alike (the drool's release and fall, the egg's hatch, the statue
/// fragments' crack) are one array of steps each reuses.
/// </summary>
internal static class TorizoProjectileAnimations
{
    /// <summary>
    /// A drool drop's release: it starts falling, harmless and in front of Samus, then moves behind her. The shared runs are
    /// declared before the steps that spread them in, so static initialization builds them first.
    /// </summary>
    private static readonly AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>[] DroolRelease =
    [
        Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoDroolFall)),
        Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
        Show(F.Drool, 2, 5),
        Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus)),
    ];

    /// <summary>A drool drop's falling pose, which its animation loops on while it falls.</summary>
    private static readonly AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>[] DroolFalling =
    [
        Show(F.Drool, 3, 64),
    ];

    /// <summary>A cracked egg's hatch: it turns harmful and shot-blocking, then continues in its hatched animation.</summary>
    private static readonly AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>[] EggHatch =
    [
        Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
        Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.BlocksShots)),
        Cue(new TorizoProjectileCue.Signal(K.Hatch)),
    ];

    /// <summary>A statue fragment cracking off the hand: the impact sound, then its fall.</summary>
    private static readonly AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>[] StatueFragmentCracking =
    [
        Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
        Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.BombTorizoStatueFragmentFall)),
    ];

    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<TorizoProjectileSegment, TorizoProjectileFrame, TorizoProjectileCue> Segments { get; } = new(
        new Dictionary<TorizoProjectileSegment, AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>[]>
    {
        [P.LowHealthExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 0)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(F.LowHealthExplosion, 0, 2),
            Show(F.LowHealthExplosion, 1, 2),
            Show(F.LowHealthExplosion, 2, 3),
            Show(F.LowHealthExplosion, 3, 3),
            Show(F.LowHealthExplosion, 4, 2),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DeathExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.GoToSometimes(Target: P.DeathSmoke)),
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 16)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(F.DeathExplosion, 0, 4),
            Show(F.DeathExplosion, 1, 6),
            Show(F.DeathExplosion, 2, 5),
            Show(F.DeathExplosion, 3, 5),
            Show(F.DeathExplosion, 4, 5),
            Show(F.DeathExplosion, 5, 6),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DeathSmoke] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 4)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(F.DeathSmoke, 0, 8),
            Show(F.DeathSmoke, 1, 8),
            Show(F.DeathSmoke, 2, 8),
            Show(F.DeathSmoke, 3, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DroolFourFrameDelay] = [Show(F.Drool, 0, 2), Show(F.Drool, 1, 2), .. DroolRelease, .. DroolFalling],
        [P.DroolTwoFrameDelay] = [Show(F.Drool, 1, 2), .. DroolRelease, .. DroolFalling],
        [P.DroolNoDelay] = [.. DroolRelease, .. DroolFalling],
        [P.DroolWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DroolFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Show(F.DroolSplash, 0, 8),
            Show(F.DroolSplash, 1, 8),
            Show(F.DroolSplash, 2, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ExplosiveSwipe] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BombTorizoSwipe, Priority: SoundRequestPriority.WhenIdle)),
            Show(F.ExplosiveSwipe, 0, 5),
            Show(F.ExplosiveSwipe, 1, 5),
            Show(F.ExplosiveSwipe, 2, 5),
            Show(F.ExplosiveSwipe, 3, 5),
            Show(F.ExplosiveSwipe, 4, 5),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.FirstStatueFragment] = [Show(F.StatueFragment, 0, 128), .. StatueFragmentCracking, Show(F.StatueFragment, 1, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SecondStatueFragment] = [Show(F.StatueFragment, 2, 120), .. StatueFragmentCracking, Show(F.StatueFragment, 3, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.ThirdStatueFragment] = [Show(F.StatueFragment, 4, 112), .. StatueFragmentCracking, Show(F.StatueFragment, 5, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.FourthStatueFragment] = [Show(F.StatueFragment, 6, 104), .. StatueFragmentCracking, Show(F.StatueFragment, 7, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.FifthStatueFragment] = [Show(F.StatueFragment, 8, 96), .. StatueFragmentCracking, Show(F.StatueFragment, 9, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SixthStatueFragment] = [Show(F.StatueFragment, 10, 88), .. StatueFragmentCracking, Show(F.StatueFragment, 11, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SeventhStatueFragment] = [Show(F.StatueFragment, 12, 80), .. StatueFragmentCracking, Show(F.StatueFragment, 13, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.EighthStatueFragment] = [Show(F.StatueFragment, 14, 72), .. StatueFragmentCracking, Show(F.StatueFragment, 15, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.NinthStatueFragment] = [Show(F.StatueFragment, 16, 64), .. StatueFragmentCracking, Show(F.StatueFragment, 17, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.TenthStatueFragment] = [Show(F.StatueFragment, 18, 64), .. StatueFragmentCracking, Show(F.StatueFragment, 19, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.EleventhStatueFragment] = [Show(F.StatueFragment, 20, 64), .. StatueFragmentCracking, Show(F.StatueFragment, 21, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.TwelfthStatueFragment] = [Show(F.StatueFragment, 22, 64), .. StatueFragmentCracking, Show(F.StatueFragment, 23, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.ThirteenthStatueFragment] = [Show(F.StatueFragment, 24, 64), .. StatueFragmentCracking, Show(F.StatueFragment, 25, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.FourteenthStatueFragment] = [Show(F.StatueFragment, 26, 64), .. StatueFragmentCracking, Show(F.StatueFragment, 27, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.FifteenthStatueFragment] = [Show(F.StatueFragment, 28, 64), .. StatueFragmentCracking, Show(F.StatueFragment, 29, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.SixteenthStatueFragment] = [Show(F.StatueFragment, 30, 64), .. StatueFragmentCracking, Show(F.StatueFragment, 31, 112), Cue(new TorizoProjectileCue.Signal(K.Delete))],
        [P.ChozoOrbMovingLeft] = [Show(F.ChozoOrbMovingLeft, 0, 85)],
        [P.ChozoOrbMovingRight] = [Show(F.ChozoOrbMovingRight, 0, 85)],
        [P.ChozoOrbWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(F.ChozoOrbWallImpact, 0, 4),
            Show(F.ChozoOrbWallImpact, 1, 4),
            Show(F.ChozoOrbWallImpact, 2, 4),
            Show(F.ChozoOrbWallImpact, 3, 4),
            Show(F.ChozoOrbWallImpact, 4, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ChozoOrbFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(F.ChozoOrbFloorImpact, 0, 4),
            Show(F.ChozoOrbFloorImpact, 1, 5),
            Show(F.ChozoOrbFloorImpact, 2, 6),
            Show(F.ChozoOrbFloorImpact, 3, 7),
            Show(F.ChozoOrbFloorImpact, 4, 8),
            Show(F.ChozoOrbFloorImpact, 5, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ChozoOrbShot] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Show(F.ChozoOrbShot, 0, 4),
            Show(F.ChozoOrbShot, 1, 4),
            Show(F.ChozoOrbShot, 2, 4),
            Show(F.ChozoOrbShot, 3, 4),
            Show(F.ChozoOrbShot, 4, 4),
            Cue(new TorizoProjectileCue.Signal(K.SpawnOrbDrops)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.SonicBoomFiredLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(F.SonicBoomFiredLeft, 0, 6),
            Show(F.SonicBoomFiredLeft, 1, 6),
            Show(F.SonicBoomMovingLeft, 0, 80),
        ],
        [P.SonicBoomFiredRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
            Show(F.SonicBoomFiredRight, 0, 6),
            Show(F.SonicBoomFiredRight, 1, 6),
            Show(F.SonicBoomMovingRight, 0, 80),
        ],
        [P.SonicBoomWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.Signal(K.StopBlockingShots)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamus)),
            Cue(new TorizoProjectileCue.Signal(K.DrawInFront)),
            Cue(new TorizoProjectileCue.Signal(K.ReturnToOrigin)),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 5), Center: new(0, 0)))),
            Show(F.SonicBoomWallImpact, 0, 2),
            Show(F.SonicBoomWallImpact, 1, 2),
            Show(F.SonicBoomWallImpact, 2, 3),
            Show(F.SonicBoomWallImpact, 3, 3),
            Show(F.SonicBoomWallImpact, 4, 2),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.LandingDustRightFoot] =
        [
            Show(F.LandingDustRightFoot, 0, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(F.LandingDustRightFoot, 1, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(F.LandingDustRightFoot, 2, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(F.LandingDustRightFoot, 3, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.LandingDustLeftFoot] =
        [
            Show(F.LandingDustLeftFoot, 0, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(F.LandingDustLeftFoot, 1, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(F.LandingDustLeftFoot, 2, 4),
            Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
            Show(F.LandingDustLeftFoot, 3, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EggBouncingLeft] = [Show(F.EggBouncingLeft, 0, 48)],
        [P.EggHatchingLeft] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Show(F.EggHatchingLeft, 0, 4),
            Show(F.EggHatchingLeft, 1, 4),
            Show(F.EggHatchingLeft, 2, 4),
            .. EggHatch,
        ],
        [P.EggBouncingRight] = [Show(F.EggBouncingRight, 0, 48)],
        [P.EggHatchingRight] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Show(F.EggHatchingRight, 0, 4),
            Show(F.EggHatchingRight, 1, 4),
            Show(F.EggHatchingRight, 2, 4),
            .. EggHatch,
        ],
        [P.EggHatchedLeft] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.ShotPipeBug, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.GoldenTorizoEggCharge)),
            Show(F.EggHatchedLeft, 0, 6),
            Show(F.EggHatchedLeft, 1, 6),
            Show(F.EggHatchedLeft, 2, 6),
            Show(F.EggHatchedLeft, 3, 6),
        ],
        [P.EggHatchedRight] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.ShotPipeBug, Priority: SoundRequestPriority.Normal)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.GoldenTorizoEggCharge)),
            Show(F.EggHatchedRight, 0, 6),
            Show(F.EggHatchedRight, 1, 6),
            Show(F.EggHatchedRight, 2, 6),
            Show(F.EggHatchedRight, 3, 6),
        ],
        [P.EggBreakLeft] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Show(F.EggBreakLeft, 0, 4),
            Show(F.EggBreakLeft, 1, 4),
            Show(F.EggBreakLeft, 2, 4),
            Show(F.EggBreakLeft, 3, 4),
            Show(F.EggBreakLeft, 4, 10),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EggBreakRight] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Show(F.EggBreakRight, 0, 4),
            Show(F.EggBreakRight, 1, 4),
            Show(F.EggBreakRight, 2, 4),
            Show(F.EggBreakRight, 3, 4),
            Show(F.EggBreakRight, 4, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.SuperMissileRight] =
        [
            Show(F.SuperMissileHeldRight, 0, 48),
            Cue(new TorizoProjectileCue.Signal(K.AimSuperMissileTowardSamus)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileThrown)),
            Show(F.SuperMissileSpinningRight, 0, 2),
            Show(F.SuperMissileSpinningRight, 1, 2),
            Show(F.SuperMissileSpinningRight, 2, 2),
            Show(F.SuperMissileSpinningRight, 3, 2),
            Show(F.SuperMissileSpinningRight, 4, 2),
            Show(F.SuperMissileSpinningRight, 5, 2),
            Show(F.SuperMissileSpinningRight, 6, 2),
            Show(F.SuperMissileSpinningRight, 7, 2),
        ],
        [P.SuperMissileLeft] =
        [
            Show(F.SuperMissileHeldLeft, 0, 48),
            Cue(new TorizoProjectileCue.Signal(K.AimSuperMissileAwayFromSamus)),
            Cue(new TorizoProjectileCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileThrown)),
            Show(F.SuperMissileSpinningLeft, 0, 2),
            Show(F.SuperMissileSpinningLeft, 1, 2),
            Show(F.SuperMissileSpinningLeft, 2, 2),
            Show(F.SuperMissileSpinningLeft, 3, 2),
            Show(F.SuperMissileSpinningLeft, 4, 2),
            Show(F.SuperMissileSpinningLeft, 5, 2),
            Show(F.SuperMissileSpinningLeft, 6, 2),
            Show(F.SuperMissileSpinningLeft, 7, 2),
        ],
        [P.SuperMissileImpact] =
        [
            Cue(new TorizoProjectileCue.SetRadius(Radius: new(16, 16))),
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Cue(new TorizoProjectileCue.SetProperties(Properties: EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus)),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(F.SuperMissileImpact, 0, 5),
            Show(F.SuperMissileImpact, 1, 5),
            Show(F.SuperMissileImpact, 2, 5),
            Show(F.SuperMissileImpact, 3, 5),
            Show(F.SuperMissileImpact, 4, 5),
            Show(F.SuperMissileImpact, 5, 5),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeamWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Show(F.EyeBeamWallImpact, 0, 4),
            Show(F.EyeBeamWallImpact, 1, 4),
            Show(F.EyeBeamWallImpact, 2, 4),
            Show(F.EyeBeamWallImpact, 3, 4),
            Show(F.EyeBeamWallImpact, 4, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeamFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.ClearFrameBehavior)),
            Show(F.EyeBeamFloorImpact, 0, 8),
            Cue(new TorizoProjectileCue.Signal(K.ReplayWhileEyeBeamExplosionsDisabled)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(F.EyeBeamFloorImpact, 1, 4),
            Show(F.EyeBeamFloorImpact, 2, 5),
            Cue(new TorizoProjectileCue.ClearProperties(Properties: EnemyProjectilePropertyFlags.HarmlessToSamus)),
            Show(F.EyeBeamFloorImpact, 3, 6),
            Show(F.EyeBeamFloorImpact, 4, 7),
            Show(F.EyeBeamFloorImpact, 5, 8),
            Show(F.EyeBeamFloorImpact, 6, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeam] =
        [
            Show(F.EyeBeam, 0, 1),
            Show(F.EyeBeam, 1, 1),
            Show(F.EyeBeam, 2, 1),
            Show(F.EyeBeam, 3, 1),
            Show(F.EyeBeam, 4, 1),
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
        [P.NinthStatueFragment] = new([new(P.NinthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.TenthStatueFragment] = new([new(P.TenthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.EleventhStatueFragment] = new([new(P.EleventhStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.TwelfthStatueFragment] = new([new(P.TwelfthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.ThirteenthStatueFragment] = new([new(P.ThirteenthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.FourteenthStatueFragment] = new([new(P.FourteenthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.FifteenthStatueFragment] = new([new(P.FifteenthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
        [P.SixteenthStatueFragment] = new([new(P.SixteenthStatueFragment, 0, 5, 1)], AnimationEnd.Holds),
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

    /// <summary>The sixteen statue fragments' animations, indexed by fragment.</summary>
    private static readonly TorizoProjectileSegment[] StatueFragments =
    [
        P.FirstStatueFragment, P.SecondStatueFragment, P.ThirdStatueFragment, P.FourthStatueFragment,
        P.FifthStatueFragment, P.SixthStatueFragment, P.SeventhStatueFragment, P.EighthStatueFragment,
        P.NinthStatueFragment, P.TenthStatueFragment, P.EleventhStatueFragment, P.TwelfthStatueFragment,
        P.ThirteenthStatueFragment, P.FourteenthStatueFragment, P.FifteenthStatueFragment, P.SixteenthStatueFragment,
    ];

    /// <summary>The animation each sleeping egg resumes into when its hatch timer wakes it; no other animation sleeps.</summary>
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

    /// <summary>The animation of statue fragment <paramref name="index"/>, of the sixteen the breaking statue hand emits.</summary>
    internal static TorizoProjectileSegment StatueFragment(int index) => (uint)index < (uint)StatueFragments.Length
        ? StatueFragments[index]
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Bomb Torizo's statue hand emits sixteen fragments.");

    /// <summary>The animation a sleeping egg resumes into when its hatch timer wakes it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="animation"/> does not sleep.</exception>
    internal static TorizoProjectileSegment AfterSleep(TorizoProjectileSegment animation) =>
        ResumesAfterSleep.TryGetValue(animation, out P resumed)
            ? resumed
            : throw new ArgumentOutOfRangeException(nameof(animation), animation, "The animation does not sleep.");

    /// <summary>Shows pose <paramref name="phase"/> of the run <paramref name="kind"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue> Show(TorizoProjectileFrameKind kind, int phase, int duration) =>
        new AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>.Frame(new(kind, phase), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>.Event(cue);
}
