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
/// departure because its main-gameplay predicate no longer succeeds.
/// </remarks>
public sealed partial class CeresDepartureState
{

    /// <summary>Current portion of the departure sequence.</summary>
    public CeresDeparturePhase Phase { get; private set; } = CeresDeparturePhase.Inactive;

    /// <summary>
    /// The elevator hold, lasting <see cref="CeresElevatorDepartureDefinitions.DepartureHoldFrames"/> from when Samus
    /// reaches the elevator and the room's main behaviour begins the departure.
    /// </summary>
    public UpdateCountdown Hold { get; private set; } = UpdateCountdown.Ended();

    /// <summary>The blackout fade: fully lit until the fade, then one level darker on every update until black.</summary>
    private readonly ScreenFade fade = new(DisplayBrightness.Full);

    /// <summary>The screen's brightness during the blackout, from full down to black.</summary>
    public DisplayBrightness Brightness => fade.Level;

    /// <summary>Begins the departure when Samus reaches the elevator, as the Ceres room asks.</summary>
    /// <exception cref="InvalidOperationException">The departure has already begun or completed; a state instance admits the sequence only once.</exception>
    public void Begin()
    {
        if (Phase != CeresDeparturePhase.Inactive)
            throw new InvalidOperationException("Ceres elevator departure was started twice.");

        Hold = UpdateCountdown.Lasting(CeresElevatorDepartureDefinitions.DepartureHoldFrames);
        fade.Restart(DisplayBrightness.Full, 0);
        Phase = CeresDeparturePhase.HoldingOnElevator;
    }

    /// <summary>
    /// Applies the elevator hold's post-gameplay decrement and reports the transition to the blackout.
    /// </summary>
    /// <returns>Whether the hold ran out this update and the fade to black began.</returns>
    /// <exception cref="InvalidOperationException">The elevator-hold phase is not active.</exception>
    public bool StepHoldAfterGameplay()
    {
        if (Phase != CeresDeparturePhase.HoldingOnElevator)
            throw new InvalidOperationException("Ceres departure hold is not active.");

        // The hold is counted after the main gameplay update returns, and ends on its last update.
        if (!Hold.Step())
            return false;

        Phase = CeresDeparturePhase.FadingToBlack;
        return true;
    }

    /// <summary>
    /// Takes one step of the fade to black after ordinary gameplay.
    /// </summary>
    /// <returns>Whether this step reached black and completed the departure; the departure then saves the checkpoint and starts the next cinematic.</returns>
    /// <exception cref="InvalidOperationException">The blackout-fade phase is not active.</exception>
    public bool StepFadeAfterGameplay()
    {
        if (Phase != CeresDeparturePhase.FadingToBlack)
            throw new InvalidOperationException("Ceres departure fade is not active.");

        // The fade steps on every update; the step that reaches black completes the departure.
        if (!fade.Darken())
            return false;

        Phase = CeresDeparturePhase.Complete;
        return true;
    }
}
