namespace SuperMetroid.Core.Assets;

/// <summary>Asset filename and supported JSON schema revision for editable area-map landmark anchors.</summary>
public static class MapLandmarkFormat
{
    /// <summary>Supported revision of the complete named-landmark coordinate schema.</summary>
    public const int Version = 1;
    /// <summary>Asset filename loaded by the area-map presentation catalog for landmark drawing positions.</summary>
    public const string FileName = "map-landmarks.json";
}
