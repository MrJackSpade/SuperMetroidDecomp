using SuperMetroid.Core.Areas.Ceres;

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

    /// <summary>Live elevator-hold countdown seeded to 60 by Samus code two.</summary>
    public int HoldFramesRemaining { get; private set; }

    /// <summary>
    /// Master brightness during the blackout fade, from fifteen (full) down to zero (black); every step is
    /// guarded by the level above one, so the value never leaves that range.
    /// </summary>
    public int Brightness { get; private set; } = 15;

    /// <summary>Starts the elevator hold from the room main's one-frame request.</summary>
    /// <exception cref="InvalidOperationException">The departure has already begun or completed; a state instance admits the sequence only once.</exception>
    public void Begin()
    {
        if (Phase != CeresDeparturePhase.Inactive)
            throw new InvalidOperationException("Ceres elevator departure was started twice.");

        HoldFramesRemaining = CeresElevatorShaftRoomMainState.DepartureHoldFrames;
        Brightness = 15;
        Phase = CeresDeparturePhase.HoldingOnElevator;
    }

    /// <summary>
    /// Applies the elevator hold's post-gameplay decrement and reports the transition to the blackout.
    /// </summary>
    /// <returns>Whether the wrapping countdown reached zero or ran below it and entered the fade; the triggering admission frame is not counted.</returns>
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
    /// Applies one blackout fade-out call after ordinary gameplay.
    /// </summary>
    /// <returns>Whether brightness one went black and completed this timing owner; the caller owns checkpoint saving and cinematic dispatch.</returns>
    /// <exception cref="InvalidOperationException">The blackout-fade phase is not active.</exception>
    public bool StepFadeAfterGameplay()
    {
        if (Phase != CeresDeparturePhase.FadingToBlack)
            throw new InvalidOperationException("Ceres departure fade is not active.");

        // Room main explicitly sets both fade delays to zero. The fade-out therefore
        // steps brightness on every call. Brightness one becomes a blanked screen
        // rather than a representable brightness zero, which is exposed here as Complete.
        if (Brightness > 1)
        {
            Brightness--;
            return false;
        }

        Brightness = 0;
        Phase = CeresDeparturePhase.Complete;
        return true;
    }
}
