namespace SuperMetroid.Core.Assets;

/// <summary>JSON schema for the six named Zebes world-map label anchors.</summary>
public sealed record WorldMapLabelDocument
{
    /// <summary>Gets the schema version required by <see cref="WorldMapLabelFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the label anchors keyed by the six case-sensitive area names.</summary>
    public required Dictionary<string, Position16<OamPixels>> Areas { get; init; }
}
