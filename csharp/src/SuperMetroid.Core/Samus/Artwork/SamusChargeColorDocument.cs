using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable RGB5 body-palette rows for charge, pseudo-Screw, and Hyper-shot playback; each row contains sixteen native OBJ slots, including the nonvisible transparent slot.</summary>
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
