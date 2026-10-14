/// <summary>A collectible fixture's optional room size and collectible block (#633).</summary>
internal readonly record struct CollectibleRoomLayout
{
    /// <summary>Room size.</summary>
    public Extent16<RoomBlocks>? RoomSize { get; init; }

    /// <summary>Block.</summary>
    public Position16<RoomBlocks>? Block { get; init; }
}
