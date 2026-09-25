namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control and draw-selector bytes for Bomb Torizo's crumbling
/// Chozo hand. The physical draw lists at $9877/$989D remain separate.
/// </summary>
internal static class BombTorizoHandPlmProgramDefinitions
{
    /// <summary>First byte of the hand instruction list at $84:D368.</summary>
    internal const ushort FirstAddress = 0xd368;
    /// <summary>Last byte of the hand instruction list at $84:D3C6.</summary>
    internal const ushort LastAddress = 0xd3c6;

    private static readonly byte[] Program = Convert.FromHexString(
        "01007798C1863BD3B48678007798E587" +
        "000400B2AD006E6000779857D3000030" +
        "00779857D302000F00779857D304000E" +
        "00779857D306000D00779857D308000C" +
        "00779857D30A000B00779857D30C000A" +
        "00779857D30E0001009D98C7D3BC86");

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
