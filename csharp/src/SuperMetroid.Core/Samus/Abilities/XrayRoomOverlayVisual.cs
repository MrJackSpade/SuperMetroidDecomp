using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One authored room-specific X-ray overlay block and its visual position.</summary>
/// <param name="Block">Room position in 16-pixel blocks, before subtracting the foreground scroll.</param>
/// <param name="Appearance">The block the overlay shows and its mirroring; only vertical mirroring changes how it is shown.</param>
public readonly record struct XrayRoomOverlayVisual(Position16<RoomBlocks> Block, RoomBlockAppearance Appearance);
