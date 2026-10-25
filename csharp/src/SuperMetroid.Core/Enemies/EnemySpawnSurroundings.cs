using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a room's population load hands the enemies it spawns: the room's level data, Samus, and the frame's controller
/// input and camera. An enemy spawned on another enemy's behalf has <see cref="None"/>.
/// </summary>
/// <param name="Level">The room's level data, or null outside a population load.</param>
/// <param name="Samus">Samus, or null outside a population load.</param>
/// <param name="ControllerInput">The frame's controller input.</param>
/// <param name="Camera">The destination camera's position.</param>
[RoomScopeBoundary("A per-call input, built for one population load and never kept.")]
internal sealed record EnemySpawnSurroundings(RoomLevelData? Level, SamusState? Samus, ControllerButton ControllerInput, Position16<RoomPixels> Camera)
{
    /// <summary>The surroundings of a spawn outside a population load.</summary>
    public static EnemySpawnSurroundings None { get; } = new(null, null, ControllerButton.None, default);
}
