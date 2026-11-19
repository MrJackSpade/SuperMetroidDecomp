using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable JSON schema for ordered visible-caret sprite parts; positioning and blink timing remain cinematic-script behavior, not authored frame sequences.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the narration caret's packed sprite parts, which only the import reads to compose its pictures; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its CompositionFiles), which the installer deletes before it publishes.")]
public sealed record IntroCaretSpriteDocument
{
    /// <summary>Schema revision; the reader accepts only <see cref="IntroCaretSpriteVersion.Current"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Requires <c>caret-visible</c> with at most 128 ordered OAM parts.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
