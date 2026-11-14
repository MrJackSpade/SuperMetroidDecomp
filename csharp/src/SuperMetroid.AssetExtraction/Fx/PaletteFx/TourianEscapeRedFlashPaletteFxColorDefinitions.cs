using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the first two Tourian escape palette loops.</summary>
/// <remarks>
/// The shutter and background programs contain fourteen uniform records apiece. Their
/// 140 BGR555 words remain live presentation data; this catalog owns palette placement,
/// record timing, waits, and loop control.
/// </remarks>
public static class TourianEscapeRedFlashPaletteFxColorDefinitions
{

    /// <summary>Shutter and background loops with distinct colour starts, frame durations, and color extents.</summary>
    private static readonly TourianEscapeRedFlashPaletteFxColorDefinition[] Definitions =
    [
        new()
        {
            Owner = TourianEscapeRedFlashPaletteOwner.Shutter,
            Object = ColourEffectId.TourianShutterRedFlashing,
            Duration = 2,
            ColorsPerFrame = 6,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.TourianShutterRedFlashing, 6),
        },
        new()
        {
            Owner = TourianEscapeRedFlashPaletteOwner.Background,
            Object = ColourEffectId.TourianBackgroundRedFlashing,
            Duration = 4,
            ColorsPerFrame = 4,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.TourianBackgroundRedFlashing, 4),
        },
    ];

    /// <summary>The shutter and background red-flash programs in definition order.</summary>
    public static IReadOnlyList<TourianEscapeRedFlashPaletteFxColorDefinition> All { get; } = Array.AsReadOnly(Definitions);
}
