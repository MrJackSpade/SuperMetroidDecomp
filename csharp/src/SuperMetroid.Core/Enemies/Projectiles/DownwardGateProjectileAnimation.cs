using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Downward gate's typed animation.</summary>
internal sealed class DownwardGateProjectileAnimation(DownwardGateProjectileProgram program)
    : EnemyProjectileProgramAnimation<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> Programs => DownwardGateProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(DownwardGateProjectileFrame frame) =>
        DownwardGateProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
