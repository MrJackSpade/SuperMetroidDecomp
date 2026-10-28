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
    internal static readonly Dictionary<ElevatorPlatformDraw, ElevatorPlatformStrip[]> Strips = new()
    {
        [ElevatorPlatformDraw.First] =
        [
            new(3, 0, [RoomLevelWord.Create(0x85, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock)]),
            new(0, 1, [RoomLevelWord.Create(0x85, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock)]),
            new(0, 0,
            [
                RoomLevelWord.Create(0x88, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x89, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x89, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x88, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
        [ElevatorPlatformDraw.Second] =
        [
            new(3, 0, [RoomLevelWord.Create(0x86, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock)]),
            new(0, 1, [RoomLevelWord.Create(0x86, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock)]),
            new(0, 0,
            [
                RoomLevelWord.Create(0x8a, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8b, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8b, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8a, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
            ]),
        ],
        [ElevatorPlatformDraw.Third] =
        [
            new(3, 0, [RoomLevelWord.Create(0x87, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock)]),
            new(0, 1, [RoomLevelWord.Create(0x87, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock)]),
            new(0, 0,
            [
                RoomLevelWord.Create(0x8c, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8d, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8d, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x8c, LevelBlockFlipFlags.Horizontal, RoomCollisionType.SolidBlock),
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
