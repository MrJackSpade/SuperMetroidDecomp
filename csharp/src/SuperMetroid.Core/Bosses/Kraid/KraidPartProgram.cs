namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The programs of Kraid's foot, arm, lints and fingernails, declared in the order the cartridge authors them in
/// bank $A7. Kraid's routines depend on that order: they ask whether a part's program lies before a given program,
/// as the native routines compare instruction pointers.
/// </summary>
public enum KraidPartProgram
{
    /// <summary><c>InstList_KraidFoot_Initial</c> (<c>$A7:86E7</c>).</summary>
    FootInitial,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_Neutral</c> (<c>$A7:86ED</c>).</summary>
    FootNeutral,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingForward_0</c> (<c>$A7:86F3</c>).</summary>
    FootWalkingForward,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingForward_1</c> (<c>$A7:87BB</c>).</summary>
    FootWalkingForwardFinished,
    /// <summary><c>InstList_KraidFoot_LungeForward_0</c> (<c>$A7:87BD</c>).</summary>
    FootLungeForward,
    /// <summary><c>InstList_KraidFoot_LungeForward_1</c> (<c>$A7:8885</c>).</summary>
    FootLungeForwardFinished,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingBackwards_0</c> (<c>$A7:8887</c>).</summary>
    FootWalkingBackward,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingBackwards_1</c> (<c>$A7:8939</c>).</summary>
    FootWalkingBackwardLoop,
    /// <summary><c>InstList_KraidArm_Normal_0</c> (<c>$A7:89F3</c>).</summary>
    ArmNormal,
    /// <summary><c>InstList_KraidArm_Normal_1</c> (<c>$A7:8A37</c>).</summary>
    ArmNormalPause,
    /// <summary><c>InstList_KraidArm_Slow</c> (<c>$A7:8A41</c>).</summary>
    ArmSlow,
    /// <summary><c>InstList_KraidArm_RisingSinking</c> (<c>$A7:8AA4</c>).</summary>
    ArmRisingOrSinking,
    /// <summary><c>InstList_KraidArm_Dying_PreparingToLungeForward</c> (<c>$A7:8AF0</c>).</summary>
    ArmDyingOrPreparingToLunge,
    /// <summary><c>kKraid_Ilist_8AFE</c> (<c>$A7:8AFE</c>).</summary>
    LintInitial,
    /// <summary><c>kKraid_Ilist_8B04</c> (<c>$A7:8B04</c>).</summary>
    LintPostGrowth,
    /// <summary><c>InstList_KraidNail</c> (<c>$A7:8B0A</c>).</summary>
    NailLoop,
}
