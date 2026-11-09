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

    /// <summary>How the elevatube's block collides: solid. What it shows is its installed picture (#1557).</summary>
    internal const RoomCollisionType Collision = RoomCollisionType.SolidBlock;
}
