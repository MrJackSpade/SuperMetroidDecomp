namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// What Fune or Namihe is doing: waiting to spit, or spitting.
/// Its values are arbitrary.
/// </summary>
public enum FuneNamiheAction
{
    /// <summary>Increments the wrapping cooldown before a signed comparison with its cooldown time; on expiry it clears the counter, selects the active mouth animation, and starts <see cref="Spitting"/>.</summary>
    FuneWaitForCooldown = 1,
    /// <summary>Selects the active mouth animation only when Samus's modular vertical distance is strictly below its whole-pixel vertical proximity, then starts <see cref="Spitting"/> without a cooldown.</summary>
    NamiheWaitForSamus,
    /// <summary>Spits: the mouth animation's cues move the mouth, fire and sound, and its finishing cue restores the wait (#1814).</summary>
    Spitting,
}
