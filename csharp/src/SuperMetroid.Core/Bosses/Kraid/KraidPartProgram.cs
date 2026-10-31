namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The programs of Kraid's foot, arm, lints and fingernails, declared in the order the cartridge authors them in
/// bank $A7. Kraid's routines depend on that order: they ask whether a part's program lies before a given program,
/// as the native routines compare instruction pointers.
/// </summary>
public enum KraidPartProgram
{
    /// <summary><c>InstList_KraidFoot_Initial</c>.</summary>
    FootInitial,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_Neutral</c>.</summary>
    FootNeutral,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingForward_0</c>.</summary>
    FootWalkingForward,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingForward_1</c>.</summary>
    FootWalkingForwardFinished,
    /// <summary><c>InstList_KraidFoot_LungeForward_0</c>.</summary>
    FootLungeForward,
    /// <summary><c>InstList_KraidFoot_LungeForward_1</c>.</summary>
    FootLungeForwardFinished,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingBackwards_0</c>.</summary>
    FootWalkingBackward,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingBackwards_1</c>.</summary>
    FootWalkingBackwardLoop,
    /// <summary><c>InstList_KraidArm_Normal_0</c>.</summary>
    ArmNormal,
    /// <summary><c>InstList_KraidArm_Normal_1</c>.</summary>
    ArmNormalPause,
    /// <summary><c>InstList_KraidArm_Slow</c>.</summary>
    ArmSlow,
    /// <summary><c>InstList_KraidArm_RisingSinking</c>.</summary>
    ArmRisingOrSinking,
    /// <summary><c>InstList_KraidArm_Dying_PreparingToLungeForward</c>.</summary>
    ArmDyingOrPreparingToLunge,
    /// <summary><c>kKraid_Ilist_8AFE</c>.</summary>
    LintInitial,
    /// <summary><c>kKraid_Ilist_8B04</c>.</summary>
    LintPostGrowth,
    /// <summary><c>InstList_KraidNail</c>.</summary>
    NailLoop,
}
