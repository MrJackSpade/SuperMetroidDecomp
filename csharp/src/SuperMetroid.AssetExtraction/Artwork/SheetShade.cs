using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One shade the import pre-colours a sheet in (#1512): the colours of the sheet's inks under one step of a colour effect,
/// derived from the cartridge's colour data for that effect. Inks it leaves out keep what lies under it.
/// </summary>
/// <param name="Shade">The shade's name, which the game's draws give.</param>
/// <param name="Inks">The inks it recolours, each once, in rising order; an ink is its colour group's place times sixteen plus its colour.</param>
/// <param name="Colours">The colour each ink shows under it.</param>
internal sealed record SheetShade(PictureShade Shade, int[] Inks, RgbColor[] Colours)
{
    /// <summary>
    /// Whether a draw showing the shade takes part in colour math over every part: a frontend colouring's drawn shade,
    /// whose sprite set the cartridge ORs into each part's palette, does when that set is one colour math covers.
    /// </summary>
    internal bool TakesColourMath { get; init; }

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
        var inks = new List<int>(pictured.Count * colours.Length);
        var shown = new List<RgbColor>(pictured.Count * colours.Length);
        foreach (int frameSet in pictured)
        {
            for (int colour = 0; colour < colours.Length; colour++)
            {
                inks.Add(frameSet * colours.Length + colour);
                shown.Add(colours[colour]);
            }
        }
        return new SheetShade(shade, [.. inks], [.. shown]) { TakesColourMath = set >= SpriteColourMathDefinitions.FirstSetTakingColourMath };
    }

    /// <summary>
    /// The shade that recolours <paramref name="colours"/>.Length inks from <paramref name="firstInk"/> on, as a run of a
    /// colour group's colours.
    /// </summary>
    internal static SheetShade Run(PictureShade shade, int firstInk, ReadOnlySpan<RgbColor> colours) =>
        new(shade, [.. Enumerable.Range(firstInk, colours.Length)], colours.ToArray());

    /// <summary>The manifest entries of <paramref name="shades"/>, or null for none.</summary>
    /// <exception cref="InvalidDataException">Two shades share a name, or a shade's lists differ in length.</exception>
    internal static Dictionary<string, SpriteSheetShadeDocument>? Documents(IReadOnlyList<SheetShade>? shades)
    {
        if (shades is null || shades.Count == 0)
            return null;
        var documents = new Dictionary<string, SpriteSheetShadeDocument>(StringComparer.Ordinal);
        foreach (SheetShade shade in shades)
        {
            if (shade.Inks.Length != shade.Colours.Length)
                throw new InvalidDataException($"Shade {shade.Shade.Key} recolours {shade.Inks.Length} inks with {shade.Colours.Length} colours.");
            if (!documents.TryAdd(shade.Shade.Key, new SpriteSheetShadeDocument
                {
                    Inks = shade.Inks,
                    Colours = [.. shade.Colours.Select(colour => colour.ToWord())],
                    ColourMath = shade.TakesColourMath,
                }))
            {
                throw new InvalidDataException($"A sheet names shade {shade.Shade.Key} twice.");
            }
        }
        return documents;
    }
}
