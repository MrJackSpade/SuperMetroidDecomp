namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// What a Fune is doing: waiting for its cooldown to spit, or spitting.
/// Its values are arbitrary.
/// </summary>
public enum FuneAction
{
    /// <summary>Counts the cooldown up each update; once it reaches the cooldown time, shows the active mouth animation, starts <see cref="Spitting"/> and restarts the count.</summary>
    WaitForCooldown,
    /// <summary>Spits: the mouth animation's cues move the mouth, fire and sound, and its finishing cue restores the wait (#1814).</summary>
    Spitting,
}
