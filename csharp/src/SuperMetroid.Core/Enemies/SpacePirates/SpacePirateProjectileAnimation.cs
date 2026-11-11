using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's or Ninja Pirate claw's typed animation.</summary>
internal sealed class SpacePirateProjectileAnimation(SpacePirateProjectileProgram program)
    : EnemyProjectileTypedAnimation<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue>(SpacePirateProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue> Segments => SpacePirateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SpacePirateProjectileProgram, Animation<SpacePirateProjectileProgram>> StartingAt => SpacePirateProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SpacePirateProjectileFrame frame) =>
        SpacePirateProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
