using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>
/// Import-only reference DMA for comparisons with installed artwork. Linked into
/// diagnostic executables, never Core or the shipped gameplay hosts.
/// </summary>
internal static class ImportedVramOracle
{
    /// <summary>Queues a nonempty reference DMA transfer using the supplied source and VRAM destination.</summary>
    /// <param name="vram">VRAM queue that receives the copied bytes.</param>
    /// <param name="source">Address space used to read the DMA source.</param>
    /// <param name="sourceAddress">Bus address of the first source byte.</param>
    /// <param name="sizeInBytes">Number of bytes to copy; zero is rejected because queued writes reserve it.</param>
    /// <param name="encodedDestination">Native VRAM destination and increment-mode encoding.</param>
    public static void ExecuteQueued(SnesVram vram, ISnesAddressSpace source,
        int sourceAddress, ushort sizeInBytes, ushort encodedDestination)
    {
        if (sizeInBytes == 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        vram.ExecuteQueuedAssetWrite(ReadTransfer(source, sourceAddress, sizeInBytes),
            encodedDestination);
    }

    /// <summary>Applies a reference DMA transfer, interpreting a zero hardware size as 65,536 bytes.</summary>
    /// <param name="vram">VRAM receiving the transferred data.</param>
    /// <param name="source">Address space from which the source bytes are read.</param>
    /// <param name="sourceAddress">Bus address of the first source byte.</param>
    /// <param name="dmaSize">Native DMA size; zero selects the full 65,536-byte transfer.</param>
    /// <param name="encodedDestination">Native VRAM destination and increment-mode encoding.</param>
    public static void ExecuteHardware(SnesVram vram, ISnesAddressSpace source,
        int sourceAddress, ushort dmaSize, ushort encodedDestination)
    {
        byte[] bytes = ReadTransfer(source, sourceAddress, dmaSize == 0 ? 0x10000 : dmaSize);
        if (bytes.Length <= ushort.MaxValue)
            vram.ExecuteQueuedAssetWrite(bytes, encodedDestination);
        else
        {
            // The asset API reserves a zero count. A complete native DAS-zero
            // reference transfer is even-sized and can use the word-port API.
            var words = new ushort[bytes.Length / 2];
            for (int i = 0; i < words.Length; i++)
                words[i] = (ushort)(bytes[i * 2] | bytes[i * 2 + 1] << 8);
            vram.ExecuteWordTransfer(words, (ushort)(encodedDestination & 0x7fff),
                (encodedDestination & 0x8000) == 0 ? 1 : 32);
        }
    }

    /// <summary>Drains queued writes through the import-only reference source and mutable-memory view.</summary>
    /// <param name="queue">Pending VRAM writes to apply.</param>
    /// <param name="vram">VRAM that receives the drained writes.</param>
    /// <param name="source">Cartridge and mutable-memory address space used to resolve queued source bytes.</param>
    public static void Drain(VramWriteQueue queue, SnesVram vram, ISnesAddressSpace source) =>
        queue.DrainTo(vram, ReferenceMutableMemory.From(source), new ImportedTransfers(source));

    /// <summary>Reads a DMA range byte by byte, routing each address according to its mapped source region.</summary>
    /// <param name="source">Address space supplying cartridge and mutable-memory bytes.</param>
    /// <param name="sourceAddress">Bus address of the first byte in the transfer.</param>
    /// <param name="length">Number of bytes to read within the starting bank.</param>
    /// <returns>The copied bytes in transfer order.</returns>
    private static byte[] ReadTransfer(ISnesAddressSpace source, int sourceAddress, int length)
    {
        SnesAddress start = SnesAddress.FromBusAddress(sourceAddress);
        var bytes = new byte[length];
        ISnesMutableMemory memory = ReferenceMutableMemory.From(source);
        for (int i = 0; i < bytes.Length; i++)
        {
            SnesAddress address = start.AddWithinBank(i);
            bytes[i] = SnesDmaSourceMap.Classify(address) switch
            {
                SnesDmaSourceKind.Cartridge => CartridgeImportSource.Require(source)
                    .ReadCartridgeByte((int)address),
                SnesDmaSourceKind.WorkRam => memory.ReadWorkRamByte((int)address),
                SnesDmaSourceKind.SaveRam => memory.ReadSaveRamByte((int)address),
                _ => throw new InvalidOperationException($"Reference DMA source {address} is unmapped."),
            };
        }
        return bytes;
    }

    /// <summary>Resolves queued cartridge transfers by rereading the imported source range.</summary>
    /// <param name="source">Address space from which reference DMA bytes are read.</param>
    private sealed class ImportedTransfers(ISnesAddressSpace source) : IVramAssetProvider, IInstalledArtworkTransferSource
    {
        /// <summary>Rejects installed-asset lookup because this oracle represents raw cartridge transfers.</summary>
        /// <param name="asset">Asset identifier requested by the queue.</param>
        /// <returns>This method always throws because imported transfers are not installed assets.</returns>
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException($"Reference DMA has no installed asset {asset}.");

        /// <summary>Supplies bytes for cartridge-backed queued writes and declines other address regions.</summary>
        /// <param name="address">Bus address at which the queued transfer begins.</param>
        /// <param name="count">Number of bytes requested.</param>
        /// <param name="bytes">Receives the copied source bytes when the address is cartridge-mapped; otherwise default.</param>
        /// <returns><see langword="true"/> when the address maps to cartridge data; otherwise <see langword="false"/>.</returns>
        public bool TryResolve(int address, int count, out ReadOnlyMemory<byte> bytes)
        {
            if (SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) == SnesDmaSourceKind.Cartridge)
            {
                bytes = ReadTransfer(source, address, count);
                return true;
            }
            bytes = default;
            return false;
        }
    }
}

/// <summary>
/// Explicit mutable-only projection of older diagnostic guards. Each byte is
/// checked before consulting the fixture; it cannot route a cartridge address.
/// </summary>
internal static class ReferenceMutableMemory
{
    /// <summary>Returns mutable access when available, otherwise wraps the source with region checks.</summary>
    /// <param name="source">Address space whose mutable reads will be exposed.</param>
    /// <returns>The existing mutable-memory interface or a guard that restricts reads to mutable regions.</returns>
    public static ISnesMutableMemory From(ISnesAddressSpace source) =>
        source as ISnesMutableMemory ?? new GuardMemory(source);

    /// <summary>Restricts mutable-memory reads to the work-RAM and save-RAM regions.</summary>
    /// <param name="source">Underlying address space used after an address passes its region check.</param>
    private sealed class GuardMemory(ISnesAddressSpace source) : ISnesMutableMemory
    {
        /// <summary>Reads a byte only when its bus address belongs to work RAM.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The byte stored at the validated work-RAM address.</returns>
        public byte ReadWorkRamByte(int address) => Read(address, SnesDmaSourceKind.WorkRam);

        /// <summary>Reads a byte only when its bus address belongs to save RAM.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <returns>The byte stored at the validated save-RAM address.</returns>
        public byte ReadSaveRamByte(int address) => Read(address, SnesDmaSourceKind.SaveRam);

        /// <summary>Validates the mapped memory region before forwarding a byte read.</summary>
        /// <param name="address">Bus address to read.</param>
        /// <param name="kind">Mutable-memory region required for this read.</param>
        /// <returns>The byte returned by the underlying address space after validation.</returns>
        private byte Read(int address, SnesDmaSourceKind kind)
        {
            if (SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) != kind)
                throw new InvalidOperationException($"Diagnostic {kind} read rejects ${address:X6}.");
            return source.ReadByte(address);
        }
    }
}
