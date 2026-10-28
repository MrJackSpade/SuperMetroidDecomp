using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>One strip of an elevator-platform frame: the next strip's absolute offset from the PLM origin, and its cells.</summary>
/// <param name="NextX">The next strip's horizontal offset, in blocks; zero after the last strip.</param>
/// <param name="NextY">The next strip's vertical offset, in blocks; zero after the last strip.</param>
/// <param name="Cells">The strip's level words, left to right.</param>
internal readonly record struct ElevatorPlatformStrip(sbyte NextX, sbyte NextY, RoomLevelWord[] Cells);
