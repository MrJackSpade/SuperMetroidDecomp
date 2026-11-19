namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The acid-motion values used by the Lower Norfair statue.</summary>
internal static class ChozoStatueFxData
{
    /// <summary>The updates the acid waits, once the statue's cue starts it, before it lowers.</summary>
    public const int LoweringDelay = 32;

    /// <summary>The acid's downward speed while the statue lowers it: a quarter pixel an update.</summary>
    public static readonly SignedEightEight LoweringVelocity = SignedEightEight.FromPixelFraction(1, 4);
}
