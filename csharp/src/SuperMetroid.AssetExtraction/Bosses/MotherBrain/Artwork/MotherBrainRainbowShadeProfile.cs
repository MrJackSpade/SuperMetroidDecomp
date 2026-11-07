namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Mutually exclusive material-channel profiles in the body rainbow phases.</summary>
internal enum MotherBrainRainbowShadeProfile
{
    /// <summary>Phase1 green, with ceiling plate and nearest-up tissue shades.</summary>
    FirstGreenRise,
    /// <summary>Phase2 green, with ceiling plate and nearest-even tissue shades.</summary>
    SecondGreenRise,
    /// <summary>Phase5 blue, with a flat head, nearest plate and floor tissue shades.</summary>
    MixedBlue,
    /// <summary>Phase6 blue, with paired head, ceiling plate and nearest-up tissue shades.</summary>
    MaximumBlue,
    /// <summary>Phase7 green, with a head ramp, shared plate/tail and three tissue greens, retaining two independent tissue samples.</summary>
    RecoveringGreen,
}
