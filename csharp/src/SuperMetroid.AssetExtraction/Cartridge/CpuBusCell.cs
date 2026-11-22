
namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// One named CPU-bus location or bounded range (#629). Domain catalog types declare every address
/// the port knows as a static <see cref="CpuBusCell"/> with its native symbol and meaning in XML
/// documentation; raw cell primitives are constructed only from these descriptors. The repository
/// inventory and alias-coherence checks enumerate them.
/// </summary>
public sealed partial record CpuBusCell
{
    /// <summary>A cell of its parts; only its own operations make one.</summary>
    /// <param name="name">The semantic identity, unique within its declaring catalog.</param>
    /// <param name="region">The store the cell lives in.</param>
    /// <param name="address">The 24-bit CPU address of the first byte.</param>
    /// <param name="byteCount">The cell's size in bytes (element size times count for a table).</param>
    /// <param name="encoding">How the bytes encode the domain value, e.g. "u16 little-endian" or "SamusPose (u16)".</param>
    /// <param name="aliasOf">The cell this one intentionally shares bytes with under a different meaning, if any.</param>
    /// <exception cref="ArgumentOutOfRangeException">The address is past 24 bits, or the cell holds no byte.</exception>
    internal CpuBusCell(string name, CpuBusRegion region, int address, int byteCount, string encoding, CpuBusCell? aliasOf = null)
    {
        Name = name;
        Region = region;
        Address = ((uint)address & ~(uint)SnesCpuAddressLayout.AddressMask) == 0
            ? address
            : throw new ArgumentOutOfRangeException(nameof(address), address, "SNES CPU address must fit in 24 bits.");
        ByteCount = byteCount > 0
            ? byteCount
            : throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount, "A bus cell holds at least one byte.");
        Encoding = encoding;
        AliasOf = aliasOf;
    }

    /// <summary>A bounded cartridge range read by the asset importer.</summary>
    public static CpuBusCell CartridgeBytes(string name, int address, int length, string encoding, CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.Cartridge, address, length, encoding, aliasOf);

    /// <summary>The semantic identity, unique within its declaring catalog.</summary>
    internal string Name { get; init; }

    /// <summary>The store the cell lives in.</summary>
    public CpuBusRegion Region { internal get; init; }

    /// <summary>The 24-bit CPU address of the first byte.</summary>
    public int Address { get; }

    /// <summary>The cell's size in bytes.</summary>
    internal int ByteCount { get; }

    /// <summary>How the bytes encode the domain value, e.g. "u16 little-endian" or "SamusPose (u16)".</summary>
    public string Encoding { get; init; }

    /// <summary>The cell this one intentionally shares bytes with under a different meaning, if any.</summary>
    public CpuBusCell? AliasOf { get; init; }

    /// <inheritdoc />
    public override string ToString() =>
        $"{Name} ({Region} ${SnesAddressMath.BankNumber(Address):X2}:{SnesAddressMath.OffsetInBank(Address):X4}, {ByteCount} bytes, {Encoding}" +
        (AliasOf is { } target ? $", alias of {target.Name})" : ")");
}
