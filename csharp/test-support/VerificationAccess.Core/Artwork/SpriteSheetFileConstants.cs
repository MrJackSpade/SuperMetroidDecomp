using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Cartridge values and helpers of <see cref="SpriteSheetFile"/> that only verification reads.</summary>
internal static class SpriteSheetFileConstants
{
    /// <summary>Reads a sheet manifest and cuts its frames from the sheet image.</summary>
    /// <param name="manifest">The frame list and standard colours.</param>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="pixels">The image's pixels.</param>
    /// <param name="paletteIndices">Each pixel's colour group's place times sixteen plus its colour.</param>
    /// <param name="frontTiles">One where a pixel lies in one of its layer's front tiles, zero elsewhere.</param>
    /// <exception cref="InvalidDataException">The manifest is malformed, of another revision, or places a frame off the image.</exception>
    public static SpriteSheet Load(Stream manifest, string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, ReadOnlySpan<byte> frontTiles)
    {
        SpriteSheetDocument document = SpriteSheetFile.ReadDocument(manifest, name);
        SpriteImage image;
        try
        {
            image = SpriteImageFixtures.Copied(name, size, pixels, paletteIndices, SpriteSheetFile.PaletteOf(document), frontTiles);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        InstalledPictureColours.Record(image, [.. document.BackgroundSets ?? []], document.PaletteBits);
        return SpriteSheetFile.CutFrames(document, name, image);
    }
}
