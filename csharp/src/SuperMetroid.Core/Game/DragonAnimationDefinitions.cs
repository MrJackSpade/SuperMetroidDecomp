namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive Dragon animation selectors. Even/odd pairs face left/right; the
/// three pairs represent idle body, cosmetic wings, and attacking body programs.
/// </summary>
public enum DragonAnimationSelector : ushort
{
    IdleFacingLeft = 0,
    IdleFacingRight = 1,
    WingsFacingLeft = 2,
    WingsFacingRight = 3,
    AttackingFacingLeft = 4,
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
