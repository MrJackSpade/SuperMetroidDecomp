
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="RedBrinstarGlowPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 140;
    /// <summary>Red Brinstar background-glow palette-FX definition at $8D:F77D.</summary>
    public const ushort DefinitionPointer = 0xf77d;

    /// <summary>Provides Red Brinstar glow palette definition and cycle period for environmental checks.</summary>
    extension(RedBrinstarGlowPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
