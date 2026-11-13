namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Detached diagnostic identity of one live resident scroll trigger; it does not expose mutable slot or scroll-change state.</summary>
/// <param name="Block">The trigger's room block.</param>
public readonly record struct ScrollObjectSnapshot(
    Position16<RoomBlocks> Block);
