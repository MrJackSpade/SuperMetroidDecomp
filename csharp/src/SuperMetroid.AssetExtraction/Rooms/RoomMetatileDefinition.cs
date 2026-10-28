namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>One 16x16 visual block composed of four independently attributed 8x8 background tiles.</summary>
public sealed record RoomMetatileDefinition
{
    /// <summary>The 8x8 tile at local pixel (0, 0), encoded as the first native tilemap word.</summary>
    public required RoomMetatileCell TopLeft { get; init; }
    /// <summary>The 8x8 tile at local pixel (8, 0), encoded as the second native tilemap word.</summary>
    public required RoomMetatileCell TopRight { get; init; }
    /// <summary>The 8x8 tile at local pixel (0, 8), encoded as the third native tilemap word.</summary>
    public required RoomMetatileCell BottomLeft { get; init; }
    /// <summary>The 8x8 tile at local pixel (8, 8), encoded as the fourth native tilemap word.</summary>
    public required RoomMetatileCell BottomRight { get; init; }
}
