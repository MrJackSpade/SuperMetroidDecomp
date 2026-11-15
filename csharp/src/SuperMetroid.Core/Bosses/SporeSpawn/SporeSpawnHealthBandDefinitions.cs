namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's health bands (#2019): the health below which a hit speeds up his swing, and the health below which each
/// later health stage his body shows starts.
/// </summary>
internal static class SporeSpawnHealthBandDefinitions
{
    /// <summary>The health below which a damaging hit doubles his swing's angle step, keeping its direction.</summary>
    public const int FastSwingBelow = 400;

    /// <summary>
    /// The health below which each health stage after the first shows, from the first stage hurt to the last: a health of
    /// 770 or more shows the unhurt stage.
    /// </summary>
    public static IReadOnlyList<int> LaterStagesBelow { get; } = [770, 410, 70];
}
