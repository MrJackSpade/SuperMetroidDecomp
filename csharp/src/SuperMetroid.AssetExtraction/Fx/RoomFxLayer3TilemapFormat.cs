
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>The room effects' 32x33 layer-three page geometry and the installed file holding the pages.</summary>
public static class RoomFxLayer3TilemapFormat
{
    /// <summary>Shared editable JSON filename containing all six room-FX BG3 page definitions.</summary>
    public const string FileName = "room-fx-layer3-tilemaps.json";
    /// <summary>Required revision of the tile-reference document schema, independent of the native FX type identifiers.</summary>
    public const int Version = 1;
    /// <summary>Destination cells per transfer row; cell index modulo 32 selects its map column, not its character-sheet column.</summary>
    public const int WidthInTiles = 32;
    /// <summary>Rows retained from each native transfer, including its thirty-third row; these resources exceed a standard 32-by-32 BG page.</summary>
    public const int HeightInTiles = 33;
    /// <summary>Required editable cells per FX resource, 1056, preserving the complete native 32-by-33 transfer.</summary>
    public const int CellsPerPage = WidthInTiles * HeightInTiles;
    /// <summary>Compiled transfer length, $0840 bytes, with one little-endian sixteen-bit SNES BG tilemap word per cell.</summary>
    public const int PageByteCount = CellsPerPage * sizeof(ushort);

    /// <summary>The six effects that show a page, in page order: Lava, Acid, Water, Spores, Rain and Fog.</summary>
    public static IReadOnlyList<RoomFxType> Types { get; } =
        [RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water, RoomFxType.Spores, RoomFxType.Rain, RoomFxType.Fog];
}
