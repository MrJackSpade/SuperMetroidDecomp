namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase Kraid's body runs while he dies (<see cref="KraidBodyStage.Dying"/>).</summary>
public enum KraidDeathPhase
{
    /// <summary>Initializes Kraid's zero-health sequence and the authored part shutdowns.</summary>
    Initialize,
    /// <summary>Fades the room background out while continuing the head animation.</summary>
    FadeOut,
    /// <summary>Shows his body whole and parks his lints for sinking.</summary>
    ShowBody,
    /// <summary>Starts the death quake and the sinking sound's countdown, makes his body solid and sets his arm sinking.</summary>
    StartSinking,
    /// <summary>Sinks Kraid through the floor while processing authored debris, sounds, and drops.</summary>
    Sink,
    /// <summary>Hides his body once he has sunk, before the room's backdrop is restored.</summary>
    HideBody,
    /// <summary>
    /// Waits the updates his death keeps the room dark before fading it back in (#1888), counted down in the body's phase
    /// timer.
    /// </summary>
    WaitBeforeFadeIn,
    /// <summary>Fades the ordinary background in, requests room music, and publishes boss persistence when newly defeated.</summary>
    FadeInBackground,
    /// <summary>The terminal death entry for a Kraid that was alive when this encounter loaded.</summary>
    FinishedWasAlive,
    /// <summary>The terminal death entry for a Kraid that was already defeated when this encounter loaded.</summary>
    FinishedWasDead,
}
