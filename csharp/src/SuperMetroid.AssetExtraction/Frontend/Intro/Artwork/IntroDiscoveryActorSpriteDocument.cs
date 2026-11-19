using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable JSON schema for the SR388 egg, remnants, and baby Metroid compositions.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the discovery scene actors' packed sprite parts, which only the import reads to compose their pictures; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its CompositionFiles), which the installer deletes before it publishes.")]
public sealed record IntroDiscoveryActorSpriteDocument
{
    /// <summary>Must match <see cref="IntroDiscoveryActorSpriteFormat.Version"/> before any frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets all nineteen required named visual compositions in the compiled actor set.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
