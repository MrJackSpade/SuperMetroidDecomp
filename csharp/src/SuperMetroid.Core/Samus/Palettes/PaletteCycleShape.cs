namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>How a palette cycle walks its shades.</summary>
public enum PaletteCycleShape
{
    /// <summary>From first to last.</summary>
    Linear,

    /// <summary>Up to the middle and back.</summary>
    PingPong,
}
