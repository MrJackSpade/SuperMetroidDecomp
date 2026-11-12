using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// An installed image's pixels and maps with its size (#1635): a converted sheet, a picture-store page, or a sheet's image
/// assembled from the store. The one place a picture's rectangle is copied from one image to another, its colours alone or
/// its colours, recolour regions and blending together.
/// </summary>
/// <param name="Planes">The pixels, row by row from the top, with the recolour-region and blend maps where the image has them.</param>
/// <param name="Size">The image's width and height.</param>
internal readonly record struct InstalledImagePlanes(SpriteImagePixels Planes, Extent16<ImagePixels> Size)
{
    /// <summary>
    /// Copies <paramref name="source"/>'s rectangle of this image to <paramref name="target"/> with its top-left pixel at
    /// <paramref name="corner"/>. A pixel of an image without a map lies in region zero and does not blend; a target without a
    /// map keeps none, so the rectangle must hold only region zero or no blending where the target has no such map.
    /// </summary>
    /// <exception cref="ArgumentException">A rectangle reaches outside its image.</exception>
    /// <exception cref="InvalidOperationException">The rectangle holds a recolour region or blending the target has no map for.</exception>
    internal void CopyTo(FrameRectangle source, InstalledImagePlanes target, Position16<ImagePixels> corner)
    {
        CopyPixelsTo(source, target, corner);
        int width = source.Size.X;
        for (int row = 0; row < source.Size.Y; row++)
        {
            int from = ((source.Corner.Y + row) * Size.X) + source.Corner.X;
            int to = ((corner.Y + row) * target.Size.X) + corner.X;
            CopyMap(Planes.RecolourRegions, from, target.Planes.RecolourRegions, to, width, "recolour region");
            CopyMap(Planes.Blending, from, target.Planes.Blending, to, width, "blending");
        }
    }

    /// <summary>
    /// Copies the pixels of <paramref name="source"/>'s rectangle of this image to <paramref name="target"/> with its
    /// top-left pixel at <paramref name="corner"/>, and none of the maps (#1644): a stored picture is its pixels only, and
    /// the maps it is shown with are the reference's.
    /// </summary>
    /// <exception cref="ArgumentException">A rectangle reaches outside its image.</exception>
    internal void CopyPixelsTo(FrameRectangle source, InstalledImagePlanes target, Position16<ImagePixels> corner)
    {
        Ensure.NotNull(Planes);
        Ensure.NotNull(target.Planes);
        var placed = new FrameRectangle(corner, source.Size);
        if (!source.LiesInside(Size))
            throw new ArgumentException($"Picture rectangle {source} reaches outside its {Size.X}x{Size.Y} image.", nameof(source));
        if (!placed.LiesInside(target.Size))
            throw new ArgumentException($"Picture rectangle {placed} reaches outside its {target.Size.X}x{target.Size.Y} image.", nameof(corner));
        int width = source.Size.X;
        for (int row = 0; row < source.Size.Y; row++)
        {
            Planes.Pixels.AsSpan(((source.Corner.Y + row) * Size.X) + source.Corner.X, width)
                .CopyTo(target.Planes.Pixels.AsSpan(((corner.Y + row) * target.Size.X) + corner.X, width));
        }
    }

    /// <summary>
    /// Copies one row of a map from <paramref name="source"/> to <paramref name="target"/>; a missing source map holds the
    /// default value throughout, and a missing target map takes only it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The target has no map, but the source row holds another value.</exception>
    private static void CopyMap<T>(T[]? source, int from, T[]? target, int to, int width, string map)
        where T : struct, IEquatable<T>
    {
        if (target is not null)
        {
            if (source is not null)
                source.AsSpan(from, width).CopyTo(target.AsSpan(to, width));
            else
                target.AsSpan(to, width).Clear();
        }
        else if (source is not null && source.AsSpan(from, width).ContainsAnyExcept(default(T)))
        {
            throw new InvalidOperationException($"A picture's {map} map is copied to an image that has none.");
        }
    }

    /// <summary>
    /// Whether <paramref name="rectangle"/> of this image holds only what an image starts as before any picture is copied
    /// into it: clear black pixels, recolour region zero and no blending.
    /// </summary>
    internal bool IsClear(FrameRectangle rectangle) =>
        ShowsNoPixel(rectangle) && AllDefault(Planes.RecolourRegions, rectangle) && AllDefault(Planes.Blending, rectangle);

    /// <summary>Whether every pixel of <paramref name="rectangle"/> of this image is clear black, whatever its maps hold.</summary>
    internal bool ShowsNoPixel(FrameRectangle rectangle) => AllDefault(Planes.Pixels, rectangle);

    /// <summary>
    /// The box of what <paramref name="rectangle"/> of this image holds (#1646): the smallest rectangle inside it holding
    /// every pixel that is not as an image starts before any picture is copied into it, that is, every pixel that is not clear
    /// black (an opaque pixel, or a clear one keeping a colour of its own), lies in a recolour region other than zero, or
    /// blends. Everything of <paramref name="rectangle"/> outside the box is as an image starts, so copying the box alone
    /// into an image that starts clear leaves it as copying the whole rectangle does.
    /// </summary>
    /// <exception cref="ArgumentException">The rectangle reaches outside the image.</exception>
    /// <exception cref="InvalidOperationException">The rectangle holds only what an image starts as, so it has no box.</exception>
    internal FrameRectangle ContentBox(FrameRectangle rectangle)
    {
        Ensure.NotNull(Planes);
        if (!rectangle.LiesInside(Size))
            throw new ArgumentException($"Picture rectangle {rectangle} reaches outside its {Size.X}x{Size.Y} image.", nameof(rectangle));
        int left = int.MaxValue, right = -1, top = -1, bottom = -1;
        for (int row = 0; row < rectangle.Size.Y; row++)
        {
            int line = ((rectangle.Corner.Y + row) * Size.X) + rectangle.Corner.X;
            int first = Earliest(Earliest(FirstHeld(Planes.Pixels, line, rectangle.Size.X), FirstHeld(Planes.RecolourRegions, line, rectangle.Size.X)),
                FirstHeld(Planes.Blending, line, rectangle.Size.X));
            if (first < 0)
                continue;
            int last = Math.Max(Math.Max(LastHeld(Planes.Pixels, line, rectangle.Size.X), LastHeld(Planes.RecolourRegions, line, rectangle.Size.X)),
                LastHeld(Planes.Blending, line, rectangle.Size.X));
            left = Math.Min(left, first);
            right = Math.Max(right, last);
            if (top < 0)
                top = row;
            bottom = row;
        }
        if (top < 0)
            throw new InvalidOperationException($"Picture rectangle {rectangle} holds nothing, so it has no box of what it holds.");
        return new FrameRectangle(Position16<ImagePixels>.Of(rectangle.Corner.X + left, rectangle.Corner.Y + top),
            new Extent16<ImagePixels>(right - left + 1, bottom - top + 1));
    }

    /// <summary>
    /// The smallest period <paramref name="rectangle"/> of this image repeats with (#1651): the narrowest width that divides
    /// the rectangle's width, and the shortest height that divides its height, such that every pixel, recolour region and
    /// blend of the rectangle equals the one a whole number of periods across and down from it in its top-left period. The
    /// rectangle's own size where it does not repeat. Tiling the top-left period over the rectangle gives the rectangle.
    /// </summary>
    /// <exception cref="ArgumentException">The rectangle reaches outside the image.</exception>
    internal Extent16<ImagePixels> Period(FrameRectangle rectangle)
    {
        Ensure.NotNull(Planes);
        if (!rectangle.LiesInside(Size))
            throw new ArgumentException($"Picture rectangle {rectangle} reaches outside its {Size.X}x{Size.Y} image.", nameof(rectangle));
        int across = rectangle.Size.X, down = rectangle.Size.Y;
        foreach (int width in Divisors(rectangle.Size.X))
        {
            if (RepeatsAcross(rectangle, width))
            {
                across = width;
                break;
            }
        }
        foreach (int height in Divisors(rectangle.Size.Y))
        {
            if (RepeatsDown(rectangle, height))
            {
                down = height;
                break;
            }
        }
        return new Extent16<ImagePixels>(across, down);
    }

    /// <summary>The divisors of <paramref name="length"/>, smallest first, ending with the length itself.</summary>
    private static IEnumerable<int> Divisors(int length) => Enumerable.Range(1, length).Where(divisor => length % divisor == 0);

    /// <summary>Whether every row of <paramref name="rectangle"/> repeats with period <paramref name="width"/> across, in every plane.</summary>
    private bool RepeatsAcross(FrameRectangle rectangle, int width)
    {
        for (int row = 0; row < rectangle.Size.Y; row++)
        {
            int line = ((rectangle.Corner.Y + row) * Size.X) + rectangle.Corner.X;
            for (int start = width; start < rectangle.Size.X; start += width)
            {
                if (!SameRun(line, line + start, width))
                    return false;
            }
        }
        return true;
    }

    /// <summary>Whether every row of <paramref name="rectangle"/> equals the row <paramref name="height"/> rows above it, in every plane.</summary>
    private bool RepeatsDown(FrameRectangle rectangle, int height)
    {
        for (int row = height; row < rectangle.Size.Y; row++)
        {
            int line = ((rectangle.Corner.Y + row) * Size.X) + rectangle.Corner.X;
            if (!SameRun(line - (height * Size.X), line, rectangle.Size.X))
                return false;
        }
        return true;
    }

    /// <summary>Whether the <paramref name="width"/> entries from <paramref name="first"/> and from <paramref name="second"/> match in every plane.</summary>
    private bool SameRun(int first, int second, int width) =>
        Planes.Pixels.AsSpan(first, width).SequenceEqual(Planes.Pixels.AsSpan(second, width)) &&
        SameRun(Planes.RecolourRegions, first, second, width) && SameRun(Planes.Blending, first, second, width);

    /// <summary>Whether a plane's <paramref name="width"/> entries from two places match; a missing plane always does.</summary>
    private static bool SameRun<T>(T[]? plane, int first, int second, int width)
        where T : struct, IEquatable<T> =>
        plane is null || plane.AsSpan(first, width).SequenceEqual(plane.AsSpan(second, width));

    /// <summary>The earlier of two places in a row, either of which is -1 where its plane holds nothing there.</summary>
    private static int Earliest(int first, int second) => first < 0 ? second : second < 0 ? first : Math.Min(first, second);

    /// <summary>
    /// The place of the first of <paramref name="width"/> entries of <paramref name="plane"/> from <paramref name="line"/> that
    /// is not the default; -1 where none is, and for a missing plane.
    /// </summary>
    private static int FirstHeld<T>(T[]? plane, int line, int width)
        where T : struct, IEquatable<T> =>
        plane is null ? -1 : plane.AsSpan(line, width).IndexOfAnyExcept(default(T));

    /// <summary>
    /// The place of the last of <paramref name="width"/> entries of <paramref name="plane"/> from <paramref name="line"/> that
    /// is not the default; -1 where none is, and for a missing plane.
    /// </summary>
    private static int LastHeld<T>(T[]? plane, int line, int width)
        where T : struct, IEquatable<T> =>
        plane is null ? -1 : plane.AsSpan(line, width).LastIndexOfAnyExcept(default(T));

    /// <summary>Whether every entry of <paramref name="plane"/> in <paramref name="rectangle"/> is the default; a missing plane is.</summary>
    private bool AllDefault<T>(T[]? plane, FrameRectangle rectangle)
        where T : struct, IEquatable<T>
    {
        if (plane is null)
            return true;
        for (int row = 0; row < rectangle.Size.Y; row++)
        {
            if (plane.AsSpan(((rectangle.Corner.Y + row) * Size.X) + rectangle.Corner.X, rectangle.Size.X).ContainsAnyExcept(default(T)))
                return false;
        }
        return true;
    }
}
