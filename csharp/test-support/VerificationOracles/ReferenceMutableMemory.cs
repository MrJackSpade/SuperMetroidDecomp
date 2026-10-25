using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

/// <summary>
/// Explicit mutable-only projection of older diagnostic guards. Each byte is
/// checked before consulting the fixture; it cannot route a cartridge address.
/// </summary>
internal static class ReferenceMutableMemory
{
    /// <summary>Retains an existing mutable bus or wraps a read-only diagnostic bus with region checks.</summary>
    public static ISnesMutableMemory From(ISnesAddressSpace source) =>
        source as ISnesMutableMemory ?? new GuardMemory(source);

    /// <summary>Confines fallback diagnostic reads to the caller-requested mutable region.</summary>
    [CpuBusBoundary("exact bus test: mutable-only projection of a diagnostic bus that rejects reads outside the requested WRAM/SRAM kind")]
    private sealed class GuardMemory(ISnesAddressSpace source) : ISnesMutableMemory
    {
        /// <summary>Reads WRAM only after confirming the requested address maps to work memory.</summary>
        public byte ReadWorkRamByte(int address) => Read(address, SnesDmaSourceKind.WorkRam);

        /// <summary>Rejects mismatched regions before forwarding a mutable-memory read to the diagnostic bus.</summary>
        private byte Read(int address, SnesDmaSourceKind kind)
        {
            if (SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) != kind)
                throw new InvalidOperationException($"Diagnostic {kind} read rejects ${address:X6}.");
            return CpuBusRegionRead.Read(source, address);
        }
    }
}
