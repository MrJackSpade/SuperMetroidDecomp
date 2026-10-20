using SuperMetroid.Core.Programs;
using ProjectileStep = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Crocomire.CrocomireProjectileFrameKind, SuperMetroid.Core.Bosses.Crocomire.CrocomireProjectileFrame, SuperMetroid.Core.Bosses.Crocomire.CrocomireProjectileCue>;
using TongueStep = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Crocomire.CrocomireTongueProgram, SuperMetroid.Core.Bosses.Crocomire.CrocomireTongueFrame, SuperMetroid.Core.Programs.NoAnimationCue>;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's typed tongue and projectile animations (#641), in place of their native lists.</summary>
internal static class CrocomireAnimations
{
    /// <summary>Frames in the mouth projectile's looping flight.</summary>
    internal const int MouthPhaseCount = 6;
    /// <summary>Frames in the shot projectile's explosion.</summary>
    internal const int ShotPhaseCount = 5;
    /// <summary>The bridge and spike frames last the native maximum, so their loops never advance in play.</summary>
    private const int StillFrameDuration = 32767;

    /// <summary>The tongue's programs: the fight loop, the hidden tongue's bare sleep, and the melting loop.</summary>
    internal static AnimationProgramSet<CrocomireTongueProgram, CrocomireTongueFrame, NoAnimationCue> TonguePrograms { get; } = new(
        new Dictionary<CrocomireTongueProgram, TongueStep[]>
        {
            [CrocomireTongueProgram.Fight] =
            [
                .. new[] { CrocomireTongueFrame.Fight0, CrocomireTongueFrame.Fight1, CrocomireTongueFrame.Fight2, CrocomireTongueFrame.Fight3 }
                    .Select(frame => TongueStep.Show(frame, 5)),
                TongueStep.GoTo(CrocomireTongueProgram.Fight),
            ],
            [CrocomireTongueProgram.Sleep] = [TongueStep.Hold],
            [CrocomireTongueProgram.Melting] =
            [
                .. new[]
                {
                    CrocomireTongueFrame.MeltingPose0, CrocomireTongueFrame.MeltingPose1, CrocomireTongueFrame.MeltingPose2,
                    CrocomireTongueFrame.MeltingPose3, CrocomireTongueFrame.MeltingPose4,
                }.Select(frame => TongueStep.Show(frame, 5)),
                TongueStep.GoTo(CrocomireTongueProgram.Melting),
            ],
        });

    /// <summary>
    /// The projectile programs, one per projectile kind: <c>InstList_EnemyProjectile_CrocomiresProjectile</c> ($86:8FCF)
    /// loops six three-update frames; the bridge fragment ($86:8FEB) and spike-wall piece ($86:8FF3) loop one still frame;
    /// <c>InstList_EnemyProjectile_Shot_CrocomiresProjectile</c> ($86:9007) shows five four-update explosion frames, then a
    /// drop with Crocomire's chances and deletion.
    /// </summary>
    internal static AnimationProgramSet<CrocomireProjectileFrameKind, CrocomireProjectileFrame, CrocomireProjectileCue> ProjectilePrograms { get; } = new(
        new Dictionary<CrocomireProjectileFrameKind, ProjectileStep[]>
        {
            [CrocomireProjectileFrameKind.Mouth] =
            [
                .. Enumerable.Range(0, MouthPhaseCount)
                    .Select(phase => ProjectileStep.Show(new(CrocomireProjectileFrameKind.Mouth, phase), 3)),
                ProjectileStep.GoTo(CrocomireProjectileFrameKind.Mouth),
            ],
            [CrocomireProjectileFrameKind.BridgeFragment] =
            [
                ProjectileStep.Show(new(CrocomireProjectileFrameKind.BridgeFragment, 0), StillFrameDuration),
                ProjectileStep.GoTo(CrocomireProjectileFrameKind.BridgeFragment),
            ],
            [CrocomireProjectileFrameKind.SpikeWallPiece] =
            [
                ProjectileStep.Show(new(CrocomireProjectileFrameKind.SpikeWallPiece, 0), StillFrameDuration),
                ProjectileStep.GoTo(CrocomireProjectileFrameKind.SpikeWallPiece),
            ],
            [CrocomireProjectileFrameKind.Shot] =
            [
                .. Enumerable.Range(0, ShotPhaseCount)
                    .Select(phase => ProjectileStep.Show(new(CrocomireProjectileFrameKind.Shot, phase), 4)),
                ProjectileStep.Publish(CrocomireProjectileCue.SpawnDrops),
                ProjectileStep.Publish(CrocomireProjectileCue.Delete),
                ProjectileStep.Hold,
            ],
        });
}
