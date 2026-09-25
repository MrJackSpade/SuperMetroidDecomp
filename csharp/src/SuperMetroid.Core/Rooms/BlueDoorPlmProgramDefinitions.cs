namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored bank-$84 control lists for all four ordinary blue-door
/// orientations: opening, closing, and closed-cap handoff. The sixteen physical
/// draw layouts and their editable visual words are separate resources.
/// </summary>
internal static class BlueDoorPlmProgramDefinitions
{
    /// <summary>First blue-door opening list, facing left, at $84:C489.</summary>
    internal const ushort FirstAddress = 0xc489;
    /// <summary>Last byte of the closed facing-down list at $84:C54C.</summary>
    internal const ushort LastAddress = 0xc54c;
    /// <summary>Facing-left closing list at $84:C49E.</summary>
    internal const ushort ClosingLeft = 0xc49e;
    /// <summary>Facing-right closing list at $84:C4CF.</summary>
    internal const ushort ClosingRight = 0xc4cf;
    /// <summary>Facing-up closing list at $84:C500.</summary>
    internal const ushort ClosingUp = 0xc500;
    /// <summary>Facing-down closing list at $84:C531.</summary>
    internal const ushort ClosingDown = 0xc531;
    /// <summary>Facing-left closed-cap conversion list at $84:C4B1.</summary>
    internal const ushort ClosedLeft = 0xc4b1;
    /// <summary>Facing-right closed-cap conversion list at $84:C4E2.</summary>
    internal const ushort ClosedRight = 0xc4e2;
    /// <summary>Facing-up closed-cap conversion list at $84:C513.</summary>
    internal const ushort ClosedUp = 0xc513;
    /// <summary>Facing-down closed-cap conversion list at $84:C544.</summary>
    internal const ushort ClosedDown = 0xc544;

    private static readonly byte[] Program = Convert.FromHexString(
        "198C070600BFA90600CBA90600D7A95E" +
        "0077A6BC86020077A60200D7A9198C08" +
        "0200CBA90200BFA9F18A400100B3A9BC" +
        "86198C070600FBA9060007AA060013AA" +
        "5E0083A6BC86020083A6020013AA198C" +
        "08020007AA0200FBA9F18A410100EFA9" +
        "BC86198C07060037AA060043AA06004F" +
        "AA5E008FA6BC8602008FA602004FAA19" +
        "8C08020043AA020037AAF18A4201002B" +
        "AABC86198C07060073AA06007FAA0600" +
        "8BAA5E009BA6BC8602009BA602008BAA" +
        "198C0802007FAA020073AAF18A430100" +
        "67AABC86");

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
