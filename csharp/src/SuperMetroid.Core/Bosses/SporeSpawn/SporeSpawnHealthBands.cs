namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's health bands (#2019): which health stage his body shows, and when a hit speeds up his swing.</summary>
internal static class SporeSpawnHealthBands
{
    /// <summary>
    /// The health stage his body shows at <paramref name="health"/>, zero through three: one for each of
    /// <see cref="SporeSpawnHealthBandDefinitions.LaterStagesBelow"/>, from the most health down, that the health lies
    /// below.
    /// </summary>
    internal static int Stage(int health) => SporeSpawnHealthBandDefinitions.LaterStagesBelow.Count(threshold => health < threshold);

    /// <summary>Whether a damaging hit leaving him at <paramref name="health"/> doubles his swing's angle step.</summary>
    internal static bool SwingsFast(int health) => health < SporeSpawnHealthBandDefinitions.FastSwingBelow;
}
