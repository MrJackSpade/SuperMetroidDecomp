using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// A door transition a check drives through the runtime's loaded room (#2554): production hands the transition the room it
/// runs in, the source room until the destination loads and then the destination, which the runtime's room holds.
/// </summary>
internal static class DoorTransitionStepAccess
{
    /// <summary>The transition's steps in the runtime's loaded room.</summary>
    /// <param name="transition">The door transition.</param>
    extension(DoorTransitionState transition)
    {
        /// <summary>
        /// Enters the transition through the door Samus's collision entered in the runtime's loaded room and starts it, as
        /// the game enters a door as she reaches it and then begins its transition (#2554).
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <exception cref="InvalidOperationException">Samus's collision entered no door in the loaded room.</exception>
        internal void Begin(SuperMetroidRuntime runtime)
        {
            transition.Enter(runtime.EnteredDoor ?? throw new InvalidOperationException("Samus's collision entered no door."));
            transition.Begin(runtime, runtime.Room.LoadedScope);
        }

        /// <summary>Advances one update of the transition in the runtime's loaded room.</summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="audio">The audio queues.</param>
        /// <param name="controllerInput">The update's controller input.</param>
        /// <param name="loaderProgress">How far the destination's load has progressed.</param>
        internal void Step(SuperMetroidRuntime runtime, AudioQueues audio, ControllerButton controllerInput, IRoomLoadProgress loaderProgress) =>
            transition.Step(runtime, runtime.Room.LoadedScope, audio, controllerInput, loaderProgress);
    }
}
