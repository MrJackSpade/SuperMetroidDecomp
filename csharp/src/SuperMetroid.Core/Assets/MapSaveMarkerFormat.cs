namespace SuperMetroid.Core.Assets;

/// <summary>Asset filename and supported JSON revision for editable selected-save marker drawing positions.</summary>
public static class MapSaveMarkerFormat
{
    /// <summary>Required revision of the complete named-station coordinate schema.</summary>
    public const int Version = 1;
    /// <summary>Filename loaded by the area-map presentation catalog for selected-save marker anchors.</summary>
    public const string FileName = "map-save-markers.json";
}
