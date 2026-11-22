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
internal readonly partial record struct InstalledImagePlanes(SpriteImagePixels Planes, Extent16<ImagePixels> Size)
{

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
        // The box starts at the first row that holds something and takes in every later row that does.
        for (int row = 0; row < rectangle.Size.Y; row++)
        {
            if (HeldRun(LineOf(rectangle, row), rectangle.Size.X) is { } run)
                return BoxFrom(rectangle, ImageContentBox.OfRow(run, row), row + 1);
        }
        throw new InvalidOperationException($"Picture rectangle {rectangle} holds nothing, so it has no box of what it holds.");
    }

    /// <summary>The start of <paramref name="row"/> of <paramref name="rectangle"/> in the image's pixels.</summary>
    private int LineOf(FrameRectangle rectangle, int row) => ((rectangle.Corner.Y + row) * Size.X) + rectangle.Corner.X;

    /// <summary>
    /// The box of what <paramref name="rectangle"/> holds, from <paramref name="box"/>, the box of its rows before
    /// <paramref name="from"/>, taking in every row from there that holds something.
    /// </summary>
    private FrameRectangle BoxFrom(FrameRectangle rectangle, ImageContentBox box, int from)
    {
        for (int row = from; row < rectangle.Size.Y; row++)
        {
            if (HeldRun(LineOf(rectangle, row), rectangle.Size.X) is { } run)
                box = box.Including(run, row);
        }
        return new FrameRectangle(Position16<ImagePixels>.Of(rectangle.Corner.X + box.Left, rectangle.Corner.Y + box.Top),
            new Extent16<ImagePixels>(box.Right - box.Left + 1, box.Bottom - box.Top + 1));
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

    /// <summary>
    /// The first and last places of the <paramref name="width"/> entries from <paramref name="line"/> that some plane holds
    /// something other than the default at; null where no plane does.
    /// </summary>
    private ImageRowRun? HeldRun(int line, int width) =>
        Spanning(Spanning(HeldRun(Planes.Pixels, line, width), HeldRun(Planes.RecolourRegions, line, width)),
            HeldRun(Planes.Blending, line, width));

    /// <summary>The run spanning two runs of a row, either of which is null where its plane holds nothing there.</summary>
    private static ImageRowRun? Spanning(ImageRowRun? first, ImageRowRun? second) =>
        first is not { } one ? second : second is not { } other ? first : one.Spanning(other);

    /// <summary>
    /// The first and last places of the <paramref name="width"/> entries of <paramref name="plane"/> from <paramref name="line"/>
    /// that are not the default; null where none is, and for a missing plane.
    /// </summary>
    private static ImageRowRun? HeldRun<T>(T[]? plane, int line, int width)
        where T : struct, IEquatable<T>
    {
        if (plane is null)
            return null;
        ReadOnlySpan<T> run = plane.AsSpan(line, width);
        int first = run.IndexOfAnyExcept(default(T));
        return first < 0 ? null : new ImageRowRun(first, run.LastIndexOfAnyExcept(default(T)));
    }

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
