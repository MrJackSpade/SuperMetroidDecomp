using System.Buffers.Binary;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Verification's encoding of a frontend screen's background and sprite colours into the cartridge's 256-colour order as
/// colour words (#1464, #1512), to compare with the words a cartridge resource holds. The game reads no palette document
/// (#1544), so only checks still lay a screen's colours out this way.
/// </summary>
internal static class ScreenColourWords
{
    /// <summary>The image of a screen's <paramref name="backgrounds"/> and <paramref name="sprites"/> as little-endian colour words.</summary>
    internal static byte[] Words(ReadOnlySpan<RgbColor> backgrounds, ReadOnlySpan<RgbColor> sprites)
    {
        var image = new RgbColor[ScreenColourImage.ColorCount];
        backgrounds.CopyTo(image.AsSpan(GameplayColourPositionDefinitions.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        sprites.CopyTo(image.AsSpan(GameplayColourPositionDefinitions.SpriteSetsFirst, GameplayColourPositionDefinitions.SetsColours));
        var words = new byte[image.Length * ColourWordCodec.ByteCount];
        for (int color = 0; color < image.Length; color++)
            BinaryPrimitives.WriteUInt16LittleEndian(words.AsSpan(color * ColourWordCodec.ByteCount), image[color].ToWord());
        return words;
    }
}
