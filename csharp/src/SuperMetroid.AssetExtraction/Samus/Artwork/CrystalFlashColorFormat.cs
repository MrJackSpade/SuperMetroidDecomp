using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Resource identity, supported JSON version, and fixed dimensions derived from the Crystal Flash palette programs.</summary>
public static class CrystalFlashColorFormat
{
    /// <summary>Ten editable body frames, one per pointer/timer record in playback order.</summary>
    public const int BodyFrameCount = SamusPaletteDefinitions.CrystalFlash.BodyStepCount;
    /// <summary>Six editable bubble frames selected independently by the pointer table.</summary>
    public const int BubbleFrameCount = SamusPaletteDefinitions.CrystalFlash.BubblePaletteCount;
    /// <summary>Ten colors in each body frame, occupying sprite palette six colors 0..9.</summary>
    public const int BodyColorCount = 10;
    /// <summary>Six colors in each bubble frame, occupying sprite palette six colors 10..15.</summary>
    public const int BubbleColorCount = 6;
}
