namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Editable JSON schema for boss, elevator-label, and gunship drawing anchors; native identities, slot order, progression visibility, and destination rules remain application-owned.</summary>
public sealed record MapLandmarkDocument
{
    /// <summary>Schema revision, which must equal <see cref="MapLandmarkFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>One non-null pixel anchor for every identity from <see cref="MapLandmarks.AllIds"/>, keyed with exact casing; coordinates lie on the area-map canvas before scroll subtraction, with X=0..511 and Y=0..255.</summary>
    public required Dictionary<string, Position16<MapPixels>> Markers { get; init; }
}
