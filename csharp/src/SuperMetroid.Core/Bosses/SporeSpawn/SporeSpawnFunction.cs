namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The five behaviours Spore Spawn's main AI dispatches.
/// Its values are arbitrary; verification's catalog relates each member to its source (#1413).
/// </summary>
public enum SporeSpawnFunction
{
    /// <summary>Performs no main-AI movement while its animations control the idle or open phase.</summary>
    Idle = 1,
    /// <summary>Lowers the body one room pixel per AI update until Y reaches 624 and installs the fight-start list.</summary>
    Descending,
    /// <summary>Computes the phase-driven body sway about its spawn center and updates stalk segment positions.</summary>
    Moving,
    /// <summary>Computes the fixed death-drift angle and velocity toward room position (128,624), without moving the body.</summary>
    SetupDeath,
    /// <summary>Applies death drift and ceiling dust, returning to idle when both axis distances from (128,624) are below eight pixels.</summary>
    Dying,
}
