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

    /// <summary>Read-only view of the compiled, row-major 8x8 SNES 4-bpp characters, excluding unused PNG cells.</summary>
    public ReadOnlyMemory<byte> Transfer => planar;

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
    /// <param name="vram">VRAM receiving the complete character stream.</param>
    /// <param name="destinationByteAddress">Destination offset in VRAM bytes, not the PPU's word-address units.</param>
    public void LoadTo(SnesVram vram, int destinationByteAddress)
    {
        ArgumentNullException.ThrowIfNull(vram);
        vram.LoadBytes(destinationByteAddress, planar);
    }
}

/// <summary>Host-file geometry for the CRE and graphics-set 4-bpp character streams.</summary>
public static class RoomCharacterAtlasFormat
{
    /// <summary>Maximum number of 8x8 character cells per PNG row; shorter streams use only their tile count as the width.</summary>
    public const int TileColumns = 32;
    /// <summary>Bytes occupied by one 8x8 SNES character with four bitplanes.</summary>
    public const int BytesPerTile = 32;
    /// <summary>Largest accepted compiled stream: $8000 bytes, or 1024 complete 4-bpp characters.</summary>
    public const int MaximumNativeBytes = 0x8000;
    /// <summary>Editable PNG filename for the common room elements (CRE) character stream shared by room graphics sets.</summary>
    public const string CreFileName = "room-characters-cre.png";

    /// <summary>Names an atlas by its immutable cartridge source so shared graphics sets share one PNG.</summary>
    /// <param name="sourceAddress">Native character-stream source identity, formatted as uppercase hexadecimal with at least six digits.</param>
    /// <returns>The source-specific editable PNG filename.</returns>
    public static string SourceFileName(int sourceAddress) => $"room-characters-{sourceAddress:X6}.png";

    /// <summary>Validates a positive, tile-aligned stream of at most $8000 bytes and returns its 8x8 character count.</summary>
    /// <param name="nativeByteCount">Compiled transfer length in bytes.</param>
    /// <returns>The number of complete 32-byte characters, from 1 through 1024.</returns>
    /// <exception cref="InvalidDataException">The length is zero, negative, exceeds the maximum, or contains a partial character.</exception>
    public static int ValidateTileCount(int nativeByteCount)
    {
        if (nativeByteCount <= 0 || nativeByteCount > MaximumNativeBytes ||
            nativeByteCount % BytesPerTile != 0)
            throw new InvalidDataException(
                $"Room characters require 1..${MaximumNativeBytes:X} complete 4-bpp bytes; received ${nativeByteCount:X}.");
        return nativeByteCount / BytesPerTile;
    }
}
