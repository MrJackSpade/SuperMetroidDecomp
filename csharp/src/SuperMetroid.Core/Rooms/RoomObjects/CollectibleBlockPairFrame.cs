namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A block pair's frame under an item's own picture, with the terrain it writes (#2554).</summary>
/// <param name="Draw">The frame.</param>
/// <param name="Terrain">The terrain the frame writes: its pair's block for its look.</param>
internal sealed record CollectibleBlockPairFrame(CollectibleDraw Draw, RoomTerrainBlock Terrain);
