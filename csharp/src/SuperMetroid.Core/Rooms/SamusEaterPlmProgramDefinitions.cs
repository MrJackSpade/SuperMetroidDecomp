namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The two fixed bank-$84 plant instruction lists at $84:ACB8..AD37. Their
/// timers, branches, callbacks, sound operand, and draw-list selectors are
/// application-owned control data; the selected block artwork is separate.
/// The byte layout preserves the cartridge's odd one-byte timer and sound
/// operands, so word reads at native instruction boundaries remain exact.
/// </summary>
internal static class SamusEaterPlmProgramDefinitions
{
    /// <summary>Floor plant program at $84:ACB8, selected by PLM $B6CB.</summary>
    internal const ushort FloorStart = 0xacb8;
    /// <summary>Ceiling plant program at $84:ACF8, selected by PLM $B6CF.</summary>
    internal const ushort CeilingStart = 0xacf8;
    /// <summary>First byte of the following treadmill program at $84:AD38.</summary>
    internal const ushort EndExclusive = 0xad38;

    private static readonly byte[] Floor = Convert.FromHexString(
        "C18689AC4E87040500619E0500459E0500619E108C319DAC05007D9E0500" +
        "619E0500459E0500619E9DAC05007D9E3F87BFACB1ACCA8660007D9E0100" +
        "0D9EBC86");
    private static readonly byte[] Ceiling = Convert.FromHexString(
        "C18689AC4E87080500ED9E0500D19E0500ED9E108C319DAC0500099F0500" +
        "ED9E0500D19E0500ED9E9DAC0500099F3F87FFACB1ACCA866000099F0100" +
        "999EBC86");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (TryReadMechanicsByte(address, out byte low) &&
            TryReadMechanicsByte(unchecked((ushort)(address + 1)), out byte high))
        {
            value = (ushort)(low | high << 8);
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        byte[]? program = address >= FloorStart && address < CeilingStart
            ? Floor
            : address >= CeilingStart && address < EndExclusive
                ? Ceiling
                : null;
        if (program is not null)
        {
            int offset = address - (address < CeilingStart ? FloorStart : CeilingStart);
            value = program[offset];
            return true;
        }
        value = 0;
        return false;
    }
}
