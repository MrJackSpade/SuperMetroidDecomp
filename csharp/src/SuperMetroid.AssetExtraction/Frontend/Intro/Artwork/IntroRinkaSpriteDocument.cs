using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Mutable JSON authoring data for the opening flashback's Rinka sprites, not the room-enemy Rinka AI or its physical definitions.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the opening Rinkas' packed sprite parts, which only the import reads to compose their pictures; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its CompositionFiles), which the installer deletes before it publishes.")]
public sealed record IntroRinkaSpriteDocument
{
    /// <summary>Schema revision, required to equal <see cref="IntroRinkaSpriteFormat.Version"/> when loaded or written.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned ordered parts keyed by rinka-0, rinka-1, and rinka-2.</summary>
    /// <remarks>Stock frames mirror one 8-pixel tile into a 16-by-16-pixel four-part composition, ordered bottom-right, bottom-left, top-right, then top-left. Editable parts use pixel offsets and a 16-column by 32-row tile atlas; null palettes inherit the actor's selector. Loading copies compiled values, not these mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
