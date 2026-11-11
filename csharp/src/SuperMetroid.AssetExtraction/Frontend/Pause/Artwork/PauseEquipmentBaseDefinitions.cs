namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Equipment-page base artwork and the compiled footprints owned by live menu state.</summary>
public static class PauseEquipmentBaseDefinitions
{
    /// <summary>The supported editable equipment-page base schema version.</summary>
    public const int Version = 1;

    /// <summary>The embedded equipment-page base tilemap asset file name.</summary>
    public const string FileName = "pause-equipment-base.json";

    /// <summary>The 32-column, 32-row tilemap geometry and its calculated 1024-cell extent.</summary>
    public const int Columns = 32, Rows = 32, Cells = Columns * Rows;
}
