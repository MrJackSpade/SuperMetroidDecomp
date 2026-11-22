using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The draw of the elevatube room object spawned by Maridia's door setup.
/// </summary>
internal static partial class MaridiaElevatubeObjectDefinitions
{
    /// <summary>The elevatube's one draw.</summary>
    internal enum Draw
    {
        /// <summary>Places the elevatube's solid block.</summary>
        Block,
    }

    /// <summary>The draw that places the elevatube's solid block.</summary>
    internal static RoomObjectFrameDraw<Draw> BlockDraw { get; } = MaridiaElevatubeObjectDraws.Family.Of(Draw.Block);

    /// <summary>The elevatube's block (#1862): one solid block at the object's block. What it shows is its installed picture (#1557).</summary>
    internal static RoomBlockShape Shape { get; } = RoomBlockShapes.Block(RoomCollisionType.SolidBlock);
}
