using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>A typed animation of the Torizos' Chozo orbs (#2554), starting at one of its segments.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class TorizoChozoOrbAnimation(TorizoChozoOrbSegment segment, IEnemyProjectileBranchingCues<TorizoChozoOrbSegment, TorizoFamilyCue<TorizoChozoOrbCueKind>> cues)
    : EnemyProjectileFrameAnimation<TorizoChozoOrbSegment, TorizoFamilyCue<TorizoChozoOrbCueKind>>(TorizoChozoOrbAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TorizoChozoOrbSegment, EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>> Segments => TorizoChozoOrbAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TorizoChozoOrbSegment, Animation<TorizoChozoOrbSegment>> StartingAt => TorizoChozoOrbAnimations.StartingAt;
}
