using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable JSON schema for the opening flashback's fourth-hit explosion compositions.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the opening Mother Brain explosion's packed sprite parts, which only the import reads to compose its pictures; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its CompositionFiles), which the installer deletes before it publishes.")]
public sealed record IntroMotherBrainExplosionSpriteDocument
{
    /// <summary>Must match <see cref="IntroMotherBrainExplosionSpriteFormat.Version"/> before any frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the twelve named <c>small-explosion-0..5</c> and <c>big-explosion-0..5</c> compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
