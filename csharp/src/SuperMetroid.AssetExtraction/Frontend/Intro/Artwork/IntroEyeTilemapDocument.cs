using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Defines the complete ordered set of opening portrait eye frames.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the portrait's blinking-eye packed tile placements, which only the import reads to compose the eye pictures; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its LayoutFiles), which the installer deletes before it publishes.")]
public sealed record IntroEyeTilemapDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets exactly four frames in blink-selector order.</summary>
    public required IntroEyeTilemapFrame[] Frames { get; init; }
}
