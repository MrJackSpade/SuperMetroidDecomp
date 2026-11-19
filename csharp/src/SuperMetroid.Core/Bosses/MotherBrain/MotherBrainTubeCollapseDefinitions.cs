using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How the tubes about Mother Brain's tank collapse in her fake death (#2216): the free enemy projectiles the collapse
/// waits for, the waits between its steps, the blocks each step clears, where the ceiling tubes start falling, how the
/// tubes fall, and how the main tube carries her head down and lands her.
/// </summary>
internal static class MotherBrainTubeCollapseDefinitions
{
    /// <summary>The free enemy projectiles the collapse waits for before it starts.</summary>
    internal const int FreeProjectilesNeeded = 4;

    /// <summary>The updates the collapse waits after clearing a tube's blocks before the next tube falls, the last being the update it falls on.</summary>
    internal const int StepWaitUpdates = 33;

    /// <summary>
    /// The updates the collapse waits after clearing the last side tube's blocks before the main tube falls, the last being
    /// the update it falls on.
    /// </summary>
    internal const int MainTubeWaitUpdates = 3;

    /// <summary>The updates the collapse's wait is loaded with once the main tube's place is cleared; no tube follows to wait them out.</summary>
    internal const int AfterLastClearUpdates = 1;

    /// <summary>
    /// The tubes the collapse drops, in the order they fall (#2258): what falls, the blocks clearing its place leaves, and
    /// the wait after the clear before the next tube falls.
    /// </summary>
    internal static IReadOnlyList<MotherBrainTubeDrop> Drops { get; } =
    [
        new MotherBrainBottomTubeDrop(MotherBrainTubePiece.BottomLeft,
            new(new(5, 9), MotherBrainRoomMutation.ClearBottomLeftTube), StepWaitUpdates),
        new MotherBrainTopTubeDrop(RoomEnemyProjectileKind.MotherBrainTopRightTube, MotherBrainTopTubeSegment.TopRight, new(152, 47),
            new(new(9, 2), MotherBrainRoomMutation.ClearCeilingBlock), StepWaitUpdates),
        new MotherBrainTopTubeDrop(RoomEnemyProjectileKind.MotherBrainTopLeftTube, MotherBrainTopTubeSegment.TopLeft, new(104, 47),
            new(new(6, 2), MotherBrainRoomMutation.ClearCeilingBlock), StepWaitUpdates),
        new MotherBrainBottomTubeDrop(MotherBrainTubePiece.BottomRight,
            new(new(10, 9), MotherBrainRoomMutation.ClearBottomRightTube), StepWaitUpdates),
        new MotherBrainBottomTubeDrop(MotherBrainTubePiece.BottomMiddleLeft,
            new(new(6, 10), MotherBrainRoomMutation.ClearBottomMiddleSideTube), StepWaitUpdates),
        new MotherBrainTopTubeDrop(RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube, MotherBrainTopTubeSegment.TopMiddleLeft, new(120, 59),
            new(new(7, 2), MotherBrainRoomMutation.ClearCeilingTube), StepWaitUpdates),
        new MotherBrainTopTubeDrop(RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube, MotherBrainTopTubeSegment.TopMiddleRight, new(136, 59),
            new(new(8, 2), MotherBrainRoomMutation.ClearCeilingTube), StepWaitUpdates),
        new MotherBrainBottomTubeDrop(MotherBrainTubePiece.BottomMiddleRight,
            new(new(9, 10), MotherBrainRoomMutation.ClearBottomMiddleSideTube), MainTubeWaitUpdates),
        new MotherBrainBottomTubeDrop(MotherBrainTubePiece.Main,
            new(new(7, 7), MotherBrainRoomMutation.ClearBottomMiddleTubes), AfterLastClearUpdates),
    ];

    /// <summary>The gravity a falling tube gains each update.</summary>
    internal static readonly SignedEightEight TubeGravity = SignedEightEight.FromPixelFraction(3, 128);

    /// <summary>The floor a falling ceiling tube smokes and is removed at.</summary>
    internal const int CeilingTubeFloorY = 208;

    /// <summary>How far below a falling ceiling tube its dust shows as it starts to fall.</summary>
    internal const int CeilingTubeDustBelow = 8;

    /// <summary>The height from which the falling main tube is hidden.</summary>
    internal const int MainTubeHiddenFromY = 244;

    /// <summary>How far above the falling main tube Mother Brain's head is carried.</summary>
    internal const int HeadAboveMainTube = 56;

    /// <summary>The updates the room shakes as the main tube lands her.</summary>
    internal const int LandingShakeUpdates = 32;

    /// <summary>The updates between the smoke puffs of the falling main tube.</summary>
    internal const int SmokeIntervalUpdates = 9;

    /// <summary>How far right of a falling tube each of its smoke puffs shows, in the order they cycle.</summary>
    internal static IReadOnlyList<int> SmokeXOffsets { get; } = [-8, 2, -4, 6];

    /// <summary>
    /// Where the explosions about her tank burst as she fakes her death, in the order they burst (#2315), starting again
    /// after the last.
    /// </summary>
    internal static IReadOnlyList<Position16<RoomPixels>> FakeDeathExplosionPositions { get; } =
    [
        new(120, 206),
        new(138, 72),
        new(124, 170),
        new(120, 52),
        new(138, 146),
        new(124, 90),
        new(120, 132),
        new(136, 116),
    ];

    /// <summary>The ceiling block her fake death's flash clears above her tank.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> CeilingBlock =
        new(new(14, 2), MotherBrainRoomMutation.ClearCeilingBlock);

    /// <summary>The height the falling main tube's smoke puffs show at.</summary>
    internal const int SmokeY = 208;
}
