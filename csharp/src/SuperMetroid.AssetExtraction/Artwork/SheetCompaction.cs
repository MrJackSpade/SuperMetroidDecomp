using System.Runtime.InteropServices;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Holds each distinct picture of one sheet once (#1635): frames whose regions show the same picture, every pixel's colour,
/// colour place and front-tile mark alike, share one region, and the sheet is repacked with only the distinct regions, in
/// the reading order of their first frames. Sharing within a sheet keeps every frame's look, since the frames share the
/// sheet's colours and shades. A frame keeps its own name, origin and depth; only its corner moves.
/// </summary>
internal static class SheetCompaction
{
    /// <summary>The sheet with each distinct picture held once.</summary>
    /// <param name="planes">The sheet as composed.</param>
    /// <param name="manifest">Its frames.</param>
    /// <exception cref="InvalidDataException">A frame's region reaches outside the sheet.</exception>
    internal static CompactedSheet Compact(SpriteSheetPlanes planes, SpriteSheetDocument manifest)
    {
        Ensure.NotNull(manifest);
        int width = planes.Size.X;
        var distinct = new List<RegionPicture>();
        var heldAs = new Dictionary<RegionPicture, int>();
        var pictureOf = new Dictionary<FrameRegion, int>();
        foreach (FrameRegion region in manifest.Frames.Values
            .Where(frame => frame.Size.X > 0 && frame.Size.Y > 0)
            .Select(frame => new FrameRegion(frame.Corner, frame.Size))
            .Distinct()
            .OrderBy(region => region.Corner.Y).ThenBy(region => region.Corner.X))
        {
            RegionPicture picture = RegionPicture.Copy(planes, region);
            if (!heldAs.TryGetValue(picture, out int index))
            {
                index = distinct.Count;
                distinct.Add(picture);
                heldAs.Add(picture, index);
            }
            pictureOf.Add(region, index);
        }

        // Shelf packing: each distinct picture left to right in rows as wide as the composed sheet.
        int packedWidth = Math.Max(width, distinct.Count == 0 ? 0 : distinct.Max(picture => picture.Size.X));
        var corners = new Position16<ImagePixels>[distinct.Count];
        int x = 0, y = 0, rowHeight = 0;
        for (int index = 0; index < distinct.Count; index++)
        {
            Extent16<ImagePixels> size = distinct[index].Size;
            if (x + size.X > packedWidth)
            {
                y += rowHeight;
                x = 0;
                rowHeight = 0;
            }
            corners[index] = new Position16<ImagePixels>(x, y);
            x += size.X;
            rowHeight = Math.Max(rowHeight, size.Y);
        }
        int packedHeight = y + rowHeight;
        var packedSize = new Extent16<ImagePixels>(checked((ushort)packedWidth), checked((ushort)packedHeight));
        var pixels = new RgbaPixel[packedWidth * packedHeight];
        var paletteIndices = new byte[pixels.Length];
        var frontTiles = new byte[pixels.Length];
        for (int index = 0; index < distinct.Count; index++)
            distinct[index].PaintAt(corners[index], packedWidth, pixels, paletteIndices, frontTiles);

        var frames = new Dictionary<string, SpriteSheetFrameDocument>(manifest.Frames.Count, StringComparer.Ordinal);
        foreach ((string name, SpriteSheetFrameDocument frame) in manifest.Frames)
        {
            frames.Add(name, frame.Size.X > 0 && frame.Size.Y > 0
                ? frame with { Corner = corners[pictureOf[new FrameRegion(frame.Corner, frame.Size)]] }
                : frame with { Corner = new Position16<ImagePixels>(0, 0) });
        }
        return new CompactedSheet(packedSize, pixels, paletteIndices, frontTiles, manifest with { Frames = frames });
    }

    /// <summary>One frame's region on the composed sheet.</summary>
    /// <param name="Corner">Its top-left pixel.</param>
    /// <param name="Size">Its width and height.</param>
    private readonly record struct FrameRegion(Position16<ImagePixels> Corner, Extent16<ImagePixels> Size);

    /// <summary>
    /// A copy of one region's pixels, colour places and front-tile marks, equal to another only when its size and every
    /// value match, compared one by one until the first that differs; its hash only narrows the comparison.
    /// </summary>
    private sealed class RegionPicture : IEquatable<RegionPicture>
    {
        /// <summary>The region's pixels, row by row.</summary>
        private readonly RgbaPixel[] pixels;

        /// <summary>The region's colour places, row by row.</summary>
        private readonly byte[] paletteIndices;

        /// <summary>The region's front-tile marks, row by row.</summary>
        private readonly byte[] frontTiles;

        /// <summary>A cheap hash of the size and values.</summary>
        private readonly int hash;

        /// <summary>Keeps one region's copied planes.</summary>
        private RegionPicture(Extent16<ImagePixels> size, RgbaPixel[] pixels, byte[] paletteIndices, byte[] frontTiles)
        {
            Size = size;
            this.pixels = pixels;
            this.paletteIndices = paletteIndices;
            this.frontTiles = frontTiles;
            var code = new HashCode();
            code.Add(size);
            code.AddBytes(MemoryMarshal.AsBytes(pixels.AsSpan()));
            code.AddBytes(paletteIndices);
            code.AddBytes(frontTiles);
            hash = code.ToHashCode();
        }

        /// <summary>The region's width and height.</summary>
        internal Extent16<ImagePixels> Size { get; }

        /// <summary>Copies a region's planes out of a sheet.</summary>
        /// <exception cref="InvalidDataException">The region reaches outside the sheet.</exception>
        internal static RegionPicture Copy(SpriteSheetPlanes planes, FrameRegion region)
        {
            int left = region.Corner.X;
            int top = region.Corner.Y;
            int width = region.Size.X;
            int height = region.Size.Y;
            if (left + width > planes.Size.X || top + height > planes.Size.Y)
                throw new InvalidDataException(
                    $"A {width}x{height} frame at ({left}, {top}) reaches outside its {planes.Size.X}x{planes.Size.Y} sheet.");
            var pixels = new RgbaPixel[width * height];
            var paletteIndices = new byte[pixels.Length];
            var frontTiles = new byte[pixels.Length];
            for (int row = 0; row < height; row++)
            {
                int from = ((top + row) * planes.Size.X) + left;
                planes.Pixels.Slice(from, width).CopyTo(pixels.AsSpan(row * width, width));
                planes.PaletteIndices.Slice(from, width).CopyTo(paletteIndices.AsSpan(row * width, width));
                planes.FrontTiles.Slice(from, width).CopyTo(frontTiles.AsSpan(row * width, width));
            }
            return new RegionPicture(region.Size, pixels, paletteIndices, frontTiles);
        }

        /// <summary>Paints the region's planes at <paramref name="corner"/> of a packed sheet <paramref name="sheetWidth"/> wide.</summary>
        internal void PaintAt(Position16<ImagePixels> corner, int sheetWidth, RgbaPixel[] sheetPixels, byte[] sheetPaletteIndices,
            byte[] sheetFrontTiles)
        {
            int width = Size.X;
            for (int row = 0; row < Size.Y; row++)
            {
                int to = ((corner.Y + row) * sheetWidth) + corner.X;
                pixels.AsSpan(row * width, width).CopyTo(sheetPixels.AsSpan(to, width));
                paletteIndices.AsSpan(row * width, width).CopyTo(sheetPaletteIndices.AsSpan(to, width));
                frontTiles.AsSpan(row * width, width).CopyTo(sheetFrontTiles.AsSpan(to, width));
            }
        }

        /// <inheritdoc />
        public bool Equals(RegionPicture? other)
        {
            if (other is null || other.Size != Size)
                return false;
            for (int pixel = 0; pixel < pixels.Length; pixel++)
            {
                if (pixels[pixel] != other.pixels[pixel] || paletteIndices[pixel] != other.paletteIndices[pixel] ||
                    frontTiles[pixel] != other.frontTiles[pixel])
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is RegionPicture other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => hash;
    }
}
