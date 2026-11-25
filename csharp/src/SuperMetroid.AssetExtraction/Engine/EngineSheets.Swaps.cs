using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// One colour a shade changes: the colour as the sheet's pictures have it, the region of the pictures it is changed
/// in, and what the shade shows it as, each colour as red, green and blue in one number.
/// </summary>
/// <param name="From">The colour as the pictures have it.</param>
/// <param name="Region">The region the change holds in; <see cref="Everywhere"/> where it holds wherever the colour is drawn.</param>
/// <param name="To">The colour the shade shows.</param>
internal readonly partial record struct ShadeSwap(int From, int Region, int To)
{
    /// <summary>The region of a change that holds wherever its colour is drawn.</summary>
    internal const int Everywhere = -1;
}

/// <summary>
/// What a shade of a sheet does as swaps of one colour for another, for a thing that shows a shade by changing colours
/// where it is drawn, and the regions of a sheet's pictures such a thing has to tell apart.
/// </summary>
internal sealed partial class EngineSheets
{
    private readonly Dictionary<(string Family, string Sheet), Dictionary<int, SortedSet<int>>> _regionsDrawn = [];

    /// <summary>
    /// The colours one shade of a sheet leaves as they are, of those the sheet's pictures draw: each with the region
    /// it is left in, or <see cref="ShadeSwap.Everywhere"/> where the shade leaves it in every region that draws it.
    /// Black is left out: a shade that darkens leaves it as it is, and it shows as nothing either way.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation has no such sheet or shade.</exception>
    internal IReadOnlyList<(int Colour, int Region)> ShadeKeeps(string family, string sheet, string shade)
    {
        InstalledSheetDocument document = Document(family, sheet);
        Dictionary<int, SortedSet<int>> drawn = RegionsDrawn(family, sheet, document);
        var changed = new HashSet<(int Colour, int Region)>();
        foreach (((int colour, int region), RgbaPixel shown) in ShadeOf(sheet, document, shade))
        {
            if (((shown.R << 16) | (shown.G << 8) | shown.B) != colour)
                changed.Add((colour, region));
        }

        var kept = new List<(int Colour, int Region)>();
        foreach ((int colour, SortedSet<int> regions) in drawn.OrderBy(entry => entry.Key))
        {
            if (colour == 0)
                continue;
            int[] left = [.. regions.Where(region => !changed.Contains((colour, region)))];
            if (left.Length == regions.Count)
                kept.Add((colour, ShadeSwap.Everywhere));
            else
                kept.AddRange(left.Select(region => (colour, region)));
        }

        return kept;
    }

    /// <summary>
    /// The colours one shade of a sheet changes. A colour the shade shows alike in every region of the sheet's
    /// pictures that draws it is one swap that holds everywhere; a colour it changes in one region and leaves, or
    /// shows otherwise, in another is a swap for each region it changes. A colour the shade leaves as it is, or that
    /// no picture of the sheet draws, is left out.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation has no such sheet or shade.</exception>
    internal IReadOnlyList<ShadeSwap> ShadeSwaps(string family, string sheet, string shade)
    {
        InstalledSheetDocument document = Document(family, sheet);
        Dictionary<int, SortedSet<int>> drawn = RegionsDrawn(family, sheet, document);
        var changed = new SortedDictionary<int, SortedDictionary<int, int>>();
        foreach (((int colour, int region), RgbaPixel shown) in ShadeOf(sheet, document, shade))
        {
            int to = (shown.R << 16) | (shown.G << 8) | shown.B;
            if (to == colour || !drawn.TryGetValue(colour, out SortedSet<int>? regions) || !regions.Contains(region))
                continue;
            if (!changed.TryGetValue(colour, out SortedDictionary<int, int>? byRegion))
                changed[colour] = byRegion = [];
            byRegion[region] = to;
        }

        var swaps = new List<ShadeSwap>();
        foreach ((int colour, SortedDictionary<int, int> byRegion) in changed)
        {
            int first = byRegion.Values.First();
            if (byRegion.Count == drawn[colour].Count && byRegion.Values.All(to => to == first))
            {
                swaps.Add(new ShadeSwap(colour, ShadeSwap.Everywhere, first));
                continue;
            }

            foreach ((int region, int to) in byRegion)
                swaps.Add(new ShadeSwap(colour, region, to));
        }

        return swaps;
    }

