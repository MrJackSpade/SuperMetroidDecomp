using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class NorfairEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The shared complete-cycle duration.</summary>
    public const int CycleFrames = 116;

    /// <summary>Provides the native Norfair environmental palette cycle period for effect comparisons.</summary>
    extension(NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="NorfairEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => NorfairEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}
