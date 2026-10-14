namespace SuperMetroid.Core.Assets;

/// <summary>Editable schema for one or two ordered native BG tilemap pages; contains visual references rather than room collision or metatile data.</summary>
public sealed record RoomBackgroundTilemapDocument
{
    /// <summary>Schema revision; compilation requires <see cref="RoomBackgroundTilemapFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned mutable array of one or two pages in native transfer order; page arrangement is determined by the consumer.</summary>
    public required RoomBackgroundTilemapPage[] Pages { get; init; }
}
