namespace SuperMetroid.Core.Assets;

/// <summary>The installed manifest of a sprite sheet: each frame's rectangle on the sheet image, origin and blending.</summary>
public sealed record SpriteSheetDocument
{
    /// <summary>The manifest's schema revision, <see cref="SpriteSheet.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The frames by name.</summary>
    public required Dictionary<string, SpriteSheetFrameDocument> Frames { get; init; }
}
