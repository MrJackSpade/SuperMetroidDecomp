using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the Tourian escape's shutter and background red flashes and the colours each writes, for the import.</summary>
public static class TourianEscapeRedFlashPaletteFxColorDefinitions
{

    /// <summary>The shutters' red flashing.</summary>
    internal static TourianEscapeRedFlashPaletteFxColorDefinition TourianShutterRedFlashing { get; } = new(ColourEffectId.TourianShutterRedFlashing, 6);

    /// <summary>The background's red flashing.</summary>
    internal static TourianEscapeRedFlashPaletteFxColorDefinition TourianBackgroundRedFlashing { get; } = new(ColourEffectId.TourianBackgroundRedFlashing, 4);
}
