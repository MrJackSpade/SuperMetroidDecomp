using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Physical record geometry shared by Samus body import and installed admission.</summary>
public static class SamusBodyDefinitionLayout
{
    /// <summary>
    /// The native SamusTopTiles_Set0 through _SetC allocation identities in bank $92, indexed by set; pixel payload lengths
    /// and artwork remain REQUIRED under SamusBodyTileDefinition.planar.
    /// </summary>
    private static readonly SnesAddress[] TopSetPointers =
        [SnesAddress.FromBusAddress(0x92cbee), SnesAddress.FromBusAddress(0x92ccce), SnesAddress.FromBusAddress(0x92cda0), SnesAddress.FromBusAddress(0x92ce80), SnesAddress.FromBusAddress(0x92cef7), SnesAddress.FromBusAddress(0x92cf6e), SnesAddress.FromBusAddress(0x92cfe5), SnesAddress.FromBusAddress(0x92d05c), SnesAddress.FromBusAddress(0x92d0e8), SnesAddress.FromBusAddress(0x92d12e), SnesAddress.FromBusAddress(0x92d613), SnesAddress.FromBusAddress(0x92d6a6), SnesAddress.FromBusAddress(0x92d74e)];

    /// <summary>Native top-half selector chooses its named allocation; editable supplied identities remain independent.</summary>
    internal static ushort DefaultTopPointer(int set) => (uint)set < (uint)TopSetPointers.Length
        ? TopSetPointers[set].Offset
        : throw new ArgumentOutOfRangeException(nameof(set));

    /// <summary>
    /// The native SamusBottomTiles_Set0 through _SetA allocation identities in bank $92, indexed by set; pixel payload lengths
    /// and artwork remain REQUIRED under SamusBodyTileDefinition.planar.
    /// </summary>
    private static readonly SnesAddress[] BottomSetPointers =
        [SnesAddress.FromBusAddress(0x92d19e), SnesAddress.FromBusAddress(0x92d27e), SnesAddress.FromBusAddress(0x92d35e), SnesAddress.FromBusAddress(0x92d6d7), SnesAddress.FromBusAddress(0x92d406), SnesAddress.FromBusAddress(0x92d4a7), SnesAddress.FromBusAddress(0x92d54f), SnesAddress.FromBusAddress(0x92d786), SnesAddress.FromBusAddress(0x92d5f0), SnesAddress.FromBusAddress(0x92d79b), SnesAddress.FromBusAddress(0x92d605)];

    /// <summary>Native bottom-half selector chooses its named allocation; editable supplied identities remain independent.</summary>
    internal static ushort DefaultBottomPointer(int set) => (uint)set < (uint)BottomSetPointers.Length
        ? BottomSetPointers[set].Offset
        : throw new ArgumentOutOfRangeException(nameof(set));

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
