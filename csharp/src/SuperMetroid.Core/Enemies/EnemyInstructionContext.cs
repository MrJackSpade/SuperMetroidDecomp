using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>What an enemy-owned instruction may read from the frame: Samus, the room, the camera and controller input (#633).</summary>
internal readonly record struct EnemyInstructionContext
{
    /// <summary>Samus.</summary>
    public required SamusState? Samus { get; init; }

    /// <summary>Level.</summary>
    public required RoomLevelData? Level { get; init; }

    /// <summary>Camera.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Controller input.</summary>
    public required SnesButton ControllerInput { get; init; }
}
