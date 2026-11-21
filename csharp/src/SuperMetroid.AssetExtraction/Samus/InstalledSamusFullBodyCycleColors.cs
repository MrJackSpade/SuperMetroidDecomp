using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed Samus full-body cycle colours, every palette's colours kept as installed. The catalog never chooses
/// a suit, cycle phase, or animation delay.
/// </summary>
public sealed partial class InstalledSamusFullBodyCycleColors
{
    /// <summary>Every installed cycle palette's colours, by the shade they show.</summary>
    internal readonly Dictionary<SamusFullBodyCycleShade, RgbColor[]> palettes;

    /// <summary>Keeps copies of the complete installed cycle palettes.</summary>
    /// <param name="palettes">All <see cref="SamusFullBodyCycleColorFormat.PaletteCount"/> palettes of <see cref="SamusFullBodyCycleColorFormat.ColorsPerPalette"/> colours, by shade.</param>
    /// <exception cref="InvalidDataException">A palette or a colour is missing.</exception>
    internal InstalledSamusFullBodyCycleColors(IReadOnlyDictionary<SamusFullBodyCycleShade, RgbColor[]> palettes)
    {
        if (palettes.Count != SamusFullBodyCycleColorFormat.PaletteCount)
            throw new InvalidDataException($"Samus full-body cycle colors require {SamusFullBodyCycleColorFormat.PaletteCount} palettes.");
        var copy = new Dictionary<SamusFullBodyCycleShade, RgbColor[]>();
        foreach ((SamusFullBodyCycleShade palette, RgbColor[] colours) in palettes)
        {
            if (colours is not { Length: SamusFullBodyCycleColorFormat.ColorsPerPalette })
                throw new InvalidDataException($"Samus full-body cycle palette {palette} requires {SamusFullBodyCycleColorFormat.ColorsPerPalette} colors.");
            copy.Add(palette, (RgbColor[])colours.Clone());
        }
        this.palettes = copy;
    }

    /// <summary>
    /// The installed colours keyed by what they show (#1464): every cycle palette's suit colours, by family, suit and
    /// shade. The document's row order ends here.
    /// </summary>
    public SamusFullBodyCycleColours ToColours()
    {
        var shown = new Dictionary<SamusFullBodyCycleShade, ImmutableArray<RgbColor>>();
        foreach (SamusFullBodyCycleFamily family in Enum.GetValues<SamusFullBodyCycleFamily>())
        foreach (SamusSuit suit in SamusSuitListLayout.Order)
        for (int shade = 0; shade < SamusFullBodyCycleColorFormat.ShadesPerSuit; shade++)
        {
            var palette = new SamusFullBodyCycleShade(family, suit, shade);
            shown.Add(palette, [.. palettes.TryGetValue(palette, out RgbColor[]? colours) ? colours
                : throw new InvalidDataException($"Samus full-body cycle palette {palette} is not installed.")]);
        }
        return new(shown);
    }
}
