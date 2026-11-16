namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>When a hit on Spore Spawn speeds up his swing (#2019).</summary>
internal static class SporeSpawnHealthBands
{
    /// <summary>Whether a damaging hit leaving him at <paramref name="health"/> doubles his swing's angle step.</summary>
    internal static bool SwingsFast(int health) => health < SporeSpawnHealthBandDefinitions.FastSwingBelow;
}
