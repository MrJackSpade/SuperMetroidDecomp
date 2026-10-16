using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.CpuBusTestDoubles;

/// <summary>
/// One CPU read routed by address region to the source's typed contract (#629): cartridge addresses
/// to <see cref="IImportCartridgeSource"/>, WRAM and its mirrors and SRAM windows to
/// <see cref="ISnesMutableMemory"/>. A source without the contract an address needs, or an address
/// outside the runtime map, fails loudly. Used only by exact bus test doubles that wrap a source.
/// </summary>
[CpuBusAccess]
internal static class CpuBusRegionRead
{
    /// <summary>Routes one CPU address to its cartridge, WRAM, or SRAM contract and rejects unsupported regions.</summary>
    internal static byte Read(ISnesAddressSpace source, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.Cartridge => (source as IImportCartridgeSource ?? throw Missing(source, address, "cartridge"))
                .ReadCartridgeByte(address),
            SnesDmaSourceKind.WorkRam => (source as ISnesMutableMemory ?? throw Missing(source, address, "WRAM"))
                .ReadWorkRamByte(address),
            _ => throw new InvalidOperationException(
                $"CPU read ${SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4} is outside the runtime address map."),
        };

    /// <summary>Builds the failure used when a source lacks the interface required by the addressed region.</summary>
    private static InvalidOperationException Missing(ISnesAddressSpace source, int address, string region) =>
        new($"{source.GetType().FullName} cannot serve the {region} read at ${SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4}.");
}
