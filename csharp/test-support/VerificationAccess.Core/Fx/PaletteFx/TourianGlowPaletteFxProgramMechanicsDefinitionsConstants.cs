
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TourianGlowPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class TourianGlowPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The unused Tourian 4 clone definition.</summary>
    public const ushort CloneDefinitionPointer = 0xf7a5;
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 110;
    /// <summary>The live Tourian 2 definition.</summary>
    public const ushort LiveDefinitionPointer = 0xf7a1;

    /// <summary>Provides live and cloned Tourian glow definitions and their cycle period.</summary>
    extension(TourianGlowPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.CloneDefinitionPointer"/>
        internal static ushort CloneDefinitionPointer => TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.CloneDefinitionPointer;
        /// <inheritdoc cref="TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.LiveDefinitionPointer"/>
        internal static ushort LiveDefinitionPointer => TourianGlowPaletteFxProgramMechanicsDefinitionsConstants.LiveDefinitionPointer;
    }
}
