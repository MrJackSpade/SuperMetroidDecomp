namespace SuperMetroid.Core.Assets;

/// <summary>One part of a sprite sheet frame in its manifest: the part image it shows and where, from the frame's origin.</summary>
public sealed record SpriteSheetPartDocument
{
    /// <summary>The name of the part image in the manifest's part images.</summary>
    public required string Image { get; init; }

    /// <summary>The part's place from the frame's origin.</summary>
    public required Offset16<ImagePixels> Offset { get; init; }
}
