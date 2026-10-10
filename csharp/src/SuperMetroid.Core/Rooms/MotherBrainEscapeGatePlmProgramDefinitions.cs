namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Decodes bank-$84 escape-gate programs BB34..BB51: closed for six frames,
/// the unused half/open sequence (six then ninety-four frames), and three closing
/// frames of two ticks each. Each list ends with Delete. Byte selection preserves
/// all thirty bytes and all twenty-nine overlapping little-endian word views;
/// no adjacent machine code, persistent byte blob or generated cache is included.
/// </summary>
internal static class MotherBrainEscapeGatePlmProgramDefinitions
{
    /// <summary>First closed-gate instruction at $84:BB34.</summary>
    internal const ushort FirstAddress = 0xbb34;
    /// <summary>Last closing-gate instruction byte at $84:BB51.</summary>
    internal const ushort LastAddress = 0xbb51;

    /// <summary>Reads the overlapping little-endian word view beginning at an address inside a compiled escape-gate list.</summary>
    /// <param name="address">The bank-local starting byte of the candidate word.</param>
    /// <param name="value">Receives the assembled word, or zero when the start address is outside the lists.</param>
    /// <returns><see langword="true"/> when the address begins within the compiled instruction byte range.</returns>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (address < FirstAddress || address >= LastAddress) return false;
        TryReadMechanicsByte(address, out byte low);
        TryReadMechanicsByte((ushort)(address + 1), out byte high);
        value = (ushort)(low | high << 8);
        return true;
    }

    /// <summary>Reads one compiled instruction byte, preserving the native byte selection within overlapping words.</summary>
    /// <param name="address">The bank-local byte address in the closed, open, or closing escape-gate program.</param>
    /// <param name="value">Receives the selected byte, or zero when the address is outside the compiled range.</param>
    /// <returns><see langword="true"/> when the address lies within the inclusive instruction byte range.</returns>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (address < FirstAddress || address > LastAddress) return false;
        ushort word = (address & ~1) switch
        {
            0xbb34 or 0xbb3a => 6,
            0xbb3e => 94,
            0xbb44 or 0xbb48 or 0xbb4c => 2,
            0xbb38 or 0xbb42 or 0xbb50 => RoomPlmInstructionCodes.Delete,
            0xbb36 or 0xbb4e => MotherBrainEscapeGatePlmDrawDefinitions.Closed,
            0xbb3c or 0xbb4a => MotherBrainEscapeGatePlmDrawDefinitions.HalfClosed,
            _ => MotherBrainEscapeGatePlmDrawDefinitions.Open, // BB40 and BB46 only.
        };
        value = (byte)(word >> ((address & 1) * 8));
        return true;
    }
}
