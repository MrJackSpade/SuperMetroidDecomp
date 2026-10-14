
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TitleLogoFadePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete title-logo fade lasts 24 frames.</summary>
    public const int CycleFrames = TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount * TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E194</c>.</summary>
    public const ushort DefinitionPointer = 0xe194;

    /// <summary>Provides the title logo fade definition pointer and palette cycle period.</summary>
    extension(TitleLogoFadePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => TitleLogoFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
