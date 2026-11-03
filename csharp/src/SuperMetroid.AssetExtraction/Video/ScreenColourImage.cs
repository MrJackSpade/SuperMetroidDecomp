using System.Buffers.Binary;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Loads a cartridge-ordered 256-colour image into a frontend screen's colour sets, and encodes sets back into that
/// order (#1464): the one place an installed whole-screen palette's colour order is read. A screen's image holds its
/// background sets where gameplay's image holds the background sets, and its sprite sets where gameplay's holds the
/// sprite sets.
/// </summary>
public static class ScreenColourImage
{
    /// <summary>The colours of a whole-screen image.</summary>
    public const int ColorCount = SnesCgram.ColorCount;

    /// <summary>Replaces every colour of <paramref name="screen"/> with those <paramref name="image"/>, a whole 256-colour image, holds.</summary>
    /// <exception cref="ArgumentException"><paramref name="image"/> does not hold 256 colours.</exception>
    public static void LoadTo(ReadOnlySpan<RgbColor> image, ScreenPalette screen)
    {
        Ensure.LengthEqual(image, ColorCount);
        Ensure.NotNull(screen);
        screen.BackgroundSets.All.CopyFrom(
            image.Slice(GameplayColourPositionDefinitions.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        screen.SpriteSets.All.CopyFrom(
            image.Slice(GameplayColourPositionDefinitions.SpriteSetsFirst, GameplayColourPositionDefinitions.SetsColours));
    }

    /// <summary>The 256-colour image of <paramref name="screen"/>'s owners as they are now.</summary>
    public static RgbColor[] Encode(ScreenPalette screen)
    {
        Ensure.NotNull(screen);
        return Encode(screen.BackgroundSets.All.Colors, screen.SpriteSets.All.Colors);
    }

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
