using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The game state of automatic reserve-tank recovery.
/// </summary>
/// <remarks>
/// This is an outer game-state owner, not an inventory convenience. It locks Samus, leaves
/// ordinary main gameplay running under the global time freeze, transfers one
/// energy unit per frame, and publishes the room-library refill sound on
/// every eighth frame of the frame counter.
/// </remarks>
public sealed class SamusReserveAutoRecoveryState
{
    /// <summary>True between the control setup that locks her for reserve recovery and the one that restores control.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Starts only the game's automatic-mode path, which only the AUTO reserve mode selects.</summary>
    public void Begin(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (samus.Health > 0)
            throw new InvalidOperationException("Reserve recovery requires nonpositive energy.");
        if (samus.ReserveTankMode != ReserveTankMode.Auto || samus.ReserveEnergy == 0)
            throw new InvalidOperationException("Automatic reserve recovery requires enabled reserves.");

        // The reserve-recovery lock conditionally locks Samus's input and movement.
        // The pair suppresses animation as well as input/movement. A generic input
        // gate alone leaves Samus's animation running during the frozen recovery frames.
        // If X-Ray installed its own pair earlier in this gameplay frame, the reserve-recovery
        // lock replaces those modes without ending the independently running cone effect.
        // It leaves Mother Brain's rainbow-beam pair in place.
        bool rainbowBeam = samus.Control.Mode.Movement == SamusMovementMode.RainbowBeam;
        if (!rainbowBeam)
            samus.Xray.RelinquishSamusControlForReserveRecovery();
        samus.Xray.FreezeSharedTimeForReserveRecovery();
        if (!rainbowBeam)
            samus.Control.LockStationary();
        IsActive = true;
    }

    /// <summary>Runs one reserve-tank refill step after the frame's gameplay.</summary>
    public SamusReserveAutoRecoveryStep StepAfterFrame(SamusState samus, int frameCounter)
    {
        Ensure.NotNull(samus);
        if (!IsActive)
            throw new InvalidOperationException("Reserve recovery has not begun.");

        bool refillSoundRequested = samus.ReserveEnergy != 0 && SamusCadenceDefinitions.ReserveRefillSoundEveryEighthUpdate.IsDue(frameCounter);

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
                // The game's below-zero repair after this decrement handled corrupt reserve counts; a count
                // decremented from a positive value reaches zero first.
                samus.ReserveEnergy = SamusQuantity.DecrementPositive(samus.ReserveEnergy);
            }
        }

        bool completed = samus.ReserveEnergy == 0;
        if (completed)
        {
            // The reserve-recovery state clears the one shared freeze flag before it restores
            // normal control. X-Ray phase five deliberately checks that flag and skips
            // cleanup when it is zero, which is the game's Reserve Mode glitch.
            samus.Xray.ClearSharedFreezeForReserveMode();
            // Restoring Samus's control restores her normal input and movement modes unless
            // Mother Brain's rainbow-beam pair holds them.
            if (samus.Control.Mode.Movement != SamusMovementMode.RainbowBeam)
                samus.Control.Unlock();
            IsActive = false;
        }

        return new SamusReserveAutoRecoveryStep(
            refillSoundRequested,
            completed);
    }
}
