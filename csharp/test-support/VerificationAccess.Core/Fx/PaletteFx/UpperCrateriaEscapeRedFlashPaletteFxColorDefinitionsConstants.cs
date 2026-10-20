
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsConstants
{
    /// <summary>The complete loop lasts 63 frames.</summary>
    public const int CycleFrames = 63;
    /// <summary><c>PalFxDef_Crateria2</c> at <c>$8D:FFE5</c>.</summary>
    public const ushort DefinitionPointer = 0xffe5;

    /// <summary>Provides the upper Crateria escape red flash definition and cycle period.</summary>
    extension(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
    /// <summary>The first CGRAM destination byte for upper Crateria's red flash.</summary>
    public const ushort ColorByteIndex = 0x0082;
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.Rows.Color(frame, color);
    /// <summary>The loop contains fourteen timed records.</summary>
    public static int FrameCount => UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.Rows.FrameCount;
}
