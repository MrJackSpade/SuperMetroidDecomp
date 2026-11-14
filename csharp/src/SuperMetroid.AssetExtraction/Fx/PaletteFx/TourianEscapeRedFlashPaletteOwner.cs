using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive early Tourian escape red-flash program.</summary>
public enum TourianEscapeRedFlashPaletteOwner
{
    /// <summary>Tourian palette effect 8, flashing the six <see cref="ColourEffectColours.TourianShutters"/> through fourteen two-tick records.</summary>
    Shutter,
    /// <summary>Tourian palette effect 10, flashing the four <see cref="ColourEffectColours.TourianBackground"/> through fourteen four-tick records.</summary>
    Background,
}
