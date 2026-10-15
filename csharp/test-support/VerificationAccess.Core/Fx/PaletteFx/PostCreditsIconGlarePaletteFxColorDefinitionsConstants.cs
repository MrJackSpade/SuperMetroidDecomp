
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="PostCreditsIconGlarePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class PostCreditsIconGlarePaletteFxColorDefinitionsConstants
{
    /// <summary>The complete icon glare lasts fourteen frames.</summary>
    public const int CycleFrames = PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount * PostCreditsIconGlarePaletteFxColorDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E200</c>.</summary>
    public const ushort DefinitionPointer = 0xe200;

    /// <summary>Provides the post-credits icon glare palette definition and cycle period.</summary>
    extension(PostCreditsIconGlarePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="PostCreditsIconGlarePaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => PostCreditsIconGlarePaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="PostCreditsIconGlarePaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => PostCreditsIconGlarePaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
}
