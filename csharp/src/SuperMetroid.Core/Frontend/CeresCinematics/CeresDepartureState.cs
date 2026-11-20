using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Outer game-state timing for the final Ceres elevator hold and blackout.
/// </summary>
/// <remarks>
/// The Ceres room's main behaviour owns admission to this sequence, while the outer game-state loop
/// owns the subsequent elevator-hold and blackout states. Keeping this object outside the room state is
/// intentional: the room continues running through both states, but it cannot restart the
/// departure because its main-gameplay predicate no longer succeeds. Each departure is a new instance, begun holding on
/// the elevator (#2553).
/// </remarks>
public sealed class CeresDepartureState
{

    /// <summary>Begins the departure when Samus reaches the elevator, as the Ceres room asks: the elevator hold starts.</summary>
    public CeresDepartureState()
    {
    }

    /// <summary>Current portion of the departure sequence.</summary>
    public CeresDeparturePhase Phase { get; private set; } = CeresDeparturePhase.HoldingOnElevator;

    /// <summary>
    /// The elevator hold, lasting <see cref="CeresElevatorDepartureDefinitions.DepartureHoldFrames"/> from when Samus
    /// reaches the elevator and the room's main behaviour begins the departure.
    /// </summary>
    public UpdateCountdown Hold { get; } = UpdateCountdown.Lasting(CeresElevatorDepartureDefinitions.DepartureHoldFrames);

    /// <summary>The blackout fade: fully lit until the fade, then one level darker on every update until black.</summary>
    private readonly ScreenFade fade = new(DisplayBrightness.Full);

    /// <summary>The screen's brightness during the blackout, from full down to black.</summary>
    public int Brightness => fade.Level;

    /// <summary>
    /// Takes the departure's step after the main gameplay update: the elevator hold's decrement, whose last update begins
    /// the fade to black, or else one step of that fade.
    /// </summary>
    /// <returns>Whether this step reached black and completed the departure; the departure then saves the checkpoint and starts the next cinematic.</returns>
    public bool StepAfterGameplay()
    {
        if (Phase == CeresDeparturePhase.HoldingOnElevator)
        {
            // The hold is counted after the main gameplay update returns, and ends on its last update.
            if (Hold.Step())
                Phase = CeresDeparturePhase.FadingToBlack;
            return false;
        }

        // The fade, the departure's other phase, steps on every update; the step that reaches black completes the departure.
        return fade.Darken();
    }
}
