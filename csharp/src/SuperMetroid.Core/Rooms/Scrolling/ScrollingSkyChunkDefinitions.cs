namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Computes bank-$88 sky chunk identities, including bounded adjacent-code reads.</summary>
/// <remarks>Native camera arithmetic admits only indices0..8 and255. Five land
/// pages advance by800 bytes fromB180. Land indices5..8 alias the first four ocean
/// entries. Ocean has four shared pages, skips page4, then selects pages5/6.
/// Its indices6..8 read adjacent instructions, not tilemap progression. Index255
/// reads a separate instruction operand for each table. These compatibility cases
/// are explicit; all other indices reject. Every result is independently verified
/// against NTSC J/U v1.0 and pinned bank_88.asm
/// (362be646929cf8e483f692b73a6561cfc2dc1d0d). No pointer array or cache remains.</remarks>
public static class ScrollingSkyChunkDefinitions
{
    /// <summary>First sky tilemap page at $8A:B180; native pages occupy800 bytes each.</summary>
    private const int FirstPage = 0xb180;
    /// <summary>Native byte stride between successive bank-$8A scrolling-sky tilemap pages.</summary>
    private const int PageByteCount = 0x0800;
    /// <summary>The seven contiguous sky pages from <see cref="FirstPage"/>.</summary>
    private const int PageCount = 7;

    /// <summary>$88:ADB2 REP #$30: ocean index6 reads opcode/operand as word30C2.</summary>
    private const ushort OceanAdjacentStatusInstruction = 0x30c2;
    /// <summary>$88:ADB4 LDA TimeIsFrozenFlag: ocean index7 reads opcode/address low byte.</summary>
    private const ushort OceanAdjacentFreezeLoad = 0x78ad;
    /// <summary>$88:ADB6 address high byte followed by BEQ: ocean index8 readsF00A.</summary>
    private const ushort OceanAdjacentFreezeBranch = 0xf00a;

    /// <summary>$88:AF9A: land index255 reads the ocean wrapper's immediate table identity.</summary>
    public const ushort LandWrappedTop = 0xada6;
    /// <summary>$88:AFA4: ocean index255 reads the shared wrapper's TimeIsFrozenFlag address.</summary>
    public const ushort OceanWrappedTop = 0x0a78;

    /// <summary>The sky page a chunk selects; false for a compatibility word, which selects no page.</summary>
    /// <param name="pointerTable">The land or ocean table.</param>
    /// <param name="index">The chunk index.</param>
    /// <param name="page">The page's bank-$8A address when the chunk selects one.</param>
    public static bool TryGetPage(ScrollingSkyChunkTable pointerTable, int index, out ushort page)
    {
        page = Get(pointerTable, index);
        int offset = page - FirstPage;
        return offset >= 0 && offset % PageByteCount == 0 && offset / PageByteCount < PageCount;
    }

    /// <summary>The sky page, 0 through 6, a chunk selects; false for a compatibility word, which selects no page.</summary>
    /// <param name="pointerTable">The land or ocean table.</param>
    /// <param name="index">The chunk index.</param>
    /// <param name="page">The page's position among the seven sky pages when the chunk selects one.</param>
    public static bool TryGetPageIndex(ScrollingSkyChunkTable pointerTable, int index, out int page)
    {
        bool selects = TryGetPage(pointerTable, index, out ushort word);
        page = selects ? (word - FirstPage) / PageByteCount : -1;
        return selects;
    }

    /// <summary>Selects a page or native compatibility word for the exact supported domain.</summary>
    public static ushort Get(ScrollingSkyChunkTable pointerTable, int index)
    {
        bool ocean = pointerTable switch
        {
            ScrollingSkyChunkTable.Land => false,
            ScrollingSkyChunkTable.Ocean => true,
            _ => throw new ArgumentOutOfRangeException(nameof(pointerTable), pointerTable,
                "Unknown scrolling-sky chunk pointer table."),
        };
        if (index == byte.MaxValue) return ocean ? OceanWrappedTop : LandWrappedTop;
        if ((uint)index >= 9)
            throw new ArgumentOutOfRangeException(nameof(index), index,
                "Camera row selected an unreachable scrolling-sky chunk index.");
        if (!ocean)
            return (ushort)(FirstPage + (index < 5 ? index : index - 5) * PageByteCount);
        return index switch
        {
            < 4 => (ushort)(FirstPage + index * PageByteCount),
            < 6 => (ushort)(FirstPage + (index + 1) * PageByteCount),
            6 => OceanAdjacentStatusInstruction,
            7 => OceanAdjacentFreezeLoad,
            _ => OceanAdjacentFreezeBranch,
        };
    }
}
