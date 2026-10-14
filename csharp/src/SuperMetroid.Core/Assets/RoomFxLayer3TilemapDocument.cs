namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON definition of the six room-FX BG3 transfers, storing character selectors and BG attributes instead of packed SNES tilemap words.</summary>
public sealed record RoomFxLayer3TilemapDocument
{
    /// <summary>Schema revision; loading requires <see cref="RoomFxLayer3TilemapFormat.Version"/> before compiling any page.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly six pages keyed by <c>Lava</c>, <c>Acid</c>, <c>Water</c>, <c>Spores</c>, <c>Rain</c>, and <c>Fog</c>; each contains 1056 non-null cells in row-major transfer order with valid character and palette selectors.</summary>
    public required Dictionary<string, RoomBackgroundTilemapCell[]> Pages { get; init; }
}
