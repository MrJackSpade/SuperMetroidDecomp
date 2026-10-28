using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One horizontal strip of a Bomb Torizo hand draw: its origin relative to the hand PLM block and its cells.</summary>
/// <param name="Origin">The strip's origin, in blocks, relative to the hand PLM block.</param>
/// <param name="Cells">The strip's level words, left to right.</param>
internal readonly record struct BombTorizoHandStrip(Offset16<RoomBlocks> Origin, RoomLevelWord[] Cells);
