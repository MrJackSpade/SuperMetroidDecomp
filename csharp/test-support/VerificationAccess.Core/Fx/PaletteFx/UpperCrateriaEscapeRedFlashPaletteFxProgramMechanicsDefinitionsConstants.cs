
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete loop lasts 63 frames.</summary>
    public const int CycleFrames = 63;
    /// <summary><c>PalFxDef_Crateria2</c> at <c>$8D:FFE5</c>.</summary>
    public const ushort DefinitionPointer = 0xffe5;

    /// <summary>Provides the upper Crateria escape red flash definition and cycle period.</summary>
    extension(UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
