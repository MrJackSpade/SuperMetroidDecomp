namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Compiled fixed animation-program selectors and phase mappings for Dragon.</summary>
internal static class DragonAnimationDefinitions
{
    /// <summary>$A2:E5EF InstListPointers_Dragon: idle, wing and attacking body dispatch by facing.</summary>
    internal static DragonProgram Program(DragonAnimationSelector selector) => selector switch
    {
        DragonAnimationSelector.IdleFacingLeft => DragonProgram.IdleFacingLeft,
        DragonAnimationSelector.IdleFacingRight => DragonProgram.IdleFacingRight,
        DragonAnimationSelector.WingsFacingLeft => DragonProgram.WingsFacingLeft,
        DragonAnimationSelector.WingsFacingRight => DragonProgram.WingsFacingRight,
        DragonAnimationSelector.AttackingFacingLeft => DragonProgram.AttackingFacingLeft,
        DragonAnimationSelector.AttackingFacingRight => DragonProgram.AttackingFacingRight,
        _ => throw new InvalidDataException($"Dragon animation selector {selector} exceeds its six-entry domain."),
    };

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

    /// <summary>Whether a live selector faces left; rejects the reinstall sentinel and any selector outside the six.</summary>
    private static bool FacesLeft(DragonAnimationSelector selector) => selector switch
    {
        DragonAnimationSelector.IdleFacingLeft or DragonAnimationSelector.WingsFacingLeft or
            DragonAnimationSelector.AttackingFacingLeft => true,
        DragonAnimationSelector.IdleFacingRight or DragonAnimationSelector.WingsFacingRight or
            DragonAnimationSelector.AttackingFacingRight => false,
        _ => throw NotLive(selector),
    };

    /// <summary>The failure for a selector that is not one of Dragon's six live animations.</summary>
    private static InvalidDataException NotLive(DragonAnimationSelector selector) =>
        new($"Dragon animation selector {selector} is not a live phase.");
}
