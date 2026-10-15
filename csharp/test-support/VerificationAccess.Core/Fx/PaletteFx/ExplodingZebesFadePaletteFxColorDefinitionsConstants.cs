
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ExplodingZebesFadePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class ExplodingZebesFadePaletteFxColorDefinitionsConstants
{
    /// <summary>The complete one-shot fade lasts 56 frames.</summary>
    public const int CycleFrames = 56;
    /// <summary>The palette-FX definition at <c>$8D:E1C4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c4;

    /// <summary>Provides the exploding Zebes fade cycle period and native palette definition pointer.</summary>
    extension(ExplodingZebesFadePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="ExplodingZebesFadePaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ExplodingZebesFadePaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ExplodingZebesFadePaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ExplodingZebesFadePaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
}
