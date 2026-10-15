using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>N00b tube projectile's typed animation.</summary>
internal sealed class NoobTubeProjectileAnimation(NoobTubeProjectileProgram program)
    : EnemyProjectileProgramAnimation<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> Programs => NoobTubeProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(NoobTubeProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(NoobTubeProjectileFrameDefinitions.PresentationKey(
            ShowsReflectedPose && frame.ReflectedPose is int reflected ? new(reflected) : frame));

    /// <summary>
    /// Whether the last flicker cue chose the reflected pose: a reflecting flicker shows it on even frames and its first
    /// pose on odd ones.
    /// </summary>
    internal bool ShowsReflectedPose { get; set; }
}
