using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Samus's full-body cycle colours by cycle palette (#1464): for each family's shade and suit, the colours her suit shows.
/// The import pipeline decodes them from the installed document, whose row order is its own knowledge; the cycles that
/// choose a palette are Samus's.
/// </summary>
public sealed class SamusFullBodyCycleColours
{
    /// <summary>Each cycle palette's suit colours.</summary>
    private readonly Dictionary<SamusFullBodyCyclePalette, ImmutableArray<RgbColor>> palettes;

    /// <summary>Keeps the decoded palettes.</summary>
    /// <param name="palettes">Each cycle palette's suit colours, as many as Samus's suit holds.</param>
    /// <exception cref="ArgumentException">A palette does not hold a whole suit's colours.</exception>
    internal SamusFullBodyCycleColours(IReadOnlyDictionary<SamusFullBodyCyclePalette, ImmutableArray<RgbColor>> palettes)
    {
        Ensure.NotNull(palettes);
        foreach (ImmutableArray<RgbColor> colours in palettes.Values)
            Ensure.LengthEqual(colours.AsSpan(), SamusColourDefinitions.RowColours);
        this.palettes = new(palettes);
    }

    /// <summary>One colour of Samus's suit under a cycle palette.</summary>
    /// <param name="palette">The cycle palette.</param>
    /// <param name="colorIndex">The colour within Samus's suit.</param>
    /// <exception cref="ArgumentOutOfRangeException">The palette is not catalogued, or the colour is not one of the suit's.</exception>
    public RgbColor Resolve(SamusFullBodyCyclePalette palette, int colorIndex)
    {
        ImmutableArray<RgbColor> colours = Palette(palette);
        if ((uint)colorIndex >= (uint)colours.Length) throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return colours[colorIndex];
    }

    /// <summary>Shows the cycle palette's colours as Samus's suit colours.</summary>
    /// <param name="colours">Samus's colours, whose suit colours change.</param>
    /// <param name="palette">The cycle palette.</param>
    /// <exception cref="ArgumentOutOfRangeException">The palette is not catalogued.</exception>
    public void Apply(SamusColours colours, SamusFullBodyCyclePalette palette)
    {
        Ensure.NotNull(colours);
        for (int colour = 0; colour < SamusColourDefinitions.RowColours; colour++)
            colours.Suit.Write(colour, Resolve(palette, colour));
    }

    /// <summary>The suit colours of <paramref name="palette"/>.</summary>
    private ImmutableArray<RgbColor> Palette(SamusFullBodyCyclePalette palette) =>
        palettes.TryGetValue(palette, out ImmutableArray<RgbColor> colours)
            ? colours
            : throw new ArgumentOutOfRangeException(nameof(palette), $"Uncatalogued full-body palette {palette}.");
}
