using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The selected Mother Brain rainbow, drain, revival, fake-death, restoration and beam-backdrop colours.</summary>
internal sealed record MotherBrainRainbowPaletteColours
{
    /// <summary>The rainbow phases.</summary>
    public required MotherBrainPaletteFrameColours[] Rainbow { get; init; }

    /// <summary>The drain phases.</summary>
    public required MotherBrainPaletteFrameColours[] ToGrey { get; init; }

    /// <summary>The revival phases.</summary>
    public required MotherBrainPaletteFrameColours[] FromGrey { get; init; }

    /// <summary>The fake-death brain fade, or null when an older override inherits it from stock.</summary>
    public required RgbColor[][]? FakeDeathToGrey { get; init; }

    /// <summary>The restored normal phase.</summary>
    public required MotherBrainPaletteFrameColours Normal { get; init; }

    /// <summary>The beam backdrop's first colour.</summary>
    public required RgbColor BeamInitial { get; init; }

    /// <summary>The beam backdrop's hue cycle.</summary>
    public required RgbColor[] BeamCycle { get; init; }
}
