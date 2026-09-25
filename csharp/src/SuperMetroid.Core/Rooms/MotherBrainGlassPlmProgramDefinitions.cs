namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored bank-$84 instruction data for Mother Brain's glass. This is
/// control flow and draw-list selection, not the eleven physical draw payloads or
/// the neighboring $D2F9/$D30B callback machine code.
/// </summary>
internal static class MotherBrainGlassPlmProgramDefinitions
{
    /// <summary>Start of the glass PLM's main instruction list at $84:D202.</summary>
    internal const ushort FirstAddress = 0xd202;
    /// <summary>Last byte of the no-glass branch at $84:D2F8.</summary>
    internal const ushort LastAddress = 0xd2f8;

    private static readonly byte[] Program = Convert.FromHexString(
        "0E8801EDD22D880200F3D2C186E6D101" +
        "001797F9D2020011D201001D97F9D204" +
        "001BD201003197F9D2060025D20BD300" +
        "00000000000000040045970BD3000000" +
        "000000000001004597F9D2080047D201" +
        "004F97F9D20A0051D201006997F9D20C" +
        "005BD20BD30200020002000200040081" +
        "970BD3020002000200020001008197F9" +
        "D20E007DD20BD3000000000200020004" +
        "008F970BD3040004000400040001008F" +
        "97F9D210009FD20BD302000200040004" +
        "000400B7970BD3020002000400040001" +
        "00B797F9D21200C1D20BD30200020004" +
        "0004000400E7970BD302000200040004" +
        "003000E7973E880200BC8601001798BC" +
        "860100E797BC86");

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
