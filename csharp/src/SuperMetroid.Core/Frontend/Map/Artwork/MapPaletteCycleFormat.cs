namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Host filename and byte-sized schema limits for editable map highlight palettes.</summary>
public static class MapPaletteCycleFormat
{
    /// <summary>JSON presentation filename used by pause and file-select map highlight animation.</summary>
    public const string FileName = "map-highlight-cycle.json";
    /// <summary>Required cycle-document schema revision.</summary>
    public const int Version = 1;
    /// <summary>Number of colors written per frame, equal to one sixteen-color palette row.</summary>
    public const int ColorCount = 16;
    /// <summary>Maximum selected frame count, 255, preserving the animation owner's byte-sized frame cursor.</summary>
    public const int MaximumFrames = 255;
    /// <summary>Maximum authored frame hold, 254 ticks; zero is invalid and a duration of 255 marks stream termination rather than a hold.</summary>
    public const int MaximumDuration = 254;
}
