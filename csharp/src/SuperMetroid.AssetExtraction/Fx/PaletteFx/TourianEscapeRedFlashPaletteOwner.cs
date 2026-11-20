using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive early Tourian escape red-flash program.</summary>
public enum TourianEscapeRedFlashPaletteOwner
{
    /// <summary>The escape's shutters, flashing the six <see cref="ColourEffectColours.TourianShutters"/> through fourteen two-tick records.</summary>
    Shutter,
    /// <summary>The escape's background, flashing the four <see cref="ColourEffectColours.TourianBackground"/> through fourteen four-tick records.</summary>
    Background,
}
