namespace SuperMetroid.Core.Game;

/// <summary>One native Crocomire BG2 Y-scroll correction keyed by body spritemap.</summary>
internal readonly record struct CrocomireBg2VerticalCorrection(
    ushort SpritemapPointer, ushort Offset);

/// <summary>
/// Fixed scroll data used by <c>Update Crocomire BG2 scroll</c> at $A4:8B5B.
/// The cartridge searches the 17 pointers at $A4:8B79 in reverse order and,
/// on a match, adds that spritemap's third-entry Y offset from pointer + $1C.
/// This is camera/presentation alignment, not editable boss artwork.
/// </summary>
internal static class CrocomireBg2ScrollDefinitions
{
    /// <summary>First pointer of the 17-word map at $A4:8B79.</summary>
    internal const int NativeMapTable = 0xa48b79;
    /// <summary>Third-entry Y offset within each mapped bank-$A4 spritemap.</summary>
    internal const ushort ThirdEntryYOffset = 0x001c;
    /// <summary>Native $A4:8B5B vertical-scroll origin.</summary>
    internal const ushort VerticalOrigin = 0x0043;

    private static readonly CrocomireBg2VerticalCorrection[] Values =
    [
        new(0xbfc4, 0x0000), new(0xbff6, 0xfffe),
        new(0xc028, 0xfffe), new(0xc05a, 0xffff),
        new(0xc08c, 0xfffe), new(0xc0be, 0xfffe),
        new(0xc0f0, 0xffff), new(0xc122, 0xffff),
        new(0xc154, 0x0000), new(0xc186, 0x0000),
        new(0xc1b8, 0x0000), new(0xc1ea, 0x0000),
        new(0xc47a, 0x0000), new(0xc4ac, 0xfffe),
        new(0xc4de, 0xfffc), new(0xc510, 0xfffe),
        new(0xc542, 0xffff),
    ];

    internal static ReadOnlySpan<CrocomireBg2VerticalCorrection> Entries => Values;

    /// <summary>Applies the native signed-word correction, or zero if unmatched.</summary>
    internal static ushort VerticalScroll(ushort bodyY, ushort spritemapPointer)
    {
        ushort scroll = unchecked((ushort)(VerticalOrigin - bodyY));
        for (int index = Values.Length - 1; index >= 0; index--)
        {
            CrocomireBg2VerticalCorrection entry = Values[index];
            if (entry.SpritemapPointer == spritemapPointer)
                return unchecked((ushort)(scroll + entry.Offset));
        }
        return scroll;
    }
}
