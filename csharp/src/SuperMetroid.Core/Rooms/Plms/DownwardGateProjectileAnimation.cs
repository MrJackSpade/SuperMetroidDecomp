using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Downward gate's typed animation.</summary>
internal sealed class DownwardGateProjectileAnimation(DownwardGateProjectileProgram program)
    : EnemyProjectileProgramAnimation<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> Programs => DownwardGateProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(DownwardGateProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(DownwardGateProjectileFrameDefinitions.PresentationKey(frame));
}
