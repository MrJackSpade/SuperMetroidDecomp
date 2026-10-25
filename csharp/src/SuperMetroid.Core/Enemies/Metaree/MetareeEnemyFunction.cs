namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// The routine a Metaree's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum MetareeEnemyFunction : ushort
{
    /// <summary>$A3:8987, Function_Metaree_Idling: activates within 72 horizontal pixels, without checking that Samus is below, and fixes the unsigned downward speed for the attack.</summary>
    Idling = 1,
    /// <summary>$A3:89D4, Function_Metaree_PrepareToLaunchAttack: waits for the animation's attack-ready flag, then installs dive art and requests library-2 sound $5B.</summary>
    PreparingAttack,
    /// <summary>$A3:89F3, Function_Metaree_LaunchedAttack: reloads the 21-update burrow timer, probes solid blocks below, and otherwise dives at fixed whole-pixel Y speed while steering two pixels toward Samus.</summary>
    LaunchedAttack,
    /// <summary>$A3:8A5C, Function_Metaree_Burrowing: decrements the timer, emits four metal particles when it reaches eight, sinks one pixel per surviving update, and deletes the actor at zero.</summary>
    Burrowing,
}
