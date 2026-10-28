namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Native bank-$89 room-FX blend selectors, distinct from editable colors.</summary>
public static class RoomFxPaletteBlendDefinitions
{
    /// <summary>The colours one palette blend writes.</summary>
    public const int ColorCount = 3;

    /// <summary>Editable JSON filename for room-FX blend inks and optional Ceres haze tints.</summary>
    public const string FileName = "room-fx-blend-palettes.json";
    /// <summary>Supported revision of the complete eight-selection blend schema; haze tint fields are optional within this revision.</summary>
    public const int Version = 1;
}
