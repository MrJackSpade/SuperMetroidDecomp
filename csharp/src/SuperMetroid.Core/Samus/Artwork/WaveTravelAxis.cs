namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Mutually exclusive travel axes of the four native Wave instruction loops at93:8743,87C7,884B,88CF.</summary>
internal enum WaveTravelAxis
{
    /// <summary>Vertical oscillation on the centered X axis.</summary>
    Vertical,
    /// <summary>Rising diagonal oscillation.</summary>
    RisingDiagonal,
    /// <summary>Horizontal oscillation on the centered Y axis.</summary>
    Horizontal,
    /// <summary>Falling diagonal oscillation.</summary>
    FallingDiagonal,
}
