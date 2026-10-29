using System.Buffers.Binary;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Decodes a cartridge-ordered 256-colour image into a frontend screen's colour owners, and encodes owners back into that
/// order (#1464): the one place an installed whole-screen palette's colour order is read. A screen's image holds its
/// background sets where gameplay's image holds the background sets, and its sprite sets where gameplay's holds the
/// sprite sets.
/// </summary>
public static class ScreenColourImage
{
    /// <summary>The colours of a whole-screen image.</summary>
    public const int ColorCount = SnesCgram.ColorCount;

    /// <summary>The screen colours, by owner, that <paramref name="image"/>, a whole 256-colour image, holds.</summary>
    /// <exception cref="ArgumentException"><paramref name="image"/> does not hold 256 colours.</exception>
    public static ScreenColours Decode(ReadOnlySpan<RgbColor> image)
    {
        Ensure.LengthEqual(image, ColorCount);
        return new ScreenColours(
            image.Slice(GameplayColourPositionDefinitions.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours),
            image.Slice(GameplayColourPositionDefinitions.SpriteSetsFirst, GameplayColourPositionDefinitions.SetsColours));
    }

    /// <summary>The 256-colour image of <paramref name="colours"/>' owners.</summary>
    public static RgbColor[] Encode(ScreenColours colours)
    {
        Ensure.NotNull(colours);
        return Encode(colours.Backgrounds, colours.Sprites);
    }

    /// <summary>The 256-colour image of <paramref name="screen"/>'s owners as they are now.</summary>
    public static RgbColor[] Encode(ScreenPalette screen)
    {
        Ensure.NotNull(screen);
        return Encode(screen.BackgroundSets.All.Colors, screen.SpriteSets.All.Colors);
    }

    /// <summary>The image of <paramref name="colours"/> as little-endian colour words, as installed palettes store them.</summary>
    public static byte[] Words(ScreenColours colours)
    {
        RgbColor[] image = Encode(colours);
        var words = new byte[image.Length * RgbColor.ByteCount];
        for (int color = 0; color < image.Length; color++)
            BinaryPrimitives.WriteUInt16LittleEndian(words.AsSpan(color * RgbColor.ByteCount), image[color].ToWord());
        return words;
    }

    /// <summary>The image position of colour <paramref name="colour"/> of the sprite sets, set after set.</summary>
    public static int SpritePosition(int colour) =>
        GameplayColourPositionDefinitions.SpriteSetsFirst + Ensure.BetweenInclusive(0, GameplayColourPositionDefinitions.SetsColours - 1, colour);

    /// <summary>The image position of colour <paramref name="colour"/> of the background sets, set after set.</summary>
    public static int BackgroundPosition(int colour) =>
        GameplayColourPositionDefinitions.BackgroundSetsFirst + Ensure.BetweenInclusive(0, GameplayColourPositionDefinitions.SetsColours - 1, colour);

    /// <summary>The image of a background and a sprite owner's colours.</summary>
    private static RgbColor[] Encode(ReadOnlySpan<RgbColor> backgrounds, ReadOnlySpan<RgbColor> sprites)
    {
        var image = new RgbColor[ColorCount];
        backgrounds.CopyTo(image.AsSpan(GameplayColourPositionDefinitions.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        sprites.CopyTo(image.AsSpan(GameplayColourPositionDefinitions.SpriteSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        return image;
    }
}
