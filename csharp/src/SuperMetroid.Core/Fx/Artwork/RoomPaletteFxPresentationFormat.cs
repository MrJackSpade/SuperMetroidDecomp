namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>File identity and schema versions for editable room/cinematic palette-FX color assets.</summary>
public static class RoomPaletteFxPresentationFormat
{
    /// <summary>The presentation filename resolved for the room and cinematic palette-FX color catalog.</summary>
    public const string FileName = "room-palette-effects.json";
    /// <summary>Version eighteen, requiring all room/cinematic color families including the three Samus-in-heat suit palettes.</summary>
    public const int Version = (int)RoomPaletteFxPresentationRevision.Current;
}