    /// <summary>
    /// The colour a sheet's scene shows where nothing is drawn, as six hexadecimal digits; null for a sheet whose scene
    /// has no such colour of its own. A room's block sheet has one.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation has no such sheet, or names the colour otherwise than as #rrggbb.</exception>
    internal string? BackdropOf(string family, string sheet)
    {
        if (Document(family, sheet).Backdrop is not { } backdrop)
            return null;
        string colour = backdrop.Colour;
        return colour.Length == 7 && colour[0] == '#' && colour[1..].All(Uri.IsHexDigit)
            ? colour[1..].ToLowerInvariant()
            : throw new InvalidDataException($"The sheet {sheet} names its backdrop colour {colour}, which is not written as #rrggbb.");
    }

    /// <summary>Whether a sheet tells any of its pixels apart by region: some pixel lies in a region other than the first.</summary>
    /// <exception cref="InvalidDataException">The installation has no such sheet.</exception>
    internal bool HasRegions(string family, string sheet) =>
        RegionsDrawn(family, sheet, Document(family, sheet)).Values.Any(regions => regions.Max > 0);

    /// <summary>
    /// The region of each pixel of one frame of a sheet, row by row, where the frame is of the size given; null where
    /// the sheet has no such frame or the frame shows nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">The family has no such sheet, or the frame is of another size.</exception>
    internal int[]? FrameRegions(string family, string sheet, string frame, int width, int height)
    {
        InstalledSheetDocument document = Document(family, sheet);
        if (!document.Frames.TryGetValue(frame, out InstalledSheetFrameDocument? placed) || placed.Picture is null)
            return null;
        if (placed.Size.X != width || placed.Size.Y != height)
            throw new InvalidDataException($"The frame {frame} of the sheet {sheet} is {placed.Size.X} by {placed.Size.Y}, not {width} by {height}.");
        var regions = new int[width * height];
        if (ImageOf(family, sheet, document).RecolourRegions is { } held)
        {
            for (int y = 0; y < height; y++)
                held.AsSpan((placed.Corner.Y + y) * document.Size.X + placed.Corner.X, width).CopyTo(regions.AsSpan(y * width, width));
        }

        return regions;
    }

    /// <summary>The region of each pixel of one frame of a sheet, row by row, whatever its size.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame, or the frame shows nothing.</exception>
    internal int[] FrameRegions(string family, string sheet, string frame)
    {
        InstalledSheetDocument document = Document(family, sheet);
        if (!document.Frames.TryGetValue(frame, out InstalledSheetFrameDocument? placed) || placed.Picture is null)
            throw new InvalidDataException($"The sheet {sheet} has no picture for the frame {frame}.");
        return FrameRegions(family, sheet, frame, placed.Size.X, placed.Size.Y)!;
    }

    /// <summary>The regions each colour of a sheet is drawn in, over all of the sheet's pixels.</summary>
    private Dictionary<int, SortedSet<int>> RegionsDrawn(string family, string sheet, InstalledSheetDocument document)
    {
        if (_regionsDrawn.TryGetValue((family, sheet), out Dictionary<int, SortedSet<int>>? known))
            return known;
        SpriteImagePixels image = ImageOf(family, sheet, document);
        var drawn = new Dictionary<int, SortedSet<int>>();
        for (int pixel = 0; pixel < image.Pixels.Length; pixel++)
        {
            RgbaPixel shown = image.Pixels[pixel];
            if (shown.A == 0)
                continue;
            int colour = (shown.R << 16) | (shown.G << 8) | shown.B;
            if (!drawn.TryGetValue(colour, out SortedSet<int>? regions))
                drawn[colour] = regions = [];
            regions.Add(image.RecolourRegions is { } placed ? placed[pixel] : 0);
        }

        return _regionsDrawn[(family, sheet)] = drawn;
    }

    /// <summary>A sheet's whole image, put together once.</summary>
    private SpriteImagePixels ImageOf(string family, string sheet, InstalledSheetDocument document)
    {
        if (!_images.TryGetValue((family, sheet), out SpriteImagePixels? image))
            _images[(family, sheet)] = image = _store.Assemble(sheet, document);
        return image;
    }
}
