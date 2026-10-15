
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete one-shot finale lasts 300 frames.</summary>
    public const int CycleFrames = 300;
    /// <summary>The palette-FX definition at <c>$8D:E1CC</c>.</summary>
    public const ushort DefinitionPointer = 0xe1cc;

    /// <summary>Provides the Zebes explosion finale palette definition and cycle period.</summary>
    extension(ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionFinalePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
