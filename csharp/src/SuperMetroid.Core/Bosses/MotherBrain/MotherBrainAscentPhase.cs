namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's rise out of her fake death (#2208), from the mural behind her tank to her colours' return. Its values are arbitrary.</summary>
public enum MotherBrainAscentPhase
{
    /// <summary>
    /// Draws the next two rows of the mural behind her tank, top to bottom, each update (#2211); once every row is drawn,
    /// hands off to phase-two graphics setup.
    /// </summary>
    DrawMural,

    /// <summary>Shows her phase-two attack and rear-leg colours and reveals her body.</summary>
    SetupPhase2Graphics,

    /// <summary>Readies her brain and neck to be drawn, makes her body and brain tangible, gives her brain her second phase's health, and starts the suspense wait at once.</summary>
    SetupPhase2Brain,

    /// <summary>Waits out the suspense, then starts the wait before rising and falls into it.</summary>
    PauseForSuspense,

    /// <summary>Starts the floor-clipping effect, starts the hidden head animation, and starts the wait before her pause.</summary>
    PrepareForRising,

    /// <summary>Waits out the updates before her pause before rising; the update the wait runs out on is the pause's first.</summary>
    WaitBeforePause,

    /// <summary>Waits out the pause, then stands her hidden body in place and enables her body's, brain's and neck's hitboxes.</summary>
    ContinuePausing,

    /// <summary>Reveals the crouched body, starts boss music and earthquake, and enables the rising neck motion.</summary>
    StartMusicAndEarthquake,

    /// <summary>Every fourth frame raises her body two pixels with dust until she has risen, then ends the floor-clipping effect and starts her uncrouching.</summary>
    RaiseMotherBrain,

    /// <summary>Waits for the body animation to publish the standing pose before initializing grey-colour restoration.</summary>
    WaitUntilUncrouched,

    /// <summary>Restores brain sprite colors one step each time its wait runs out, then enables phase-two colour handling and drool.</summary>
    TransitionFromGray,
}
