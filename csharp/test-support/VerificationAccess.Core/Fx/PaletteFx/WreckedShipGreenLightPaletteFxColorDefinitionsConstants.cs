
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="WreckedShipGreenLightPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class WreckedShipGreenLightPaletteFxColorDefinitionsConstants
{
    /// <summary>Powered Wrecked Ship palette-FX definition at $8D:F76D.</summary>
    public const ushort PoweredDefinition = 0xf76d;

    /// <summary>Provides both powered Wrecked Ship green light palette definitions for effect comparison.</summary>
    extension(WreckedShipGreenLightPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="WreckedShipGreenLightPaletteFxColorDefinitionsConstants.PoweredDefinition"/>
        internal static ushort PoweredDefinition => WreckedShipGreenLightPaletteFxColorDefinitionsConstants.PoweredDefinition;
    }
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => WreckedShipGreenLightPaletteFxColorDefinitions.ColorFrames.Color(frame, color);
    /// <summary>Eight ten-frame records form the powered-light cycle.</summary>
    public static int FrameCount => WreckedShipGreenLightPaletteFxColorDefinitions.ColorFrames.FrameCount;
}
