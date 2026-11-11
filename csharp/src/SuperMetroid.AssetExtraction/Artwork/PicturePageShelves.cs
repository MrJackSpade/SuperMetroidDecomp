using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Shelf-packs pictures onto the picture store's pages (#1635), in the order they are placed: each picture goes to the
/// right of the one before it on the current shelf, or starts the next shelf below the tallest picture of this one when it
/// does not fit across the page; a picture that does not fit below the page's last shelf starts the next page. Every page is
/// as wide as the widest picture, but at least <see cref="LeastPageWidth"/>, and at most <see cref="RgbaPng.MaximumDimension"/>
/// tall, as tall as its pictures reach.
/// </summary>
internal sealed class PicturePageShelves
{
    /// <summary>The narrowest page.</summary>
    internal const int LeastPageWidth = 1024;

    /// <summary>The height of each page packed so far: the bottom of its tallest shelf.</summary>
    private readonly List<int> heights = [0];

    /// <summary>Where the next picture's top-left pixel lies if it fits on the current shelf: right of the last, at the shelf's top.</summary>
    private Position16<ImagePixels> next;

    /// <summary>Starts packing pages as wide as <paramref name="widestPicture"/>, but at least <see cref="LeastPageWidth"/>.</summary>
    /// <exception cref="InvalidDataException">The picture is wider than a page can be.</exception>
    internal PicturePageShelves(int widestPicture)
    {
        if (widestPicture > RgbaPng.MaximumDimension)
            throw new InvalidDataException($"A picture {widestPicture} pixels wide is wider than a {RgbaPng.MaximumDimension}-pixel picture-store page.");
        Width = Math.Max(LeastPageWidth, widestPicture);
    }

    /// <summary>Every page's width.</summary>
    internal int Width { get; }

    /// <summary>Each page's size, in page order; none before a picture is placed.</summary>
    internal IReadOnlyList<Extent16<ImagePixels>> Pages => heights[^1] == 0 ? []
        : [.. heights.Select(height => new Extent16<ImagePixels>(Width, height))];

    /// <summary>Places a picture of <paramref name="size"/> pixels and gives its page and the top-left pixel it lies at.</summary>
    /// <param name="size">The picture's width and height, neither zero.</param>
    /// <param name="corner">The pixel of its page the picture's top-left pixel lies at.</param>
    /// <returns>The page, counted from zero.</returns>
    /// <exception cref="InvalidDataException">The picture is empty, wider than the pages or taller than a page can be.</exception>
    internal int Place(Extent16<ImagePixels> size, out Position16<ImagePixels> corner)
    {
        if (size.X == 0 || size.Y == 0 || size.X > Width || size.Y > RgbaPng.MaximumDimension)
            throw new InvalidDataException($"A {size.X}x{size.Y} picture does not fit a {Width}x{RgbaPng.MaximumDimension} picture-store page.");
        int page = heights.Count - 1;
        // A picture that does not fit across the page starts the next shelf, below every picture on the page.
        if (next.X + size.X > Width)
            next = new Position16<ImagePixels>(0, heights[page]);
        if (next.Y + size.Y > RgbaPng.MaximumDimension)
        {
            heights.Add(0);
            page++;
            next = Position16<ImagePixels>.Of(0, 0);
        }
        corner = next;
        next = new Position16<ImagePixels>(next.X + size.X, next.Y);
        heights[page] = Math.Max(heights[page], next.Y + size.Y);
        return page;
    }
}
