namespace SuperMetroid.Core.Assets;

/// <summary>Calculated repeated-row identities in the three suit-loading palettes.</summary>
/// <remarks>Supported NTSC J/U programs $8D:DB62/DCC8/DE2E contain nine
/// sixteen-color records. Every even record repeats the normal palette; record3
/// repeats record1. Records5 and7 contain distinct shades. The mapping is checked
/// against complete original rows, including transparent entries, for #1165.
/// This removes repeated rows, without assigning a retention disposition to
/// the four remaining artwork rows or caching generated colors.</remarks>
public static class LoadingPaletteColorDefinitions
{
    /// <summary>Maps a color address to its earliest identical native row.</summary>
    /// <remarks>First color words are $8D:DB6B/DCD1/DE37. Counted groups
    /// advance79 bytes with records36 bytes apart; the final row is313 bytes
    /// after the first. Only the32 color bytes in each record are owned.
    /// Non-color addresses return false and zero, including wrong byte parity.</remarks>
    public static bool TryCanonicalPointer(ushort pointer, out ushort canonical) =>
        TryProgram(pointer, 0xdb6b, out canonical) ||
        TryProgram(pointer, 0xdcd1, out canonical) ||
        TryProgram(pointer, 0xde37, out canonical);

    /// <summary>Resolves an explicit edit before following a calculated loading alias.</summary>
    internal static bool TryReadColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value) =>
        colors.TryGetValue(pointer, out value) ||
        (TryCanonicalPointer(pointer, out ushort canonical) && colors.TryGetValue(canonical, out value));

    /// <summary>Shares identical suit slots with the Power palette for the same shade.</summary>
    /// <remarks>Original complete RGB5 words establish each equality. Varia's
    /// normal row differs at0/2/10/11 and bright rows at1/2/10/11/12. Gravity's
    /// normal row differs at2/10/11/12 and bright rows at2/10/11. All other slots
    /// share the Power input, including transparent entries where equal.
    /// Differing custom colors remain explicit overrides before this mapping.</remarks>
    private static ushort ShareSuit(int first, int rowOffset, int colorOffset)
    {
        int slot = colorOffset / 2;
        bool distinct = first == 0xdb6b || (first == 0xdcd1
            ? (rowOffset == 0 ? slot is 0 or 2 or 10 or 11 : slot is 1 or 2 or 10 or 11 or 12)
            : (rowOffset == 0 ? slot is 2 or 10 or 11 or 12 : slot is 2 or 10 or 11));
        return (ushort)((distinct ? first : 0xdb6b) + rowOffset + colorOffset);
    }

    private static bool TryProgram(ushort pointer, int first, out ushort canonical)
    {
        canonical = 0;
        int offset = pointer - first;
        int finalColor = offset - 313;
        if ((uint)finalColor < 32 && (finalColor & 1) == 0)
        {
            canonical = ShareSuit(first, 0, finalColor);
            return true;
        }
        if ((uint)offset >= 4 * 79) return false;
        int group = offset / 79;
        int within = offset % 79;
        int record = within / 36;
        int color = within % 36;
        if (record >= 2 || color >= 32 || (color & 1) != 0) return false;
        canonical = ShareSuit(first, record == 0 ? 0 :
            (group < 2 ? 36 : group * 79 + 36), color);
        return true;
    }
}
