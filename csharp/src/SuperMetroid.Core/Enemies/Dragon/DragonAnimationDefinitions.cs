namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Compiled fixed animation selectors and phase mappings for Dragon.</summary>
internal static class DragonAnimationDefinitions
{
    /// <summary>Idle, wing and attacking body animations by facing.</summary>
    internal static readonly Dictionary<DragonAnimationSelector, DragonSegment> Animations = new()
    {
        [DragonAnimationSelector.IdleFacingLeft] = DragonSegment.IdleFacingLeft,
        [DragonAnimationSelector.IdleFacingRight] = DragonSegment.IdleFacingRight,
        [DragonAnimationSelector.WingsFacingLeft] = DragonSegment.WingsFacingLeft,
        [DragonAnimationSelector.WingsFacingRight] = DragonSegment.WingsFacingRight,
        [DragonAnimationSelector.AttackingFacingLeft] = DragonSegment.AttackingFacingLeft,
        [DragonAnimationSelector.AttackingFacingRight] = DragonSegment.AttackingFacingRight,
    };
}
