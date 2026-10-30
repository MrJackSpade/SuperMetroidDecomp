using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Three-block vertical rescue-wall mutations, independent of editable art.</summary>
internal static class EscapeAnimalPlmDrawDefinitions
{
    /// <summary>The solid block each of the four rescue-wall draws repeats down its three-block column.</summary>
    internal static readonly Dictionary<EscapeAnimalDraw, RoomTerrainBlock> Cells = new()
    {
        [EscapeAnimalDraw.Frame0] = RoomTerrainBlock.Of(0x53, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Frame1] = RoomTerrainBlock.Of(0x54, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Frame2] = RoomTerrainBlock.Of(0x55, BlockMirroring.None, RoomCollisionType.SolidBlock),
        [EscapeAnimalDraw.Blank] = RoomTerrainBlock.Of(0xff, BlockMirroring.None, RoomCollisionType.SolidBlock),
    };

    /// <summary>The blocks in each rescue-wall column.</summary>
    internal const int WallHeight = 3;
}
