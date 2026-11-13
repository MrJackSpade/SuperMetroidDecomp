namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The programs of Kraid's foot, arm, lints and fingernails, declared in the order the original game authors them.
/// Kraid's routines depend on that order: they ask whether a part's program lies before a given program,
/// as the original routines compare script positions.
/// </summary>
public enum KraidPartSegment
{
    /// <summary>Kraid foot animation script: initial.</summary>
    FootInitial,
    /// <summary>Kraid foot animation script: Kraid is big, neutral.</summary>
    FootNeutral,
    /// <summary>Kraid foot animation script: Kraid is big, walking forward, part 0.</summary>
    FootWalkingForward,
    /// <summary>Kraid foot animation script: Kraid is big, walking forward, part 1.</summary>
    FootWalkingForwardFinished,
    /// <summary>Kraid foot animation script: lunge forward, part 0.</summary>
    FootLungeForward,
    /// <summary>Kraid foot animation script: lunge forward, part 1.</summary>
    FootLungeForwardFinished,
    /// <summary>Kraid foot animation script: Kraid is big, walking backwards, part 0.</summary>
    FootWalkingBackward,
    /// <summary>Kraid foot animation script: Kraid is big, walking backwards, part 1.</summary>
    FootWalkingBackwardLoop,
    /// <summary>Kraid arm animation script: normal, part 0.</summary>
    ArmNormal,
    /// <summary>Kraid arm animation script: normal, part 1.</summary>
    ArmNormalPause,
    /// <summary>Kraid arm animation script: slow.</summary>
    ArmSlow,
    /// <summary>Kraid arm animation script: rising sinking.</summary>
    ArmRisingOrSinking,
    /// <summary>Kraid arm animation script: dying, preparing to lunge forward.</summary>
    ArmDyingOrPreparingToLunge,
    /// <summary>The lint's first program, before Kraid has grown.</summary>
    LintInitial,
    /// <summary>The lint's program after Kraid has grown.</summary>
    LintPostGrowth,
    /// <summary>Kraid nail animation script.</summary>
    NailLoop,
}
