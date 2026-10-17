using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One authored room-specific X-ray overlay tile and its visual position.</summary>
/// <param name="Block">Room position in 16-pixel blocks, before subtracting layer-one scroll.</param>
/// <param name="Metatile">Visual metatile reference expanded into the X-ray BG tilemap: the level word's visual block index and flip flags, not a collision/BTS word.</param>
public readonly record struct XrayRoomOverlayVisual(Position16<RoomBlocks> Block, RoomLevelWord Metatile);
