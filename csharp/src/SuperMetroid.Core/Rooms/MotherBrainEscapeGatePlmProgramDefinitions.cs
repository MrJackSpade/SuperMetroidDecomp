namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bank-$84 instruction data for the gate that closes in the room after
/// Mother Brain. The interval includes the normal closed list, an unused
/// half-closed/open list, and the door-transition closing list.
/// </summary>
internal static class MotherBrainEscapeGatePlmProgramDefinitions
{
    /// <summary>First closed-gate instruction at $84:BB34.</summary>
    internal const ushort FirstAddress = 0xbb34;
    /// <summary>Last closing-gate instruction byte at $84:BB51.</summary>
    internal const ushort LastAddress = 0xbb51;

    private static readonly byte[] Program = Convert.FromHexString(
        "06008B94BC8606007F945E007394BC860200739402007F9402008B94BC86");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - FirstAddress;
        if ((uint)offset < Program.Length - 1)
        {
            value = (ushort)(Program[offset] | Program[offset + 1] << 8);
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        int offset = address - FirstAddress;
        if ((uint)offset < Program.Length)
        {
            value = Program[offset];
            return true;
        }
        value = 0;
        return false;
    }
}
