using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Mutable JSON authoring data for the opening flashback's Rinka sprites, not the room-enemy Rinka AI or its physical definitions.</summary>
public sealed record IntroRinkaSpriteDocument
{
    /// <summary>Schema revision, required to equal <see cref="IntroRinkaSpriteFormat.Version"/> when loaded or written.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned ordered parts keyed by rinka-0, rinka-1, and rinka-2.</summary>
    /// <remarks>Stock frames mirror one 8-pixel tile into a 16-by-16-pixel four-part composition, ordered bottom-right, bottom-left, top-right, then top-left. Editable parts use pixel offsets and a 16-column by 32-row tile atlas; null palettes inherit the actor's selector. Loading copies compiled values, not these mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
