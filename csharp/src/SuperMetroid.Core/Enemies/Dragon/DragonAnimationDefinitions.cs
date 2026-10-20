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
        DragonAnimationSelector.ForceReinstall => throw new InvalidDataException($"Dragon animation selector ${(int)selector:X4} exceeds its six-entry domain."),
        _ => throw new InvalidDataException($"Dragon animation selector ${(int)selector:X4} exceeds its six-entry domain."),
    };
    /// <summary>Preserves the phase pair while selecting its left or right member.</summary>
    internal static DragonAnimationSelector WithFacing(
        DragonAnimationSelector selector,
        bool facingLeft)
    {
        int index = RequireLiveIndex(selector);
        return (DragonAnimationSelector)((index & ~1) | (facingLeft ? 0 : 1));
    }

    /// <summary>Returns the attacking-body selector with the source selector's facing.</summary>
    internal static DragonAnimationSelector AttackingWithSameFacing(
        DragonAnimationSelector selector) =>
        (DragonAnimationSelector)(4 | (RequireLiveIndex(selector) & 1));

    /// <summary>Returns the idle-body selector with the source selector's facing.</summary>
    internal static DragonAnimationSelector IdleWithSameFacing(
        DragonAnimationSelector selector) =>
        (DragonAnimationSelector)(RequireLiveIndex(selector) & 1);

    /// <summary>Rejects the reinstall sentinel and any selector outside Dragon's six live animations.</summary>
    private static int RequireLiveIndex(DragonAnimationSelector selector)
    {
        int index = (int)selector;
        if ((uint)index >= 6)
        {
            throw new InvalidDataException(
                $"Dragon animation selector ${index:X4} is not a live phase.");
        }

        return index;
    }
}
