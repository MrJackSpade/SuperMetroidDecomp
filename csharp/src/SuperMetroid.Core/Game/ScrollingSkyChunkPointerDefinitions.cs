namespace SuperMetroid.Core.Game;

/// <summary>Computes bank-$88 sky chunk identities, including bounded adjacent-code reads.</summary>
/// <remarks>Native camera arithmetic admits only indices0..8 and255. Five land
/// pages advance by800 bytes fromB180. Land indices5..8 alias the first four ocean
/// entries. Ocean has four shared pages, skips page4, then selects pages5/6.
/// Its indices6..8 read adjacent instructions, not tilemap progression. Index255
/// reads a separate instruction operand for each table. These compatibility cases
/// are explicit; all other indices reject. Every result is independently verified
/// against NTSC J/U v1.0 and pinned bank_88.asm
/// (362be646929cf8e483f692b73a6561cfc2dc1d0d). No pointer array or cache remains.</remarks>
public static class ScrollingSkyChunkPointerDefinitions
{
    /// <summary>First sky tilemap page at $8A:B180; native pages occupy800 bytes each.</summary>
    private const int FirstPage = 0xb180;
    private const int PageByteCount = 0x0800;

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

    /// <summary>Selects a page or native compatibility word for the exact supported domain.</summary>
    public static ushort Get(int pointerTable, int index)
    {
        bool ocean = pointerTable switch
        {
            RoomFxRomData.ScrollingSky.LandChunkPointerTableAddress => false,
            RoomFxRomData.ScrollingSky.OceanChunkPointerTableAddress => true,
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