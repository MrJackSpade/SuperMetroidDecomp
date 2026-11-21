namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The phases of Crystal Flash's movement: inactive, then raising Samus, draining her ammunition and finishing.</summary>
public abstract partial record CrystalFlashPhase
{
    /// <summary>Allows only the cases below.</summary>
    private CrystalFlashPhase()
    {
    }

    /// <summary>Crystal Flash does not move Samus; its suit colouring may still be waiting to restore her normal colours.</summary>
    public sealed partial record Inactive : CrystalFlashPhase;

    /// <summary>The phases in which Crystal Flash holds Samus's movement: raising her, draining her ammunition and finishing.</summary>
    public abstract partial record Moving : CrystalFlashPhase
    {
        /// <summary>Allows only the cases below.</summary>
        private Moving()
        {
        }

        /// <summary>Samus rises: her whole Y position goes up by <see cref="CrystalFlashDefinitions.RaiseStep"/> for <see cref="CrystalFlashDefinitions.RaiseUpdates"/>, then the raised center is captured and the bubble effect is requested.</summary>
        public sealed partial record Raising : Moving;

        /// <summary>The ammunition drains: missiles, supers, then power bombs are consumed, each restoring <see cref="CrystalFlashDefinitions.EnergyPerAmmunition"/> at the drain's cadence.</summary>
        public sealed partial record DrainingAmmo : Moving;

        /// <summary>Samus settles: Y increases by one pixel whenever it differs from the retained raised position, and the phase waits for the standing movement type before releasing input locks and requesting suit shade restoration.</summary>
        public sealed partial record Finishing : Moving;
    }
}
