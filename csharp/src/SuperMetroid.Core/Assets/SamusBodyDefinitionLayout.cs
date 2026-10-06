using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Physical record geometry shared by Samus body import and installed admission.</summary>
public static class SamusBodyDefinitionLayout
{
    /// <summary>$92:CBEE, native SamusTopTiles_Set0 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet0 = 0xcbee;
    /// <summary>$92:CCCE, native SamusTopTiles_Set1 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet1 = 0xccce;
    /// <summary>$92:CDA0, native SamusTopTiles_Set2 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet2 = 0xcda0;
    /// <summary>$92:CE80, native SamusTopTiles_Set3 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet3 = 0xce80;
    /// <summary>$92:CEF7, native SamusTopTiles_Set4 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet4 = 0xcef7;
    /// <summary>$92:CF6E, native SamusTopTiles_Set5 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet5 = 0xcf6e;
    /// <summary>$92:CFE5, native SamusTopTiles_Set6 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet6 = 0xcfe5;
    /// <summary>$92:D05C, native SamusTopTiles_Set7 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet7 = 0xd05c;
    /// <summary>$92:D0E8, native SamusTopTiles_Set8 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet8 = 0xd0e8;
    /// <summary>$92:D12E, native SamusTopTiles_Set9 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSet9 = 0xd12e;
    /// <summary>$92:D613, native SamusTopTiles_SetA allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSetA = 0xd613;
    /// <summary>$92:D6A6, native SamusTopTiles_SetB allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSetB = 0xd6a6;
    /// <summary>$92:D74E, native SamusTopTiles_SetC allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort TopSetC = 0xd74e;
    /// <summary>Native top-half selector chooses its named allocation; editable supplied identities remain independent.</summary>
    internal static ushort DefaultTopPointer(int set) => set switch
    {
        0 => TopSet0,
        1 => TopSet1,
        2 => TopSet2,
        3 => TopSet3,
        4 => TopSet4,
        5 => TopSet5,
        6 => TopSet6,
        7 => TopSet7,
        8 => TopSet8,
        9 => TopSet9,
        10 => TopSetA,
        11 => TopSetB,
        12 => TopSetC,
        _ => throw new ArgumentOutOfRangeException(nameof(set)),
    };

    /// <summary>$92:D19E, native SamusBottomTiles_Set0 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet0 = 0xd19e;
    /// <summary>$92:D27E, native SamusBottomTiles_Set1 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet1 = 0xd27e;
    /// <summary>$92:D35E, native SamusBottomTiles_Set2 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet2 = 0xd35e;
    /// <summary>$92:D6D7, native SamusBottomTiles_Set3 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet3 = 0xd6d7;
    /// <summary>$92:D406, native SamusBottomTiles_Set4 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet4 = 0xd406;
    /// <summary>$92:D4A7, native SamusBottomTiles_Set5 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet5 = 0xd4a7;
    /// <summary>$92:D54F, native SamusBottomTiles_Set6 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet6 = 0xd54f;
    /// <summary>$92:D786, native SamusBottomTiles_Set7 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet7 = 0xd786;
    /// <summary>$92:D5F0, native SamusBottomTiles_Set8 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet8 = 0xd5f0;
    /// <summary>$92:D79B, native SamusBottomTiles_Set9 allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSet9 = 0xd79b;
    /// <summary>$92:D605, native SamusBottomTiles_SetA allocation identity; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private const ushort BottomSetA = 0xd605;
    /// <summary>Native bottom-half selector chooses its named allocation; editable supplied identities remain independent.</summary>
    internal static ushort DefaultBottomPointer(int set) => set switch
    {
        0 => BottomSet0,
        1 => BottomSet1,
        2 => BottomSet2,
        3 => BottomSet3,
        4 => BottomSet4,
        5 => BottomSet5,
        6 => BottomSet6,
        7 => BottomSet7,
        8 => BottomSet8,
        9 => BottomSet9,
        10 => BottomSetA,
        _ => throw new ArgumentOutOfRangeException(nameof(set)),
    };

    /// <summary>$92:0000, bank identity for the physical body DMA definition records.</summary>
    public const int BankBase = 0x920000;

    /// <summary>$92:D7D3, exclusive end of the contiguous seven-byte body DMA definitions.</summary>
    public const int EndOffset = 0xd7d3;

    /// <summary>Lowest valid bank-$92 ROM-half offset for a definition-group start.</summary>
    public const int MinimumSetOffset = 0x8000;

    /// <summary>
    /// Requires every physical record between each sorted set start and the next
    /// start (or EndOffset). It does not restrict a frame's cross-group selection.
    /// </summary>
    internal static void ValidateCompleteGroups(ushort[] topPointers, ushort[] bottomPointers,
        SamusBodyTileDefinition[][] top, SamusBodyTileDefinition[][] bottom)
    {
        ushort[] sorted = topPointers.Concat(bottomPointers).Order().ToArray();
        var counts = new Dictionary<ushort, int>();
        for (int index = 0; index < sorted.Length; index++)
        {
            int start = sorted[index], end = index + 1 == sorted.Length ? EndOffset : sorted[index + 1];
            int length = end - start;
            if (start < MinimumSetOffset || length <= 0 ||
                length % SamusRenderingRomData.TileTransfers.DefinitionByteCount != 0)
                throw new InvalidDataException($"Samus definition group ${start:X4} has invalid physical bounds.");
            counts.Add(sorted[index], length / SamusRenderingRomData.TileTransfers.DefinitionByteCount);
        }
        ValidateHalf(topPointers, top);
        ValidateHalf(bottomPointers, bottom);

        void ValidateHalf(ushort[] pointers, SamusBodyTileDefinition[][] groups)
        {
            for (int set = 0; set < pointers.Length; set++)
                if (groups[set].Length != counts[pointers[set]])
                    throw new InvalidDataException(
                        $"Samus definition group ${pointers[set]:X4} requires {counts[pointers[set]]} physical records, not {groups[set].Length}.");
        }
    }
}
