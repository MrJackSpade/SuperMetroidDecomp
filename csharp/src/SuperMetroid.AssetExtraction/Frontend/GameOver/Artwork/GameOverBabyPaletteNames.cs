using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>Names the game-over document's Baby palettes, by which the import reads and writes their colours.</summary>
internal static class GameOverBabyPaletteNames
{
    /// <summary>Each compiled Baby palette phase with its authored color-array identity, in schema order.</summary>
    private static readonly GameOverBabyPaletteName[] Names =
    [
        new(GameOverBabyColouring.Idle, "Baby.Idle"),
        new(GameOverBabyColouring.ClosedCry, "Baby.ClosedCry"),
        new(GameOverBabyColouring.MiddleCry, "Baby.MiddleCry"),
        new(GameOverBabyColouring.OpenCry, "Baby.OpenCry"),
    ];

    /// <summary>Maps a compiled Baby palette phase to its authored color-array identity.</summary>
    private static readonly IReadOnlyDictionary<GameOverBabyColouring, string> NamesByPhase =
        Names.ToDictionary(entry => entry.Phase, entry => entry.Name);

    /// <summary>The Baby palette identities in schema order: idle, closed-cry, middle-cry and open-cry (#2563).</summary>
    internal static IReadOnlyList<string> InSchemaOrder { get; } = [.. Names.Select(entry => entry.Name)];

    /// <summary>Maps a compiled Baby palette phase to its authored color-array identity.</summary>
    /// <param name="palette">Idle, closed-cry, middle-cry, or open-cry palette phase.</param>
    /// <returns>The corresponding <c>Baby.</c> palette dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    internal static string BabyPaletteName(GameOverBabyColouring palette) => Ensure.Listed(NamesByPhase, palette);
}
