using static SuperMetroid.Core.Samus.Artwork.SamusBodyDefinitionLayout;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where each body set's default definitions start, and the check that groups are complete.</summary>
public static class SamusBodyDefinitionStarts
{
    /// <summary>Native top-half selector chooses its named allocation; editable supplied identities remain independent.</summary>
    internal static int DefaultTopStart(int set) => (uint)set < (uint)TopSetStarts.Length
        ? TopSetStarts[set]
        : throw new ArgumentOutOfRangeException(nameof(set));

    /// <summary>Native bottom-half selector chooses its named allocation; editable supplied identities remain independent.</summary>
    internal static int DefaultBottomStart(int set) => (uint)set < (uint)BottomSetStarts.Length
        ? BottomSetStarts[set]
        : throw new ArgumentOutOfRangeException(nameof(set));

    /// <summary>
    /// Requires every record between each sorted set start and the next start (or the end of the records). It does not
    /// restrict a frame's cross-group selection.
    /// </summary>
    internal static void ValidateCompleteGroups(int[] topStarts, int[] bottomStarts,
        SamusBodyTileTransfer[][] top, SamusBodyTileTransfer[][] bottom)
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

        void ValidateHalf(int[] starts, SamusBodyTileTransfer[][] groups)
        {
            for (int set = 0; set < starts.Length; set++)
                if (groups[set].Length != counts[starts[set]])
                    throw new InvalidDataException(
                        $"Samus definition group at record {starts[set]} requires {counts[starts[set]]} physical records, not {groups[set].Length}.");
        }
    }
}
