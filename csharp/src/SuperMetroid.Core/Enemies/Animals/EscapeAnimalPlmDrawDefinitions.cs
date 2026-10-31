using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Three-block vertical rescue-wall mutations, independent of editable art.</summary>
internal static class EscapeAnimalPlmDrawDefinitions
{
    /// <summary>The solid block each of the four rescue-wall draws repeats down its three-block column.</summary>
    internal static readonly Dictionary<EscapeAnimalDraw, RoomTerrainBlock> Cells = new()
    {
        [EscapeAnimalDraw.Frame0] = RoomTerrainBlock.Of(83, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Frame1] = RoomTerrainBlock.Of(84, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Frame2] = RoomTerrainBlock.Of(85, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Blank] = RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.SolidBlock),
    };

    /// <summary>The blocks in each rescue-wall column.</summary>
    internal const int WallHeight = 3;
}
