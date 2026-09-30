namespace SuperMetroid.Core.Rooms;

/// <summary>Structural bounds of the native room-population parser and decoded input.</summary>
public static class RoomPlmPopulationFormat
{
    /// <summary>Six bytes per native bank-$8F placement: header, block X/Y, argument.</summary>
    public const int RecordByteCount = 6;
    /// <summary>The bounded parser admits at most 256 reads, including the zero header.</summary>
    public const int MaximumParserIterations = 256;
    /// <summary>At most 255 nonzero records leave room for that bounded terminator read.</summary>
    public const int MaximumRecordCount = MaximumParserIterations - 1;
}
