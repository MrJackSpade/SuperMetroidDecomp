namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>The installed map highlight cycle's schema: each frame's hold, in playback order (#1693).</summary>
public sealed partial record MapHighlightTimingDocument
{
    /// <summary>Schema revision; loading requires <see cref="MapHighlightTimingFile.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Each frame's hold in map-palette animation updates, one or more, for one or more frames.</summary>
    public required int[] DurationTicks { get; init; }
}
