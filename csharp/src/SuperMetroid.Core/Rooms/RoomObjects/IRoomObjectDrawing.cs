using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's installed room-object pictures (#1395, #1557), which each family's drawing draws its frames with
/// (<see cref="RoomObjectDrawFamily{TFrame}"/>, #2034), and the pictures of the blocks setups write of their own accord.
/// </summary>
internal interface IRoomObjectDrawing
{
    /// <summary>The installed pictures of type <typeparamref name="TCatalog"/>, one family's catalog.</summary>
    /// <typeparam name="TCatalog">The catalog's type.</typeparam>
    TCatalog Installed<TCatalog>() where TCatalog : class;

    /// <summary>
    /// What <paramref name="block"/>, a block a setup or a reaction writes of its own accord, shows: its installed picture
    /// (#1580). The writer gives it its collision.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed setup-block pictures are not bound, or lack the block.</exception>
    RoomBlockAppearance SetupBlock(RoomObjectSetupBlock block);
}
