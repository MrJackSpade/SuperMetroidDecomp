namespace SuperMetroid.Core.Game;

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
        ArgumentNullException.ThrowIfNull(samus);
        if (unchecked((short)samus.Health) > 0)
            throw new InvalidOperationException("Reserve recovery requires nonpositive energy.");
        if ((samus.ReserveTankMode & 1) == 0 || samus.ReserveEnergy == 0)
            throw new InvalidOperationException("Automatic reserve recovery requires enabled reserves.");

        // CallSomeSamusCode($1B) conditionally installs the ordinary locked handler pair.
        // The pair suppresses animation as well as input/movement. A generic input
        // gate alone leaves AnimateSamus running during the frozen recovery frames.
        samus.SetStationaryScriptControlLock(true);
        IsActive = true;
    }

    /// <summary>Runs <c>RefillHealthFromReserveTanks</c> once after the frame's accepted NMI.</summary>
    public SamusReserveAutoRecoveryStep StepAfterNmi(SamusState samus, ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (!IsActive)
            throw new InvalidOperationException("Reserve recovery has not begun.");

        bool refillSoundRequested = samus.ReserveEnergy != 0 && (nmiFrameCounter & 7) == 0;
        ushort healthBefore = samus.Health;
        ushort reserveBefore = samus.ReserveEnergy;

        if (samus.ReserveEnergy != 0)
        {
            samus.Health = unchecked((ushort)(samus.Health + 1));
            if (unchecked((short)(samus.Health - samus.MaxHealth)) >= 0)
            {
                samus.Health = samus.MaxHealth;
                samus.ReserveEnergy = 0;
            }
            else
            {
                samus.ReserveEnergy = unchecked((ushort)(samus.ReserveEnergy - 1));
                if (samus.ReserveEnergy == 0)
                {
                    // This is the ordinary exhaustion branch. The signed-underflow repair
                    // below is retained for corrupt/debug WRAM parity even though a valid
                    // reserve word reaches exactly zero first.
                    samus.ReserveEnergy = 0;
                }
                else if ((samus.ReserveEnergy & 0x8000) != 0)
                {
                    samus.Health = unchecked((ushort)(samus.Health + samus.ReserveEnergy));
                    samus.ReserveEnergy = 0;
                }
            }
        }

        bool completed = samus.ReserveEnergy == 0;
        if (completed)
        {
            // CallSomeSamusCode($10) restores the ordinary input/movement handlers unless
            // a demo recorder owns them. This host never overlays demo playback here.
            samus.SetStationaryScriptControlLock(false);
            IsActive = false;
        }

        return new SamusReserveAutoRecoveryStep(
            healthBefore,
            samus.Health,
            reserveBefore,
            samus.ReserveEnergy,
            refillSoundRequested,
            completed);
    }
}

/// <summary>Inspectable publication from one native reserve-refill call.</summary>
public readonly record struct SamusReserveAutoRecoveryStep(
    ushort HealthBefore,
    ushort HealthAfter,
    ushort ReserveBefore,
    ushort ReserveAfter,
    bool RefillSoundRequested,
    bool Completed);
