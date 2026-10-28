using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Mutable JSON authoring data for SR388's six shell fragments, moving slime, and four slime-impact frames; actor physics and instruction timing are not stored here.</summary>
public sealed record IntroEggEffectSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="IntroEggEffectSpriteFormat.Version"/> when loaded or written.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the caller-owned frame arrays keyed by fragment-0..fragment-5, slime-moving, and slime-impact-0..slime-impact-3; array order determines OAM draw order.</summary>
    /// <remarks>Parts use pixel offsets and 8- or 16-pixel OBJ sizes within a 16-column by 32-row tile atlas. A null palette inherits the drawing actor's selector. Loading compiles and copies these values rather than retaining the mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
