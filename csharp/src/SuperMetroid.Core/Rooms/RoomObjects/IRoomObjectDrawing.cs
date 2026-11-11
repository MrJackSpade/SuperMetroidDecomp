using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Turns room-object draw lists into level changes and background picture updates, as room-level families draw (#1395).</summary>
internal interface IRoomObjectDrawing
{
    /// <summary>Draws <paramref name="draw"/> with its block origin at <paramref name="origin"/>, with its family's installed visuals and its compiled collision.</summary>
    void Draw(RoomLevelData level, Position16<RoomBlocks> origin, RoomObjectDraw draw);

    /// <summary>
    /// What <paramref name="block"/>, a block a setup or a reaction writes of its own accord, shows: its installed picture
    /// (#1580). The writer gives it its collision.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed setup-block pictures are not bound, or lack the block.</exception>
    RoomBlockAppearance SetupBlock(RoomObjectSetupBlock block);
}
