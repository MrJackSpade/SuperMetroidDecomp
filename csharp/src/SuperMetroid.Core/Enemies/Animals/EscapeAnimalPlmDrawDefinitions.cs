using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Three-block vertical rescue-wall mutations, independent of editable art.</summary>
internal static class EscapeAnimalPlmDrawDefinitions
{
    /// <summary>The solid block each of the four rescue-wall draws repeats down its three-block column.</summary>
    internal static readonly Dictionary<EscapeAnimalDraw, RoomLevelWord> Cells = new()
    {
        [EscapeAnimalDraw.Frame0] = RoomLevelWord.Create(0x53, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Frame1] = RoomLevelWord.Create(0x54, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Frame2] = RoomLevelWord.Create(0x55, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Blank] = RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
    };

    /// <summary>The blocks in each rescue-wall column.</summary>
    internal const int WallHeight = 3;
}
