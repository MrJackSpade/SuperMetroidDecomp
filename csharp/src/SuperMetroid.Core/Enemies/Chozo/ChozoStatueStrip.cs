using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One horizontal run of a Chozo statue draw: where the next run starts relative to this one, and its cells.</summary>
/// <param name="NextX">The next run's horizontal offset, in blocks; zero after the last run.</param>
/// <param name="NextY">The next run's vertical offset, in blocks; zero after the last run.</param>
/// <param name="Cells">The run's level words, left to right.</param>
internal readonly record struct ChozoStatueStrip(sbyte NextX, sbyte NextY, RoomLevelWord[] Cells);
