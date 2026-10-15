namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One authored room-specific X-ray overlay tile and its visual position.</summary>
/// <param name="Block">Room position in 16-pixel blocks, before subtracting layer-one scroll.</param>
/// <param name="Word">Twelve-bit visual metatile reference expanded into the X-ray BG tilemap, not a collision/BTS word.</param>
public readonly record struct XrayRoomOverlayVisual(Position16<RoomBlocks> Block, ushort Word);
