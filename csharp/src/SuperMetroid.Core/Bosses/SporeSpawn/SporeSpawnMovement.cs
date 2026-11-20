namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The five behaviours Spore Spawn runs each update.
/// Its values are arbitrary.
/// </summary>
public enum SporeSpawnMovement
{
    /// <summary>Performs no main-AI movement while its animations control the idle or open phase.</summary>
    Idle,
    /// <summary>
    /// Lowers the body by <see cref="SporeSpawnArenaDefinitions.DescentSpeed"/> each AI update until it reaches the arena's centre height
    /// (<see cref="SporeSpawnArenaDefinitions.CentreY"/>) and starts the fight-start animation.
    /// </summary>
    Descending,
    /// <summary>Computes the phase-driven body sway about its spawn center and updates stalk segment positions.</summary>
    Moving,
    /// <summary>Computes the fixed death-drift angle and velocity toward the arena's centre, without moving the body.</summary>
    SetupDeath,
    /// <summary>
    /// Applies death drift and ceiling dust, returning to idle once the drift has brought him close to the arena's centre on
    /// both axes (<see cref="SporeSpawnArenaDefinitions.DeathDriftSettleDistance"/>).
    /// </summary>
    Dying,
}
