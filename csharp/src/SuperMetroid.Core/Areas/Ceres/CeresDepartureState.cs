namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Outer game-state timing for the final Ceres elevator hold and blackout.
/// </summary>
/// <remarks>
/// Room main <c>$89:ACC3</c> owns admission to this sequence, while bank <c>$82</c>
/// owns the subsequent states $20 and $21. Keeping this object outside the room state is
/// intentional: the room continues running through both states, but it cannot restart the
/// departure because its native <c>game_state == 8</c> predicate no longer succeeds.
/// </remarks>
public sealed class CeresDepartureState
{

    /// <summary>Current bank-$82 portion of the departure sequence.</summary>
    public CeresDeparturePhase Phase { get; private set; } = CeresDeparturePhase.Inactive;

    /// <summary>Live state-$20 countdown seeded to 60 by Samus code two.</summary>
    public NativeCountdown HoldFramesRemaining { get; private set; }

    /// <summary>Low nibble of native INIDISP while state $21 fades out.</summary>
    public byte Brightness { get; private set; } = 15;

    /// <summary>Starts state $20 from the room main's one-frame request.</summary>
    /// <exception cref="InvalidOperationException">The departure has already begun or completed; a state instance admits the sequence only once.</exception>
    public void Begin()
    {
        if (Phase != CeresDeparturePhase.Inactive)
            throw new InvalidOperationException("Ceres elevator departure was started twice.");

        HoldFramesRemaining = NativeCountdown.Start(CeresElevatorShaftRoomMainState.DepartureHoldFrames);
        Brightness = 15;
        Phase = CeresDeparturePhase.HoldingOnElevator;
    }

    /// <summary>
    /// Applies state <c>$20</c>'s post-gameplay decrement and reports the transition to $21.
    /// </summary>
    /// <returns>Whether the wrapping 16-bit countdown reached zero or signed underflow and entered the fade; the triggering admission frame is not counted.</returns>
    /// <exception cref="InvalidOperationException">The elevator-hold phase is not active.</exception>
    public bool StepHoldAfterGameplay()
    {
        if (Phase != CeresDeparturePhase.HoldingOnElevator)
            throw new InvalidOperationException("Ceres departure hold is not active.");

        // The native routine decrements after GameState_8 returns, then accepts both zero
        // and signed underflow. Begin() always seeds 60, so zero is the reachable result;
        // preserving the signed check documents and tests the actual 16-bit contract.
        HoldFramesRemaining = HoldFramesRemaining.Decrement().Remaining;
        if (!HoldFramesRemaining.IsZero && !HoldFramesRemaining.IsNegative)
            return false;

        Phase = CeresDeparturePhase.FadingToBlack;
        return true;
    }

    /// <summary>
    /// Applies one state-<c>$21</c> <c>HandleFadeOut</c> call after ordinary gameplay.
    /// </summary>
    /// <returns>Whether brightness one reached forced blank and completed this timing owner; the caller owns checkpoint saving and cinematic dispatch.</returns>
    /// <exception cref="InvalidOperationException">The blackout-fade phase is not active.</exception>
    public bool StepFadeAfterGameplay()
    {
        if (Phase != CeresDeparturePhase.FadingToBlack)
            throw new InvalidOperationException("Ceres departure fade is not active.");

        // Room main explicitly sets both fade words to zero. HandleFadeOut therefore
        // changes INIDISP on every call. Brightness one becomes hardware forced blank
        // rather than a representable brightness zero, which is exposed here as Complete.
        if (Brightness > 1)
        {
            Brightness = NativeWordCounter.DecrementByte(Brightness);
            return false;
        }

        Brightness = 0;
        Phase = CeresDeparturePhase.Complete;
        return true;
    }
}
