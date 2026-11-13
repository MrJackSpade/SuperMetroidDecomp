using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon goop's or wall-turret shot's typed animation.</summary>
internal sealed class DraygonProjectileAnimation(DraygonProjectileSegment segment)
    : EnemyProjectileTypedAnimation<DraygonProjectileSegment, DraygonProjectileFrame, DraygonProjectileCue>(DraygonProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DraygonProjectileSegment, DraygonProjectileFrame, DraygonProjectileCue> Segments =>
        DraygonProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DraygonProjectileSegment, Animation<DraygonProjectileSegment>> StartingAt =>
        DraygonProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(DraygonProjectileFrame frame) =>
        DraygonProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
