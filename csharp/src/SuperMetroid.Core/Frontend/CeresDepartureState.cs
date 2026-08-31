using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

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
    private ushort holdFramesRemaining;
    private byte brightness = 15;

    /// <summary>Current bank-$82 portion of the departure sequence.</summary>
    public CeresDeparturePhase Phase { get; private set; } = CeresDeparturePhase.Inactive;

    /// <summary>Live state-$20 countdown seeded to 60 by Samus code two.</summary>
    public ushort HoldFramesRemaining => holdFramesRemaining;

    /// <summary>Low nibble of native INIDISP while state $21 fades out.</summary>
    public byte Brightness => brightness;

    /// <summary>Starts state $20 from the room main's one-frame request.</summary>
    public void Begin()
    {
        if (Phase != CeresDeparturePhase.Inactive)
            throw new InvalidOperationException("Ceres elevator departure was started twice.");

        holdFramesRemaining = CeresElevatorShaftRoomMainState.DepartureHoldFrames;
        brightness = 15;
        Phase = CeresDeparturePhase.HoldingOnElevator;
    }

    /// <summary>
    /// Applies state <c>$20</c>'s post-gameplay decrement and reports the transition to $21.
    /// </summary>
    public bool StepHoldAfterGameplay()
    {
        if (Phase != CeresDeparturePhase.HoldingOnElevator)
            throw new InvalidOperationException("Ceres departure hold is not active.");

        // The native routine decrements after GameState_8 returns, then accepts both zero
        // and signed underflow. Begin() always seeds 60, so zero is the reachable result;
        // preserving the signed check documents and tests the actual 16-bit contract.
        holdFramesRemaining = unchecked((ushort)(holdFramesRemaining - 1));
        if (holdFramesRemaining != 0 && unchecked((short)holdFramesRemaining) >= 0)
            return false;

        Phase = CeresDeparturePhase.FadingToBlack;
        return true;
    }

    /// <summary>
    /// Applies one state-<c>$21</c> <c>HandleFadeOut</c> call after ordinary gameplay.
    /// </summary>
    public bool StepFadeAfterGameplay()
    {
        if (Phase != CeresDeparturePhase.FadingToBlack)
            throw new InvalidOperationException("Ceres departure fade is not active.");

        // Room main explicitly sets both fade words to zero. HandleFadeOut therefore
        // changes INIDISP on every call. Brightness one becomes hardware forced blank
        // rather than a representable brightness zero, which is exposed here as Complete.
        if (brightness > 1)
        {
            brightness--;
            return false;
        }

        brightness = 0;
        Phase = CeresDeparturePhase.Complete;
        return true;
    }

    /// <summary>Applies the current master-brightness nibble to a software-rendered frame.</summary>
    public void ApplyBrightness(Span<Rgba32> pixels)
    {
        MasterBrightnessFilter.Apply(pixels, brightness);
    }
}

/// <summary>Named bank-$82 phases owned by <see cref="CeresDepartureState"/>.</summary>
public enum CeresDeparturePhase
{
    Inactive,
    HoldingOnElevator,
    FadingToBlack,
    Complete,
}
