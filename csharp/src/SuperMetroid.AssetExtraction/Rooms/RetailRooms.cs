using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Every retail room, which the import walks to picture, upload and lay out each room's states (#1502). The game only
/// enters the room a door names.
/// </summary>
internal static class RetailRooms
{
    /// <summary>Every retail room, in authored order.</summary>
    internal static IReadOnlyList<RoomDefinition> All => RoomDefinitions.Retail;
}
