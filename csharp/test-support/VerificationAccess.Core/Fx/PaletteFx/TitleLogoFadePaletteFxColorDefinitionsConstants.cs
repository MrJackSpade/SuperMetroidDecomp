
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TitleLogoFadePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TitleLogoFadePaletteFxColorDefinitionsConstants
{
    /// <summary>Each record lasts three frames.</summary>
    public const ushort FrameDuration = 3;
    /// <summary>The palette-FX definition at <c>$8D:E194</c>.</summary>
    public const ushort DefinitionPointer = 0xe194;

    /// <summary>Provides the title logo fade definition pointer and palette cycle period.</summary>
    extension(TitleLogoFadePaletteFxColorDefinitions)
    {
        /// <summary>The complete title-logo fade lasts 24 frames.</summary>
        internal static int CycleFrames => TitleLogoFadePaletteFxColorDefinitionsConstants.FrameCount * TitleLogoFadePaletteFxColorDefinitionsConstants.FrameDuration;
        /// <inheritdoc cref="TitleLogoFadePaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => TitleLogoFadePaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
    /// <summary>The fade writes CGRAM from byte index <c>$0142</c>.</summary>
    public const ushort ColorByteIndex = 0x0142;
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => TitleLogoFadePaletteFxColorDefinitions.Rows.Color(frame, color);
    /// <summary>The one-shot fade contains eight timed records.</summary>
    public static int FrameCount => TitleLogoFadePaletteFxColorDefinitions.Rows.FrameCount;
}
