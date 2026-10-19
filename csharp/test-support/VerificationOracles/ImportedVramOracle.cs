using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Import-only reference DMA for comparisons with installed artwork. Linked into
/// diagnostic executables, never Core or the shipped gameplay hosts.
/// </summary>
internal static class ImportedVramOracle
{
    /// <summary>Applies a nonzero queued asset transfer from a reference bus at its encoded VRAM destination.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    public static void ExecuteQueued(SnesVram vram, ISnesAddressSpace source,
        int sourceAddress, ushort sizeInBytes, ushort encodedDestination)
    {
        if (sizeInBytes == 0) throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        vram.ExecuteQueuedAssetWrite(ReadTransfer(source, sourceAddress, sizeInBytes),
            encodedDestination);
    }

    /// <summary>Models hardware DAS zero as a full 64 KiB DMA, using the word port when the asset API cannot express it.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
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
                words[i] = (ushort)(SnesLittleEndian.Word(bytes[i * 2], bytes[i * 2 + 1]));
            vram.ExecuteWordTransfer(words, (ushort)(encodedDestination & 0x7fff),
                (encodedDestination & 0x8000) == 0 ? 1 : 32);
        }
    }

    /// <summary>Drains queued transfers with mutable-memory reads and an import-only cartridge resolver.</summary>
    public static void Drain(VramWriteQueue queue, SnesVram vram, ISnesAddressSpace source) =>
        queue.DrainTo(vram, new ImportedTransfers(source), null, BgArtworkFixtures.For(vram));

    /// <summary>Reads a bank-contained DMA span from the cartridge, WRAM, or SRAM region named by each address.</summary>
    [CpuBusBoundary("exact bus test: reference DMA source read (cartridge, WRAM or SRAM) for installed-artwork VRAM comparisons")]
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
                _ => throw new InvalidOperationException($"Reference DMA source {address} is unmapped."),
            };
        }
        return bytes;
    }

    /// <summary>Supplies cartridge bytes to a reference transfer while rejecting installed-asset lookups.</summary>
    private sealed class ImportedTransfers(ISnesAddressSpace source) : IVramAssetProvider, IInstalledArtworkTransferSource
    {
        /// <summary>Rejects asset IDs because this oracle represents bus DMA rather than installed resources.</summary>
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException($"Reference DMA has no installed asset {asset}.");

        /// <summary>Supplies a cartridge span only when the DMA source is mapped to ROM.</summary>
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
