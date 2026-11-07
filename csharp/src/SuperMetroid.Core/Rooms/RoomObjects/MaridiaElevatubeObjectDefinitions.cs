using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The draw of the hardcoded elevatube PLM spawned by Maridia's door setup.
/// </summary>
internal static class MaridiaElevatubeObjectDefinitions
{
    /// <summary>The elevatube's one draw.</summary>
    internal enum Draw
    {
        /// <summary>Places the elevatube's solid block.</summary>
        Block,
    }

    /// <summary>The draw that places the elevatube's solid block.</summary>
    internal static RoomObjectFrameDraw<Draw> BlockDraw { get; } = RoomObjectDraw.Of(Draw.Block);

    /// <summary>The elevatube's block: a solid block showing block $180, the block Kraid's crumble draw also shows.</summary>
    internal static RoomTerrainBlock Terrain { get; } = RoomTerrainBlock.Of(384, BlockMirroring.None, RoomCollisionType.SolidBlock);
}
