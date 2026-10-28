namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Native save-marker index validity, distinct from editable drawing coordinates.</summary>
public static class MapSaveMarkerDefinitions
{
    /// <summary>$82:C80B lists retain sixteen station-index slots per area, including unused entries.</summary>
    public const int SlotsPerArea = 16;
}
