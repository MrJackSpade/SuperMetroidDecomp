namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical X offsets used by Ridley's private Ceres-door draw hook. The cartridge reads
/// <c>$A6:A321-$A6:A324</c> one byte at a time from a word table, producing 0, 0, -4, -1.
/// </summary>
internal static class CeresDoorQuakeDefinitions
{
    private static readonly sbyte[] XOffsets = [0, 0, -4, -1];

    /// <summary>Returns the signed offset selected by the earthquake timer's low two bits.</summary>
    internal static sbyte XOffset(ushort earthquakeTimer) =>
        XOffsets[earthquakeTimer & 3];
}
