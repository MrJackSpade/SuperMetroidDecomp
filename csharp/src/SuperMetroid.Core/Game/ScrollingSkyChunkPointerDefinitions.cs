namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable bank-$88 scrolling-sky chunk pointers, including the native indexed
/// reads beyond each declared table. No room frame needs the ROM for this lookup.
/// </summary>
/// <remarks>
/// The native camera position is masked to $07F8 before the -16/+240 row probes,
/// selecting only indexes 0..8 or 255. Land starts at $88:AD9C; ocean starts at
/// $88:ADA6. Indexes 6..8 can read adjacent tables or wrapper instructions, while
/// index 255 reaches $88:AF9A or $88:AFA4. Every listed word matches the pinned
/// NTSC J/U v1.0 cartridge, including those out-of-table reads.
/// </remarks>
public static class ScrollingSkyChunkPointerDefinitions
{
    private static readonly ushort[] LandWords =
    [
        0xB180, 0xB980, 0xC180, 0xC980, 0xD180,
        0xB180, 0xB980, 0xC180, 0xC980,
    ];

    private static readonly ushort[] OceanWords =
    [
        0xB180, 0xB980, 0xC180, 0xC980, 0xD980,
        0xE180, 0x30C2, 0x78AD, 0xF00A,
    ];

    /// <summary>Land lookup words at $88:AD9C for reachable indexes zero through eight.</summary>
    public static ReadOnlySpan<ushort> Land => LandWords;

    /// <summary>Ocean lookup words at $88:ADA6 for reachable indexes zero through eight.</summary>
    public static ReadOnlySpan<ushort> Ocean => OceanWords;

    /// <summary>$88:AF9A, native land-table index 255 at the top of the room.</summary>
    public const ushort LandWrappedTop = 0xADA6;

    /// <summary>$88:AFA4, native ocean-table index 255 at the top of the room.</summary>
    public const ushort OceanWrappedTop = 0x0A78;

    /// <summary>Reads exactly the bounded indices selected by the native camera arithmetic.</summary>
    public static ushort Get(int pointerTable, int index)
    {
        ReadOnlySpan<ushort> words = pointerTable switch
        {
            RoomFxRomData.ScrollingSky.LandChunkPointerTableAddress => LandWords,
            RoomFxRomData.ScrollingSky.OceanChunkPointerTableAddress => OceanWords,
            _ => throw new ArgumentOutOfRangeException(nameof(pointerTable), pointerTable,
                "Unknown scrolling-sky chunk pointer table."),
        };
        if ((uint)index < words.Length) return words[index];
        if (index == byte.MaxValue)
            return pointerTable == RoomFxRomData.ScrollingSky.LandChunkPointerTableAddress
                ? LandWrappedTop : OceanWrappedTop;
        throw new ArgumentOutOfRangeException(nameof(index), index,
            "Camera row selected an unreachable scrolling-sky chunk index.");
    }
}
