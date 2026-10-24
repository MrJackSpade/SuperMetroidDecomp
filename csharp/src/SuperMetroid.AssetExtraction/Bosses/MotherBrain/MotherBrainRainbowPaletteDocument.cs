using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Editable Mother Brain palette schema, separating selected RGB5 colors from engine-owned attack/fade cadence and HDMA loop control.</summary>
public sealed record MotherBrainRainbowPaletteDocument
{
    /// <summary>Schema revision: version 3 includes the fake-death fade; version 2 is loadable only with a current-stock presentation supplying that missing sequence.</summary>
    public required int Version { get; init; }
    /// <summary>Ten native $AD:E434 rainbow-list phases, each containing 15 body/brain colors and 15 rear-leg colors with no trailing word.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] Rainbow { get; init; }
    /// <summary>Eight $AD:EF87 drain phases, each containing 15 body/brain colors, five rear-leg colors, and a trailing word written to WRAM $017C.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] ToGrey { get; init; }
    /// <summary>Eight $AD:ED9C revival phases, each containing 13 body/brain colors, five rear-leg colors, and a trailing WRAM word; the final two body/brain inks are preserved.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] FromGrey { get; init; }
    /// <summary>Eight ordered rows of three brain-only RGB5 inks corresponding to $AD:ED8A, required by version 3 and applied beginning at CGRAM color $91.</summary>
    public PaletteRgb5[][]? FakeDeathToGrey { get; init; }
    /// <summary>Normal restoration frame with 15 shared body/brain inks and 15 rear-leg inks, matching the roles of $A9:9474/$9494; no trailing word is allowed.</summary>
    public required MotherBrainRainbowPaletteFrameDocument Normal { get; init; }
    /// <summary>RGB5 fixed backdrop color for the first active rainbow-beam HDMA frame, before the native color cursor advances.</summary>
    public required PaletteRgb5 BeamInitial { get; init; }
    /// <summary>Exactly 38 RGB5 backdrop samples from the native $88:E833 hue cycle; the terminating signed $FFFF word is engine control and is not included.</summary>
    public required PaletteRgb5[] BeamCycle { get; init; }
}
