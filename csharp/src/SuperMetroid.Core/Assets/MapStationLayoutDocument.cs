namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema for all map-station marker drawing positions.</summary>
public sealed record MapStationLayoutDocument
{
    /// <summary>Gets the map-station layout schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets one screen-pixel drawing position for every known station-marker identifier.</summary>
    public required Dictionary<string, Position16<MapPixels>> Markers { get; init; }
}
