namespace SuperMetroid.Core.Game;

/// <summary>Cartridge movement data used by Kraid's belly-platform firing routine.</summary>
internal static class KraidPlatformMovement
{
    /// <summary>
    /// Negative 16.16 displacement from KraidLint_XSubSpeed / KraidLint_XSpeed
    /// at $A7:A926/A928, subtracted by $A7:B89B and the riding branch at $A7:B8EB.
    /// </summary>
    public const int FiringDisplacement = -0x00038000;

    /// <summary>
    /// Minimum whole-pixel extra Samus X displacement at $A7:B8FB-B903. Native
    /// clamps the whole word to -16 without changing the fractional word.
    /// </summary>
    public const int MinimumCarryWholePixels = -16;
}
