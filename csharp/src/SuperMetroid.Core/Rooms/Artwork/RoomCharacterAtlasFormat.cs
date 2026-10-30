namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Host-file geometry for the CRE and graphics-set 4-bpp character streams.</summary>
public static class RoomCharacterAtlasFormat
{
    /// <summary>Maximum number of 8x8 character cells per PNG row; shorter streams use only their tile count as the width.</summary>
    public const int TileColumns = 32;
    /// <summary>Bytes occupied by one 8x8 SNES character with four bitplanes.</summary>
    public const int BytesPerTile = 32;
    /// <summary>Largest accepted compiled stream: $8000 bytes, or 1024 complete 4-bpp characters.</summary>
    public const int MaximumByteCount = 32768;

    /// <summary>Validates a positive, tile-aligned stream of at most $8000 bytes and returns its 8x8 character count.</summary>
    /// <param name="byteCount">Compiled transfer length in bytes.</param>
    /// <returns>The number of complete 32-byte characters, from 1 through 1024.</returns>
    /// <exception cref="InvalidDataException">The length is zero, negative, exceeds the maximum, or contains a partial character.</exception>
    public static int ValidateTileCount(int byteCount)
    {
        if (byteCount <= 0 || byteCount > MaximumByteCount ||
            byteCount % BytesPerTile != 0)
            throw new InvalidDataException(
                $"Room characters require 1..${MaximumByteCount:X} complete 4-bpp bytes; received ${byteCount:X}.");
        return byteCount / BytesPerTile;
    }
}
