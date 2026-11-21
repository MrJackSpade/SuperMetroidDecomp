using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Where the door transition stands: resting before the first door or after a door's transition has completed, or one
/// door's transition under way (<see cref="OrdinaryDoorTransition"/>), which carries everything that transition holds.
/// </summary>
internal abstract partial class DoorTransitionStage
{
    /// <summary>No door transition has begun.</summary>
    internal static DoorTransitionStage Inactive { get; } = new Resting(new DoorTransitionPhase.Inactive());

    /// <summary>The last door's transition has finished and ordinary gameplay may resume.</summary>
    internal static DoorTransitionStage Complete { get; } = new Resting(new DoorTransitionPhase.Complete());

    /// <summary>The transition's stage as the frontend and the update's audio read it.</summary>
    internal abstract DoorTransitionPhase Phase { get; }

    /// <summary>A stage with no transition under way.</summary>
    /// <param name="phase"><see cref="DoorTransitionPhase.Inactive"/> or <see cref="DoorTransitionPhase.Complete"/>.</param>
    private sealed partial class Resting(DoorTransitionPhase phase) : DoorTransitionStage
    {
        /// <inheritdoc/>
        internal override DoorTransitionPhase Phase { get; } = phase;
    }
}
