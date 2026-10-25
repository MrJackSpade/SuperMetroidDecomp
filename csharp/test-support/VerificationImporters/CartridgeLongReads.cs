using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// The linear 24-bit cartridge read (#223) that only verification oracles need: a long pointer whose bank
/// byte follows its word on the 24-bit bus. Production reads its longs bank-relative through
/// <see cref="CartridgeWordReads"/>.
/// </summary>
[EncodingOwner("cartridge 24-bit long reads")]
internal static class CartridgeLongReads
{
    /// <summary>Adds a linear 24-bit pointer read to imported cartridge sources for verification oracles.</summary>
    extension(IImportCartridgeSource cartridge)
    {
        /// <summary>Reads the little-endian 24-bit cartridge value (a long pointer) at <paramref name="cpuAddress"/> on the linear bus.</summary>
        public int ReadCartridgeLong(int cpuAddress)
        {
            Ensure.NotNull(cartridge);
            ushort low = cartridge.ReadCartridgeWord(cpuAddress);
            return low | cartridge.ReadCartridgeByte(cpuAddress + 2) << 16;
        }
    }
}
