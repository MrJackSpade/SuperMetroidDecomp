using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>What each scroll-extension room object writes at its block: the extension collision and the step back toward the trigger it extends.</summary>
internal static class ScrollExtensionDefinitions
{
    /// <summary>Each scroll extension's collision and the signed block step toward its trigger.</summary>
    internal static readonly IReadOnlyDictionary<RoomObjectKind, ScrollExtension> Extensions = new Dictionary<RoomObjectKind, ScrollExtension>
    {
        [RoomObjectKind.RightwardsScrollExtension] = new(RoomCollisionType.HorizontalExtension, -1),
        [RoomObjectKind.LeftwardsScrollExtension] = new(RoomCollisionType.HorizontalExtension, 1),
        [RoomObjectKind.DownwardsScrollExtension] = new(RoomCollisionType.VerticalExtension, -1),
        [RoomObjectKind.UpwardsScrollExtension] = new(RoomCollisionType.VerticalExtension, 1),
    };
}
