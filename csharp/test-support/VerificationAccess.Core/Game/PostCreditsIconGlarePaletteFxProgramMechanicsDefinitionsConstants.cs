using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete icon glare lasts fourteen frames.</summary>
    public const int CycleFrames = PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount * PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E200</c>.</summary>
    public const ushort DefinitionPointer = 0xe200;

    /// <summary>Provides the post-credits icon glare palette definition and cycle period.</summary>
    extension(PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => PostCreditsIconGlarePaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
