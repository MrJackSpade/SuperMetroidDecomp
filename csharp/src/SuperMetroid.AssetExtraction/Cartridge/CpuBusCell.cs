using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// One named CPU-bus location or bounded range (#629). Domain catalog types declare every address
/// the port knows as a static <see cref="CpuBusCell"/> with its native symbol and meaning in XML
/// documentation; raw cell primitives are constructed only from these descriptors. The repository
/// inventory and alias-coherence checks enumerate them.
/// </summary>
/// <param name="Name">The semantic identity, unique within its declaring catalog.</param>
/// <param name="Region">The store the cell lives in.</param>
/// <param name="Address">The 24-bit CPU address of the first byte.</param>
/// <param name="ByteCount">The cell's size in bytes (element size times count for a table).</param>
/// <param name="Encoding">How the bytes encode the domain value, e.g. "u16 little-endian" or "SamusPose (u16)".</param>
/// <param name="AliasOf">The cell this one intentionally shares bytes with under a different meaning, if any.</param>
[CpuBusAccess]
public sealed record CpuBusCell(string Name, CpuBusRegion Region, int Address, int ByteCount, string Encoding,
    CpuBusCell? AliasOf = null)
{
    /// <summary>A bounded cartridge range read by the asset importer.</summary>
    public static CpuBusCell CartridgeBytes(string name, int address, int length, string encoding, CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.Cartridge, address, length, encoding, aliasOf);

    /// <summary>The 24-bit CPU address of the first byte.</summary>
    public int Address { get; } = ((uint)Address & ~(uint)SnesCpuAddressLayout.AddressMask) == 0
        ? Address
        : throw new ArgumentOutOfRangeException(nameof(Address), Address, "SNES CPU address must fit in 24 bits.");

    /// <summary>The cell's size in bytes.</summary>
    public int ByteCount { get; } = ByteCount > 0
        ? ByteCount
        : throw new ArgumentOutOfRangeException(nameof(ByteCount), ByteCount, "A bus cell holds at least one byte.");

    /// <inheritdoc />
    public override string ToString() =>
        $"{Name} ({Region} ${SnesAddressMath.BankNumber(Address):X2}:{SnesAddressMath.OffsetInBank(Address):X4}, {ByteCount} bytes, {Encoding}" +
        (AliasOf is { } target ? $", alias of {target.Name})" : ")");
}
