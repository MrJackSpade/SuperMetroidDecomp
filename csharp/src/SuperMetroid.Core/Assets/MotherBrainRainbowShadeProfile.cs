namespace SuperMetroid.Core.Assets;

/// <summary>Mutually exclusive material-channel profiles in the native body rainbow phases.</summary>
internal enum MotherBrainRainbowShadeProfile
{
    /// <summary>$AD:E486: phase1 green, with ceiling plate and nearest-up tissue shades.</summary>
    FirstGreenRise,
    /// <summary>$AD:E4C2: phase2 green, with ceiling plate and nearest-even tissue shades.</summary>
    SecondGreenRise,
    /// <summary>$AD:E576: phase5 blue, with a flat head, nearest plate and floor tissue shades.</summary>
    MixedBlue,
    /// <summary>$AD:E5B2: phase6 blue, with paired head, ceiling plate and nearest-up tissue shades.</summary>
    MaximumBlue,
    /// <summary>$AD:E5EE: phase7 green, with a head ramp, shared plate/tail and three tissue greens, retaining two independent tissue samples.</summary>
    RecoveringGreen,
}
