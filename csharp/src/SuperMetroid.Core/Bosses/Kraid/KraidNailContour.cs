namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Pushes Kraid's fingernails along his contour: the ledge a nail is level with selects its left offset.</summary>
internal static class KraidNailContour
{
    /// <summary>Returns a left offset for one of the six reachable contour records.</summary>
    internal static int LeftAt(int record) =>
        CoordinateAxis.Wrap(KraidNailContourDefinitions.Lefts[CheckRecord(record)]);

    /// <summary>Returns the comparison boundary paired with one reachable left offset.</summary>
    internal static int TopAt(int record) =>
        CoordinateAxis.Wrap(KraidNailContourDefinitions.Tops[CheckRecord(record)]);

    /// <summary>Returns the native selected left offset for a wrapped nail-minus-body Y word.</summary>
    public static int LeftOffset(int relativeY)
    {
        for (int record = 0; record < KraidNailContourDefinitions.RecordCount; record++)
            if (CoordinateAxis.Difference(TopAt(record), relativeY) < 0)
                return LeftAt(record);
        throw new InvalidOperationException("Kraid contour definitions failed their exhaustive signed-word coverage invariant.");
    }

    /// <summary>Rejects a record outside the six reachable contour records.</summary>
    private static int CheckRecord(int record) =>
        (uint)record < KraidNailContourDefinitions.RecordCount ? record : throw new ArgumentOutOfRangeException(nameof(record));
}
