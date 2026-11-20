namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>The shape of the Baby Metroid's installed colours: its innard pulse and its three complete target images.</summary>
public static class BabyMetroidColorDefinitions
{
    /// <summary>The innard-pulse images, dimming and then retracing; the count does not encode the palette-update cadence or cry timing.</summary>
    public const int NormalFrameCount = 8;

    /// <summary>The colours in each innard-pulse image: the four innard inks of the Baby Metroid's colour set, not a complete set.</summary>
    public const int NormalColorsPerFrame = 4;

    /// <summary>The colours in each complete target image (sidehopper, Baby Metroid, dead sidehopper), including the transparent first colour.</summary>
    public const int TargetColorCount = 16;
}
