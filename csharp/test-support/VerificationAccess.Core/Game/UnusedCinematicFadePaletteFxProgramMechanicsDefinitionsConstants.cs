using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="UnusedCinematicFadePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete unused fade lasts 22 frames.</summary>
    public const int CycleFrames = UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameCount * UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>The unused palette-FX definition at <c>$8D:E1EC</c>.</summary>
    public const ushort DefinitionPointer = 0xe1ec;

    /// <summary>Provides the unused cinematic fade definition pointer and palette cycle period.</summary>
    extension(UnusedCinematicFadePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => UnusedCinematicFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
