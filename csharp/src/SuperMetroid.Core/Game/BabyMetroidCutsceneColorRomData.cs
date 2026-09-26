namespace SuperMetroid.Core.Game;

/// <summary>Authored RGB5 images for the Mother Brain cutscene Baby Metroid.</summary>
public static class BabyMetroidCutsceneColorRomData
{
    /// <summary>$A9:94D4, colors 1..15 of the initial sprite-palette-seven image.</summary>
    public const int InitialSource = 0xa994d4;

    /// <summary>$AD:E8E2, seven fade-frame pointers; index zero is never displayed.</summary>
    public const int FadePointerTable = 0xade8e2;

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
}
