using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SnesVram"/> members production does not use.</summary>
internal static class SnesVramAccess
{
    /// <summary>Runs literal WRAM or SRAM DMA into VRAM for hardware boundary checks.</summary>
    extension(SnesVram self)
    {
        /// <summary>
        /// Executes a literal DMA channel transfer, including the SNES DAS-zero convention.
        /// </summary>
        /// <param name="memory">Live WRAM/SRAM from which the DMA channel reads; never a cartridge source.</param>
        /// <param name="sourceAddress">Fixed source bank plus initial 16-bit offset.</param>
        /// <param name="dmaSize">
        /// Raw 16-bit DAS register. Values one through <c>$FFFF</c> transfer that many bytes;
        /// zero transfers <c>$10000</c> bytes because the channel decrements through the full
        /// sixteen-bit counter before reaching zero again.
        /// </param>
        /// <param name="encodedDestination">
        /// Initial VMADD word plus this model's VMAIN-column marker in bit 15.
        /// </param>
        [PpuLayoutBoundary("exact hardware test: drives one general-purpose DMA into VRAM")]
        internal void ExecuteHardwareMemoryDmaWrite(
            ISnesMutableMemory memory,
            int sourceAddress,
            ushort dmaSize,
            ushort encodedDestination)
        {
            Ensure.NotNull(memory);
            if ((uint)sourceAddress > 0x00ff_ffff)
                throw new ArgumentOutOfRangeException(nameof(sourceAddress), sourceAddress, "DMA source must be a 24-bit CPU address.");

            int effectiveSize = dmaSize == 0 ? 0x10000 : dmaSize;
            self.ExecuteDmaWrite((ISnesMutableMemory)(memory), (SnesAddress)(SnesAddress.FromBusAddress(sourceAddress)), (int)(effectiveSize), (ushort)(encodedDestination));
        }
    }
}
