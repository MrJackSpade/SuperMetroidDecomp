using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn spore's, emitter's or stalk segment's typed animation.</summary>
internal sealed class SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment segment)
    : EnemyProjectileTypedAnimation<SporeSpawnProjectileSegment, SporeSpawnProjectileFrame, SporeSpawnProjectileCue>(SporeSpawnProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SporeSpawnProjectileSegment, SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Segments => SporeSpawnProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt => SporeSpawnProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SporeSpawnProjectileFrame frame) =>
        SporeSpawnProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
