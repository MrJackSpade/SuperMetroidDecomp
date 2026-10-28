namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Pushes Kraid's fingernails along his contour: the ledge a nail is level with selects its left offset.</summary>
internal static class KraidNailContour
{
    /// <summary>Returns a left offset for one of the six reachable contour records.</summary>
    internal static ushort LeftAt(int record) =>
        unchecked((ushort)KraidNailContourDefinitions.Lefts[CheckRecord(record)]);

    /// <summary>Returns the comparison boundary paired with one reachable left offset.</summary>
    internal static ushort TopAt(int record) =>
        unchecked((ushort)KraidNailContourDefinitions.Tops[CheckRecord(record)]);

    /// <summary>Returns the native selected left offset for a wrapped nail-minus-body Y word.</summary>
    public static ushort LeftOffset(int relativeY)
    {
        for (int record = 0; record < KraidNailContourDefinitions.RecordCount; record++)
            if (unchecked((short)(TopAt(record) - relativeY)) < 0)
                return LeftAt(record);
        throw new InvalidOperationException("Kraid contour definitions failed their exhaustive signed-word coverage invariant.");
    }

    /// <summary>Rejects a record outside the six reachable contour records.</summary>
    private static int CheckRecord(int record) =>
        (uint)record < KraidNailContourDefinitions.RecordCount ? record : throw new ArgumentOutOfRangeException(nameof(record));
}
