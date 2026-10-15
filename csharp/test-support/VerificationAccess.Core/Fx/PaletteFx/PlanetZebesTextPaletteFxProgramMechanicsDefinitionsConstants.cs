
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="PlanetZebesTextPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Each complete one-shot fade lasts 24 frames.</summary>
    public const int CycleFrames = PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount * PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>$8D:E1B0: native fade-in definition, immediately followed by the four-byte fade-out definition.</summary>
    public const ushort FadeInDefinition = 0xE1B0;

    /// <summary>Provides Zebes text fade definition and cycle period for opening palette checks.</summary>
    extension(PlanetZebesTextPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants.FadeInDefinition"/>
        internal static ushort FadeInDefinition => PlanetZebesTextPaletteFxProgramMechanicsDefinitionsConstants.FadeInDefinition;
    }
}
