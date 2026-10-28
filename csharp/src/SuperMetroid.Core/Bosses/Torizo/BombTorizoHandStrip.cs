using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One horizontal strip of a Bomb Torizo hand draw: its origin relative to the hand PLM block and its cells.</summary>
/// <param name="OriginX">The strip's horizontal origin, in blocks, relative to the hand PLM block.</param>
/// <param name="OriginY">The strip's vertical origin, in blocks, relative to the hand PLM block.</param>
/// <param name="Cells">The strip's level words, left to right.</param>
internal readonly record struct BombTorizoHandStrip(sbyte OriginX, sbyte OriginY, RoomLevelWord[] Cells);
