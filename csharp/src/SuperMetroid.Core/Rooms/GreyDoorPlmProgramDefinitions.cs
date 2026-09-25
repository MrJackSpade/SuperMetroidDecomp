namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored bank-$84 instruction streams for all four ordinary grey
/// doors. Each orientation has a closing list and a condition-gated resident
/// list with flash and opening branches. The physical draws and editable tile
/// choices are separate resources; Bomb Torizo's special door is also separate.
/// </summary>
internal static class GreyDoorPlmProgramDefinitions
{
    /// <summary>First ordinary grey-door closing list at $84:BE59.</summary>
    internal const ushort FirstAddress = 0xbe59;
    /// <summary>Final byte of the facing-down opening list at $84:BFFC.</summary>
    internal const ushort LastAddress = 0xbffc;

    private static readonly byte[] Program = Convert.FromHexString(
        "020077A60200CBA6198C080200BFA60200B3A60100A7A6728AB1C4248A84BE3F" +
        "BE0100A7A6B48624877EBE248AA8BEC1860FBD0300B3A90400A7A60300B3A904" +
        "00A7A60300B3A90400A7A624878CBE918A01ADBE198C070400B3A60400BFA604" +
        "00CBA6010077A6BC86020083A60200FBA6198C080200EFA60200E3A60100D7A6" +
        "728AE2C4248AEDBE3FBE0100D7A6B4862487E7BE248A11BFC1860FBD0300EFA9" +
        "0400D7A60300EFA90400D7A60300EFA90400D7A62487F5BE918A0116BF198C07" +
        "0400E3A60400EFA60400FBA6010083A6BC8602008FA602002BA7198C0802001F" +
        "A7020013A7010007A7728A13C5248A56BF3FBE010007A7B486248750BF248A7A" +
        "BFC1860FBD03002BAA040007A703002BAA040007A703002BAA040007A724875E" +
        "BF918A017FBF198C07040013A704001FA704002BA701008FA6BC8602009BA602" +
        "005BA7198C0802004FA7020043A7010037A7728A44C5248ABFBF3FBE010037A7" +
        "B4862487B9BF248AE3BFC1860FBD030067AA040037A7030067AA040037A70300" +
        "67AA040037A72487C7BF918A01E8BF198C07040043A704004FA704005BA70100" +
        "9BA6BC86");

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
