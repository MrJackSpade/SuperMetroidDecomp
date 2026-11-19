namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Golden Torizo's seven health bands (#2242), each 2048 health wide from none upward, the highest holding the rest: he
/// starts at 13500 health, in <see cref="Unhurt"/>, and each lower band shows him more hurt.
/// </summary>
public static class GoldenTorizoHealthBandDefinitions
{
    /// <summary>Golden Torizo at 12288 health or more, the band he starts in.</summary>
    public static GoldenTorizoHealthBand Unhurt { get; } = new(12288, "unhurt");

    /// <summary>Golden Torizo from 10240 through 12287 health.</summary>
    public static GoldenTorizoHealthBand Grazed { get; } = new(10240, "grazed");

    /// <summary>Golden Torizo from 8192 through 10239 health.</summary>
    public static GoldenTorizoHealthBand Hurt { get; } = new(8192, "hurt");

    /// <summary>Golden Torizo from 6144 through 8191 health.</summary>
    public static GoldenTorizoHealthBand Wounded { get; } = new(6144, "wounded");

    /// <summary>Golden Torizo from 4096 through 6143 health.</summary>
    public static GoldenTorizoHealthBand BadlyWounded { get; } = new(4096, "badly-wounded");

    /// <summary>Golden Torizo from 2048 through 4095 health.</summary>
    public static GoldenTorizoHealthBand GravelyWounded { get; } = new(2048, "gravely-wounded");

    /// <summary>Golden Torizo below 2048 health, his most hurt colours.</summary>
    public static GoldenTorizoHealthBand NearlyDead { get; } = new(0, "nearly-dead");

    /// <summary>The bands, highest first: the order his health is tested against them.</summary>
    public static IReadOnlyList<GoldenTorizoHealthBand> HighestFirst { get; } =
        [Unhurt, Grazed, Hurt, Wounded, BadlyWounded, GravelyWounded, NearlyDead];

    /// <summary>The bands, lowest first: the order his installed health colours hold them in.</summary>
    public static IReadOnlyList<GoldenTorizoHealthBand> LowestFirst { get; } =
        [NearlyDead, GravelyWounded, BadlyWounded, Wounded, Hurt, Grazed, Unhurt];
}
