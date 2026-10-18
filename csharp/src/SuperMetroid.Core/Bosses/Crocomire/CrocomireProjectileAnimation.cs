using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire projectile's typed animation.</summary>
internal sealed class CrocomireProjectileAnimation : EnemyProjectileTimelineAnimation<CrocomireProjectileFrame, CrocomireProjectileCue>
{
    /// <summary>Plays <paramref name="kind"/>'s program.</summary>
    internal CrocomireProjectileAnimation(CrocomireProjectileFrameKind kind) => Kind = kind;

    /// <summary>The program played.</summary>
    internal CrocomireProjectileFrameKind Kind { get; }

    /// <inheritdoc/>
    private protected override AnimationTimeline<CrocomireProjectileFrame, CrocomireProjectileCue> Timeline => Kind switch
    {
        CrocomireProjectileFrameKind.Mouth => CrocomireAnimations.Mouth,
        CrocomireProjectileFrameKind.BridgeFragment => CrocomireAnimations.BridgeFragment,
        CrocomireProjectileFrameKind.SpikeWallPiece => CrocomireAnimations.SpikeWallPiece,
        CrocomireProjectileFrameKind.Shot => CrocomireAnimations.Shot,
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireProjectileFrameKind)} {Kind}."),
    };

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CrocomireProjectileFrame frame) =>
        CrocomireProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
