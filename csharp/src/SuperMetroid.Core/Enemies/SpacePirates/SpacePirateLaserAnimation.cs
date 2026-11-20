using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class SpacePirateLaserAnimation(SpacePirateProjectileSegment segment, IEnemyProjectileCues<SpacePirateLaserCue> cues)
    : EnemyProjectileFrameAnimation<SpacePirateProjectileSegment, SpacePirateLaserCue>(SpacePirateLaserAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SpacePirateProjectileSegment, EnemyProjectileFrameId, SpacePirateLaserCue> Segments => SpacePirateLaserAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>> StartingAt => SpacePirateLaserAnimations.StartingAt;
}
