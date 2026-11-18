namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain drained by the Baby Metroid (#2208), from being taken aback to lying grey. Its values are arbitrary.</summary>
public enum MotherBrainDrainedPhase
{
    /// <summary>The body's next turn after baby attachment changes form/neck motion and immediately starts the regain-balance pause.</summary>
    TakenAback,

    /// <summary>Maintains the rainbow colours until the pause runs out, then initializes the independent painful-walking sequence.</summary>
    RegainBalance,

    /// <summary>Keeps her brain shaking and her rainbow colours, and staggers her back and forth until she begins the stage her beam runs out in, which stops the beam's sound and shows her drained colours.</summary>
    FiringRainbowBeam,

    /// <summary>Finishes her stagger's last stages, then starts her dying drool and raises her neck to retreat.</summary>
    RainbowBeamRunOut,

    /// <summary>Walks her back really slowly to the back of the room, then stops her upper neck's movement and starts her slump.</summary>
    MoveToBackOfRoom,

    /// <summary>Waits for her neck to finish rising, stops her drool, and once she stands crouches her fast.</summary>
    GoIntoLowPowerMode,

    /// <summary>Waits out the crouched pause, then starts her greying from its first step and begins it at once.</summary>
    PrepareTransitionToGrey,

    /// <summary>Greys her one step each time the wait runs out; the step after the last gives her brain her third phase's health and leaves her a grey corpse awaiting revival.</summary>
    TransitionToGrey,
}
