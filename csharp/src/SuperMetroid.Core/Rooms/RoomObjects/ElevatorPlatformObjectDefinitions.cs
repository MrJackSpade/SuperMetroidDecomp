using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The elevator platform's complete animation loop and its three layouts (#1554). Timing, layout selection, collision
/// kinds and offsets are authored; what each frame shows is its installed picture (#1557).
/// </summary>
internal static class ElevatorPlatformObjectDefinitions
{
    /// <summary>The platform's loop, which it plays from its first update for as long as it stays: each layout four updates, there and back.</summary>
    internal static IReadOnlyList<ElevatorPlatformFrame> Cycle { get; } =
    [
        new(ElevatorPlatformDraws.Family.Of(ElevatorPlatformDraw.Dim), 4),
        new(ElevatorPlatformDraws.Family.Of(ElevatorPlatformDraw.Glowing), 4),
        new(ElevatorPlatformDraws.Family.Of(ElevatorPlatformDraw.Bright), 4),
        new(ElevatorPlatformDraws.Family.Of(ElevatorPlatformDraw.Glowing), 4),
    ];

    /// <summary>
    /// The platform every frame draws (#1862): its two upper edge blocks, the object's block and the one three right of it,
    /// and the four-block row below them, all solid.
    /// </summary>
    private static readonly RoomBlockShape Platform = new(
    [
        new(new(0, 0), RoomCollisionType.SolidBlock), new(new(3, 0), RoomCollisionType.SolidBlock),
        new(new(0, 1), RoomCollisionType.SolidBlock), new(new(1, 1), RoomCollisionType.SolidBlock),
        new(new(2, 1), RoomCollisionType.SolidBlock), new(new(3, 1), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>The block shape each of the three frames draws: the same platform, whose pictures differ.</summary>
    internal static readonly Dictionary<ElevatorPlatformDraw, RoomBlockShape> Shapes = new()
    {
        [ElevatorPlatformDraw.Dim] = Platform,
        [ElevatorPlatformDraw.Glowing] = Platform,
        [ElevatorPlatformDraw.Bright] = Platform,
    };
}
