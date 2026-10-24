using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Ending;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingFontAtlas"/> members production does not use.</summary>
internal static class EndingFontAtlasAccess
{
    /// <summary>Reconstructs the ending font from exact-size planar bytes for legacy fixtures.</summary>
    extension(EndingFontCharacters)
    {
        /// <summary>Constructs the fallback cartridge-backed atlas used by focused legacy ending tests.</summary>
        internal static EndingFontCharacters FromPlanarBytes(ReadOnlySpan<byte> planar)
        {
            if (planar.Length != EndingFontAtlasFormat.ByteCount)
                throw new InvalidDataException(
                    $"Ending font contains {planar.Length} planar bytes; expected {EndingFontAtlasFormat.ByteCount}.");
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, EndingFontAtlasFormat.BitsPerPixel,
                EndingFontAtlasFormat.TilesPerRow, out _);
            return new EndingFontCharacters(pixels);
        }
    }
}
