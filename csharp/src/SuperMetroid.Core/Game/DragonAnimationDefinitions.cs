namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive Dragon animation selectors. Even/odd pairs face left/right; the
/// three pairs represent idle body, cosmetic wings, and attacking body programs.
/// </summary>
public enum DragonAnimationSelector : ushort
{
    /// <summary>Table ordinal zero at $A2:E5EF selects InstList_Dragon_Idle_FacingLeft ($E59B), the left-facing body's one-frame pose followed by sleep.</summary>
    IdleFacingLeft = 0,
    /// <summary>Table ordinal one at $A2:E5F1 selects InstList_Dragon_Idle_FacingRight ($E5AD), the right-facing body's one-frame pose followed by sleep.</summary>
    IdleFacingRight = 1,
    /// <summary>Table ordinal two at $A2:E5F3 selects InstList_Dragon_Wings_FacingLeft ($E5A1), the separate cosmetic wing slot's looping pair of five-update poses.</summary>
    WingsFacingLeft = 2,
    /// <summary>Table ordinal three at $A2:E5F5 selects InstList_Dragon_Wings_FacingRight ($E5B3), the right-facing cosmetic wing loop rather than a body AI phase.</summary>
    WingsFacingRight = 3,
    /// <summary>Table ordinal four at $A2:E5F7 selects InstList_Dragon_Attacking_FacingLeft ($E5BF), the five-pose extension/retraction ending in the $E5FB completion callback and sleep.</summary>
    AttackingFacingLeft = 4,
    /// <summary>Table ordinal five at $A2:E5F9 selects InstList_Dragon_Attacking_FacingRight ($E5D7), the mirrored body attack whose completion lets AI launch a fireball and reinstall the next attack.</summary>
    AttackingFacingRight = 5,

    /// <summary>Native installed-selector sentinel that forces the requested list to reload.</summary>
    ForceReinstall = 0xffff,
}

/// <summary>Compiled fixed animation-program selectors and phase mappings for Dragon.</summary>
internal static class DragonAnimationDefinitions
{
    /// <summary>$A2:E5EF InstListPointers_Dragon: idle, wing and attacking body dispatch by facing.</summary>
    internal static ushort InstructionList(DragonAnimationSelector selector) => selector switch
    {
        DragonAnimationSelector.IdleFacingLeft => DragonInstructionProgramDefinitions.IdleFacingLeft,
        DragonAnimationSelector.IdleFacingRight => DragonInstructionProgramDefinitions.IdleFacingRight,
        DragonAnimationSelector.WingsFacingLeft => DragonInstructionProgramDefinitions.WingsFacingLeft,
        DragonAnimationSelector.WingsFacingRight => DragonInstructionProgramDefinitions.WingsFacingRight,
        DragonAnimationSelector.AttackingFacingLeft => DragonInstructionProgramDefinitions.AttackingFacingLeft,
        DragonAnimationSelector.AttackingFacingRight => DragonInstructionProgramDefinitions.AttackingFacingRight,
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

    /// <summary>Validates a live animation phase before pair arithmetic is applied.</summary>
    /// <param name="selector">Selector value that must identify one of the six installed Dragon programs.</param>
    /// <returns>The selector's zero-based instruction-list table ordinal.</returns>
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
