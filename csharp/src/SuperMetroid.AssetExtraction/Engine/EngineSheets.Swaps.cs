using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What a shade of a sheet does as swaps of one colour for another, for a thing that shows a shade by changing colours where it is drawn.</summary>
internal sealed partial class EngineSheets
{
    /// <summary>
    /// The colours one shade of a sheet changes, each as the sheet's pictures have it and as the shade shows it, as
    /// red, green and blue in one number. A colour the shade leaves as it is is left out.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The installation has no such sheet or shade; or the shade cannot be shown by swapping colours: it shows one colour
    /// of the sheet as two, or changes a colour in one part of the sheet's pictures that another part also has and keeps.
    /// </exception>
    internal IReadOnlyDictionary<int, int> ShadeSwaps(string family, string sheet, string shade)
    {
        InstalledSheetDocument document = Document(family, sheet);
        var swaps = new SortedDictionary<int, int>();
        var regionsChanged = new Dictionary<int, HashSet<int>>();
        foreach (((int colour, int region), RgbaPixel shown) in ShadeOf(sheet, document, shade))
        {
            int to = (shown.R << 16) | (shown.G << 8) | shown.B;
            if (swaps.TryGetValue(colour, out int other) && other != to)
                throw new InvalidDataException($"The shade {shade} of the sheet {sheet} shows the colour #{colour:x6} as #{other:x6} in one part and #{to:x6} in another: a swap of colours cannot tell them apart.");
            swaps[colour] = to;
            if (!regionsChanged.TryGetValue(colour, out HashSet<int>? regions))
                regionsChanged[colour] = regions = [];
            regions.Add(region);
        }

        // A colour the shade changes must not also be drawn in a part of the sheet the shade leaves alone.
        if (!_images.TryGetValue((family, sheet), out SpriteImagePixels? image))
            _images[(family, sheet)] = image = _store.Assemble(sheet, document);
        for (int pixel = 0; pixel < image.Pixels.Length; pixel++)
        {
            RgbaPixel drawn = image.Pixels[pixel];
            if (drawn.A == 0)
                continue;
            int colour = (drawn.R << 16) | (drawn.G << 8) | drawn.B;
            int region = image.RecolourRegions is { } placed ? placed[pixel] : 0;
            if (regionsChanged.TryGetValue(colour, out HashSet<int>? changed) && !changed.Contains(region))
                throw new InvalidDataException($"The shade {shade} of the sheet {sheet} changes the colour #{colour:x6} in one part of its pictures and leaves it in another: a swap of colours cannot tell them apart.");
        }

        foreach (int same in swaps.Where(swap => swap.Key == swap.Value).Select(swap => swap.Key).ToArray())
            swaps.Remove(same);
        return swaps;
    }
}
