using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>What each scroll-extension room object writes at its block: the extension collision and the step back toward the trigger it extends.</summary>
internal static class ScrollExtensionDefinitions
{
    /// <summary>Each scroll extension's collision and the signed block step toward its trigger.</summary>
    internal static readonly IReadOnlyDictionary<RoomObjectHeaderId, ScrollExtension> Extensions = new Dictionary<RoomObjectHeaderId, ScrollExtension>
    {
        [RoomObjectHeaderId.RightwardsScrollExtension] = new(RoomCollisionType.HorizontalExtension, -1),
        [RoomObjectHeaderId.LeftwardsScrollExtension] = new(RoomCollisionType.HorizontalExtension, 1),
        [RoomObjectHeaderId.DownwardsScrollExtension] = new(RoomCollisionType.VerticalExtension, -1),
        [RoomObjectHeaderId.UpwardsScrollExtension] = new(RoomCollisionType.VerticalExtension, 1),
    };
}
