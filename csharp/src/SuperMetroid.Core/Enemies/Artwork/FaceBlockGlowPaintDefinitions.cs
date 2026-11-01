namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Reviewed eye/mouth glow composition, copied to inks9..12 by E893-E8AA.
/// Six selected intensity roles specify this painted glow; timers, Morph gate and blue-face artwork are excluded.</summary>
internal static class FaceBlockGlowPaintDefinitions
{
    /// <summary>Resting middle red and brightest accent both use18.</summary>
    internal const int MiddleRestRed = 18;
    /// <summary>Resting red shadow.</summary>
    internal const int ShadowRestRed = 10;
    /// <summary>Main glow's strongest green channel.</summary>
    internal const int PeakGreen = 25;
    /// <summary>Main-glow blue advances one unit per light depth.</summary>
    internal const int BlueDepthStep = 1;
    /// <summary>Resting accent adds one red/green unit to the red shadow.</summary>
    internal const int RestWarmAddition = 1;
    /// <summary>First active accent red, separate from its warm resting paint.</summary>
    internal const int ActiveAccentRed = 7;
    /// <summary>Four light depths followed by their reverse, including both repeated ends.</summary>
    internal const int PeakDepth = 3;
}
