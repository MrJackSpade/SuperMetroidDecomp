
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="UnusedCinematicFadePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class UnusedCinematicFadePaletteFxColorDefinitionsConstants
{
    /// <summary>The complete unused fade lasts 22 frames.</summary>
    public const int CycleFrames = UnusedCinematicFadePaletteFxColorDefinitions.FrameCount * UnusedCinematicFadePaletteFxColorDefinitions.FrameDuration;
    /// <summary>The unused palette-FX definition at <c>$8D:E1EC</c>.</summary>
    public const ushort DefinitionPointer = 0xe1ec;

    /// <summary>Provides the unused cinematic fade definition pointer and palette cycle period.</summary>
    extension(UnusedCinematicFadePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="UnusedCinematicFadePaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => UnusedCinematicFadePaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="UnusedCinematicFadePaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => UnusedCinematicFadePaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
}
