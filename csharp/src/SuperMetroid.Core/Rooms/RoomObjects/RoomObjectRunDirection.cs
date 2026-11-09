namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The direction a room-object block run writes its blocks in, starting from the run's first block.</summary>
internal enum RoomObjectRunDirection
{
    /// <summary>Each following block is one column to the right, along a row.</summary>
    Horizontal,
    /// <summary>Each following block is one row below, down a column.</summary>
    Vertical,
}
