using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>N00b tube projectile's typed animation.</summary>
internal sealed class NoobTubeProjectileAnimation(NoobTubeProjectileSegment segment)
    : EnemyProjectileTypedAnimation<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue>(NoobTubeProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue> Segments => NoobTubeProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>> StartingAt => NoobTubeProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(NoobTubeProjectileFrame frame) =>
        ShowsMirroredFrame && frame.Mirrored is { } mirrored ? mirrored : frame.Shown;

    /// <summary>
    /// Whether the last flicker cue chose the shard's mirror image: a mirroring flicker shows it on even updates and the
    /// shard itself on odd ones.
    /// </summary>
    internal bool ShowsMirroredFrame { get; set; }
}
