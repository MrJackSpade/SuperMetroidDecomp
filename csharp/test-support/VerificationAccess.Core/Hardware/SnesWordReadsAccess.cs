using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SnesWordReads"/> members production does not use.</summary>
[CpuBusAccess]
internal static class SnesWordReadsAccess
{
    /// <summary>Word reads over mutable SNES memory that only verification performs.</summary>
    extension(ISnesMutableMemory memory)
    {
        /// <summary>Reads the little-endian WRAM word at <paramref name="cpuAddress"/> on the linear 24-bit bus.</summary>
        public ushort ReadWorkRamWord(int cpuAddress)
        {
            Ensure.NotNull(memory);
            byte low = memory.ReadWorkRamByte(cpuAddress);
            return (ushort)(low | memory.ReadWorkRamByte(cpuAddress + 1) << 8);
        }
    }
}
