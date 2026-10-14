
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete loop lasts 42 frames.</summary>
    public const int CycleFrames = 42;
    /// <summary><c>PalFxDef_Crateria8</c> at <c>$8D:FFD9</c>.</summary>
    public const ushort DefinitionPointer = 0xffd9;

    /// <summary>Provides the red flash palette definition and cycle period used during Old Tourian escape.</summary>
    extension(OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
