using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>How the blocks of a plain terrain the opening builds collide, read from the terrain production builds of them (#2553).</summary>
internal static class PlainTerrainCollision
{
    /// <summary>How each of <paramref name="blocks"/> collides, row by row, in the plain terrain of <paramref name="size"/> built of them.</summary>
    internal static RoomCollisionType[] Of(Extent16<RoomBlocks> size, PlainTerrainBlock[] blocks)
    {
        RoomLevelData level = new PlainTerrain(size, blocks).Level;
        return [.. Enumerable.Range(0, blocks.Length).Select(block => level.GetCollisionBlock(size.CellAtRowMajorIndex(block)).CollisionType)];
    }
}
