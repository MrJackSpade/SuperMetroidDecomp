using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The state of one permanent-item room object the room holds.</summary>
/// <param name="Block">The room block the item lies at.</param>
/// <param name="Location">The item location the pickup records as collected.</param>
/// <param name="Kind">The permanent item the room object holds.</param>
public readonly record struct CollectibleObjectSnapshot(
    Position16<RoomBlocks> Block,
    ItemLocation Location,
    InWorldCollectibleKind Kind);
