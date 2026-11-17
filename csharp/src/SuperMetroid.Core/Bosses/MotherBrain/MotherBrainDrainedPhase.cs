namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain drained by the Baby Metroid (#2208), from being taken aback to lying grey. Its values are arbitrary.</summary>
public enum MotherBrainDrainedPhase
{
    /// <summary>The body's next turn after baby attachment changes form/neck motion and immediately starts the regain-balance pause.</summary>
    TakenAback,

    /// <summary>Maintains the rainbow colours until the pause runs out, then initializes the independent painful-walking sequence.</summary>
    RegainBalance,

    /// <summary>Refreshes head shake, applies rainbow colours, and advances painful walking until its sixth step stops the beam's sound and the brain's colours.</summary>
    FiringRainbowBeam,

    /// <summary>Continues painful walking through its eighth step, then selects dying drool and the retreating neck posture.</summary>
    RainbowBeamRunOut,

    /// <summary>Requests really-slow backward walking to X 40 before disabling upper-neck motion and entering low-power setup.</summary>
    MoveToBackOfRoom,

    /// <summary>Waits for both neck movement indices to clear, disables drool, and requests fast crouching once standing.</summary>
    GoIntoLowPowerMode,

    /// <summary>Waits out the crouched pause, clears the colour step counter, and immediately begins the grey transition's steps.</summary>
    PrepareTransitionToGrey,

    /// <summary>Steps through eight drained-grey colour steps and one step past the last, restores head health 36,000, and enters the inanimate revival setup.</summary>
    TransitionToGrey,
}
