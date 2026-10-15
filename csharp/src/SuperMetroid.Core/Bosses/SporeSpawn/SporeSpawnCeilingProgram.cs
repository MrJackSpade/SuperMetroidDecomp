namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's ceiling PLM programs (#1296), in place of their bank-$84 lists.</summary>
internal enum SporeSpawnCeilingProgram
{
    /// <summary>Crumbles the ceiling, then runs into the clear (<c>InstList_PLM_CrumbleSporeSpawnCeiling</c>).</summary>
    Crumble,
    /// <summary>Clears the ceiling (<c>InstList_PLM_ClearSporeSpawnCeiling</c>).</summary>
    Clear,
}
