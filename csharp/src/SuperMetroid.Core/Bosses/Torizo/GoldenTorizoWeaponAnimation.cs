using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>A typed animation of Golden Torizo's caught super missile and its eye beam (#2554), starting at one of its segments.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class GoldenTorizoWeaponAnimation(GoldenTorizoWeaponSegment segment, IEnemyProjectileBranchingCues<GoldenTorizoWeaponSegment, TorizoFamilyCue<GoldenTorizoWeaponCueKind>> cues)
    : EnemyProjectileFrameAnimation<GoldenTorizoWeaponSegment, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>(GoldenTorizoWeaponAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<GoldenTorizoWeaponSegment, EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>> Segments => GoldenTorizoWeaponAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<GoldenTorizoWeaponSegment, Animation<GoldenTorizoWeaponSegment>> StartingAt => GoldenTorizoWeaponAnimations.StartingAt;
}
