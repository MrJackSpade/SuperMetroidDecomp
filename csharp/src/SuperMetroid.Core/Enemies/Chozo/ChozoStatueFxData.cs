namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>NTSC acid-motion operands used by the Lower Norfair statue.</summary>
internal static class ChozoStatueFxData
{
    /// <summary>Delay before motion.</summary>
    public const int LoweringDelay = 32;

    /// <summary>The acid's downward speed while the statue lowers it: a quarter pixel an update.</summary>
    public static readonly SignedEightEight LoweringVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);
}
