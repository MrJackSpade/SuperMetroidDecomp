namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's rise out of her fake death (#2208), from the mural behind her tank to her colours' return. Its values are arbitrary.</summary>
public enum MotherBrainAscentPhase
{
    /// <summary>
    /// Draws the next two rows of the mural behind her tank, top to bottom, each update (#2211); once every row is drawn,
    /// hands off to phase-two graphics setup.
    /// </summary>
    DrawMural,

    /// <summary>Shows her phase-two attack and rear-leg colours and shows the prepared enemy background.</summary>
    SetupPhase2Graphics,

    /// <summary>Configures articulated drawing and blending, makes head/body tangible, sets head health 18,000, and immediately starts the suspense wait.</summary>
    SetupPhase2Brain,

    /// <summary>Waits out the suspense, then starts the wait before rising and falls into it.</summary>
    PauseForSuspense,

    /// <summary>Starts the floor-clipping effect, starts the hidden head animation, and starts readying her standing body and the pause that follows it.</summary>
    PrepareForRising,

    /// <summary>Readies her standing body one update at a time; the update that finishes it is the pause's first.</summary>
    ReadyStandingBody,

    /// <summary>Waits out the pause before placing the hidden body/background and enabling its hitboxes.</summary>
    ContinuePausing,

    /// <summary>Reveals the crouched body, starts boss music and earthquake, and enables the rising neck motion.</summary>
    StartMusicAndEarthquake,

    /// <summary>Every fourth frame of the frame counter raises the body two pixels with dust and matching background scroll, then ends the floor-clipping effect and starts uncrouching.</summary>
    RaiseMotherBrain,

    /// <summary>Waits for the body animation to publish the standing pose before initializing grey-colour restoration.</summary>
    WaitUntilUncrouched,

    /// <summary>Restores brain sprite colors one step each time its wait runs out, then enables phase-two colour handling and drool.</summary>
    TransitionFromGray,
}
