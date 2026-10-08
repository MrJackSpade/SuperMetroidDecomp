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

    /// <summary>Fifteen packed RGB5 words in the initial $A9:94D4 image, filling OBJ palette seven colors 1..15 while retaining its transparent color zero.</summary>
    public const int InitialColorCount = 15;
    /// <summary>$A9:CDA6's fourteen-word transfer length, filling OBJ palette seven colors 1..14; the initial black color fifteen is not rewritten by the fade.</summary>
    public const int FadeColorCount = 14;
    /// <summary>Six displayed fade images, native palette indices 1..6; the unused index-zero image is skipped and the cutscene signals completion at index seven rather than loading a seventh row.</summary>
    public const int FadeFrameCount = 6;

    /// <summary>Resolves the native identity of a displayed BabyMetroidFadingToBlackPalettes row, from $AD:E90C through $AD:E998 in 28-byte strides; the cutscene owns when each row is published.</summary>
    /// <param name="paletteIndex">One-based native fade index 1..6, not a zero-based artwork ordinal or gameplay-update count.</param>
    /// <returns>The 24-bit bank-$AD source address for fourteen packed RGB5 words; the last row is entirely black.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The fade index is outside 1..6.</exception>
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
