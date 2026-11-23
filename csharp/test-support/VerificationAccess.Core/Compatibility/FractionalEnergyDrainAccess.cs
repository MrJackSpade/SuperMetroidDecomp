namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="FractionalEnergyDrain"/>: a drain a check states, reached as a drain reaches it.</summary>
internal static class FractionalEnergyDrainAccess
{
    /// <summary>The parts the cartridge counts one energy in: its drains and periodic damage borrow an energy at 65536.</summary>
    internal const int CartridgePartsPerEnergy = 65536;

    /// <summary>States a drain through its own arithmetic.</summary>
    extension(FractionalEnergyDrain)
    {
        /// <summary>
        /// A drain holding <paramref name="heldOver"/> parts over, 0 through one part short of an energy: a new drain after
        /// the fraction that leaves exactly that much of the energy it takes.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="heldOver"/> is outside one energy's parts.</exception>
        internal static FractionalEnergyDrain HoldingOver(int heldOver) => FractionalEnergyDrain.Start.After(
            (CartridgePartsPerEnergy - Ensure.BetweenInclusive(0, CartridgePartsPerEnergy - 1, heldOver)) %
                CartridgePartsPerEnergy);
    }
}
