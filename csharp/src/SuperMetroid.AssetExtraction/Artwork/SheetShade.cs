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
                }))
            {
                throw new InvalidDataException($"A sheet names shade {shade.Shade.Key} twice.");
            }
        }
        return documents;
    }
}
