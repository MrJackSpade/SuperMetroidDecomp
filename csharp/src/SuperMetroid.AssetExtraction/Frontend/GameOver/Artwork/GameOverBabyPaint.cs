using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>
/// The game-over Baby's four installed sixteen-colour phases, kept as installed: the animation shows one phase's colours
/// with one of the three fixed Baby images, and the import pre-colours the Baby's shades in them (#1512).
/// </summary>
internal sealed class GameOverBabyPaint
{
    /// <summary>The colours the Baby shows in each phase.</summary>
    private readonly Dictionary<GameOverBabyColouring, RgbColor[]> palettes;

    /// <summary>Retains the four installed phases the presentation loader validated.</summary>
    /// <exception cref="InvalidDataException">A phase is missing or does not hold sixteen colours.</exception>
    internal GameOverBabyPaint(IReadOnlyDictionary<GameOverBabyColouring, RgbColor[]> palettes)
    {
        this.palettes = [];
        foreach (GameOverBabyColouring palette in Enum.GetValues<GameOverBabyColouring>())
        {
            if (!palettes.TryGetValue(palette, out RgbColor[]? colours) || colours is not { Length: 16 })
                throw new InvalidDataException($"The game-over Baby's {palette} colours require sixteen colours.");
            this.palettes.Add(palette, [.. colours]);
        }
    }

    /// <summary>The sixteen colours the Baby shows in <paramref name="palette"/>, which the import pre-colours its shade in (#1512).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    internal RgbColor[] Colours(GameOverBabyColouring palette)
    {
        var colours = new RgbColor[16];
        for (int colour = 0; colour < colours.Length; colour++)
            colours[colour] = Read(palette, colour);
        return colours;
    }

    /// <summary>The installed colour <paramref name="color"/> of phase <paramref name="palette"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase or colour is outside the Baby's colours.</exception>
    internal RgbColor Read(GameOverBabyColouring palette, int color)
    {
        if (!palettes.TryGetValue(palette, out RgbColor[]? colours)) throw new ArgumentOutOfRangeException(nameof(palette));
        if ((uint)color >= 16) throw new ArgumentOutOfRangeException(nameof(color));
        return colours[color];
    }
}
