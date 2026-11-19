using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable JSON schema for the scientist scenes' baby-Metroid compositions.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the scientists' packed sprite parts, which only the import reads to compose their pictures; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its CompositionFiles), which the installer deletes before it publishes.")]
public sealed record IntroScientistSpriteDocument
{
    /// <summary>Must match <see cref="IntroScientistSpriteFormat.Version"/> before any visual frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets all six required named baby-Metroid visual compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
