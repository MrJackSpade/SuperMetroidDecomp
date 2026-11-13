using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Downward gate's typed animation.</summary>
internal sealed class DownwardGateProjectileAnimation(DownwardGateProjectileProgram program)
    : EnemyProjectileFrameAnimation<DownwardGateProjectileProgram, DownwardGateProjectileCue>(DownwardGateProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DownwardGateProjectileProgram, EnemyProjectileFrameId, DownwardGateProjectileCue> Segments => DownwardGateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DownwardGateProjectileProgram, Animation<DownwardGateProjectileProgram>> StartingAt => DownwardGateProjectileAnimations.StartingAt;
}
