using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Downward gate's typed animation.</summary>
internal sealed class DownwardGateProjectileAnimation(DownwardGateProjectileProgram program)
    : EnemyProjectileTypedAnimation<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue>(DownwardGateProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DownwardGateProjectileProgram, DownwardGateProjectileFrame, DownwardGateProjectileCue> Segments => DownwardGateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DownwardGateProjectileProgram, Animation<DownwardGateProjectileProgram>> StartingAt => DownwardGateProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(DownwardGateProjectileFrame frame) =>
        DownwardGateProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
