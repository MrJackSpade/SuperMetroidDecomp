using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One authored room-specific X-ray overlay block and its visual position.</summary>
/// <param name="Block">Room position in 16-pixel blocks, before subtracting the foreground scroll.</param>
/// <param name="Appearance">The block the overlay shows and its mirroring; the block shows mirrored as its appearance mirrors it.</param>
public readonly partial record struct XrayRoomOverlayVisual(Position16<RoomBlocks> Block, RoomBlockAppearance Appearance);
