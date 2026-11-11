using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Every picture one installation's sheets render, kept as its pixels (#1635): a picture is rendered once in an
/// installation, within a sheet and across sheets. Before a sheet is rendered, each picture region its frames name is
/// compared with the pictures already rendered, pixel by pixel; a region whose pixels all match one already rendered is
/// refused with an error naming both, so the composer that asked for the second copy can be found. A cheap hash of the
/// pixels only narrows which pictures are compared; equality is always the full pixel comparison. Frames that share one
/// region show one picture and are admitted once. Shared by every composer of the installation run, which render on
/// many threads.
/// </summary>
internal sealed class SheetPictureRegistry
{
    /// <summary>Every picture rendered so far, by its pixels, with where it was first rendered.</summary>
    private readonly ConcurrentDictionary<PicturePixels, RenderedPicture> rendered = new();

    /// <summary>
    /// Admits the pictures <paramref name="installed"/>'s frames show on sheet <paramref name="sheet"/>, all
    /// <paramref name="size"/> pixels in reading order. A picture is its pixels with their recolour regions and blending
    /// (#1633, #1635); frames of one sheet that show the same picture already share its rectangle.
    /// </summary>
    /// <exception cref="InvalidDataException">The picture was rendered before, on this sheet or another.</exception>
    internal void Admit(string sheet, Extent16<ImagePixels> size, InstalledSheet installed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sheet);
        Ensure.NotNull(installed);
        foreach (IGrouping<PictureRegion, string> region in installed.Manifest.Frames
            .GroupBy(frame => new PictureRegion(frame.Value.Corner, frame.Value.Size), frame => frame.Key)
            .OrderBy(region => region.Key.Corner.Y).ThenBy(region => region.Key.Corner.X))
        {
            if (region.Key.Size.X == 0 || region.Key.Size.Y == 0)
                continue;
            var picture = new RenderedPicture(sheet, string.Join(", ", region.Order(StringComparer.Ordinal)), region.Key);
            PicturePixels pixels = PicturePixels.Copy(size, installed.Pixels, region.Key);
            if (!rendered.TryAdd(pixels, picture))
            {
                RenderedPicture first = rendered[pixels];
                throw new InvalidDataException(
                    $"Sheet {picture.Sheet} renders frame(s) [{picture.Frames}] at {picture.Region}, the same picture, pixel for " +
                    $"pixel, as frame(s) [{first.Frames}] of sheet {first.Sheet} at {first.Region}. A picture is rendered once " +
                    "per installation (#1635): the composer asking for the second copy must reuse the first.");
            }
        }
    }

    private readonly record struct PictureRegion(Position16<ImagePixels> Corner, Extent16<ImagePixels> Size)
    {
        /// <inheritdoc />
        public override string ToString() => $"({Corner.X}, {Corner.Y}) {Size.X}x{Size.Y}";
    }

    /// <summary>Where a rendered picture was first rendered.</summary>
    /// <param name="Sheet">The sheet.</param>
    /// <param name="Frames">The frames that show it there.</param>
    /// <param name="Region">Its region on that sheet.</param>
    private sealed record RenderedPicture(string Sheet, string Frames, PictureRegion Region);

    /// <summary>
    /// A copy of one picture's pixels, equal to another only when its size and every pixel match, compared pixel by pixel
    /// until the first that differs; its hash only narrows the comparison.
    /// </summary>
    private sealed class PicturePixels : IEquatable<PicturePixels>
    {
        /// <summary>The picture's width and height.</summary>
        private readonly Extent16<ImagePixels> size;

        /// <summary>The picture's pixels, row by row.</summary>
        private readonly RgbaPixel[] pixels;

        /// <summary>Each pixel's recolour region, row by row; zero throughout for a sheet without a region map.</summary>
        private readonly int[] regions;

        /// <summary>Whether each pixel blends, row by row; false throughout for a sheet without a blend map.</summary>
        private readonly bool[] blending;

        /// <summary>A cheap hash of the size and pixels.</summary>
        private readonly int hash;

        /// <summary>Keeps one picture's pixels, regions and blending, all <paramref name="size"/> in reading order.</summary>
        private PicturePixels(Extent16<ImagePixels> size, RgbaPixel[] pixels, int[] regions, bool[] blending)
        {
            this.size = size;
            this.pixels = pixels;
            this.regions = regions;
            this.blending = blending;
            var code = new HashCode();
            code.Add(size);
            code.AddBytes(MemoryMarshal.AsBytes(pixels.AsSpan()));
            hash = code.ToHashCode();
        }

        /// <summary>Copies <paramref name="region"/> of an installed sheet of <paramref name="sheetSize"/> pixels.</summary>
        /// <exception cref="InvalidDataException">The region reaches outside the sheet.</exception>
        internal static PicturePixels Copy(Extent16<ImagePixels> sheetSize, SpriteImagePixels sheet, PictureRegion region)
        {
            int left = region.Corner.X;
            int top = region.Corner.Y;
            int width = region.Size.X;
            int height = region.Size.Y;
            if (left + width > sheetSize.X || top + height > sheetSize.Y)
                throw new InvalidDataException($"Frame region {region} reaches outside its {sheetSize.X}x{sheetSize.Y} sheet.");
            var pixels = new RgbaPixel[width * height];
            var regions = new int[pixels.Length];
            var blending = new bool[pixels.Length];
            for (int row = 0; row < height; row++)
            {
                int from = ((top + row) * sheetSize.X) + left;
                sheet.Pixels.AsSpan(from, width).CopyTo(pixels.AsSpan(row * width, width));
                if (sheet.RecolourRegions is { } sheetRegions)
                    sheetRegions.AsSpan(from, width).CopyTo(regions.AsSpan(row * width, width));
                if (sheet.Blending is { } sheetBlending)
                    sheetBlending.AsSpan(from, width).CopyTo(blending.AsSpan(row * width, width));
            }
            return new PicturePixels(region.Size, pixels, regions, blending);
        }

        /// <summary>Whether both hold the same picture, compared pixel by pixel until the first difference.</summary>
        public bool Equals(PicturePixels? other)
        {
            if (other is null || other.size != size)
                return false;
            for (int pixel = 0; pixel < pixels.Length; pixel++)
            {
                if (pixels[pixel] != other.pixels[pixel] || regions[pixel] != other.regions[pixel] || blending[pixel] != other.blending[pixel])
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is PicturePixels other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => hash;
    }
}
