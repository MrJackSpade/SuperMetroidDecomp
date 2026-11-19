namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Editable JSON schema for one common-room-element or tileset-specific table of 16x16 visual blocks; collision and placement are not included.</summary>
public sealed record RoomMetatileDocument
{
    /// <summary>Schema revision, required to equal <see cref="RoomMetatileFormat.Version"/> when compiling the table.</summary>
    public required int Version { get; init; }
    /// <summary>Non-null, ordered block definitions whose count must match the selected native table length divided by eight.</summary>
    public required RoomMetatileDefinition[] Blocks { get; init; }
}
