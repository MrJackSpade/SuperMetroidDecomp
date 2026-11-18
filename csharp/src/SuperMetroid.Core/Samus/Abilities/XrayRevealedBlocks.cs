namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// What the X-ray reveals over the screen (#1431, #1514): the room's blocks the reveal shows by place, from the block the
/// screen's top-left corner lies in, and where that first block lies in the foreground, so each block shows where the
/// room's own block lies.
/// </summary>
/// <param name="Origin">The foreground pixel the first block's top-left corner lies on.</param>
/// <param name="Grid">The blocks by place; null where a block lies beyond the room.</param>
public readonly record struct XrayRevealedBlocks(Position16<RoomPixels> Origin, XrayRevealGrid Grid);
