namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema for selected-save marker drawing anchors; native station identities, load destinations, and initial scrolling remain application-owned.</summary>
public sealed record MapSaveMarkerDocument
{
    /// <summary>Schema revision, which loading requires to equal <see cref="MapSaveMarkerFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>One non-null area-map pixel anchor per usable station, keyed as <c>Area.Save.slot</c> with exact casing; X=0..511 and Y=0..255 are measured before scroll subtraction.</summary>
    public required Dictionary<string, Position16<MapPixels>> Markers { get; init; }
}
