namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Verification's native view of the Chozo statue PLM routines: the bank-$84 words naming them.</summary>
internal static class ChozoStatuePlmNativeRoutines
{
    /// <summary>$84:D15C, wait for blank air at (4,8), then install Lower Norfair BTS $83.</summary>
    public const ushort WaitForLowerNorfairHand = 0xd15c;
}
