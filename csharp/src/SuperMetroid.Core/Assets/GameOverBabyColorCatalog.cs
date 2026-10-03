using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Game-over Baby color inputs with calculated cry-phase brightness steps.</summary>
/// <remarks>
/// Native $82:BD97..BE16 contains four sixteen-color phases. Middle/Open cry brighten
/// the preceding cry phase by three independently saturated RGB5 steps. Color zero
/// instead aliases Idle color zero. Import stores only differing target channels,
/// so independently edited source and target colors remain exact. Stock input consists
/// of 31 base words and four differing components, whose separate review remains open;
/// this conversion does not assert that those inputs qualify for retention.
/// </remarks>
internal sealed class GameOverBabyColorCatalog
{
    private readonly Dictionary<int, ushort> inputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> differences = new();

    /// <summary>Captures values from the four named palettes already validated by the presentation loader.</summary>
    internal GameOverBabyColorCatalog(IReadOnlyDictionary<string, ushort[]> palettes)
    {
        for (int phase = 0; phase < 4; phase++)
        for (int color = 0; color < 16; color++)
        {
            ushort supplied = palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)phase)][color];
            int key = phase * 16 + color;
            if (phase == 0 || (phase == 1 && color != 0))
            {
                inputs.Add(key, supplied);
                continue;
            }
            ushort expected = color == 0
                ? palettes[GameOverPresentationDefinitions.BabyPaletteName(GameOverBabyPalette.Idle)][0]
                : BrightenCry(palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)(phase - 1))][color]);
            if (supplied != expected)
                differences.Add(key, new(supplied, expected));
        }
    }

    internal ushort Read(GameOverBabyPalette palette, int color)
    {
        _ = GameOverPresentationDefinitions.BabyPaletteName(palette);
        if ((uint)color >= 16) throw new IndexOutOfRangeException();
        int phase = (int)palette;
        int key = phase * 16 + color;
        if (inputs.TryGetValue(key, out ushort value)) return value;
        ushort expected = color == 0 ? inputs[0] : BrightenCry(Read((GameOverBabyPalette)(phase - 1), color));
        return differences.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }

    /// <summary>RGB5 additive brightness, +3 per component, saturating each component at 31 before packing.</summary>
    internal static ushort BrightenCry(ushort color)
    {
        if (color > 0x7fff) throw new ArgumentOutOfRangeException(nameof(color));
        return (ushort)(Math.Min(31, (color & 31) + 3) |
            Math.Min(31, (color >> 5 & 31) + 3) << 5 |
            Math.Min(31, (color >> 10 & 31) + 3) << 10);
    }
}
