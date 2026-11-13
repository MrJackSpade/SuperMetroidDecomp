namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One Speed Booster block selected by the special-block tables when Samus touches Speed Booster terrain: its kind
/// and its crumble.
/// </summary>
/// <param name="Kind">The block's room-object kind.</param>
/// <param name="Change">The block's crumble.</param>
internal readonly record struct SpeedBoosterBlockObjectDefinition(
    RoomObjectKind Kind,
    BlockReactionChange Change);
