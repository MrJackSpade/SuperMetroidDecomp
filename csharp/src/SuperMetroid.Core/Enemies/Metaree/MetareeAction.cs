namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// The behaviour a Metaree's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum MetareeAction
{
    /// <summary>Dives once Samus is within 72 pixels either side, whether or not she is below, at a downward speed set as it starts.</summary>
    Idling,
    /// <summary>Waits for the animation to signal it is ready to attack, then starts the dive animation and plays the dive sound.</summary>
    PreparingAttack,
    /// <summary>Reloads the 21-update burrow timer, probes solid blocks below, and otherwise dives at fixed whole-pixel Y speed while steering two pixels toward Samus.</summary>
    LaunchedAttack,
    /// <summary>Decrements the timer, emits four metal particles when it reaches eight, sinks one pixel per surviving update, and deletes the actor at zero.</summary>
    Burrowing,
}
