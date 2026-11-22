using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// One ordinary door transition once its destination has loaded (#2554): the door's opening scroll, which slides the
/// destination in and then places Samus in its doorway, the destination's fade in, and the stage the transition has
/// reached, from its wait for the scroll to its last fade step.
/// </summary>
/// <param name="scroll">The door's opening scroll, begun as the destination loaded.</param>
/// <param name="fadeIn">The steps of the destination's fade in.</param>
internal sealed partial class OpeningDoorTransition(DoorOpeningScrollState scroll, GradualFadeSteps fadeIn) : DoorTransitionStage
{
    /// <summary>The door's opening scroll, begun as the destination loaded.</summary>
    internal DoorOpeningScrollState Scroll { get; } = Ensure.NotNull(scroll);

    /// <summary>The steps of the destination's fade in (#1512). What the fade shows is the runtime's <see cref="DoorFade"/>.</summary>
    internal GradualFadeSteps FadeSteps { get; } = Ensure.NotNull(fadeIn);

    /// <inheritdoc/>
    internal override DoorTransitionPhase Phase => RunningPhase;

    /// <summary>
    /// The stage this transition has reached, one of the phases only a transition under way holds (#2553); it begins at
    /// the wait for the door's opening scroll.
    /// </summary>
    internal DoorTransitionPhase.Running RunningPhase { get; private set; } = new DoorTransitionPhase.Running.WaitForDoorOpeningScroll();

    /// <summary>Moves the transition on to <paramref name="phase"/>, which the next update runs.</summary>
    internal void Enter(DoorTransitionPhase.Running phase) => RunningPhase = phase;
}
