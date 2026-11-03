using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Pre-colours composed sheets in further shades (#1512), beside any their composer gave them.</summary>
internal static class ShadedSheets
{
    /// <summary><paramref name="sheet"/>, also pre-coloured in <paramref name="shades"/>.</summary>
    /// <exception cref="InvalidDataException">A shade's name is one the sheet already has, or its lists differ in length.</exception>
    internal static ComposedSpriteSheet With(ComposedSpriteSheet sheet, IReadOnlyList<SheetShade> shades)
    {
        Ensure.NotNull(sheet);
        Ensure.NotNull(shades);
        if (SheetShade.Documents(shades) is not { } added)
            return sheet;
        var documents = new Dictionary<string, SpriteSheetShadeDocument>(sheet.Manifest.Shades ?? [], StringComparer.Ordinal);
        foreach ((string key, SpriteSheetShadeDocument document) in added)
        {
            if (!documents.TryAdd(key, document))
                throw new InvalidDataException($"Sheet already has shade {key}.");
        }
        return sheet with { Manifest = sheet.Manifest with { Shades = documents } };
    }
}
