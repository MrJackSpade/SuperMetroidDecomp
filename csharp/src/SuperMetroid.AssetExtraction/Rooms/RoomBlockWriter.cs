namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Something that writes blocks into a room while it runs (#1636): a room object's frames, a block reaction's setup, the
/// X-ray reveal's blocks; with the room states it can run in, whose tilesets' block sheets must picture what it writes.
/// </summary>
/// <param name="Blocks">Every block it can write, by its index among those its room loads.</param>
/// <param name="RunsIn">Whether it can write its blocks while a room state runs.</param>
internal sealed record RoomBlockWriter(IReadOnlySet<int> Blocks, Func<InventoriedRoomState, bool> RunsIn);
