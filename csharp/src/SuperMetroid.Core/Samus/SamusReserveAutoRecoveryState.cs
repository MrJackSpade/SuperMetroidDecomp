using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Game state <c>$1B</c>: automatic reserve-tank recovery from <c>$82:DC10-$82:DC7F</c>.
/// </summary>
/// <remarks>
/// This is an outer game-state owner, not an inventory convenience. It locks Samus, leaves
/// ordinary state-eight gameplay running under the global time-freeze word, transfers one
/// energy unit per accepted frame, and publishes the native library-three refill sound on
/// every eighth accepted NMI.
/// </remarks>
public sealed class SamusReserveAutoRecoveryState
{
    /// <summary>True between Samus command <c>$1B</c> and command <c>$10</c>.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Starts only the retail automatic-mode path selected by reserve-mode bit zero.</summary>
    public void Begin(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (samus.Health > 0)
            throw new InvalidOperationException("Reserve recovery requires nonpositive energy.");
        if (samus.ReserveTankMode != ReserveTankMode.Auto || samus.ReserveEnergy == 0)
            throw new InvalidOperationException("Automatic reserve recovery requires enabled reserves.");

        // CallSomeSamusCode($1B) conditionally installs the ordinary locked handler pair.
        // The pair suppresses animation as well as input/movement. A generic input
        // gate alone leaves AnimateSamus running during the frozen recovery frames.
        // If X-Ray installed its own pair earlier in this state-eight frame, command
        // $1B replaces those handlers without deleting the independently running HDMA.
        // It leaves Mother Brain's rainbow-beam pair in place ($90:F414).
        bool rainbowBeam = samus.Control.Mode.Beta == SamusBetaHandler.RainbowBeam;
        if (!rainbowBeam)
            samus.Xray.RelinquishSamusControlForReserveRecovery();
        samus.Xray.FreezeSharedTimeForReserveRecovery();
        if (!rainbowBeam)
            samus.Control.LockStationary();
        IsActive = true;
    }

    /// <summary>Runs <c>RefillHealthFromReserveTanks</c> once after the frame's accepted NMI.</summary>
    public SamusReserveAutoRecoveryStep StepAfterFrame(SamusState samus, int frameCounter)
    {
        Ensure.NotNull(samus);
        if (!IsActive)
            throw new InvalidOperationException("Reserve recovery has not begun.");

        bool refillSoundRequested = samus.ReserveEnergy != 0 && (frameCounter & 7) == 0;

        if (samus.ReserveEnergy != 0)
        {
            samus.Health++;
            if (samus.Health >= samus.MaxHealth)
            {
                samus.Health = samus.MaxHealth;
                samus.ReserveEnergy = 0;
            }
            else
            {
                // Native's signed-underflow repair after this decrement handled corrupt reserve words; a count
                // decremented from a positive value reaches zero first.
                samus.ReserveEnergy = SamusQuantity.DecrementPositive(samus.ReserveEnergy);
            }
        }

        bool completed = samus.ReserveEnergy == 0;
        if (completed)
        {
            // State $1B clears the one shared freeze word before command $10 restores
            // normal control. X-Ray phase five deliberately checks that word and skips
            // cleanup when it is zero, which is the cartridge's Reserve Mode glitch.
            samus.Xray.ClearSharedFreezeForReserveMode();
            // CallSomeSamusCode($10) restores the ordinary input/movement handlers unless
            // Mother Brain's rainbow-beam pair holds them ($90:F2E3).
            if (samus.Control.Mode.Beta != SamusBetaHandler.RainbowBeam)
                samus.Control.Unlock();
            IsActive = false;
        }

        return new SamusReserveAutoRecoveryStep(
            refillSoundRequested,
            completed);
    }
}
