using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Selected-save marker artwork anchors; neither load targets nor scroll origins are content.</summary>
/// <remarks>
/// Independently reviewed for #1165: all34 stock marker pairs in the six $82:C80B
/// views equal the native load-station map projection. Calculate stock coordinates
/// through FileSelectMapLoadAnchors and retain only independently edited X/Y fields.
/// Imported artwork changes never feed back into compiled load targets or scrolling.
/// Original JSON names, coordinate bounds and serialization remain unchanged.
/// </remarks>
public sealed class MapSaveMarkerLayout
{
    /// <summary>Stores only marker coordinates that differ from compiled station projections; a null axis keeps its compiled value.</summary>
    private readonly Dictionary<string, (int? X, int? Y)> coordinateOverrides;

    /// <summary>Creates a layout from the sparse coordinate overrides validated by <see cref="Load"/>.</summary>
    /// <param name="coordinateOverrides">Marker-keyed X/Y overrides, with null axes indicating that the compiled coordinate should be used.</param>
    private MapSaveMarkerLayout(Dictionary<string, (int? X, int? Y)> coordinateOverrides) =>
        this.coordinateOverrides = coordinateOverrides;

    /// <summary>Gets the selected station's drawing anchor in area-map pixels before scroll subtraction, using each edited coordinate independently and otherwise projecting its compiled load placement.</summary>
    /// <param name="area">One of the six Zebes areas; Ceres has no save-map marker layout.</param>
    /// <param name="index">Native station slot, 0..15, whose compiled load-station record has a nonzero room pointer; not an ordinal among usable markers.</param>
    /// <returns>The drawing position, without altering the station's load target or initial map scrolling.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area or station slot is outside its supported range.</exception>
    /// <exception cref="InvalidDataException">The station slot is unused.</exception>
    public MapLabelPoint Get(AreaId area, int index)
    {
        string id = MapSaveMarkerDefinitions.Id(area, index);
        FileSelectMapAnchor basis = FileSelectMapLoadAnchors.Get(area, index);
        var edited = coordinateOverrides.GetValueOrDefault(id);
        return new(edited.X ?? basis.X, edited.Y ?? basis.Y);
    }
    /// <summary>Loads all 34 supported save/elevator marker anchors, validates the complete identity set and coordinate bounds, and retains only coordinates differing from compiled station projections.</summary>
    /// <param name="json">Caller-owned stream containing the editable save-marker JSON document.</param>
    /// <returns>A drawing-only layout with independently editable X/Y coordinates.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, marker identities, or coordinates are invalid.</exception>
    public static MapSaveMarkerLayout Load(Stream json)
    {
        MapSaveMarkerDocument document;
        try { document = JsonAssetDocument.Read<MapSaveMarkerDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Save marker layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid save marker layout JSON.", error); }
        if (document.Version != MapSaveMarkerFormat.Version || document.Markers is null ||
            document.Markers.Count != MapSaveMarkerDefinitions.AllIds().Count())
            throw new InvalidDataException("Save marker layout requires version 1 and every known marker.");
        var overrides = new Dictionary<string, (int? X, int? Y)>(StringComparer.Ordinal);
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        foreach (int index in MapSaveMarkerDefinitions.Indices((AreaId)area))
        {
            string id = MapSaveMarkerDefinitions.Id((AreaId)area, index);
            if (!document.Markers.TryGetValue(id, out var point) || point is null || point.X is < 0 or > 511 || point.Y is < 0 or > 255)
                throw new InvalidDataException($"Save marker {id} requires X=0..511 and Y=0..255.");
            FileSelectMapAnchor basis = FileSelectMapLoadAnchors.Get((AreaId)area, index);
            int? x = point.X == basis.X ? null : point.X;
            int? y = point.Y == basis.Y ? null : point.Y;
            if (x.HasValue || y.HasValue) overrides.Add(id, (x, y));
        }
        return new(overrides);
    }
    /// <summary>Serializes the complete marker document as UTF-8 JSON and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete editable marker layout to validate and serialize.</param>
    public static void Write(Stream json, MapSaveMarkerDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
/// <summary>Editable JSON schema for selected-save marker drawing anchors; native station identities, load destinations, and initial scrolling remain application-owned.</summary>
public sealed record MapSaveMarkerDocument
{
    /// <summary>Schema revision, which loading requires to equal <see cref="MapSaveMarkerFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>One non-null area-map pixel anchor per usable station, keyed as <c>Area.Save.slot</c> with exact casing; X=0..511 and Y=0..255 are measured before scroll subtraction.</summary>
    public required Dictionary<string, MapLabelPoint> Markers { get; init; }
}
/// <summary>Asset filename and supported JSON revision for editable selected-save marker drawing positions.</summary>
public static class MapSaveMarkerFormat
{
    /// <summary>Required revision of the complete named-station coordinate schema.</summary>
    public const int Version = 1;
    /// <summary>Filename loaded by the area-map presentation catalog for selected-save marker anchors.</summary>
    public const string FileName = "map-save-markers.json";
}
