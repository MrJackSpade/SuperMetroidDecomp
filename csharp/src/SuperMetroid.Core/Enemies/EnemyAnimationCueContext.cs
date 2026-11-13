using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>What an enemy animation's cue may read from the update: Samus, the room, the camera and controller input (#633, #1624).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyAnimationCueContext
{
    /// <summary>Samus.</summary>
    public required SamusState? Samus { get; init; }

    /// <summary>Level.</summary>
    public required RoomLevelData? Level { get; init; }

    /// <summary>Camera.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Controller input.</summary>
    public required ControllerButton ControllerInput { get; init; }
}
