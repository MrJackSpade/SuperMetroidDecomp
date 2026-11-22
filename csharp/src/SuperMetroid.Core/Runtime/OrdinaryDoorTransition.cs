using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// One ordinary door transition, from its beginning to its last fade step: the door, captured before the destination
/// replaces the source, whether each of the two rooms selects the isolated door transition, Samus's source-room position
/// that the door's scroll is placed from, the steps of the fade under way, the destination-setup wait and the stage it has
/// reached.
/// </summary>
internal sealed partial class OrdinaryDoorTransition : DoorTransitionStage
{
    /// <summary>Starts a transition at its sound drain, the source room's fade out over <paramref name="fadeOut"/>.</summary>
    /// <param name="door">The door being gone through.</param>
    /// <param name="sourceIsolated">Whether the source room selects the isolated door transition.</param>
    /// <param name="destinationIsolated">Whether the destination room selects the isolated door transition.</param>
    /// <param name="sourceSamus">Samus's source-room position as the transition begins.</param>
    /// <param name="fadeOut">The steps of the source room's fade out.</param>
    internal OrdinaryDoorTransition(
        DoorDefinition door,
        bool sourceIsolated,
        bool destinationIsolated,
        FixedPosition<RoomPixels> sourceSamus,
        GradualFadeSteps fadeOut)
    {
        Door = Ensure.NotNull(door);
        SourceIsolated = sourceIsolated;
        DestinationIsolated = destinationIsolated;
        SourceSamus = sourceSamus;
        FadeSteps = Ensure.NotNull(fadeOut);
    }

    /// <summary>The door being gone through, captured before the destination room replaces the source.</summary>
    internal DoorDefinition Door { get; }

    /// <summary>Whether the source room selects the isolated door transition, for the transition's display.</summary>
    internal bool SourceIsolated { get; }

    /// <summary>Whether the destination room selects the isolated door transition, for the transition's display.</summary>
    internal bool DestinationIsolated { get; }

    /// <summary>
    /// Samus's source-room position used to compute the final door-scroll placement: taken as the transition begins and
    /// again just before the door's scrolling setup moves her.
    /// </summary>
    internal FixedPosition<RoomPixels> SourceSamus { get; set; }

    /// <summary>
    /// The steps of the door's current fade on the shared step counter: the source room's fade out, then the destination's
    /// fade in (#1512). What the fade shows is the runtime's <see cref="DoorFade"/>.
    /// </summary>
    internal GradualFadeSteps FadeSteps { get; set; }

    /// <summary>The destination-setup wait, started once the source camera has aligned.</summary>
    internal UpdateCountdown DestinationSetupWait { get; set; } = UpdateCountdown.Ended();

    /// <inheritdoc/>
    internal override DoorTransitionPhase Phase => RunningPhase;

    /// <summary>
    /// The stage this transition has reached, one of the phases only a transition under way holds (#2553); it begins at
    /// the sound drain.
    /// </summary>
    internal DoorTransitionPhase.Running RunningPhase { get; private set; } = new DoorTransitionPhase.Running.WaitForSoundQueues();

    /// <summary>Moves the transition on to <paramref name="phase"/>, which the next update runs.</summary>
    internal void Enter(DoorTransitionPhase.Running phase) => RunningPhase = phase;
}
