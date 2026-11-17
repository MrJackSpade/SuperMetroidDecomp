namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The animations of Kraid's foot, arm, lints and fingernails (#1886). Kraid's behaviour starts them by name and never asks
/// where a part's playback stands in one: the animations mark the moments it waits on (the arm reached its pause, the walk
/// finished or reached its loop) as their own cues (#2301).
/// </summary>
public enum KraidPartSegment
{
    /// <summary>The foot's standing pose before Kraid's rise ends.</summary>
    FootInitial,
    /// <summary>The grown foot at rest.</summary>
    FootNeutral,
    /// <summary>The grown foot's forward walk, which ends on its neutral pose.</summary>
    FootWalkingForward,
    /// <summary>The foot's forward lunge, which ends on its neutral pose.</summary>
    FootLungeForward,
    /// <summary>The grown foot's backward walk, which loops.</summary>
    FootWalkingBackward,
    /// <summary>The arm's normal swing, which runs on into its pause.</summary>
    ArmNormal,
    /// <summary>The arm's pause after its normal swing, which slows the arm below half health.</summary>
    ArmNormalPause,
    /// <summary>The arm's slow swing below half health.</summary>
    ArmSlow,
    /// <summary>The arm's swing while Kraid rises or sinks.</summary>
    ArmRisingOrSinking,
    /// <summary>The arm raised while Kraid dies or prepares to lunge.</summary>
    ArmDyingOrPreparingToLunge,
    /// <summary>The lint's first animation, before Kraid has grown.</summary>
    LintInitial,
    /// <summary>The lint's animation after Kraid has grown.</summary>
    LintPostGrowth,
    /// <summary>The fingernail's spin.</summary>
    NailLoop,
}
