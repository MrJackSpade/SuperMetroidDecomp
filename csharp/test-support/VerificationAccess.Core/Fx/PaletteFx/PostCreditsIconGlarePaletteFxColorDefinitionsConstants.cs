
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="PostCreditsIconGlarePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class PostCreditsIconGlarePaletteFxColorDefinitionsConstants
{
    /// <summary>Each record lasts one frame.</summary>
    public const ushort FrameDuration = 1;
    /// <summary>The palette-FX definition at <c>$8D:E200</c>.</summary>
    public const ushort DefinitionPointer = 0xe200;

    /// <summary>Provides the post-credits icon glare palette definition and cycle period.</summary>
    extension(PostCreditsIconGlarePaletteFxColorDefinitions)
    {
        /// <summary>The complete icon glare lasts fourteen frames.</summary>
        internal static int CycleFrames => PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount * PostCreditsIconGlarePaletteFxColorDefinitionsConstants.FrameDuration;
        /// <inheritdoc cref="PostCreditsIconGlarePaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => PostCreditsIconGlarePaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
    /// <summary>The instruction-list entry at <c>$8D:DF94</c>.</summary>
    public const ushort ProgramStart = 0xdf94;
    /// <summary>The glare writes CGRAM from byte index <c>$01E0</c>.</summary>
    public const ushort ColorByteIndex = 0x01e0;
}
