using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>Names the game-over document's Baby palettes, by which the import reads and writes their colours.</summary>
internal static class GameOverBabyPaletteNames
{
    /// <summary>Maps a compiled Baby palette phase to its authored color-array identity.</summary>
    private static readonly IReadOnlyDictionary<GameOverBabyColouring, string> NamesByPhase =
        new Dictionary<GameOverBabyColouring, string>
        {
            [GameOverBabyColouring.Idle] = "Baby.Idle",
            [GameOverBabyColouring.ClosedCry] = "Baby.ClosedCry",
            [GameOverBabyColouring.MiddleCry] = "Baby.MiddleCry",
            [GameOverBabyColouring.OpenCry] = "Baby.OpenCry",
        };

    /// <summary>Maps a compiled Baby palette phase to its authored color-array identity.</summary>
    /// <param name="palette">Idle, closed-cry, middle-cry, or open-cry palette phase.</param>
    /// <returns>The corresponding <c>Baby.</c> palette dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    internal static string BabyPaletteName(GameOverBabyColouring palette) => Ensure.Listed(NamesByPhase, palette);
}
