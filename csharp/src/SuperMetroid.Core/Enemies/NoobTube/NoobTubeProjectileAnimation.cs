using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// A n00b tube piece's typed animation: the crack's or a bubble's, which start at their segments, or a glass shard's, which
/// its spawn starts by the shard (<see cref="NoobTubeProjectileAnimations.ShardAnimations"/>, #2044).
/// </summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class NoobTubeProjectileAnimation(IEnemyProjectileCues<NoobTubeProjectileCue> owner, Animation<NoobTubeProjectileSegment> animation)
    : EnemyProjectileTypedAnimation<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue>(animation, EnemyProjectileCueRunner<NoobTubeProjectileSegment, NoobTubeProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue> Segments => NoobTubeProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>> StartingAt => NoobTubeProjectileAnimations.StartingAt;

    /// <summary>The segment <paramref name="started"/> starts at: a shard's flight, or the segment the crack's or a bubble's is keyed by.</summary>
    private protected override NoobTubeProjectileSegment SegmentStartedAt(Animation<NoobTubeProjectileSegment> started) =>
        NoobTubeProjectileAnimations.ShardAnimations.Values.Any(shard => ReferenceEquals(shard, started))
            ? started.Phases[0].Segment
            : base.SegmentStartedAt(started);

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(NoobTubeProjectileFrame frame) =>
        ShowsMirroredFrame && frame.Mirrored is { } mirrored ? mirrored : frame.Shown;

    /// <summary>
    /// Whether the last flicker cue chose the shard's mirror image: a mirroring flicker shows it on even updates and the
    /// shard itself on odd ones.
    /// </summary>
    internal bool ShowsMirroredFrame { get; set; }
}
