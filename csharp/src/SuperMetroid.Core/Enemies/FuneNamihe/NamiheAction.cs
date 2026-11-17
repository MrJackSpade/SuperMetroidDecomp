namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// What a Namihe is doing: waiting for Samus to spit, or spitting.
/// Its values are arbitrary.
/// </summary>
public enum NamiheAction
{
    /// <summary>Waits for Samus to come vertically closer than its proximity (<see cref="CoordinateAxis.Distance"/>), then starts the spitting animation and <see cref="Spitting"/>.</summary>
    WaitForSamus,
    /// <summary>Spits: the mouth animation's cues move the mouth, fire and sound, and its finishing cue restores the wait (#1814).</summary>
    Spitting,
}
