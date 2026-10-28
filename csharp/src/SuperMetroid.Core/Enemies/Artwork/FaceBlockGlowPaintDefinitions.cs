namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Reviewed eye/mouth glow composition at $A8:E7CC-E80B, copied to inks9..12 by E893-E8AA.
/// Six selected intensity roles specify this painted glow; timers, Morph gate and blue-face artwork are excluded.</summary>
internal static class FaceBlockGlowPaintDefinitions
{
    /// <summary>$A8:E7CE/E7EA: resting middle red and brightest accent both use18.</summary>
    internal const int MiddleRestRed = 18;
    /// <summary>$A8:E7D0: resting red shadow.</summary>
    internal const int ShadowRestRed = 10;
    /// <summary>$A8:E7E4: main glow's strongest green channel.</summary>
    internal const int PeakGreen = 25;
    /// <summary>$A8:E7CC/E7D4/E7DC/E7E4: main-glow blue advances one unit per light depth.</summary>
    internal const int BlueDepthStep = 1;
    /// <summary>$A8:E7D2: resting accent adds one red/green unit to the red shadow.</summary>
    internal const int RestWarmAddition = 1;
    /// <summary>$A8:E7DA: first active accent red, separate from its warm resting paint.</summary>
    internal const int ActiveAccentRed = 7;
    /// <summary>$A8:E7CC-E80B: four light depths followed by their reverse, including both repeated ends.</summary>
    internal const int PeakDepth = 3;
}
