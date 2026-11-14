
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Cartridge values of <see cref="NintendoLogoFadePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class NintendoLogoFadePaletteFxColorDefinitionsConstants
{
    /// <summary>Each record lasts three frames.</summary>
    public const ushort FrameDuration = 3;
    /// <summary>The copyright palette-FX definition at <c>$8D:E19C</c>.</summary>
    public const ushort CopyrightDefinitionPointer = 0xe19c;

    /// <summary>Provides the copyright palette definition and fade cycle timing.</summary>
    extension(NintendoLogoFadePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightDefinitionPointer"/>
        internal static ushort CopyrightDefinitionPointer => NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightDefinitionPointer;
        /// <summary>The copyright entry runs the fade for 24 frames.</summary>
        internal static int CycleFrames => NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameCount * NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameDuration;
    }
    /// <summary>The copyright entry at <c>$8D:C7F2</c>.</summary>
    public const ushort CopyrightEntry = 0xc7f2;
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => NintendoLogoFadePaletteFxColorDefinitions.ColorFrames.Color(frame, color);
    /// <summary>The shared one-shot fade contains eight timed records.</summary>
    public static int FrameCount => NintendoLogoFadePaletteFxColorDefinitions.ColorFrames.FrameCount;
}
