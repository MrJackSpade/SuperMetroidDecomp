using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>A BG2 frame lookup: the family named in failures, whether the installed artwork has it, and its writes.</summary>
/// <param name="Family">The family named when the frame is missing.</param>
/// <param name="Found">Whether the installed artwork has the frame.</param>
/// <param name="Writes">The frame's tilemap writes, when found.</param>
internal readonly record struct EnemyBg2FrameLookup(string Family, bool Found, ReadOnlyMemory<EnemyBg2TilemapWrite> Writes);
