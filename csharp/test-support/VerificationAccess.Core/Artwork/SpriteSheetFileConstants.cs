using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Cartridge values and helpers of <see cref="SpriteSheetFile"/> that only verification reads.</summary>
internal static class SpriteSheetFileConstants
{
    /// <summary>
    /// Installs a composed sheet as the import installs it (#1544) and cuts its frames from the installed image, as the
    /// loader cuts an installed sheet's.
    /// </summary>
    /// <param name="composed">The composed manifest: the frame list and the colours the sheet was composed against.</param>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="pixels">The image's pixels.</param>
    /// <param name="paletteIndices">Each pixel's composed palette index.</param>
    /// <param name="frontTiles">One where a pixel lies in one of its layer's front tiles, zero elsewhere.</param>
    /// <exception cref="InvalidDataException">The manifest is malformed or places a frame off the image.</exception>
    public static SpriteSheet Load(SpriteSheetDocument composed, string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, ReadOnlySpan<byte> frontTiles)
    {
        SpriteImage image;
        InstalledSheetDocument installed;
        try
        {
            image = ComposedPictures.Install(name, size, pixels, paletteIndices, frontTiles, composed, out installed);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        return SpriteSheetFile.CutFrames(installed, name, image);
    }
}
