using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The terrain writes PLM families share: drawing a block and replacing its collision kind and behaviour.</summary>
internal static class PlmLevelWrites
{
    /// <summary>
    /// Writes <paramref name="terrain"/> at a block; the room shows <paramref name="shown"/> there when a family's
    /// installed artwork replaces the block's own appearance.
    /// </summary>
    internal static void DrawTerrain(RoomLevelData level, int blockIndex, RoomTerrainBlock terrain, RoomBlockAppearance? shown = null) =>
        level.SetPlmTerrain(blockIndex, terrain, shown ?? terrain.Appearance);

    /// <summary>
    /// Replaces a block's collision kind and behaviour, keeping its appearance, as a PLM converts the block it sits on.
    /// </summary>
    internal static void WritePlmCollisionAndBehavior(RoomLevelData level, int blockIndex, RoomCollisionType collision, RoomBlockBehavior behavior)
    {
        RoomTerrainBlock original = level.GetPlmCollisionBlockByIndex(blockIndex).Terrain;
        level.SetPlmTerrain(blockIndex, original.WithCollision(collision));
        level.SetPlmBehavior(blockIndex, behavior);
    }
}
