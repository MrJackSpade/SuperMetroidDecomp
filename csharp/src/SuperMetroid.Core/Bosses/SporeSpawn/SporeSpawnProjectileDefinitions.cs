namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Compiled spawn geometry and movement mechanics for Spore Spawn projectiles.</summary>
internal static class SporeSpawnProjectileDefinitions
{
    /// <summary>The spores' 256-step signed movement stream, in pixels; a spore reads its X step and the next entry as its Y step, wrapping from the last entry to the first.</summary>
    internal static readonly int[] Movement =
    [
        0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0,
        0,1,1,0,1,0,0,1,1,0,0,1,1,0,0,1,
        1,0,0,1,1,0,1,0,0,1,1,0,0,1,1,0,
        1,0,0,1,1,0,0,1,1,0,1,0,0,1,1,0,
        1,0,0,1,1,0,1,0,0,1,1,0,1,0,1,0,
        0,1,1,0,1,0,1,0,0,1,1,0,1,0,1,0,
        1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,
        1,0,0,-1,1,0,1,0,1,0,1,0,1,0,1,0,
        0,-1,1,0,1,0,0,0,-1,1,-1,1,-1,1,-1,0,
        0,1,-1,1,-1,1,-1,0,0,1,-1,1,-1,0,0,1,
        -1,0,0,1,-1,0,0,1,-1,0,-1,1,-1,1,-1,0,
        0,1,-1,0,-1,1,-1,0,0,1,-1,0,-1,1,-1,0,
        -1,0,0,1,-1,0,-1,0,0,1,-1,0,-1,0,-1,0,
        -1,1,-1,0,-1,0,-1,0,-1,0,-1,0,-1,0,-1,0,
        -1,0,-1,0,0,-1,-1,0,-1,0,-1,0,-1,-1,-1,0,
        -1,0,-1,0,0,-1,-1,0,-1,0,-1,-1,-1,0,0,0,
    ];
}
