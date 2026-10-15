
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="MaridiaEnvironmentalPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class MaridiaEnvironmentalPaletteFxColorDefinitionsConstants
{
    /// <summary>$8D:F795: native Maridia1 sand-pit palette-FX definition; subsequent selected definitions occupy four bytes each.</summary>
    public const ushort SandPitDefinition = 0xF795;

    /// <summary>Provides the native sand pit palette definition for Maridia environment checks.</summary>
    extension(MaridiaEnvironmentalPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.SandPitDefinition"/>
        internal static ushort SandPitDefinition => MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.SandPitDefinition;
    }
    /// <summary>$8D:F4EB/F543: selected sand background palette slot2.</summary>
    internal const int SandPalette = 2;
    /// <summary>$8D:F57B: selected waterfall background palette slot3.</summary>
    internal const int WaterfallPalette = 3;
    /// <summary>$8D:F4EB: first sand-pit color4 within its palette.</summary>
    internal const int SandPitFirstColor = 4;
    /// <summary>$8D:F543: first sand-fall color8 within its palette.</summary>
    internal const int FallingFirstColor = 8;
    /// <summary>$8D:F57B operand0068: waterfall first color4 in palette3; the native source comment incorrectly labels colors8..F.</summary>
    internal const int WaterfallFirstColor = 4;
    /// <summary>$8D:F4ED/F545: authored ten-frame sand cadence.</summary>
    internal const ushort SandDuration = 10;
    /// <summary>$8D:F57D: selected two-frame waterfall cadence.</summary>
    internal const ushort WaterfallDuration = 2;
}
