
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="OldTourianEscapeRedFlashPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class OldTourianEscapeRedFlashPaletteFxColorDefinitionsConstants
{
    /// <summary>The complete loop lasts 42 frames.</summary>
    public const int CycleFrames = 42;
    /// <summary><c>PalFxDef_Crateria8</c> at <c>$8D:FFD9</c>.</summary>
    public const ushort DefinitionPointer = 0xffd9;

    /// <summary>Provides the red flash palette definition and cycle period used during Old Tourian escape.</summary>
    extension(OldTourianEscapeRedFlashPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="OldTourianEscapeRedFlashPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => OldTourianEscapeRedFlashPaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="OldTourianEscapeRedFlashPaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => OldTourianEscapeRedFlashPaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => OldTourianEscapeRedFlashPaletteFxColorDefinitions.Rows.Color(frame, color);
    /// <summary>The loop contains fourteen timed records.</summary>
    public static int FrameCount => OldTourianEscapeRedFlashPaletteFxColorDefinitions.Rows.FrameCount;
}
