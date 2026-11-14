namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The direction a run of the cartridge's room-object draw list writes its blocks in, starting from the run's first block
/// (#1862): the import's knowledge of how the cartridge stores a frame; the game draws block shapes.
/// </summary>
internal enum RoomObjectRunDirection
{
    /// <summary>Each following block is one column to the right, along a row.</summary>
    Horizontal,
    /// <summary>Each following block is one row below, down a column.</summary>
    Vertical,
}
