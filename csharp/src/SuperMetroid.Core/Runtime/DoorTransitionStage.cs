using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Where the door transition stands: resting before the first door or after a door's transition has completed, or one
/// door's transition under way (<see cref="OrdinaryDoorTransition"/>), which carries everything that transition holds.
/// </summary>
internal abstract class DoorTransitionStage
{
    /// <summary>No door transition has begun.</summary>
    internal static DoorTransitionStage Inactive { get; } = new Resting(DoorTransitionPhase.Inactive);

    /// <summary>The last door's transition has finished and ordinary gameplay may resume.</summary>
    internal static DoorTransitionStage Complete { get; } = new Resting(DoorTransitionPhase.Complete);

    /// <summary>The transition's stage as the frontend and the update's audio read it.</summary>
    internal abstract DoorTransitionPhase Phase { get; }

    /// <summary>
    /// Samus has entered <paramref name="door"/> and the transition through it has yet to begin; until it does, the stage
    /// reads as the rest it entered from.
    /// </summary>
    /// <param name="door">The door Samus entered, which the transition goes through.</param>
    /// <param name="resting">The rest the door was entered from.</param>
    internal sealed class DoorEntered(DoorDefinition door, DoorTransitionStage resting) : DoorTransitionStage
    {
        /// <summary>The door Samus entered, which the transition goes through.</summary>
        internal DoorDefinition Door { get; } = door;

        /// <summary>The rest the door was entered from.</summary>
        internal DoorTransitionStage EnteredFrom { get; } = resting;

        /// <inheritdoc/>
        internal override DoorTransitionPhase Phase => EnteredFrom.Phase;
    }

    /// <summary>A stage with no transition under way.</summary>
    /// <param name="phase"><see cref="DoorTransitionPhase.Inactive"/> or <see cref="DoorTransitionPhase.Complete"/>.</param>
    private sealed class Resting(DoorTransitionPhase phase) : DoorTransitionStage
    {
        /// <inheritdoc/>
        internal override DoorTransitionPhase Phase { get; } = phase;
    }
}
