using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The $B70B elevator-platform PLM's complete $84:AFB6 instruction loop and its
/// three tile-layout lists. Timing, list selection, collision words, and offsets
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
            new(new(3, 0), [RoomLevelWord.Create(0x85, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(0, 1), [RoomLevelWord.Create(0x85, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 0),
            [
                RoomLevelWord.Create(0x88, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x89, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x89, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x88, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
        [ElevatorPlatformDraw.Second] =
        [
            new(new(3, 0), [RoomLevelWord.Create(0x86, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(0, 1), [RoomLevelWord.Create(0x86, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 0),
            [
                RoomLevelWord.Create(0x8a, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8b, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8b, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8a, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
        [ElevatorPlatformDraw.Third] =
        [
            new(new(3, 0), [RoomLevelWord.Create(0x87, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(0, 1), [RoomLevelWord.Create(0x87, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock)]),
            new(new(0, 0),
            [
                RoomLevelWord.Create(0x8c, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8d, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8d, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8c, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
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
