using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One room-character stream compiled from editable, palette-indexed 8x8 artwork.
/// The room's palette and metatile definitions assign the visible colors and placement.
/// </summary>
public sealed class RoomCharacterAtlas
{
    /// <summary>Owned planar 4-bpp transfer bytes for the authored room characters.</summary>
    internal readonly byte[] planar;

    /// <summary>Retains the newly compiled planar bytes without exposing a mutable document buffer.</summary>
    private RoomCharacterAtlas(byte[] planar) => this.planar = planar;

    /// <summary>Read-only view of the compiled, row-major 8x8 SNES 4-bpp characters, excluding unused PNG cells.</summary>
    public ReadOnlyMemory<byte> Characters => planar;

    /// <summary>Identity of selected decoded pixels; PNG metadata and display padding do not participate.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomCharacterAtlas),
        content => content.Append("characters", planar));

    /// <summary>Loads exactly the number of characters in the corresponding native stream.</summary>
    /// <param name="png">Indexed artwork with indices 0..15; unused cells in the final tile row must contain only index zero.</param>
    /// <param name="nativeByteCount">Required transfer length in bytes, comprising complete 32-byte characters.</param>
    /// <returns>The compiled character stream; PNG palette colors are not used to select runtime colors.</returns>
    public static RoomCharacterAtlas Load(Stream png, int nativeByteCount)
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
