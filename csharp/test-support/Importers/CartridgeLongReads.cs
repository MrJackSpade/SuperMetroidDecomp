namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The linear 24-bit cartridge read (#223) that only verification oracles need: a long pointer whose bank
/// byte follows its word on the 24-bit bus. Production reads its longs bank-relative through
/// <see cref="CartridgeWordReads"/>.
/// </summary>
[CpuBusAccess]
[SnesEncodingOwner]
internal static class CartridgeLongReads
{
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
