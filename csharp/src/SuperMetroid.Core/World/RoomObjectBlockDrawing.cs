using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's installed room-object pictures, which each family's drawing draws its frames with (#2034), and the pictures
/// of the blocks setups write of their own accord.
/// </summary>
/// <param name="visuals">The host's installed room-object artwork, which outlives each room's drawing.</param>
internal sealed class RoomObjectBlockDrawing(RoomObjectVisuals visuals) : IRoomObjectDrawing
{
    /// <summary>The host's installed room-object artwork.</summary>
    private readonly RoomObjectVisuals _visuals = Ensure.NotNull(visuals);

    /// <inheritdoc/>
    public TCatalog Installed<TCatalog>() where TCatalog : class => _visuals.Catalog<TCatalog>();

    /// <inheritdoc/>
    public RoomBlockAppearance SetupBlock(RoomObjectSetupBlock block) =>
        Installed<InstalledRoomObjectSetupBlocks>().Shown(block);
}
