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
public sealed class CeresDepartureState
{

    /// <summary>Current portion of the departure sequence.</summary>
    public CeresDeparturePhase Phase { get; private set; } = CeresDeparturePhase.Inactive;

    /// <summary>
    /// The updates left in the elevator hold, started at <see cref="CeresElevatorDepartureDefinitions.DepartureHoldFrames"/>
    /// when Samus reaches the elevator and the room's main behaviour begins the departure.
    /// </summary>
    public int HoldFramesRemaining { get; private set; }

    /// <summary>The blackout fade: fully lit until the fade, then one level darker on every update until black.</summary>
    private readonly ScreenFade fade = new(DisplayBrightness.Full);

    /// <summary>Master brightness during the blackout fade, from full down to black.</summary>
    public int Brightness => fade.Level;

    /// <summary>Starts the elevator hold from the room main's one-frame request.</summary>
    /// <exception cref="InvalidOperationException">The departure has already begun or completed; a state instance admits the sequence only once.</exception>
    public void Begin()
    {
        if (Phase != CeresDeparturePhase.Inactive)
            throw new InvalidOperationException("Ceres elevator departure was started twice.");

        HoldFramesRemaining = CeresElevatorDepartureDefinitions.DepartureHoldFrames;
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

        // The game decrements after the main gameplay state returns, then accepts both zero and a count run past zero. Begin()
        // always seeds 60 and the hold ends on its expiry, so zero is the reachable result.
        if (--HoldFramesRemaining > 0)
            return false;

        Phase = CeresDeparturePhase.FadingToBlack;
        return true;
    }

    /// <summary>
    /// Takes one step of the fade to black after ordinary gameplay.
    /// </summary>
    /// <returns>Whether brightness one went black and completed this timing owner; the departure then saves the checkpoint and starts the next cinematic.</returns>
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
