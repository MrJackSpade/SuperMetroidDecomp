
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ExplodingZebesFadePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete one-shot fade lasts 56 frames.</summary>
    public const int CycleFrames = 56;
    /// <summary>The palette-FX definition at <c>$8D:E1C4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c4;

    /// <summary>Provides the exploding Zebes fade cycle period and native palette definition pointer.</summary>
    extension(ExplodingZebesFadePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ExplodingZebesFadePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
