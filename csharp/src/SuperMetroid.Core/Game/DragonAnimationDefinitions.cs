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
    /// <summary>
    /// Idle-left/right, wings-left/right, and attack-left/right lists at
    /// <c>$A2:E5EF-$A2:E5FA</c>.
    /// </summary>
    private static readonly ushort[] InstructionLists =
    [
        0xe59b,
        0xe5ad,
        0xe5a1,
        0xe5b3,
        0xe5bf,
        0xe5d7,
    ];

    /// <summary>Returns the authored instruction list for a live selector.</summary>
    internal static ushort InstructionList(DragonAnimationSelector selector)
    {
        int index = (int)selector;
        if ((uint)index >= InstructionLists.Length)
        {
            throw new InvalidDataException(
                $"Dragon animation selector ${index:X4} exceeds its six-entry table.");
        }

        return InstructionLists[index];
    }

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
        if ((uint)index >= InstructionLists.Length)
        {
            throw new InvalidDataException(
                $"Dragon animation selector ${index:X4} is not a live phase.");
        }

        return index;
    }
}
