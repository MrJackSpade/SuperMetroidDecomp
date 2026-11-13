using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's or Ninja Pirate claw's typed animation.</summary>
internal sealed class SpacePirateProjectileAnimation(SpacePirateProjectileProgram program)
    : EnemyProjectileFrameAnimation<SpacePirateProjectileProgram, SpacePirateProjectileCue>(SpacePirateProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SpacePirateProjectileProgram, EnemyProjectileFrameId, SpacePirateProjectileCue> Segments => SpacePirateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SpacePirateProjectileProgram, Animation<SpacePirateProjectileProgram>> StartingAt => SpacePirateProjectileAnimations.StartingAt;
}
