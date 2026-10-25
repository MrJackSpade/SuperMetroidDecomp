using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>Writes into whichever address space a test holds: a writable test bus or the runtime's WRAM image.</summary>
internal static class SnesAddressSpaceWrites
{
    /// <summary>Writes through an address space held as its common identity.</summary>
    extension(ISnesAddressSpace self)
    {
        /// <summary>Writes <paramref name="value"/> at <paramref name="address"/>.</summary>
        /// <exception cref="InvalidOperationException">The address space has no write path.</exception>
        internal void WriteByte(int address, byte value)
        {
            switch (self)
            {
                case ISnesWritableAddressSpace writable:
                    writable.WriteByte(address, value);
                    break;
                case SuperMetroidAddressSpace image:
                    image.WriteByte(address, value);
                    break;
                default:
                    throw new InvalidOperationException($"{self.GetType().Name} has no write path; it cannot take a write at ${address:X6}.");
            }
        }
    }
}
