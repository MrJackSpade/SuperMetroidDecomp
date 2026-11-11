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

    /// <summary>Admits every picture region of a sheet about to be rendered.</summary>
    /// <param name="sheet">Names the sheet in a refusal.</param>
    /// <param name="planes">The sheet's composed pixels.</param>
    /// <param name="manifest">The sheet's frames, each naming its region.</param>
    /// <exception cref="InvalidDataException">A region's pixels match a picture this installation already rendered.</exception>
    internal void Admit(string sheet, SpriteSheetPlanes planes, SpriteSheetDocument manifest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sheet);
        Ensure.NotNull(manifest);
        foreach (IGrouping<PictureRegion, string> region in manifest.Frames
            .GroupBy(frame => new PictureRegion(frame.Value.Corner, frame.Value.Size), frame => frame.Key)
            .OrderBy(region => region.Key.Corner.Y).ThenBy(region => region.Key.Corner.X))
        {
            if (region.Key.Size.X == 0 || region.Key.Size.Y == 0)
                continue;
            var picture = new RenderedPicture(sheet, string.Join(", ", region.Order(StringComparer.Ordinal)), region.Key);
            PicturePixels pixels = PicturePixels.Copy(planes, region.Key);
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

    /// <summary>One picture's place on its sheet.</summary>
    /// <param name="Corner">The region's top-left pixel.</param>
    /// <param name="Size">The region's width and height.</param>
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

        /// <summary>A cheap hash of the size and pixels.</summary>
        private readonly int hash;

        /// <summary>Keeps a picture's size, pixels and hash.</summary>
        private PicturePixels(Extent16<ImagePixels> size, RgbaPixel[] pixels)
        {
            this.size = size;
            this.pixels = pixels;
            var code = new HashCode();
            code.Add(size);
            code.AddBytes(MemoryMarshal.AsBytes(pixels.AsSpan()));
            hash = code.ToHashCode();
        }

        /// <summary>Copies a region's pixels out of a sheet's planes.</summary>
        /// <exception cref="InvalidDataException">The region reaches outside the sheet.</exception>
        internal static PicturePixels Copy(SpriteSheetPlanes planes, PictureRegion region)
        {
            int left = region.Corner.X;
            int top = region.Corner.Y;
            int width = region.Size.X;
            int height = region.Size.Y;
            if (left + width > planes.Size.X || top + height > planes.Size.Y)
                throw new InvalidDataException($"Frame region {region} reaches outside its {planes.Size.X}x{planes.Size.Y} sheet.");
            var copy = new RgbaPixel[width * height];
            for (int row = 0; row < height; row++)
                planes.Pixels.Slice(((top + row) * planes.Size.X) + left, width).CopyTo(copy.AsSpan(row * width, width));
            return new PicturePixels(region.Size, copy);
        }

        /// <inheritdoc />
        public bool Equals(PicturePixels? other)
        {
            if (other is null || other.size != size)
                return false;
            for (int pixel = 0; pixel < pixels.Length; pixel++)
            {
                if (pixels[pixel] != other.pixels[pixel])
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is PicturePixels other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => hash;
    }
}
