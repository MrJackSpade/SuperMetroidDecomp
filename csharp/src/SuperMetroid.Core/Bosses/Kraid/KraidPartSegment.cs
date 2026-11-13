namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The animations of Kraid's foot, arm, lints and fingernails, declared in the order the game authors them.
/// Kraid's behaviour depends on that order: it asks whether a part's animation lies before a given animation,
/// as the game compares animation positions.
/// </summary>
public enum KraidPartSegment
{
    /// <summary>Kraid foot animation: initial.</summary>
    FootInitial,
    /// <summary>Kraid foot animation: Kraid is big, neutral.</summary>
    FootNeutral,
    /// <summary>Kraid foot animation: Kraid is big, walking forward, part 0.</summary>
    FootWalkingForward,
    /// <summary>Kraid foot animation: Kraid is big, walking forward, part 1.</summary>
    FootWalkingForwardFinished,
    /// <summary>Kraid foot animation: lunge forward, part 0.</summary>
    FootLungeForward,
    /// <summary>Kraid foot animation: lunge forward, part 1.</summary>
    FootLungeForwardFinished,
    /// <summary>Kraid foot animation: Kraid is big, walking backwards, part 0.</summary>
    FootWalkingBackward,
    /// <summary>Kraid foot animation: Kraid is big, walking backwards, part 1.</summary>
    FootWalkingBackwardLoop,
    /// <summary>Kraid arm animation: normal, part 0.</summary>
    ArmNormal,
    /// <summary>Kraid arm animation: normal, part 1.</summary>
    ArmNormalPause,
    /// <summary>Kraid arm animation: slow.</summary>
    ArmSlow,
    /// <summary>Kraid arm animation: rising sinking.</summary>
    ArmRisingOrSinking,
    /// <summary>Kraid arm animation: dying, preparing to lunge forward.</summary>
    ArmDyingOrPreparingToLunge,
    /// <summary>The lint's first animation, before Kraid has grown.</summary>
    LintInitial,
    /// <summary>The lint's animation after Kraid has grown.</summary>
    LintPostGrowth,
    /// <summary>Kraid nail animation.</summary>
    NailLoop,
}
