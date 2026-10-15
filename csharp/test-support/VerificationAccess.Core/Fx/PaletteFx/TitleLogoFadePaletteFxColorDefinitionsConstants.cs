
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TitleLogoFadePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TitleLogoFadePaletteFxColorDefinitionsConstants
{
    /// <summary>The complete title-logo fade lasts 24 frames.</summary>
    public const int CycleFrames = TitleLogoFadePaletteFxColorDefinitions.FrameCount * TitleLogoFadePaletteFxColorDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E194</c>.</summary>
    public const ushort DefinitionPointer = 0xe194;

    /// <summary>Provides the title logo fade definition pointer and palette cycle period.</summary>
    extension(TitleLogoFadePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="TitleLogoFadePaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TitleLogoFadePaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="TitleLogoFadePaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => TitleLogoFadePaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
}
