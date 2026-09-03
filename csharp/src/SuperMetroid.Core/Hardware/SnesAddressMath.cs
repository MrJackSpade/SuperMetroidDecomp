namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Address arithmetic whose wrapping rules come from the 65C816 operand width rather than
/// host integer arithmetic. Callers choose this helper only for native 16-bit pointer walks;
/// ordinary linear bus transfers must continue to cross banks normally.
/// </summary>
public static class SnesAddressMath
{
    /// <summary>
    /// Adds a byte displacement to the low sixteen bits while retaining the original bank.
    /// This models the native routines that increment a 16-bit pointer under a separately
    /// fixed data bank and therefore wrap <c>$xx:FFFF</c> to <c>$xx:0000</c>.
    /// </summary>
    public static int AddWithinBank(int address, int byteCount) =>
        (int)SnesAddress.FromBusAddress(address).AddWithinBank(byteCount);
}
