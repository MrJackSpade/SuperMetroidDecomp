namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Physical record geometry shared by Samus body import and installed admission.</summary>
public static class SamusBodyDefinitionLayout
{
    /// <summary>The first record of SamusTopTiles_Set0 through _SetC, indexed by set; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private static readonly int[] TopSetStarts = [0, 32, 62, 94, 111, 128, 145, 162, 182, 192, 371, 392, 416];

    /// <summary>Native top-half selector chooses its named allocation; editable supplied identities remain independent.</summary>
    internal static int DefaultTopStart(int set) => (uint)set < (uint)TopSetStarts.Length
        ? TopSetStarts[set]
        : throw new ArgumentOutOfRangeException(nameof(set));

    /// <summary>The first record of SamusBottomTiles_Set0 through _SetA, indexed by set; pixel payload lengths and artwork remain REQUIRED under SamusBodyTileDefinition.planar.</summary>
    private static readonly int[] BottomSetStarts = [208, 240, 272, 399, 296, 319, 343, 424, 366, 427, 369];

    /// <summary>Native bottom-half selector chooses its named allocation; editable supplied identities remain independent.</summary>
    internal static int DefaultBottomStart(int set) => (uint)set < (uint)BottomSetStarts.Length
        ? BottomSetStarts[set]
        : throw new ArgumentOutOfRangeException(nameof(set));

    /// <summary>Number of seven-byte body DMA definition records, stored one after another.</summary>
    public const int RecordCount = 435;

    /// <summary>
    /// Requires every record between each sorted set start and the next start (or the end of the records). It does not
    /// restrict a frame's cross-group selection.
    /// </summary>
    internal static void ValidateCompleteGroups(int[] topStarts, int[] bottomStarts,
        SamusBodyTileDefinition[][] top, SamusBodyTileDefinition[][] bottom)
    {
        int[] sorted = [.. topStarts.Concat(bottomStarts).Order()];
        var counts = new Dictionary<int, int>();
        for (int index = 0; index < sorted.Length; index++)
        {
            int start = sorted[index], end = index + 1 == sorted.Length ? RecordCount : sorted[index + 1];
            if (start < 0 || end <= start)
                throw new InvalidDataException($"Samus definition group at record {start} has invalid physical bounds.");
            counts.Add(start, end - start);
        }
        ValidateHalf(topStarts, top);
        ValidateHalf(bottomStarts, bottom);

        void ValidateHalf(int[] starts, SamusBodyTileDefinition[][] groups)
        {
            for (int set = 0; set < starts.Length; set++)
                if (groups[set].Length != counts[starts[set]])
                    throw new InvalidDataException(
                        $"Samus definition group at record {starts[set]} requires {counts[starts[set]]} physical records, not {groups[set].Length}.");
        }
    }
}
