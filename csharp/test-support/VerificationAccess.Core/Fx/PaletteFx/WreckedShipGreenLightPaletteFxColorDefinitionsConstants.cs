
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="WreckedShipGreenLightPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class WreckedShipGreenLightPaletteFxColorDefinitionsConstants
{
    /// <summary>Powered Wrecked Ship palette-FX definition at $8D:F76D.</summary>
    public const ushort PoweredDefinition = 0xf76d;
    /// <summary>Alternate caller of the powered-light program at $8D:F771.</summary>
    public const ushort PoweredDefinitionAlternate = 0xf771;

    /// <summary>Provides both powered Wrecked Ship green light palette definitions for effect comparison.</summary>
    extension(WreckedShipGreenLightPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="WreckedShipGreenLightPaletteFxColorDefinitionsConstants.PoweredDefinition"/>
        internal static ushort PoweredDefinition => WreckedShipGreenLightPaletteFxColorDefinitionsConstants.PoweredDefinition;
        /// <inheritdoc cref="WreckedShipGreenLightPaletteFxColorDefinitionsConstants.PoweredDefinitionAlternate"/>
        internal static ushort PoweredDefinitionAlternate => WreckedShipGreenLightPaletteFxColorDefinitionsConstants.PoweredDefinitionAlternate;
    }
    /// <summary>The first powered-light destination byte in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x0098;
}
