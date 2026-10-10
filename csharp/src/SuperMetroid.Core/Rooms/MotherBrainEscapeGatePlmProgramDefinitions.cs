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

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (address is < FirstAddress or >= LastAddress) return false;
        TryReadMechanicsByte(address, out byte low);
        TryReadMechanicsByte((ushort)(address + 1), out byte high);
        value = (ushort)(low | high << 8);
        return true;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (address is < FirstAddress or > LastAddress) return false;
        ushort word = (address & ~1) switch
        {
            0xbb34 or 0xbb3a => 6,
            0xbb3e => 94,
            0xbb44 or 0xbb48 or 0xbb4c => 2,
            0xbb38 or 0xbb42 or 0xbb50 => (ushort)RoomPlmInstruction.Delete,
            0xbb36 or 0xbb4e => (ushort)MotherBrainEscapeGateDraw.Closed,
            0xbb3c or 0xbb4a => (ushort)MotherBrainEscapeGateDraw.HalfClosed,
            _ => (ushort)MotherBrainEscapeGateDraw.Open, // BB40 and BB46 only.
        };
        value = (byte)(word >> ((address & 1) * 8));
        return true;
    }
}
