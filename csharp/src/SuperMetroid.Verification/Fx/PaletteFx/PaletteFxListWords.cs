namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>How the palette-FX ROM oracle (#1298) reads a bank-$8D list word.</summary>
internal static class PaletteFxListWords
{
    /// <summary>The bank the palette-FX lists live in.</summary>
    internal const int ListBank = 0x8d0000;

    /// <summary>List words from here up are commands; below it, timers and colours.</summary>
    internal const ushort CommandWord = 0x8000;
}
