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

    /// <summary>The updates the collapse waits after clearing a tube's blocks before the next tube falls.</summary>
    internal const int StepWaitUpdates = 33;

    /// <summary>The updates the collapse waits after clearing the last side tube's blocks before the main tube falls.</summary>
    internal const int MainTubeWaitUpdates = 3;

    /// <summary>The blocks the bottom left tube leaves as it falls.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> BottomLeftTubeBlocks =
        new(new(5, 9), MotherBrainRoomMutation.ClearBottomLeftTube);

    /// <summary>The ceiling block the top right tube leaves.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> TopRightTubeCeiling =
        new(new(9, 2), MotherBrainRoomMutation.ClearCeilingBlock);

    /// <summary>The ceiling block the top left tube leaves.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> TopLeftTubeCeiling =
        new(new(6, 2), MotherBrainRoomMutation.ClearCeilingBlock);

    /// <summary>The blocks the bottom right tube leaves as it falls.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> BottomRightTubeBlocks =
        new(new(10, 9), MotherBrainRoomMutation.ClearBottomRightTube);

    /// <summary>The blocks the bottom middle-left tube leaves as it falls.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> BottomMiddleLeftTubeBlocks =
        new(new(6, 10), MotherBrainRoomMutation.ClearBottomMiddleSideTube);

    /// <summary>The ceiling tube block the top middle-left tube leaves.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> TopMiddleLeftTubeCeiling =
        new(new(7, 2), MotherBrainRoomMutation.ClearCeilingTube);

    /// <summary>The ceiling tube block the top middle-right tube leaves.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> TopMiddleRightTubeCeiling =
        new(new(8, 2), MotherBrainRoomMutation.ClearCeilingTube);

    /// <summary>The blocks the bottom middle-right tube leaves as it falls.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> BottomMiddleRightTubeBlocks =
        new(new(9, 10), MotherBrainRoomMutation.ClearBottomMiddleSideTube);

    /// <summary>The blocks the main tube and the tubes beside it leave as it falls.</summary>
    internal static readonly RoomObjectSpawnRequest<MotherBrainRoomMutation> BottomMiddleTubesBlocks =
        new(new(7, 7), MotherBrainRoomMutation.ClearBottomMiddleTubes);

    /// <summary>Where the top right tube starts falling.</summary>
    internal static readonly Position16<RoomPixels> TopRightTubeStart = new(152, 47);

    /// <summary>Where the top left tube starts falling.</summary>
    internal static readonly Position16<RoomPixels> TopLeftTubeStart = new(104, 47);

    /// <summary>Where the top middle-left tube starts falling.</summary>
    internal static readonly Position16<RoomPixels> TopMiddleLeftTubeStart = new(120, 59);

    /// <summary>Where the top middle-right tube starts falling.</summary>
    internal static readonly Position16<RoomPixels> TopMiddleRightTubeStart = new(136, 59);

    /// <summary>The gravity a falling tube gains each update.</summary>
    internal static readonly SignedEightEight TubeGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(6);

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

    /// <summary>The height the falling main tube's smoke puffs show at.</summary>
    internal const int SmokeY = 208;
}
