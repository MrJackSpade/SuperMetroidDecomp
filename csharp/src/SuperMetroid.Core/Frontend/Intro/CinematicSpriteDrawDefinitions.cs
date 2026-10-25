namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Origin admission operands from the bank-$8B cinematic sprite draw dispatcher.</summary>
internal static class CinematicSpriteDrawDefinitions
{
    /// <summary>$8B:9746, DrawCinematicSpriteObjects_Intro: bias Y by 128 before the unsigned admission test.</summary>
    public const int OriginYBias = 128;

    /// <summary>$8B:9746: admit biased Y strictly below $01FF, corresponding to signed origins -128 through 382.</summary>
    public const int BiasedOriginYLimit = 511;

    /// <summary>Origins at or below this row (or above the screen) draw through the off-screen clipping path.</summary>
    public const int OnScreenOriginYLimit = 256;
}
