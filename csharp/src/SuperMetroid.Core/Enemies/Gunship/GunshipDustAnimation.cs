using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>A gunship liftoff dust cloud's typed animation.</summary>
internal sealed class GunshipDustAnimation(GunshipDustProgram program)
    : EnemyProjectileProgramAnimation<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> Programs =>
        GunshipDustAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(GunshipDustFrame frame) =>
        GunshipDustVisualDefinitions.ShownProjectileFrames[frame];
}
