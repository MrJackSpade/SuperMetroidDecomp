using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>
/// Import-only reference DMA for comparisons with installed artwork. Linked into
/// diagnostic executables, never Core or the shipped gameplay hosts.
/// </summary>
internal static class ImportedVramOracle
{
    public static void ExecuteQueued(SnesVram vram, ISnesAddressSpace source,
        int sourceAddress, ushort sizeInBytes, ushort encodedDestination)
    {
        if (sizeInBytes == 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        vram.ExecuteQueuedAssetWrite(ReadTransfer(source, sourceAddress, sizeInBytes),
            encodedDestination);
    }

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

    public static void Drain(VramWriteQueue queue, SnesVram vram, ISnesAddressSpace source) =>
        queue.DrainTo(vram, ReferenceMutableMemory.From(source), new ImportedTransfers(source));

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

    private sealed class ImportedTransfers(ISnesAddressSpace source) : IVramAssetProvider, IRomArtworkSource
    {
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException($"Reference DMA has no installed asset {asset}.");

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
    public static ISnesMutableMemory From(ISnesAddressSpace source) =>
        source as ISnesMutableMemory ?? new GuardMemory(source);

    private sealed class GuardMemory(ISnesAddressSpace source) : ISnesMutableMemory
    {
        public byte ReadWorkRamByte(int address) => Read(address, SnesDmaSourceKind.WorkRam);
        public byte ReadSaveRamByte(int address) => Read(address, SnesDmaSourceKind.SaveRam);

        private byte Read(int address, SnesDmaSourceKind kind)
        {
            if (SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) != kind)
                throw new InvalidOperationException($"Diagnostic {kind} read rejects ${address:X6}.");
            return source.ReadByte(address);
        }
    }
}
