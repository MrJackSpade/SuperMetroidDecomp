namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The state of one occupied permanent-item PLM slot.</summary>
/// <param name="BlockIndex">The item owner's foreground block index in room level data.</param>
/// <param name="ItemBit">The collected-item bit the pickup sets, or null for an item placed without one.</param>
/// <param name="Kind">The permanent item kind decoded from the header's position in its presentation table.</param>
public readonly record struct CollectibleObjectSnapshot(
    int BlockIndex,
    int? ItemBit,
    InWorldCollectibleKind Kind);
