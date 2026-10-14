using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="TorizoBellyPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TorizoBellyPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 52;

    /// <summary>Provides the Torizo belly palette cycle period for effect timing checks.</summary>
    extension(TorizoBellyPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TorizoBellyPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TorizoBellyPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}
