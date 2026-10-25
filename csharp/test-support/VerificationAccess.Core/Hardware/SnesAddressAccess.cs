using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SnesAddress"/> members production does not use.</summary>
internal static class SnesAddressAccess
{
    /// <summary>Validates upper LoROM cartridge addresses in verification fixtures.</summary>
    extension(SnesAddress)
    {
        /// <summary>
        /// Creates an address that must refer to an upper LoROM cartridge window.
        /// </summary>
        internal static SnesAddress FromUpperLoRom(byte bank, ushort offset)
        {
            var address = new SnesAddress(bank, offset);
            if (!address.IsUpperLoRomWindow)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(offset),
                    offset,
                    "An upper LoROM cartridge address must be in the $8000-$FFFF window.");
            }
            return address;
        }
    }
}
