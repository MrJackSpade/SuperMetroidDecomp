namespace SuperMetroid.Core.Enemies.Dragon;

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
