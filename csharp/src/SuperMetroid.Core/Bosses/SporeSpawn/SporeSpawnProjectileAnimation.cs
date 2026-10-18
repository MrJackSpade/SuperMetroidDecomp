using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn spore's, emitter's or stalk segment's typed animation.</summary>
internal sealed class SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram program)
    : EnemyProjectileProgramAnimation<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Programs =>
        SporeSpawnProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SporeSpawnProjectileFrame frame) =>
        SporeSpawnProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
