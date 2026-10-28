namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Compiled timing metadata for the Crystal Flash body-palette cycle. Palette pointers
/// remain presentation data; this catalog owns only the fixed delays interleaved with
/// those pointers by <c>HandleCrystalFlashPalette</c> at <c>$91:DB93</c>.
/// </summary>
internal static class CrystalFlashPaletteTimingDefinitions
{
    /// <summary>Number of authored Crystal Flash body-palette timing records.</summary>
    public const ushort RecordCount = 10;

    /// <summary>Bytes occupied by each interleaved palette-pointer/timer record.</summary>
    public const ushort RecordByteCount = 4;

    /// <summary>Every authored body-palette record remains active for ten handler calls.</summary>
    public const ushort AuthoredDuration = 10;
}
