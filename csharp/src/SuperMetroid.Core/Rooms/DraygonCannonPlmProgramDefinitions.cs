namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Retail left/right Draygon cannon PLM control lists. The intervening bank-$84
/// lists belong to unused diagonal orientations and are deliberately not claimed.
/// Physical cannon draw payloads remain separate from these mechanics words.
/// </summary>
internal static class DraygonCannonPlmProgramDefinitions
{
    /// <summary>Right-facing shielded and destroyed lists, $84:DCDE-DD26.</summary>
    internal const ushort RightStart = 0xdcde;
    /// <summary>End of right-facing list, $84:DD26.</summary>
    internal const ushort RightEnd = 0xdd26;
    /// <summary>Left-facing shielded and destroyed lists, $84:DDB9-DE01.</summary>
    internal const ushort LeftStart = 0xddb9;
    /// <summary>End of left-facing list, $84:DE01.</summary>
    internal const ushort LeftEnd = 0xde01;

    private static readonly byte[] Right = Convert.FromHexString(
        "248AF0DCC18664DB0800CD9FB4862487" +
        "E6DCCD8A0311DD0300CD9F0400DD9F03" +
        "00CD9F0400DD9F0300CD9F0400DD9F24" +
        "87E6DC8EDB06002DA006003DA006004D" +
        "A006005DA0248713DD");
    private static readonly byte[] Left = Convert.FromHexString(
        "248ACBDDC18664DB0800EDA0B4862487" +
        "C1DDCD8A03ECDD0300EDA0040001A103" +
        "00EDA0040001A10300EDA0040001A124" +
        "87C1DD36DC060065A1060079A106008D" +
        "A10600A1A12487EEDD");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (TryGetBytes(address, out byte[]? bytes, out int offset) && offset + 1 < bytes.Length)
        {
            value = (ushort)(bytes[offset] | bytes[offset + 1] << 8);
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (TryGetBytes(address, out byte[]? bytes, out int offset))
        {
            value = bytes[offset];
            return true;
        }
        value = 0;
        return false;
    }

    private static bool TryGetBytes(ushort address, out byte[] bytes, out int offset)
    {
        if (address >= RightStart && address <= RightEnd)
        {
            bytes = Right;
            offset = address - RightStart;
            return true;
        }
        if (address >= LeftStart && address <= LeftEnd)
        {
            bytes = Left;
            offset = address - LeftStart;
            return true;
        }
        bytes = Array.Empty<byte>();
        offset = 0;
        return false;
    }
}
