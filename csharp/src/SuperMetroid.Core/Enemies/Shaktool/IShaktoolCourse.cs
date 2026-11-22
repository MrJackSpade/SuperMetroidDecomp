using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// How a Shaktool segment moves while its motion runs, as its definition names it (#2452, #2554): the leading saw stands as
/// the chain's fixed anchor, every later segment orbits the one its chain lists before it, the centre facing the one listed
/// after it and the trailing saw driving the chain through the terrain. Each course is an object holding the segment it
/// moves and the places it moves between, so a copy of the game's state holds it as one.
/// </summary>
internal interface IShaktoolCourse
{
    /// <summary>The segment the course moves.</summary>
    ShaktoolSegmentState State { get; }

    /// <summary>Moves the segment one update along its course.</summary>
    /// <param name="level">The room's level data, which the trailing saw drives through.</param>
    void Advance(RoomLevelData level);
}
