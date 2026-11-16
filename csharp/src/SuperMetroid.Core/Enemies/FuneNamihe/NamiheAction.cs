namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// What a Namihe is doing: waiting for Samus to spit, or spitting.
/// Its values are arbitrary.
/// </summary>
public enum NamiheAction
{
    /// <summary>Selects the active mouth animation only when Samus's modular vertical distance is strictly below its whole-pixel vertical proximity, then starts <see cref="Spitting"/>.</summary>
    WaitForSamus,
    /// <summary>Spits: the mouth animation's cues move the mouth, fire and sound, and its finishing cue restores the wait (#1814).</summary>
    Spitting,
}
