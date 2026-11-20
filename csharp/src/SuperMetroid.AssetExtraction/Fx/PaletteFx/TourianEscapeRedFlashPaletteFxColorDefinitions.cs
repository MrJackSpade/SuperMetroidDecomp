using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the Tourian escape's shutter and background red flashes and the colours each writes, for the import.</summary>
public static class TourianEscapeRedFlashPaletteFxColorDefinitions
{

    /// <summary>Shutter and background loops with distinct colour starts, frame durations, and color extents.</summary>
    private static readonly TourianEscapeRedFlashPaletteFxColorDefinition[] Definitions =
    [
        new()
        {
            Owner = TourianEscapeRedFlashPaletteOwner.Shutter,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.TourianShutterRedFlashing, 6),
        },
        new()
        {
            Owner = TourianEscapeRedFlashPaletteOwner.Background,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.TourianBackgroundRedFlashing, 4),
        },
    ];

    /// <summary>The shutter and background red-flash programs in definition order.</summary>
    public static IReadOnlyList<TourianEscapeRedFlashPaletteFxColorDefinition> All { get; } = Array.AsReadOnly(Definitions);
}
