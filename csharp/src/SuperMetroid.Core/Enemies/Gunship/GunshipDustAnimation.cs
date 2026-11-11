using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>A gunship liftoff dust cloud's typed animation.</summary>
internal sealed class GunshipDustAnimation(GunshipDustProgram program)
    : EnemyProjectileTypedAnimation<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>(GunshipDustAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> Segments => GunshipDustAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<GunshipDustProgram, Animation<GunshipDustProgram>> StartingAt => GunshipDustAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(GunshipDustFrame frame) =>
        GunshipDustVisualDefinitions.ShownProjectileFrames[frame];
}
