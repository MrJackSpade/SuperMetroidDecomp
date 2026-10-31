using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The $B70B elevator-platform PLM's complete $84:AFB6 instruction loop and its
/// three tile-layout lists. Timing, list selection, collision kinds, and offsets
/// are cartridge-defined; the underlying tiles remain the room tileset artwork.
/// </summary>
internal static class ElevatorPlatformPlmDefinitions
{
    /// <summary>
    /// Three frames at $84:AA97..AADE, each with two upper edge cells and a
    /// four-cell lower row. All cells are solid. Edge art advances one tile per
    /// frame; lower art advances two, mirrored about the platform centre.
    /// Continuations are absolute offsets (3,0), (0,1), then the terminator.
    /// </summary>
    internal static readonly Dictionary<ElevatorPlatformDraw, PlmBlockStrip[]> Strips = new()
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

    /// <summary>The three platform frames' stable artwork identifiers.</summary>
    internal static readonly Dictionary<ElevatorPlatformDraw, string> VisualIds = new()
    {
        [ElevatorPlatformDraw.First] = "first-frame",
        [ElevatorPlatformDraw.Second] = "second-frame",
        [ElevatorPlatformDraw.Third] = "third-frame",
    };
}
