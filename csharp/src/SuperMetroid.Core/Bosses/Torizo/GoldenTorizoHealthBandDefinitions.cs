namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Golden Torizo's health bands (#2242), each equally wide from none upward, the highest holding the rest: he
/// starts at full health, in <see cref="Unhurt"/>, and each lower band shows him more hurt.
/// </summary>
public static class GoldenTorizoHealthBandDefinitions
{
    /// <summary>Golden Torizo in his highest band, the one he starts in.</summary>
    private static GoldenTorizoHealthBand Unhurt { get; } = new(12288, "unhurt");

    /// <summary>Golden Torizo just below his highest band.</summary>
    public static GoldenTorizoHealthBand Grazed { get; } = new(10240, "grazed");

    /// <summary>Golden Torizo hurt, below <see cref="Grazed"/>.</summary>
    public static GoldenTorizoHealthBand Hurt { get; } = new(8192, "hurt");

    /// <summary>Golden Torizo wounded, below <see cref="Hurt"/>.</summary>
    public static GoldenTorizoHealthBand Wounded { get; } = new(6144, "wounded");

    /// <summary>Golden Torizo badly wounded, below <see cref="Wounded"/>.</summary>
    public static GoldenTorizoHealthBand BadlyWounded { get; } = new(4096, "badly-wounded");

    /// <summary>Golden Torizo gravely wounded, below <see cref="BadlyWounded"/>.</summary>
    private static GoldenTorizoHealthBand GravelyWounded { get; } = new(2048, "gravely-wounded");

    /// <summary>Golden Torizo in his lowest band, his most hurt colours.</summary>
    public static GoldenTorizoHealthBand NearlyDead { get; } = new(0, "nearly-dead");

    /// <summary>The bands, highest first: the order his health is tested against them.</summary>
    public static IReadOnlyList<GoldenTorizoHealthBand> HighestFirst { get; } =
        [Unhurt, Grazed, Hurt, Wounded, BadlyWounded, GravelyWounded, NearlyDead];

    /// <summary>The bands, lowest first: the order his installed health colours hold them in.</summary>
    public static IReadOnlyList<GoldenTorizoHealthBand> LowestFirst { get; } =
        [NearlyDead, GravelyWounded, BadlyWounded, Wounded, Hurt, Grazed, Unhurt];
}
