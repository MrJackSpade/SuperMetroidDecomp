using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Reads an installed room-character PNG into the planar characters its native stream holds (#1409).</summary>
public static class RoomCharacterAtlasPng
{
    /// <summary>Loads exactly the number of characters in the corresponding native stream.</summary>
    /// <param name="png">Indexed artwork with indices 0..15; unused cells in the final tile row must contain only index zero.</param>
    /// <param name="nativeByteCount">Required transfer length in bytes, comprising complete 32-byte characters.</param>
    /// <returns>The compiled character stream; PNG palette colors are not used to select runtime colors.</returns>
    /// <exception cref="InvalidDataException">The PNG is malformed, the wrong size, or holds artwork in unused cells.</exception>
    public static RoomCharacterAtlas Read(Stream png, int nativeByteCount)
    {
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(nativeByteCount);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(png, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));

        // The last PNG row may contain unused display cells. Reject edits there rather
        // than silently throwing them away when compiling only native characters.
        for (int tile = tileCount; tile < columns * rows; tile++)
        {
            int x = (tile % columns) * 8;
            int y = (tile / columns) * 8;
            for (int row = 0; row < 8; row++)
            for (int column = 0; column < 8; column++)
                if (image.Pixels[(y + row) * image.Size.X + x + column] != 0)
                    throw new InvalidDataException($"Room atlas has artwork in unused tile {tile}.");
        }

        byte[] encoded = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4);
        return new(encoded.AsSpan(0, nativeByteCount).ToArray());
    }
}
