using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Shares a sheet's pictures between its frames (#1633): frames whose installed pictures are the same, pixel for pixel in
/// colour, recolour region and blending, hold one picture. Composed frames that differed only in where their colours came
/// from install the same picture; the first frame in reading order keeps its rectangle, every later one shows that
/// rectangle, and the rectangle a later one held is cleared wherever no remaining frame lies.
/// </summary>
internal static class InstalledFrameSharing
{
    /// <summary>
    /// Points every frame of <paramref name="frames"/> whose picture an earlier frame already holds at that frame's
    /// rectangle, and clears what it held from <paramref name="pixels"/>, <paramref name="regions"/> and
    /// <paramref name="blending"/>, all <paramref name="size"/> pixels in reading order.
    /// </summary>
    internal static void Share(Dictionary<string, InstalledSheetFrameDocument> frames, Extent16<ImagePixels> size,
        RgbaPixel[] pixels, int[]? regions, bool[] blending)
    {
        Ensure.NotNull(frames);
        Ensure.NotNull(pixels);
        Ensure.NotNull(blending);
        var held = new List<Rectangle>();
        var vacated = new List<Rectangle>();
        var shownAt = new Dictionary<Rectangle, Rectangle>();
        foreach (Rectangle rectangle in frames.Values.Where(frame => frame.Size.X > 0 && frame.Size.Y > 0)
            .Select(frame => new Rectangle(frame.Corner, frame.Size)).Distinct()
            .OrderBy(rectangle => rectangle.Corner.Y).ThenBy(rectangle => rectangle.Corner.X))
        {
            int same = held.FindIndex(earlier => Same(earlier, rectangle, size, pixels, regions, blending));
            if (same >= 0)
            {
                shownAt.Add(rectangle, held[same]);
                vacated.Add(rectangle);
            }
            else
            {
                held.Add(rectangle);
            }
        }
        if (vacated.Count == 0)
            return;
        foreach (string name in frames.Keys.ToArray())
        {
            InstalledSheetFrameDocument frame = frames[name];
            if (shownAt.TryGetValue(new Rectangle(frame.Corner, frame.Size), out Rectangle holder))
                frames[name] = frame with { Corner = holder.Corner };
        }
        foreach (Rectangle rectangle in vacated)
            Clear(rectangle, held, size, pixels, regions, blending);
    }

    /// <summary>Whether two frame rectangles hold the same installed picture.</summary>
    private static bool Same(Rectangle first, Rectangle second, Extent16<ImagePixels> size, RgbaPixel[] pixels, int[]? regions,
        bool[] blending)
    {
        if (first.Size != second.Size)
            return false;
        for (int y = 0; y < first.Size.Y; y++)
        {
            int left = (first.Corner.Y + y) * size.X + first.Corner.X;
            int right = (second.Corner.Y + y) * size.X + second.Corner.X;
            for (int x = 0; x < first.Size.X; x++)
            {
                if (pixels[left + x] != pixels[right + x] || blending[left + x] != blending[right + x]
                    || (regions is not null && regions[left + x] != regions[right + x]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>Clears the pixels of <paramref name="rectangle"/> that no rectangle of <paramref name="kept"/> covers.</summary>
    private static void Clear(Rectangle rectangle, List<Rectangle> kept, Extent16<ImagePixels> size, RgbaPixel[] pixels,
        int[]? regions, bool[] blending)
    {
        for (int y = rectangle.Corner.Y; y < rectangle.Corner.Y + rectangle.Size.Y; y++)
        {
            for (int x = rectangle.Corner.X; x < rectangle.Corner.X + rectangle.Size.X; x++)
            {
                var at = new Position16<ImagePixels>(x, y);
                if (kept.Any(other => other.Covers(at)))
                    continue;
                int pixel = y * size.X + x;
                pixels[pixel] = default;
                blending[pixel] = false;
                if (regions is not null)
                    regions[pixel] = 0;
            }
        }
    }

    /// <summary>A frame's rectangle on its sheet.</summary>
    /// <param name="Corner">The rectangle's top-left pixel.</param>
    /// <param name="Size">The rectangle's width and height.</param>
    private readonly record struct Rectangle(Position16<ImagePixels> Corner, Extent16<ImagePixels> Size)
    {
        /// <summary>Whether the rectangle covers <paramref name="pixel"/>.</summary>
        public bool Covers(Position16<ImagePixels> pixel) =>
            pixel.X >= Corner.X && pixel.X < Corner.X + Size.X && pixel.Y >= Corner.Y && pixel.Y < Corner.Y + Size.Y;
    }
}
