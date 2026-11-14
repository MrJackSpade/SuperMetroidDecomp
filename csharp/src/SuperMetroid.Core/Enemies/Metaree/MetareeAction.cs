namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// The behaviour a Metaree's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum MetareeAction
{
    /// <summary>Activates within 72 horizontal pixels, without checking that Samus is below, and fixes the unsigned downward speed for the attack.</summary>
    Idling,
    /// <summary>Waits for the animation's attack-ready flag, then installs dive art and requests the enemy-library dive sound.</summary>
    PreparingAttack,
    /// <summary>Reloads the 21-update burrow timer, probes solid blocks below, and otherwise dives at fixed whole-pixel Y speed while steering two pixels toward Samus.</summary>
    LaunchedAttack,
    /// <summary>Decrements the timer, emits four metal particles when it reaches eight, sinks one pixel per surviving update, and deletes the actor at zero.</summary>
    Burrowing,
}
