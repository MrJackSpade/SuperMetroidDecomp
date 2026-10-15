
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="NorfairEnvironmentalPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class NorfairEnvironmentalPaletteFxColorDefinitionsConstants
{
    /// <summary>The shared complete-cycle duration.</summary>
    public const int CycleFrames = 116;

    /// <summary>Provides the native Norfair environmental palette cycle period for effect comparisons.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="NorfairEnvironmentalPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => NorfairEnvironmentalPaletteFxColorDefinitionsConstants.CycleFrames;
    }
}
