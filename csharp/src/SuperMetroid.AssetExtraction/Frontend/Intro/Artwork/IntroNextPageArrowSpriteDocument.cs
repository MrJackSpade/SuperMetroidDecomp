using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// Editable JSON schema for the Japanese text's next-page arrow compositions (#1655); its position, cadence and hidden delay
/// remain the narration's behaviour, not authored frames.
/// </summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the next-page arrow's packed sprite parts, which only the import reads to compose its pictures; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its CompositionFiles), which the installer deletes before it publishes.")]
public sealed record IntroNextPageArrowSpriteDocument
{
    /// <summary>Must match <see cref="IntroNextPageArrowSpriteFormat.Version"/> before any visual frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>The arrow's three named visual compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
