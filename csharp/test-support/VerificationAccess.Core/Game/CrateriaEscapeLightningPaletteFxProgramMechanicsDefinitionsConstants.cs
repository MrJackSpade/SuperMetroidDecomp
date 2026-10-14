using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Both complete loops last 98 frames.</summary>
    public const int CycleFrames = 98;

    /// <summary>Provides the escape lightning palette cycle period used to check its imported program.</summary>
    extension(CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}
