namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase Kraid's body runs while he dies (<see cref="KraidBodyStage.Dying"/>).</summary>
public enum KraidDeathPhase
{
    /// <summary>Initializes Kraid's zero-health sequence and the authored part shutdowns.</summary>
    Initialize,
    /// <summary>Fades the room background out while continuing the head animation.</summary>
    FadeOut,
    /// <summary>Shows his whole body and moves on to his sinking.</summary>
    ShowBody,
    /// <summary>Starts the death quake and the wait for the sinking body's first rumble, makes his body solid and sets his arm sinking.</summary>
    StartSinking,
    /// <summary>Sinks Kraid through the floor while processing authored debris, sounds, and drops.</summary>
    Sink,
    /// <summary>Hides his body once he has sunk, before the room's backdrop is restored.</summary>
    HideBody,
    /// <summary>
    /// Waits the updates his death keeps the room dark before fading it back in (#1888), on the body's phase wait.
    /// </summary>
    WaitBeforeFadeIn,
    /// <summary>Fades the ordinary background in, requests room music, and records Kraid defeated.</summary>
    FadeInBackground,
    /// <summary>His death has played out: his body is given a dead enemy's properties once the camera leaves the room's left edge.</summary>
    Finished,
}
