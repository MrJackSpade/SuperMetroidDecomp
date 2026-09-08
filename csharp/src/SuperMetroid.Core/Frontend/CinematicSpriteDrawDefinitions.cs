namespace SuperMetroid.Core.Frontend;

/// <summary>Origin admission operands from the bank-$8B cinematic sprite draw dispatcher.</summary>
internal static class CinematicSpriteDrawDefinitions
{
    /// <summary>$8B:9746, DrawCinematicSpriteObjects_Intro: bias Y by 128 before the unsigned admission test.</summary>
    public const int OriginYBias = 128;

    /// <summary>$8B:9746: admit biased Y strictly below $01FF, corresponding to signed origins -128 through 382.</summary>
    public const int BiasedOriginYLimit = 511;

    /// <summary>$8B:9746: a nonzero Y high byte selects $81:8853 instead of the on-screen $81:879F loader.</summary>
    public const int OriginYHighByteMask = 0xff00;
}
