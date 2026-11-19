using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The import's body colour rows for charge, pseudo-Screw, and Hyper-shot playback (#1637), import data only the import
/// pre-colours Samus's shades from; each row contains sixteen native OBJ slots, including the nonvisible transparent slot.
/// </summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: Samus's charged-beam, pseudo Screw Attack and Hyper shot colour rows, which only the import reads to pre-colour her shades; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record SamusChargeColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Power/Varia/Gravity suits, each with six playback phases.</summary>
    public required PaletteRgb5[][][] ChargedBeam { get; init; }
    /// <summary>Power/Varia/Gravity suits, each with six playback phases.</summary>
    public required PaletteRgb5[][][] PseudoScrew { get; init; }
    /// <summary>Ten Hyper-shot glow frames in playback order.</summary>
    public required PaletteRgb5[][] HyperShot { get; init; }
}
