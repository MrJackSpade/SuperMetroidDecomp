namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One Speed Booster block selected by the special-block tables when Samus touches Speed Booster terrain: its identity
/// and its crumble.
/// </summary>
/// <param name="Header">The block's room-object identity.</param>
/// <param name="Change">The block's crumble.</param>
internal readonly record struct SpeedBoosterBlockObjectDefinition(
    RoomObjectHeaderId Header,
    BlockReactionChange Change);
