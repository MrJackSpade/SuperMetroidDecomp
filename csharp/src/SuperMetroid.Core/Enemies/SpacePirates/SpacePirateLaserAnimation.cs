using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class SpacePirateLaserAnimation(SpacePirateLaserSegment segment, IEnemyProjectileCues<SpacePirateLaserCue> cues)
    : EnemyProjectileFrameAnimation<SpacePirateLaserSegment, SpacePirateLaserCue>(SpacePirateLaserAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SpacePirateLaserSegment, EnemyProjectileFrameId, SpacePirateLaserCue> Segments => SpacePirateLaserAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SpacePirateLaserSegment, Animation<SpacePirateLaserSegment>> StartingAt => SpacePirateLaserAnimations.StartingAt;
}
