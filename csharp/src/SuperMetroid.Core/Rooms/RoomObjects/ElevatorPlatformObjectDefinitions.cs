using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The elevator platform's complete animation loop and its three layouts (#1554). Timing, layout selection, collision
/// kinds and offsets are authored; what each frame shows is its installed picture (#1557).
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
    /// Three frames, each with two upper edge cells and a four-cell lower row. All cells are solid. The strips start at
    /// the origin, then at (3,0) and (0,1) from it.
    /// </summary>
    internal static readonly Dictionary<ElevatorPlatformDraw, RoomObjectBlockStrip[]> Strips = new()
    {
        [ElevatorPlatformDraw.First] =
        [
            new(new(0, 0), [RoomCollisionType.SolidBlock]),
            new(new(3, 0), [RoomCollisionType.SolidBlock]),
            new(new(0, 1),
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ]),
        ],
        [ElevatorPlatformDraw.Second] =
        [
            new(new(0, 0), [RoomCollisionType.SolidBlock]),
            new(new(3, 0), [RoomCollisionType.SolidBlock]),
            new(new(0, 1),
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ]),
        ],
        [ElevatorPlatformDraw.Third] =
        [
            new(new(0, 0), [RoomCollisionType.SolidBlock]),
            new(new(3, 0), [RoomCollisionType.SolidBlock]),
            new(new(0, 1),
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ]),
        ],
    };
}
