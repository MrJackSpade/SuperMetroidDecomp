using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Mutable JSON authoring data for the intro flashback's Mother Brain, distinct from boss gameplay and the separate explosion sprites.</summary>
public sealed record IntroMotherBrainSpriteDocument
{
    /// <summary>Gets the schema revision, which must equal <see cref="IntroMotherBrainSpriteFormat.Version"/> when loaded or written.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the caller-owned ordered parts keyed by mother-brain-frame-0, mother-brain-frame-1, and mother-brain-frame-2.</summary>
    /// <remarks>Stock frames are 48-by-48-pixel compositions of nine 16-pixel OBJs. Editable parts use pixel offsets and tile coordinates in a 16-column by 32-row atlas; null palettes inherit the actor's selector. Loading copies compiled values, not these mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
