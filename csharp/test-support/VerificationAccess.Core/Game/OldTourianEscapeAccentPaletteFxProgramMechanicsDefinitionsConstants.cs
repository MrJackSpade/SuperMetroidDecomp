using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Both complete loops last 64 frames.</summary>
    public const int CycleFrames = 64;

    /// <summary>Provides the Old Tourian escape accent palette cycle period.</summary>
    extension(OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}
