namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// A frontend screen's whole colour image in the cartridge's 256-colour order (#1464, #1512): its background colours where
/// gameplay's image holds the background sets, and its sprite colours where gameplay's holds the sprite sets.
/// </summary>
public static class ScreenColourImage
{
    /// <summary>The colours of a whole-screen image.</summary>
    public const int ColorCount = SnesCgram.ColorCount;
}
