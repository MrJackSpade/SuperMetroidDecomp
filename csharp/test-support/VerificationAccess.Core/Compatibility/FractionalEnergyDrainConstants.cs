namespace SuperMetroid.Core.Compatibility;

/// <summary>Cartridge values and helpers of <see cref="FractionalEnergyDrain"/> that only verification reads.</summary>
internal static class FractionalEnergyDrainConstants
{
    /// <summary>A drain holding <paramref name="heldOver"/> parts over, 0 through one part short of an energy, as a parity check states one.</summary>
    public static FractionalEnergyDrain HoldingOver(int heldOver) => new(heldOver);
}
