namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Schema revision, native block geometry, logical character coordinates, and editable file identities for room metatile tables.</summary>
public static class RoomMetatileFormat
{
    /// <summary>Supported JSON schema revision for editable room block composition.</summary>
    public const int Version = 1;
    /// <summary>Eight bytes per native 16x16 block: four little-endian 16-bit BG tilemap words.</summary>
    public const int BytesPerBlock = RoomAssetRomData.GraphicsLayout.BytesPerBlockDefinition;
    /// <summary>Maximum accepted definitions in one atlas, limiting its compiled native table to $2000 bytes.</summary>
    public const int MaximumBlockCount = 1024;
    /// <summary>Logical character-catalog columns used to split or compose the tilemap word's ten-bit character index.</summary>
    public const int TileColumns = 32;
    /// <summary>Logical character-catalog rows; together with 32 columns these address all 1024 BG character indices.</summary>
    public const int TileRows = 32;
    /// <summary>Eight background palette selections available in a SNES BG tilemap word.</summary>
    public const int PaletteCount = 8;
    /// <summary>Editable common-room-element block filename, corresponding to native TileTable_CRE at $B9:A09D rather than a tileset-specific table.</summary>
    public const string CreFileName = "room-blocks-cre.json";

    /// <summary>The editable JSON file name of a block table; graphics sets that share a table share one file.</summary>
    public static string FileName(SuperMetroid.Core.Rooms.LevelData.RoomBlockSet set) =>
        $"room-blocks-{SuperMetroid.Core.Assets.InstalledArtworkNames.Hyphenated(set.ToString())}.json";

    /// <summary>Validates that a native table length contains 1..1024 complete eight-byte definitions and returns its block count.</summary>
    /// <param name="byteCount">Decompressed native table length in bytes.</param>
    /// <returns>The number of complete four-quadrant block definitions.</returns>
    /// <exception cref="InvalidDataException">The length is nonpositive, exceeds $2000 bytes, or is not divisible by eight.</exception>
    public static int ValidateBlockCount(int byteCount)
    {
        if (byteCount <= 0 || byteCount > MaximumBlockCount * BytesPerBlock ||
            byteCount % BytesPerBlock != 0)
            throw new InvalidDataException(
                $"Room metatiles require 1..{MaximumBlockCount} complete eight-byte blocks; got {byteCount} bytes.");
        return byteCount / BytesPerBlock;
    }
}
