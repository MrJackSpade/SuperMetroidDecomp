using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One shade the import pre-colours a sheet in (#1512): the colours of the sheet's inks under one step of a colour effect,
/// derived from the cartridge's colour data for that effect. Inks it leaves out keep what lies under it.
/// </summary>
/// <param name="Shade">The shade's name, which the game's draws give.</param>
/// <param name="InkColours">
/// The colour each ink it recolours shows under it, by ink in rising order; an ink is its colour group's place times
/// sixteen plus its colour.
/// </param>
internal sealed partial record SheetShade(PictureShade Shade, ImmutableSortedDictionary<int, RgbColor> InkColours)
{
    /// <summary>
    /// Whether a draw showing the shade takes part in colour math over every part: a frontend colouring's drawn shade,
    /// whose sprite set the cartridge ORs into each part's palette, does when that set is one colour math covers.
    /// </summary>
    internal bool TakesColourMath { get; init; }

    /// <summary>
    /// The palette row the draws showing the shade select in place of the family's, whose inks show the shade's colours as
    /// that row draws them (#2643): Crocomire's skeleton-arm colours, which show only on his skeleton, drawn with palette
    /// index zero, so each part in its own row. Null for a shade shown on the family's draws.
    /// </summary>
    internal int? DrawRow { get; init; }

    /// <summary>The inks the shade recolours, in rising order.</summary>
    internal IEnumerable<int> Inks => InkColours.Keys;

    /// <summary>The colours the shade shows, in the order of the inks they show at.</summary>
    internal IEnumerable<RgbColor> Colours => InkColours.Values;

    /// <summary>The shade that shows each of <paramref name="inkColours"/>' colours at its ink.</summary>
    /// <exception cref="ArgumentException">An ink is named twice.</exception>
    internal static SheetShade Of(PictureShade shade, IEnumerable<KeyValuePair<int, RgbColor>> inkColours) =>
        new(shade, ImmutableSortedDictionary.CreateRange(Ensure.NotNull(inkColours)));

    /// <summary>The shade that shows each of <paramref name="inks"/> in the colour <paramref name="colourOf"/> gives it.</summary>
    /// <exception cref="ArgumentException">An ink is named twice.</exception>
    internal static SheetShade Of(PictureShade shade, IEnumerable<int> inks, Func<int, RgbColor> colourOf)
    {
        Ensure.NotNull(inks);
        Ensure.NotNull(colourOf);
        return Of(shade, inks.Select(ink => KeyValuePair.Create(ink, colourOf(ink))));
    }

    /// <summary>The shade named <paramref name="shade"/> showing every ink of <paramref name="parts"/> in its part's colour.</summary>
    /// <exception cref="InvalidDataException">Two parts give one ink different colours.</exception>
    internal static SheetShade Combined(PictureShade shade, IEnumerable<SheetShade> parts)
    {
        Ensure.NotNull(parts);
        var colours = new Dictionary<int, RgbColor>();
        foreach (SheetShade part in parts)
        {
            foreach ((int ink, RgbColor colour) in part.InkColours)
            {
                if (colours.TryGetValue(ink, out RgbColor earlier) && earlier != colour)
                    throw new InvalidDataException($"Shade {shade.Key} gives ink {ink} two colours.");
                colours[ink] = colour;
            }
        }
        return Of(shade, colours);
    }

    /// <summary>
    /// The drawn shade of a colouring held in sprite set <paramref name="set"/>: a run of <paramref name="colours"/> from
    /// <paramref name="firstInk"/>, taking part in colour math when the set is one colour math covers.
    /// </summary>
    internal static SheetShade DrawnRun(PictureShade shade, int firstInk, ReadOnlySpan<RgbColor> colours, int set) =>
        Run(shade, firstInk, colours) with { TakesColourMath = set >= SpriteColourMathDefinitions.FirstSetTakingColourMath };

    /// <summary>
    /// The drawn shade of a colouring held in sprite set <paramref name="set"/> over frames pictured in the sets
    /// <paramref name="pictured"/> holds (#1513): each of those sets' inks shows the colouring's <paramref name="colours"/>,
    /// colour for colour, so a frame pictured in another colouring's set shows in this one's; it takes part in colour math
    /// when the colouring's set is one colour math covers.
    /// </summary>
    internal static SheetShade DrawnOver(PictureShade shade, int set, SortedSet<int> pictured, ReadOnlySpan<RgbColor> colours)
    {
        Ensure.NotNull(pictured);
        var runs = new List<SheetShade>(pictured.Count);
        foreach (int frameSet in pictured)
            runs.Add(Run(shade, frameSet * colours.Length, colours));
        return Combined(shade, runs) with { TakesColourMath = set >= SpriteColourMathDefinitions.FirstSetTakingColourMath };
    }

    /// <summary>
    /// The shade that recolours <paramref name="colours"/>.Length inks from <paramref name="firstInk"/> on, as a run of a
    /// colour group's colours: each colour of the run at the ink as far past the first as the colour is into the run.
    /// </summary>
    internal static SheetShade Run(PictureShade shade, int firstInk, ReadOnlySpan<RgbColor> colours)
    {
        var builder = ImmutableSortedDictionary.CreateBuilder<int, RgbColor>();
        int ink = firstInk;
        foreach (RgbColor colour in colours)
            builder.Add(ink++, colour);
        return new(shade, builder.ToImmutable());
    }

    /// <summary>The manifest entries of <paramref name="shades"/>, or null for none.</summary>
    /// <exception cref="InvalidDataException">Two shades share a name.</exception>
    internal static Dictionary<string, SpriteSheetShadeDocument>? Documents(IReadOnlyList<SheetShade>? shades)
    {
        if (shades is null || shades.Count == 0)
            return null;
        var documents = new Dictionary<string, SpriteSheetShadeDocument>(StringComparer.Ordinal);
        foreach (SheetShade shade in shades)
        {
            if (!documents.TryAdd(shade.Shade.Key, new SpriteSheetShadeDocument
                {
                    InkColours = new SortedDictionary<int, RgbColor>(shade.InkColours),
                    ColourMath = shade.TakesColourMath,
                }))
            {
                throw new InvalidDataException($"A sheet names shade {shade.Shade.Key} twice.");
            }
        }
        return documents;
    }
}
