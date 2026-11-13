using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's or Ninja Pirate claw's typed animation.</summary>
internal sealed class SpacePirateProjectileAnimation(SpacePirateProjectileSegment program)
    : EnemyProjectileFrameAnimation<SpacePirateProjectileSegment, SpacePirateProjectileCue>(SpacePirateProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SpacePirateProjectileSegment, EnemyProjectileFrameId, SpacePirateProjectileCue> Segments => SpacePirateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>> StartingAt => SpacePirateProjectileAnimations.StartingAt;
}
