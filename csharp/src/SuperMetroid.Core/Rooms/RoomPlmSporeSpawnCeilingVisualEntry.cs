namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Spore Spawn ceiling frame in native draw-run order.</summary>
/// <param name="Id">Case-sensitive compiled identity: <c>clear-ceiling</c> or <c>crumble-frame-0</c> through <c>crumble-frame-2</c>.</param>
/// <param name="Blocks">Four mutable input visual words in row-major order across the two-by-two 16-by-16-block square; only the ten-bit metatile index and two parent flip bits are allowed.</param>
public sealed record RoomPlmSporeSpawnCeilingVisualEntry(string Id, ushort[] Blocks);
