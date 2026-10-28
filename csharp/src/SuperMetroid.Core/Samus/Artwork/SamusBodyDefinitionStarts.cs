using static SuperMetroid.Core.Samus.Artwork.SamusBodyDefinitionLayout;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where each body set's default definitions start, and how many definitions each set's start leaves room for.</summary>
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
    /// The number of definitions each set start holds: every record between it and the next start in sorted order (or
    /// the end of the records). It does not restrict a frame's cross-group selection.
    /// </summary>
    /// <exception cref="InvalidDataException">A start is negative, repeated or past the end of the records.</exception>
    internal static IReadOnlyDictionary<int, int> GroupLengths(IReadOnlyList<int> topStarts, IReadOnlyList<int> bottomStarts)
    {
        Ensure.NotNull(topStarts);
        Ensure.NotNull(bottomStarts);
        int[] sorted = [.. topStarts.Concat(bottomStarts).Order()];
        var counts = new Dictionary<int, int>();
        for (int index = 0; index < sorted.Length; index++)
        {
            int start = sorted[index], end = index + 1 == sorted.Length ? RecordCount : sorted[index + 1];
            if (start < 0 || end <= start)
                throw new InvalidDataException($"Samus definition group at record {start} has invalid physical bounds.");
            counts.Add(start, end - start);
        }
        return counts;
    }
}
