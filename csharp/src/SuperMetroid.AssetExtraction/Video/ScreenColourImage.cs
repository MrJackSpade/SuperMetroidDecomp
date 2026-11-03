using System.Buffers.Binary;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Encodes a frontend screen's background and sprite colours into the cartridge's 256-colour order (#1464, #1512): the one
/// place an installed whole-screen palette's colour order is written. A screen's image holds its background colours where
/// gameplay's image holds the background sets, and its sprite colours where gameplay's holds the sprite sets.
/// </summary>
public static class ScreenColourImage
{
    /// <summary>The colours of a whole-screen image.</summary>
    public const int ColorCount = SnesCgram.ColorCount;

    /// <summary>
    /// The image of a screen's <paramref name="backgrounds"/> and <paramref name="sprites"/> as little-endian colour words,
    /// as installed palettes store them (#1512).
    /// </summary>
    public static byte[] Words(ReadOnlySpan<RgbColor> backgrounds, ReadOnlySpan<RgbColor> sprites) => Words(Encode(backgrounds, sprites));

    /// <summary>The colour words of <paramref name="image"/>, little-endian.</summary>
    private static byte[] Words(RgbColor[] image)
    {
        var words = new byte[image.Length * RgbColor.ByteCount];
        for (int color = 0; color < image.Length; color++)
            BinaryPrimitives.WriteUInt16LittleEndian(words.AsSpan(color * RgbColor.ByteCount), image[color].ToWord());
        return words;
    }

    /// <summary>The image of a background and a sprite owner's colours.</summary>
    private static RgbColor[] Encode(ReadOnlySpan<RgbColor> backgrounds, ReadOnlySpan<RgbColor> sprites)
    {
        var image = new RgbColor[ColorCount];
        backgrounds.CopyTo(image.AsSpan(GameplayColourPositionDefinitions.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        sprites.CopyTo(image.AsSpan(GameplayColourPositionDefinitions.SpriteSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        return image;
    }
}
