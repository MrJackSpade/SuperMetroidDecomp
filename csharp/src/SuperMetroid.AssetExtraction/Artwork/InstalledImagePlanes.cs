using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// An installed image's pixels and maps with its size (#1635): a converted sheet, a picture-store page, or a sheet's image
/// assembled from the store. The one place a picture's rectangle is copied from one image to another, its colours, recolour
/// regions and blending together.
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
            int from = ((source.Corner.Y + row) * Size.X) + source.Corner.X;
            int to = ((corner.Y + row) * target.Size.X) + corner.X;
            Planes.Pixels.AsSpan(from, width).CopyTo(target.Planes.Pixels.AsSpan(to, width));
            CopyMap(Planes.RecolourRegions, from, target.Planes.RecolourRegions, to, width, "recolour region");
            CopyMap(Planes.Blending, from, target.Planes.Blending, to, width, "blending");
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
        AllDefault(Planes.Pixels, rectangle) && AllDefault(Planes.RecolourRegions, rectangle) && AllDefault(Planes.Blending, rectangle);

    /// <summary>Whether <paramref name="rectangle"/> of this image has a pixel in a recolour region other than zero.</summary>
    internal bool HasRecolourRegions(FrameRectangle rectangle) => !AllDefault(Planes.RecolourRegions, rectangle);

    /// <summary>Whether <paramref name="rectangle"/> of this image has a pixel the blend map says blends.</summary>
    internal bool HasBlending(FrameRectangle rectangle) => !AllDefault(Planes.Blending, rectangle);

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
