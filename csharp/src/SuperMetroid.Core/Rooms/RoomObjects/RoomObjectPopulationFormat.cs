namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Structural bounds of the room-population parser and decoded input.</summary>
public static class RoomObjectPopulationFormat
{
    /// <summary>The bounded parser admits at most 256 reads, including the zero header.</summary>
    public const int MaximumParserIterations = 256;
    /// <summary>At most 255 nonzero records leave room for that bounded terminator read.</summary>
    public const int MaximumRecordCount = MaximumParserIterations - 1;
}
