using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The state of one occupied permanent-item room-object slot.</summary>
/// <param name="Block">The room block the item lies at.</param>
/// <param name="Location">The item location the pickup records as collected, or null for an item placed without one.</param>
/// <param name="Kind">The permanent item kind decoded from its definition's position in its presentation table.</param>
public readonly record struct CollectibleObjectSnapshot(
    Position16<RoomBlocks> Block,
    ItemLocation? Location,
    InWorldCollectibleKind Kind);
