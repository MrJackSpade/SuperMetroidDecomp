namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's health bands (#2019, #2242): the four his body's colours show, each lower one more hurt, and the health
/// below which a hit speeds up his swing.
/// </summary>
public static class SporeSpawnHealthBandDefinitions
{
    /// <summary>The health below which a damaging hit doubles his swing's angle step, keeping its direction.</summary>
    public const int FastSwingBelow = 400;

    /// <summary>Spore Spawn at 770 health or more, his own colours.</summary>
    private static SporeSpawnHealthBand Unhurt { get; } = new(770, "unhurt");

    /// <summary>Spore Spawn from 410 through 769 health, his first hurt colours.</summary>
    private static SporeSpawnHealthBand Wounded { get; } = new(410, "wounded");

    /// <summary>Spore Spawn from 70 through 409 health, his second hurt colours.</summary>
    private static SporeSpawnHealthBand BadlyWounded { get; } = new(70, "badly-wounded");

    /// <summary>Spore Spawn below 70 health, his most hurt colours.</summary>
    private static SporeSpawnHealthBand NearlyDead { get; } = new(null, "nearly-dead");

    /// <summary>The bands, highest first: the order his health is tested against them and his installed health colours hold them.</summary>
    public static IReadOnlyList<SporeSpawnHealthBand> HighestFirst { get; } = [Unhurt, Wounded, BadlyWounded, NearlyDead];
}
