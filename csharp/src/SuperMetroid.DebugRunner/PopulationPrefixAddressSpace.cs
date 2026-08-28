using SuperMetroid.Core.Hardware;

/// <summary>
/// Read-only audit decorator that exposes an unchanged prefix of one retail enemy population
/// as a complete native list. Only its following three bytes are replaced with the standard
/// <c>$FFFF, quota</c> terminator; headers, parameters, code, graphics, and level data remain
/// cartridge-backed. Production room loading never references this DebugRunner-only type.
/// </summary>
internal sealed class PopulationPrefixAddressSpace : ISnesAddressSpace
{
    private readonly ISnesAddressSpace _inner;
    private readonly int _terminatorAddress;
    private readonly byte _deathQuota;

    public PopulationPrefixAddressSpace(
        ISnesAddressSpace inner,
        ushort populationPointer,
        int retainedRecordCount,
        byte deathQuota)
    {
        ArgumentNullException.ThrowIfNull(inner);
        if (retainedRecordCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retainedRecordCount));

        _inner = inner;
        _terminatorAddress = 0xa10000 |
            unchecked((ushort)(populationPointer + retainedRecordCount * 16));
        _deathQuota = deathQuota;
    }

    public byte ReadByte(int address)
    {
        if (address == _terminatorAddress || address == _terminatorAddress + 1)
            return 0xff;
        if (address == _terminatorAddress + 2)
            return _deathQuota;
        return _inner.ReadByte(address);
    }

    public void WriteByte(int address, byte value) =>
        _inner.WriteByte(address, value);
}
