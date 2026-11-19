using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's rainbow, drain, revival, fake-death and restoration colours as the import data holds them (#1637): only
/// the import reads them, to pre-colour her pictures, and the installation holds none of them. Her beam backdrop's colours,
/// which the game works with, are installed by purpose (<see cref="MotherBrainBeamColourDocument"/>).
/// </summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: Mother Brain's rainbow, drain, revival and fake-death colour rows, which only the import reads to pre-colour her shades; the installed MotherBrainBeamColourDocument holds the beam colours the game works with; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record MotherBrainRainbowPaletteDocument
{
    /// <summary>Schema revision; the reader accepts only <see cref="MotherBrainRainbowPaletteFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ten native $AD:E434 rainbow-list phases, each containing 15 body/brain colors and 15 rear-leg colors with no trailing word.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] Rainbow { get; init; }
    /// <summary>Eight $AD:EF87 drain phases, each containing 15 body/brain colors, five rear-leg colors, and a trailing word written to WRAM $017C.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] ToGrey { get; init; }
    /// <summary>Eight $AD:ED9C revival phases, each containing 13 body/brain colors, five rear-leg colors, and a trailing WRAM word; the final two body/brain inks are preserved.</summary>
    public required MotherBrainRainbowPaletteFrameDocument[] FromGrey { get; init; }
    /// <summary>Eight ordered rows of three brain-only RGB5 inks corresponding to $AD:ED8A, applied beginning at CGRAM color $91.</summary>
    public PaletteRgb5[][]? FakeDeathToGrey { get; init; }
    /// <summary>Normal restoration frame with 15 shared body/brain inks and 15 rear-leg inks, matching the roles of $A9:9474/$9494; no trailing word is allowed.</summary>
    public required MotherBrainRainbowPaletteFrameDocument Normal { get; init; }
}
