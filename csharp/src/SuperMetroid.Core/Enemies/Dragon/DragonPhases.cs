namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon's animation phases: their programs and the same phase or phase pair with another facing.</summary>
internal static class DragonPhases
{
    /// <summary>The program of an animation.</summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> is outside the six.</exception>
    internal static DragonSegment Animation(DragonAnimationSelector selector) =>
        DragonAnimationDefinitions.Animations.TryGetValue(selector, out DragonSegment segment)
            ? segment
            : throw NotLive(selector);

    /// <summary>Preserves the phase pair while selecting its left or right member.</summary>
    internal static DragonAnimationSelector WithFacing(
        DragonAnimationSelector selector,
        bool facingLeft) => selector switch
    {
        DragonAnimationSelector.IdleFacingLeft or DragonAnimationSelector.IdleFacingRight => facingLeft
            ? DragonAnimationSelector.IdleFacingLeft
            : DragonAnimationSelector.IdleFacingRight,
        DragonAnimationSelector.WingsFacingLeft or DragonAnimationSelector.WingsFacingRight => facingLeft
            ? DragonAnimationSelector.WingsFacingLeft
            : DragonAnimationSelector.WingsFacingRight,
        DragonAnimationSelector.AttackingFacingLeft or DragonAnimationSelector.AttackingFacingRight => facingLeft
            ? DragonAnimationSelector.AttackingFacingLeft
            : DragonAnimationSelector.AttackingFacingRight,
        _ => throw NotLive(selector),
    };

    /// <summary>Returns the attacking-body selector with the source selector's facing.</summary>
    internal static DragonAnimationSelector AttackingWithSameFacing(
        DragonAnimationSelector selector) => FacesLeft(selector)
        ? DragonAnimationSelector.AttackingFacingLeft
        : DragonAnimationSelector.AttackingFacingRight;

    /// <summary>Returns the idle-body selector with the source selector's facing.</summary>
    internal static DragonAnimationSelector IdleWithSameFacing(
        DragonAnimationSelector selector) => FacesLeft(selector)
        ? DragonAnimationSelector.IdleFacingLeft
        : DragonAnimationSelector.IdleFacingRight;

    /// <summary>Whether an animation faces left; rejects any selector outside the six.</summary>
    private static bool FacesLeft(DragonAnimationSelector selector) => selector switch
    {
        DragonAnimationSelector.IdleFacingLeft or DragonAnimationSelector.WingsFacingLeft or
            DragonAnimationSelector.AttackingFacingLeft => true,
        DragonAnimationSelector.IdleFacingRight or DragonAnimationSelector.WingsFacingRight or
            DragonAnimationSelector.AttackingFacingRight => false,
        _ => throw NotLive(selector),
    };

    /// <summary>The failure for a selector that is not one of Dragon's six animations.</summary>
    private static InvalidDataException NotLive(DragonAnimationSelector selector) =>
        new($"Dragon animation selector {selector} is not one of its six animations.");
}
