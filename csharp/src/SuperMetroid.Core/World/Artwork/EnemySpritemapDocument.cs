using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>Versioned, semantic enemy frame names mapped to editable OAM parts.</summary>
public sealed record EnemySpritemapDocument
{
    /// <summary>Composition schema revision; current loading requires <see cref="EnemySpritemapSchema.Current"/>, or a supported older revision accompanied by a complete stock catalog for merging.</summary>
    public required int Version { get; init; }
    /// <summary>Complete case-sensitive semantic frame keys for the selected schema, each mapped to an ordered non-null OAM-part array; parts describe visual offsets, size, tiles, palette, priority, and reflections rather than gameplay hitboxes.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    /// <summary>Visual-only native-frame-to-displayed-frame bindings; never AI timing.</summary>
    public Dictionary<string, string>? DisplayFrames { get; init; }
}
