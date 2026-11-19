using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable JSON schema for the ten Hyper Beam projectile palette rows; the separate native palette program retains frame durations, destination selection, and loop control.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the Hyper Beam's own colour cycle rows, which only the import reads to pre-colour its shades; ProjectilePresentationFiles.Extract writes it to the projectiles' import data, which the installer deletes before it publishes.")]
public sealed record HyperBeamFxColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="HyperBeamFxColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ten ordered rows of eight RGB5 colors corresponding to payloads at <c>$8D:D906 + 20 * row</c>; row colors map to OBJ palette 6 colors 1..8, and every supplied channel remains independently editable.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}
