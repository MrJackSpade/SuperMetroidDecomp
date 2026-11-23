namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase Kraid's body runs while he dies, up to his sinking (<see cref="KraidStageRun.Dying"/>).</summary>
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
    /// <summary>Sinks Kraid through the floor while processing authored debris, sounds, and drops; his sunk body hides next.</summary>
    Sink,
}
