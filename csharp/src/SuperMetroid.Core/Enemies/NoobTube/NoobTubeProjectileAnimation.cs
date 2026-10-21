using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Enemies.NoobTube.Artwork;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>N00b tube projectile's typed animation.</summary>
internal sealed class NoobTubeProjectileAnimation(NoobTubeProjectileProgram program)
    : EnemyProjectileProgramAnimation<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> Programs => NoobTubeProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(NoobTubeProjectileFrame frame) =>
        NoobTubeProjectileFrameDefinitions.ShownProjectileFrames[
            new(ShowsReflectedPose && frame.ReflectedPose is int reflected ? reflected : frame.Pose)];

    /// <summary>
    /// Whether the last flicker cue chose the reflected pose: a reflecting flicker shows it on even frames and its first
    /// pose on odd ones.
    /// </summary>
    internal bool ShowsReflectedPose { get; set; }
}
