namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>File identity and schema versions for editable room/cinematic palette-FX color assets.</summary>
public static class RoomPaletteFxPresentationFormat
{
    /// <summary>The presentation filename resolved for the room and cinematic palette-FX color catalog.</summary>
    public const string FileName = "room-palette-effects.json";
    /// <summary>Version seventeen, whose existing color edits can be retained while missing Samus heat rows come from current stock.</summary>
    public const int PreviousVersion = 17;
    /// <summary>Version eighteen, requiring all room/cinematic color families including the three Samus-in-heat suit palettes.</summary>
    public const int Version = 18;
}
