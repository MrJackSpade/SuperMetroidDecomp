namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bomb Torizo's bank-$84 grey-door instruction lists. The closing list is
/// interrupted by the native $BA6F Bombs-condition callback, whose machine
/// code is deliberately not part of this compiled instruction data.
/// </summary>
internal static class BombTorizoGreyDoorPlmProgramDefinitions
{
    /// <summary>Bomb-gated closing instruction list at $84:BA4C.</summary>
    internal const ushort ClosingStart = 0xba4c;
    /// <summary>Last closing-list byte before callback code at $84:BA6E.</summary>
    internal const ushort ClosingEnd = 0xba6e;
    /// <summary>Condition-gated resident instruction list at $84:BA7F.</summary>
    internal const ushort ResidentStart = 0xba7f;
    /// <summary>Last opening-list byte before unused setup code at $84:BAD0.</summary>
    internal const ushort ResidentEnd = 0xbad0;

    private static readonly byte[] Closing = Convert.FromHexString(
        "020083A66FBA4CBA280083A6198C080200FBA60200EFA60200E3A60100D7A624877FBA");
    private static readonly byte[] Resident = Convert.FromHexString(
        "728AE2C4248A93BA3FBE0100D7A6B48624878DBA248AB7BAC1860FBD0300EFA9" +
        "0400D7A60300EFA90400D7A60300EFA90400D7A624879BBA918A01BCBA198C07" +
        "0400E3A60400EFA60400FBA6010083A6BC86");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (TryGetBytes(address, out byte[] bytes, out int offset) && offset + 1 < bytes.Length)
        {
            value = (ushort)(bytes[offset] | bytes[offset + 1] << 8);
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (TryGetBytes(address, out byte[] bytes, out int offset))
        {
            value = bytes[offset];
            return true;
        }
        value = 0;
        return false;
    }

    private static bool TryGetBytes(ushort address, out byte[] bytes, out int offset)
    {
        if (address >= ClosingStart && address <= ClosingEnd)
        {
            bytes = Closing;
            offset = address - ClosingStart;
            return true;
        }
        if (address >= ResidentStart && address <= ResidentEnd)
        {
            bytes = Resident;
            offset = address - ResidentStart;
            return true;
        }
        bytes = Array.Empty<byte>();
        offset = 0;
        return false;
    }
}
