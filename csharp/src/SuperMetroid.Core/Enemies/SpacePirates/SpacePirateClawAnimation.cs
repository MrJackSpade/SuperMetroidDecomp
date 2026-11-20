using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Ninja Pirate claw's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class SpacePirateClawAnimation(SpacePirateClawSegment segment, IEnemyProjectileCues<SpacePirateClawCue> cues)
    : EnemyProjectileFrameAnimation<SpacePirateClawSegment, SpacePirateClawCue>(SpacePirateClawAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SpacePirateClawSegment, EnemyProjectileFrameId, SpacePirateClawCue> Segments => SpacePirateClawAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SpacePirateClawSegment, Animation<SpacePirateClawSegment>> StartingAt => SpacePirateClawAnimations.StartingAt;
}
