namespace SuperMetroid.Core.Hardware;

/// <summary>Which CPU-visible store a <see cref="CpuBusCell"/> lives in.</summary>
public enum CpuBusRegion
{
    /// <summary>Work RAM, banks $7E/$7F and their low-window system-bank mirrors.</summary>
    WorkRam,

    /// <summary>Battery-backed save RAM, banks $70-$7D/$F0-$FF lower window.</summary>
    SaveRam,

    /// <summary>Cartridge ROM, read only by the asset importer.</summary>
    Cartridge,
}

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
    /// <summary>A little-endian WRAM word.</summary>
    public static CpuBusCell WramWord(string name, int address, string encoding = "u16 little-endian", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, 2, encoding, aliasOf);

    /// <summary>A WRAM byte.</summary>
    public static CpuBusCell WramByte(string name, int address, string encoding = "u8", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, 1, encoding, aliasOf);

    /// <summary>A bounded WRAM byte range.</summary>
    public static CpuBusCell WramBytes(string name, int address, int length, string encoding = "u8[]", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, length, encoding, aliasOf);

    /// <summary>A bounded table of little-endian WRAM words.</summary>
    public static CpuBusCell WramWords(string name, int address, int count, string encoding = "u16[] little-endian", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.WorkRam, address, count * 2, encoding, aliasOf);

    /// <summary>A bounded SRAM byte range.</summary>
    public static CpuBusCell SramBytes(string name, int address, int length, string encoding = "u8[]", CpuBusCell? aliasOf = null) =>
        new(name, CpuBusRegion.SaveRam, address, length, encoding, aliasOf);

    /// <summary>The 24-bit CPU address of the first byte.</summary>
    public int Address { get; } = Address is >= 0 and <= SnesCpuAddressLayout.AddressMask
        ? Address
        : throw new ArgumentOutOfRangeException(nameof(Address), Address, "SNES CPU address must fit in 24 bits.");

    /// <summary>The cell's size in bytes.</summary>
    public int ByteCount { get; } = ByteCount > 0
        ? ByteCount
        : throw new ArgumentOutOfRangeException(nameof(ByteCount), ByteCount, "A bus cell holds at least one byte.");

    /// <summary>Requires this to be a <paramref name="region"/> cell of exactly <paramref name="byteCount"/> bytes.</summary>
    internal void Require(CpuBusRegion region, int byteCount)
    {
        if (Region != region || ByteCount != byteCount)
            throw new ArgumentException($"{this} is not a {byteCount}-byte {region} cell.");
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{Name} ({Region} ${Address >> 16:X2}:{Address & 0xffff:X4}, {ByteCount} bytes, {Encoding}" +
        (AliasOf is { } target ? $", alias of {target.Name})" : ")");
}
