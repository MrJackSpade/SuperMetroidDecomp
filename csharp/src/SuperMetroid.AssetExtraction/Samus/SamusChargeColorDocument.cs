using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The import's body colour rows for charge, pseudo-Screw, and Hyper-shot playback (#1637), import data only the import
/// pre-colours Samus's shades from; each row contains sixteen native OBJ slots, including the nonvisible transparent slot.
/// </summary>
public sealed record SamusChargeColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Each suit's six playback phases, keyed by the suit's name.</summary>
    public required Dictionary<string, PaletteRgb5[][]> ChargedBeam { get; init; }
    /// <summary>Each suit's six playback phases, keyed by the suit's name.</summary>
    public required Dictionary<string, PaletteRgb5[][]> PseudoScrew { get; init; }
    /// <summary>Ten Hyper-shot glow frames in playback order.</summary>
    public required PaletteRgb5[][] HyperShot { get; init; }
}
