using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Input;

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
        /// <summary>Starts the transition through the door Samus entered in the runtime's loaded room.</summary>
        /// <param name="runtime">The runtime.</param>
        internal void Begin(SuperMetroidRuntime runtime) => transition.Begin(runtime, runtime.Room.LoadedScope);

        /// <summary>Advances one update of the transition in the runtime's loaded room.</summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="audio">The audio queues.</param>
        /// <param name="controllerInput">The update's controller input.</param>
        /// <param name="loaderProgress">How far the destination's load has progressed.</param>
        internal void Step(SuperMetroidRuntime runtime, AudioQueues audio, ControllerButton controllerInput, IRoomLoadProgress loaderProgress) =>
            transition.Step(runtime, runtime.Room.LoadedScope, audio, controllerInput, loaderProgress);
    }
}
