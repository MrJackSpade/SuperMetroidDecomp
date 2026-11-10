using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed Samus full-body cycle colours, every palette's colours kept as installed. The catalog never chooses
/// a suit, cycle phase, or animation delay.
/// </summary>
public sealed class InstalledSamusFullBodyCycleColors
{
    /// <summary>Every installed cycle palette's colours, in <see cref="SamusFullBodyCycleColorFormat.PaletteIndex"/> order.</summary>
    private readonly RgbColor[][] palettes;

    /// <summary>Keeps copies of the complete installed cycle palettes.</summary>
    /// <param name="palettes">All <see cref="SamusFullBodyCycleColorFormat.PaletteCount"/> palettes of <see cref="SamusFullBodyCycleColorFormat.ColorsPerPalette"/> colours.</param>
    /// <exception cref="InvalidDataException">A palette or a colour is missing.</exception>
    internal InstalledSamusFullBodyCycleColors(RgbColor[][] palettes)
    {
        if (palettes.Length != SamusFullBodyCycleColorFormat.PaletteCount)
            throw new InvalidDataException($"Samus full-body cycle colors require {SamusFullBodyCycleColorFormat.PaletteCount} palettes.");
        var copy = new RgbColor[palettes.Length][];
        for (int palette = 0; palette < palettes.Length; palette++)
        {
            if (palettes[palette] is not { Length: SamusFullBodyCycleColorFormat.ColorsPerPalette } colours)
                throw new InvalidDataException($"Samus full-body cycle palette {palette} requires {SamusFullBodyCycleColorFormat.ColorsPerPalette} colors.");
            copy[palette] = (RgbColor[])colours.Clone();
        }
        this.palettes = copy;
    }

    /// <summary>Returns one installed colour of a cycle palette.</summary>
    /// <param name="palette">The cycle palette.</param>
    /// <param name="colorIndex">The colour within the palette, 0 through 15.</param>
    /// <returns>The installed colour.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The palette is not catalogued or the colour is outside it.</exception>
    public RgbColor Resolve(SamusFullBodyCyclePalette palette, int colorIndex)
    {
        RgbColor[] colours = palettes[SamusFullBodyCycleColorFormat.PaletteIndex(palette)];
        if ((uint)colorIndex >= colours.Length)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return colours[colorIndex];
    }

    /// <summary>
    /// The installed colours keyed by what they show (#1464): every cycle palette's suit colours, by family, suit and
    /// shade. The document's row order ends here.
    /// </summary>
    public SamusFullBodyCycleColours ToColours()
    {
        var shown = new Dictionary<SamusFullBodyCyclePalette, ImmutableArray<RgbColor>>();
        foreach (SamusFullBodyCycleFamily family in Enum.GetValues<SamusFullBodyCycleFamily>())
        foreach (SamusSuit suit in SamusSuitDefinitions.ListOrder)
        for (int shade = 0; shade < SamusFullBodyCycleColorFormat.ShadesPerSuit; shade++)
        {
            var palette = new SamusFullBodyCyclePalette(family, suit, shade);
            shown.Add(palette, [.. palettes[SamusFullBodyCycleColorFormat.PaletteIndex(palette)]]);
        }
        return new(shown);
    }
}
