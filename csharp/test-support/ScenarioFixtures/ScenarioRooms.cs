using SuperMetroid.Core.Input;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>The room a scenario's runtime has loaded, which every scenario step runs in (#2554).</summary>
internal static class ScenarioRooms
{
    /// <summary>The room <paramref name="runtime"/> has loaded.</summary>
    /// <exception cref="InvalidOperationException">No room is loaded.</exception>
    internal static RoomScope LoadedScope(SuperMetroidRuntime runtime) =>
        SuperMetroid.Core.Ensure.NotNull(runtime).Room.Scope ?? throw new InvalidOperationException("No room is loaded.");

    /// <summary>The level data of the room <paramref name="runtime"/> has loaded.</summary>
    /// <exception cref="InvalidOperationException">No room is loaded.</exception>
    internal static RoomLevelData LoadedLevelData(SuperMetroidRuntime runtime) =>
        SuperMetroid.Core.Ensure.NotNull(runtime).Room.LevelData ?? throw new InvalidOperationException("No room level data is loaded.");

    /// <summary>Runs one gameplay update of <paramref name="runtime"/> in the room it has loaded.</summary>
    internal static RuntimeFrameOutcome Step(SuperMetroidRuntime runtime, ControllerButton playerInput, RuntimeFrameRequest request) =>
        runtime.StepFrame(playerInput, request, LoadedScope(runtime));
}
