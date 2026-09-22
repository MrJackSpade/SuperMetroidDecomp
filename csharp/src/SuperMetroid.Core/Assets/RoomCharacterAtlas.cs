using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One room-character stream compiled from editable, palette-indexed 8x8 artwork.
/// The room's palette and metatile definitions assign the visible colors and placement.
/// </summary>
public sealed class RoomCharacterAtlas
{
    private readonly byte[] planar;

    private RoomCharacterAtlas(byte[] planar) => this.planar = planar;

    public ReadOnlyMemory<byte> Transfer => planar;

    /// <summary>Loads exactly the number of characters in the corresponding native stream.</summary>
    public static RoomCharacterAtlas Load(Stream png, int nativeByteCount)
    {
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(nativeByteCount);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(png, columns * 8, rows * 8);

        // The last PNG row may contain unused display cells. Reject edits there rather
        // than silently throwing them away when compiling only native characters.
        for (int tile = tileCount; tile < columns * rows; tile++)
        {
            int x = (tile % columns) * 8;
            int y = (tile / columns) * 8;
            for (int row = 0; row < 8; row++)
            for (int column = 0; column < 8; column++)
                if (image.Pixels[(y + row) * image.Width + x + column] != 0)
                    throw new InvalidDataException($"Room atlas has artwork in unused tile {tile}.");
        }

        byte[] encoded = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4);
        return new(encoded.AsSpan(0, nativeByteCount).ToArray());
    }

    /// <summary>Uploads the compiled characters without exposing mutable backing bytes.</summary>
    public void LoadTo(SnesVram vram, int destinationByteAddress)
    {
        ArgumentNullException.ThrowIfNull(vram);
        vram.LoadBytes(destinationByteAddress, planar);
    }
}

/// <summary>Host-file geometry for the CRE and graphics-set 4-bpp character streams.</summary>
public static class RoomCharacterAtlasFormat
{
    public const int TileColumns = 32;
    public const int BytesPerTile = 32;
    public const int MaximumNativeBytes = 0x8000;
    public const string CreFileName = "room-characters-cre.png";

    /// <summary>Names an atlas by its immutable cartridge source so shared graphics sets share one PNG.</summary>
    public static string SourceFileName(int sourceAddress) => $"room-characters-{sourceAddress:X6}.png";

    public static int ValidateTileCount(int nativeByteCount)
    {
        if (nativeByteCount <= 0 || nativeByteCount > MaximumNativeBytes ||
            nativeByteCount % BytesPerTile != 0)
            throw new InvalidDataException(
                $"Room characters require 1..${MaximumNativeBytes:X} complete 4-bpp bytes; received ${nativeByteCount:X}.");
        return nativeByteCount / BytesPerTile;
    }
}
