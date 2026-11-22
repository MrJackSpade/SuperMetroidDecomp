using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.World;

/// <summary>
/// A check's reach to the room a runtime has loaded (#2554): production hands the loaded room's scope to what runs in it,
/// and a check that runs a runtime in a loaded room names that room's parts directly, failing loudly when none is loaded.
/// </summary>
internal static class LoadedRoomAccess
{
    /// <summary>The loaded room's parts a check reads.</summary>
    /// <param name="room">The runtime's room state.</param>
    extension(ActiveRoomState room)
    {
        /// <summary>The loaded room's scope.</summary>
        /// <exception cref="InvalidOperationException">No room is loaded.</exception>
        internal RoomScope LoadedScope => Ensure.NotNull(room).Scope ?? throw new InvalidOperationException("No room is loaded.");

        /// <summary>The loaded room's selected room and state.</summary>
        /// <exception cref="InvalidOperationException">No room is loaded.</exception>
        internal SelectedRoom LoadedRoom => Ensure.NotNull(room).ActiveRoom ?? throw new InvalidOperationException("No room is loaded.");

        /// <summary>The loaded room's scroll camera.</summary>
        /// <exception cref="InvalidOperationException">No room camera is loaded.</exception>
        internal ScrollBoundaryCamera LoadedCamera => Ensure.NotNull(room).Camera ?? throw new InvalidOperationException("No room camera is loaded.");

        /// <summary>The loaded room's level data.</summary>
        /// <exception cref="InvalidOperationException">No room level data is loaded.</exception>
        internal RoomLevelData LoadedLevelData => Ensure.NotNull(room).LevelData ?? throw new InvalidOperationException("No room level data is loaded.");
    }

    /// <summary>The runtime's frame operations in the room it has loaded.</summary>
    /// <param name="runtime">The runtime.</param>
    extension(SuperMetroidRuntime runtime)
    {
        /// <summary>Publishes the display frame of the loaded room.</summary>
        internal GameplayDisplayFrame PublishDisplayFrame() => runtime.PublishDisplayFrame(runtime.Room.LoadedScope);

        /// <summary>Steps one frame of <paramref name="request"/> in the loaded room.</summary>
        /// <param name="playerInput">The controller input.</param>
        /// <param name="request">What the frame runs.</param>
        internal RuntimeFrameOutcome StepFrame(ControllerButton playerInput, RuntimeFrameRequest request) =>
            runtime.StepFrame(playerInput, request, runtime.Room.LoadedScope);
    }
}
