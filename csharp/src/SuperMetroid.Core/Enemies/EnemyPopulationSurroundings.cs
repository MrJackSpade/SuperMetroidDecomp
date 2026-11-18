using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a room's population load hands the enemies it places (#2455): the room's level data, Samus, and the frame's
/// controller input and camera. An enemy spawned on another enemy's behalf has no surroundings: it initializes without
/// them (<see cref="IEnemyInitialization"/>).
/// </summary>
/// <param name="Level">The room's level data.</param>
/// <param name="Samus">Samus, as the load hands her to its enemies.</param>
/// <param name="ControllerInput">The frame's controller input.</param>
/// <param name="Camera">The destination camera's position.</param>
[RoomScopeBoundary("A per-call input, built for one population load and never kept.")]
internal sealed record EnemyPopulationSurroundings(RoomLevelData Level, SamusState Samus, GameplayAction ControllerInput, Position16<RoomPixels> Camera);
