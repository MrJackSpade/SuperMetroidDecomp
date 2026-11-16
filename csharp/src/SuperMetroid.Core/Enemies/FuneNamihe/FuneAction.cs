namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// What a Fune is doing: waiting for its cooldown to spit, or spitting.
/// Its values are arbitrary.
/// </summary>
public enum FuneAction
{
    /// <summary>Increments the wrapping cooldown before a signed comparison with its cooldown time; on expiry it clears the counter, selects the active mouth animation, and starts <see cref="Spitting"/>.</summary>
    WaitForCooldown,
    /// <summary>Spits: the mouth animation's cues move the mouth, fire and sound, and its finishing cue restores the wait (#1814).</summary>
    Spitting,
}
