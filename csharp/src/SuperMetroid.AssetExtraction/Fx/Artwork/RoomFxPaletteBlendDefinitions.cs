namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>Room-FX blend selectors, distinct from editable colors.</summary>
public static class RoomFxPaletteBlendDefinitions
{
    /// <summary>The colours one palette blend writes.</summary>
    public const int ColorCount = 3;

    /// <summary>Editable JSON filename for room-FX blend inks.</summary>
    public const string FileName = "room-fx-blend-palettes.json";
    /// <summary>
    /// Supported revision of the complete eight-selection blend schema. Revision 2 (#1544) holds the blends alone: the
    /// Ceres haze's tints are scene colours. Revision 3 (#1740) keys each blend by what it tints, not by the cartridge's
    /// colour-table offset.
    /// </summary>
    public const int Version = 3;
}
