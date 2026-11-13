using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>A gunship liftoff dust cloud's typed animation.</summary>
internal sealed class GunshipDustAnimation(GunshipDustSegment program)
    : EnemyProjectileTypedAnimation<GunshipDustSegment, GunshipDustFrame, GunshipDustCue>(GunshipDustAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<GunshipDustSegment, GunshipDustFrame, GunshipDustCue> Segments => GunshipDustAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<GunshipDustSegment, Animation<GunshipDustSegment>> StartingAt => GunshipDustAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(GunshipDustFrame frame) =>
        GunshipDustVisualDefinitions.ShownProjectileFrames[frame];
}
