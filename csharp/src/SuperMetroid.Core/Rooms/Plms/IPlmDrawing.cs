using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Turns PLM draw lists into level changes and BG tilemap updates, as room-level families draw (#1395).</summary>
internal interface IPlmDrawing
{
    /// <summary>Nonserialized item tile and palette art; physical pickup rules stay compiled.</summary>
    RoomPlmDynamicCollectibleArtCatalog? DynamicCollectibleArt { get; }

    /// <summary>Draws a PLM without header-specific visual ownership using its block origin and instruction pointer.</summary>
    void DrawPlmInstruction(RoomLevelData level, int blockIndex, ushort drawPointer);

    /// <summary>Resolves a compiled or cartridge draw list, selecting installed visuals by PLM owner while retaining physical collision words.</summary>
    void DrawPlmInstruction(RoomLevelData level, PlmHeaderId headerPointer, int blockIndex, ushort drawPointer);
}
