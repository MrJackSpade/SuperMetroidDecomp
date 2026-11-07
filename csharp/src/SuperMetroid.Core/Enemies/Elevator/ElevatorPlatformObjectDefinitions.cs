using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator-platform PLM's complete animation loop and its
/// three tile-layout lists. Timing, list selection, collision kinds, and offsets
/// are authored; the underlying tiles remain the room tileset artwork.
/// </summary>
internal static class ElevatorPlatformObjectDefinitions
{
    /// <summary>
    /// Three frames, each with two upper edge cells and a
    /// four-cell lower row. All cells are solid. Edge art advances one tile per
    /// frame; lower art advances two, mirrored about the platform centre.
    /// Continuations are absolute offsets (3,0), (0,1), then the terminator.
    /// </summary>
    internal static readonly Dictionary<ElevatorPlatformDraw, RoomObjectBlockStrip[]> Strips = new()
    {
        [ElevatorPlatformDraw.First] =
        [
            new(new(3, 0), [RoomTerrainBlock.Of(133, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(0, 1), [RoomTerrainBlock.Of(133, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 0),
            [
                RoomTerrainBlock.Of(136, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(137, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(137, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(136, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
        [ElevatorPlatformDraw.Second] =
        [
            new(new(3, 0), [RoomTerrainBlock.Of(134, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(0, 1), [RoomTerrainBlock.Of(134, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 0),
            [
                RoomTerrainBlock.Of(138, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(139, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(139, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(138, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
        [ElevatorPlatformDraw.Third] =
        [
            new(new(3, 0), [RoomTerrainBlock.Of(135, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(0, 1), [RoomTerrainBlock.Of(135, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 0),
            [
                RoomTerrainBlock.Of(140, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(141, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(141, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(140, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
    };
}
