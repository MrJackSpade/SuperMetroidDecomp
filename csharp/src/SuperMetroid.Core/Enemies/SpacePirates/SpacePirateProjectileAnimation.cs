using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's or Ninja Pirate claw's typed animation.</summary>
internal sealed class SpacePirateProjectileAnimation(SpacePirateProjectileProgram program)
    : EnemyProjectileProgramAnimation<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SpacePirateProjectileProgram, SpacePirateProjectileFrame, SpacePirateProjectileCue> Programs =>
        SpacePirateProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SpacePirateProjectileFrame frame) =>
        SpacePirateProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
