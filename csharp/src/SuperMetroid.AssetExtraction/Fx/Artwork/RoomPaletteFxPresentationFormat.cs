namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>File identity and schema versions for editable room/cinematic palette-FX color assets.</summary>
public static class RoomPaletteFxPresentationFormat
{
    /// <summary>The presentation filename resolved for the room and cinematic palette-FX color catalog.</summary>
    public const string FileName = "room-palette-effects.json";
    /// <summary>Version twenty, requiring all room/cinematic color families including the three Samus-in-heat suit palettes, with the Norfair heat glows and the Parlor escape sky named for what they show (#1554), and no colours of the cinematic fade and dark lightning no scene or room shows (#1686).</summary>
    public const int Version = 20;
}
