using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Core.Hardware;

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
            self.ExecuteDmaWrite(memory, SnesAddress.FromBusAddress(sourceAddress), effectiveSize, encodedDestination);
        }

        /// <summary>
        /// Executes one queued-table record whose A-bus source is live WRAM, as the NMI drain's DMA channel 1 would.
        /// Production queues only installed artwork; verification uses this to stage native WRAM images in VRAM.
        /// </summary>
        /// <param name="memory">Live WRAM from which the channel reads.</param>
        /// <param name="sourceAddress">Fixed source bank plus initial 16-bit offset.</param>
        /// <param name="sizeInBytes">Number of source bytes copied, 1 through 65535.</param>
        /// <param name="encodedDestination">Initial VMADD word plus this model's VMAIN-column marker in bit 15.</param>
        [CpuBusBoundary("exact bus test: a queued DMA channel 1 transfer from live WRAM")]
        internal void ExecuteQueuedMemoryWrite(ISnesMutableMemory memory, int sourceAddress, int sizeInBytes, ushort encodedDestination)
        {
            Ensure.NotNull(memory);
            if ((uint)sourceAddress > SnesBusLayout.MaximumAddress)
                throw new ArgumentOutOfRangeException(nameof(sourceAddress), sourceAddress, "DMA source must be a 24-bit CPU address.");
            if (sizeInBytes is <= 0 or > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), sizeInBytes, "A queued DMA moves 1 through 65535 bytes.");
            self.ExecuteDmaWrite(memory, SnesAddress.FromBusAddress(sourceAddress), sizeInBytes, encodedDestination);
        }

        /// <summary>
        /// The mode-$01 DMA loop into VRAM: alternate low and high port bytes, increment the word address after each
        /// high byte by the destination's step, and read each source byte from WRAM. A cartridge source needs installed
        /// artwork, and an unmapped one is rejected.
        /// </summary>
        [CpuBusBoundary("exact bus test: a DMA channel's A-bus source reads, classified per byte")]
        internal void ExecuteDmaWrite(ISnesMutableMemory memory, SnesAddress sourceAddress, int sizeInBytes, ushort encodedDestination)
        {
            if (sizeInBytes is <= 0 or > SnesBusLayout.BankBytes)
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
            VramDmaDestination destination = VramDmaDestination.FromNative(encodedDestination);
            int destinationWord = destination.Word;
            for (int byteIndex = 0; byteIndex < sizeInBytes; byteIndex++)
            {
                bool writesHighByte = (byteIndex & 1) != 0;
                SnesAddress currentSource = sourceAddress.AddWithinBank(byteIndex);
                byte value = SnesDmaSourceMap.Classify(currentSource) switch
                {
                    SnesDmaSourceKind.WorkRam => memory.ReadWorkRamByte((int)currentSource),
                    SnesDmaSourceKind.Cartridge => throw new InvalidOperationException(
                        $"VRAM DMA source {currentSource} requires installed artwork bytes."),
                    SnesDmaSourceKind.Unmapped => throw new InvalidOperationException(
                        $"VRAM DMA source ${currentSource.Bank:X2}:{currentSource.Offset:X4} is unmapped."),
                    _ => throw new InvalidOperationException(
                        $"VRAM DMA source ${currentSource.Bank:X2}:{currentSource.Offset:X4} is unmapped."),
                };
                self.LoadBytes(destinationWord * 2 + (writesHighByte ? 1 : 0), [value]);
                if (writesHighByte)
                    destinationWord = (destinationWord + destination.WordIncrement) & (SnesVram.WordCount - 1);
            }
        }
        /// <summary>The current image, read through the capture the display takes; writes pass through the DMA-aware methods.</summary>
        public ReadOnlySpan<byte> Bytes => self.CaptureImage();
        /// <summary>Reads a byte by physical VRAM byte offset, through the word that holds it.</summary>
        public byte ReadByte(int byteOffset)
        {
            if ((uint)byteOffset >= SnesVram.ByteCount)
                throw new ArgumentOutOfRangeException(nameof(byteOffset));
            return (byte)(self.ReadWord(byteOffset / 2) >> byteOffset % 2 * 8);
        }
        /// <summary>Fills Mode 7 low bytes as repeated writes to $2118 do.</summary>
        public void FillMode7MapBytes(byte value, int wordCount, ushort destinationWord = 0)
        {
            if ((uint)wordCount > SnesVram.WordCount)
                throw new ArgumentOutOfRangeException(nameof(wordCount));
            var bytes = new byte[wordCount];
            bytes.AsSpan().Fill(value);
            self.LoadMode7MapBytes(bytes, destinationWord);
        }
    }
}
