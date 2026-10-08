namespace SuperMetroid.Core.Game;

/// <summary>Authored RGB5 images for the Mother Brain cutscene Baby Metroid.</summary>
public static class BabyMetroidCutsceneColorRomData
{
    /// <summary>$A9:94D4, colors 1..15 of the initial sprite-palette-seven image.</summary>
    public const int InitialSource = 0xa994d4;

    /// <summary>$AD:E90C, first displayed fourteen-color fade-to-black image.</summary>
    public const int FirstFadeSource = 0xade90c;

    /// <summary>Byte destination $01E2: sprite palette seven, starting at color one.</summary>
    public const int DestinationByteIndex = 0x01e2;

    public const int InitialColorCount = 15;
    public const int FadeColorCount = 14;
    public const int FadeFrameCount = 6;

    public static int FadeSource(int paletteIndex) =>
        paletteIndex is >= 1 and <= FadeFrameCount
            ? FirstFadeSource + (paletteIndex - 1) * FadeColorCount * sizeof(ushort)
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex));
    /// <summary>$A9:94E6, CGRAM slot10: initial fang light color, independently chosen paint.</summary>
    public const int InitialFangLightColor = 9;
    /// <summary>$A9:94E8, CGRAM slot11: per-channel floor midpoint of fang light/dark slots10/12.</summary>
    public const int InitialFangMiddleColor = 10;
    /// <summary>$A9:94EA, CGRAM slot12: initial fang dark color, independently chosen paint.</summary>
    public const int InitialFangDarkColor = 11;
    /// <summary>$A9:94EE, CGRAM slot14: full-intensity RGB5 white.</summary>
    public const int InitialWhiteColor = 13;
    /// <summary>$A9:94F0, CGRAM slot15: zero-intensity RGB5 black.</summary>
    public const int InitialBlackColor = 14;
}
