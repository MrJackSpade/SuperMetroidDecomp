namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// The installed rainbow palette document's fade and beam geometry, which the import reads, writes and recognizes
/// (#1502); the game only plays the compiled colours.
/// </summary>
public static class MotherBrainRainbowPaletteGeometry
{
    /// <summary>Eight authored steps in each drain and revival palette pointer sequence; cadence and completion are controlled by boss AI.</summary>
    public const int GreyFrameCount = 8;

    /// <summary>38 sampled BGR555 words before the signed loop terminator.</summary>
    public const int BeamCycleColorCount = 38;

    /// <summary>The body and brain colours each drained-to-grey image rewrites.</summary>
    public const int DrainedColors = 15;

    /// <summary>The body and brain colours each revival image rewrites.</summary>
    public const int RevivalColors = 13;

    /// <summary>The rear-leg colours each drained or revival image rewrites after the body and brain colours.</summary>
    public const int BackLegCount = 5;
}
