
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="PlanetZebesTextPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class PlanetZebesTextPaletteFxColorDefinitionsConstants
{
    /// <summary>$8D:C912/C968: the authored three-frame hold.</summary>
    public const ushort FrameDuration = 3;
    /// <summary>$8D:E1B0: native fade-in definition, immediately followed by the four-byte fade-out definition.</summary>
    public const ushort FadeInDefinition = 0xE1B0;

    /// <summary>Provides Zebes text fade definition and cycle period for opening palette checks.</summary>
    extension(PlanetZebesTextPaletteFxColorDefinitions)
    {
        /// <summary>Each complete one-shot fade lasts 24 frames.</summary>
        internal static int CycleFrames => PlanetZebesTextPaletteFxColorDefinitions.FrameCount * PlanetZebesTextPaletteFxColorDefinitionsConstants.FrameDuration;
        /// <inheritdoc cref="PlanetZebesTextPaletteFxColorDefinitionsConstants.FadeInDefinition"/>
        internal static ushort FadeInDefinition => PlanetZebesTextPaletteFxColorDefinitionsConstants.FadeInDefinition;
    }
    /// <summary>$8D:C910/C966: CGRAM palette 8 for the cinematic text.</summary>
    internal const int TextPalette = 8;
}
