using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn spore's typed animation: its fall, or its burst once shot.</summary>
internal sealed partial class SporeSpawnSporeAnimation
    : EnemyProjectileFrameAnimation<SporeSpawnSporeSegment, SporeSpawnSporeCue>
{
    /// <summary>Creates a <see cref="SporeSpawnSporeAnimation"/>.</summary>
    /// <param name="owner">The spores' cue owner, which carries out the burst's cues (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public SporeSpawnSporeAnimation(IEnemyProjectileCues<SporeSpawnSporeCue> owner, SporeSpawnSporeSegment segment)
        : base(segment, SporeSpawnProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<SporeSpawnSporeSegment, SporeSpawnSporeCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<SporeSpawnSporeSegment, EnemyProjectileFrameId, SporeSpawnSporeCue> Segments => SporeSpawnProjectileAnimations.SporeSegments;
}