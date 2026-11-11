using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn spore's, emitter's or stalk segment's typed animation.</summary>
internal sealed class SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram program)
    : EnemyProjectileTypedAnimation<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue>(SporeSpawnProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Segments => SporeSpawnProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SporeSpawnProjectileProgram, Animation<SporeSpawnProjectileProgram>> StartingAt => SporeSpawnProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SporeSpawnProjectileFrame frame) =>
        SporeSpawnProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
