namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$A8 Magdollite color-cycle geometry, not animation timing.</summary>
public static class MagdollitePaletteRomData
{
    /// <summary>Four sixteen-color RGB5 rows at $A8:AC1C.</summary>
    public const int Source = 0xa8ac1c;

    /// <summary>The graphics-drawn hook cycles through all four authored rows.</summary>
    public const int FrameCount = 4;

    /// <summary>Each source row spans a complete sixteen-color OBJ palette.</summary>
    public const int SourceColorsPerFrame = 16;

    /// <summary>The hook copies colors nine through twelve from each row.</summary>
    public const int FirstAnimatedColor = 9;
    public const int AnimatedColorCount = 4;
}
