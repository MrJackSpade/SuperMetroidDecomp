using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Physical record geometry shared by Samus body import and installed admission.</summary>
public static class SamusBodyDefinitionLayout
{
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
