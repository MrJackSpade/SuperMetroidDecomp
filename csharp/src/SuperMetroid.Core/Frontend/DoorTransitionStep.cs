namespace SuperMetroid.Core.Frontend;

/// <summary>Which step of going through a door (<see cref="Progression.SuperMetroidGameState.DoorTransition"/>) runs.</summary>
internal enum DoorTransitionStep
{
    /// <summary>
    /// Samus has reached the door. An ordinary door or an upward elevator begins the transition in the same update; a
    /// downward elevator first waits out its delay.
    /// </summary>
    EnteringDoor,
    /// <summary>
    /// The transition begins: the source room's enemies pause and movement sounds are cancelled, then
    /// <see cref="Transitioning"/> follows in the same update. Entering the door runs this in its own update, so only a
    /// game restored at this step starts here.
    /// </summary>
    BeginningTransition,
    /// <summary>The multi-update room loading, scrolling and fade until gameplay resumes.</summary>
    Transitioning,
}
