using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator platform's complete animation loop and its three tile layouts (#1554). Timing, layout selection,
/// collision kinds and offsets are authored; the underlying tiles remain the room tileset artwork.
/// </summary>
internal static class ElevatorPlatformObjectDefinitions
{
    /// <summary>The platform's loop, which it plays from its first update for as long as it stays: each layout four updates, there and back.</summary>
    internal static IReadOnlyList<TerrainChangeFrame> Cycle { get; } =
    [
        new(RoomObjectDraw.Of(ElevatorPlatformDraw.First), 4),
        new(RoomObjectDraw.Of(ElevatorPlatformDraw.Second), 4),
        new(RoomObjectDraw.Of(ElevatorPlatformDraw.Third), 4),
        new(RoomObjectDraw.Of(ElevatorPlatformDraw.Second), 4),
    ];

    /// <summary>
    /// Three frames, each with two upper edge cells and a
    /// four-cell lower row. All cells are solid. Edge art advances one tile per
    /// frame; lower art advances two, mirrored about the platform centre.
    /// The strips start at the origin, then at (3,0) and (0,1) from it.
    /// </summary>
    internal static readonly Dictionary<ElevatorPlatformDraw, RoomObjectBlockStrip[]> Strips = new()
    {
        [ElevatorPlatformDraw.First] =
        [
            new(new(0, 0), [RoomTerrainBlock.Of(133, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(3, 0), [RoomTerrainBlock.Of(133, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 1),
            [
                RoomTerrainBlock.Of(136, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(137, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(137, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(136, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
        [ElevatorPlatformDraw.Second] =
        [
            new(new(0, 0), [RoomTerrainBlock.Of(134, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(3, 0), [RoomTerrainBlock.Of(134, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 1),
            [
                RoomTerrainBlock.Of(138, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(139, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(139, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(138, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
        [ElevatorPlatformDraw.Third] =
        [
            new(new(0, 0), [RoomTerrainBlock.Of(135, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(3, 0), [RoomTerrainBlock.Of(135, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 1),
            [
                RoomTerrainBlock.Of(140, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(141, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(141, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(140, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
    };
}
