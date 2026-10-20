
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="NintendoLogoFadePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class NintendoLogoFadePaletteFxColorDefinitionsConstants
{
    /// <summary>Each record lasts three frames.</summary>
    public const ushort FrameDuration = 3;
    /// <summary>The unused boot-logo palette-FX definition at <c>$8D:E198</c>.</summary>
    public const ushort BootLogoDefinitionPointer = 0xe198;
    /// <summary>The copyright palette-FX definition at <c>$8D:E19C</c>.</summary>
    public const ushort CopyrightDefinitionPointer = 0xe19c;

    /// <summary>Provides boot logo and copyright palette definitions and fade cycle timing.</summary>
    extension(NintendoLogoFadePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="NintendoLogoFadePaletteFxColorDefinitionsConstants.BootLogoDefinitionPointer"/>
        internal static ushort BootLogoDefinitionPointer => NintendoLogoFadePaletteFxColorDefinitionsConstants.BootLogoDefinitionPointer;
        /// <inheritdoc cref="NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightDefinitionPointer"/>
        internal static ushort CopyrightDefinitionPointer => NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightDefinitionPointer;
        /// <summary>Either entry runs the shared fade for 24 frames.</summary>
        internal static int CycleFrames => NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameCount * NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameDuration;
    }
    /// <summary>The unused boot-logo entry at <c>$8D:C7AC</c>.</summary>
    public const ushort BootLogoEntry = 0xc7ac;
    /// <summary>The copyright entry at <c>$8D:C7F2</c>.</summary>
    public const ushort CopyrightEntry = 0xc7f2;
    /// <summary>The boot logo writes CGRAM from byte index <c>$0132</c>.</summary>
    public const ushort BootLogoColorByteIndex = 0x0132;
    /// <summary>The copyright text writes CGRAM from byte index <c>$0192</c>.</summary>
    public const ushort CopyrightColorByteIndex = 0x0192;
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => NintendoLogoFadePaletteFxColorDefinitions.Rows.Color(frame, color);
    /// <summary>The shared one-shot fade contains eight timed records.</summary>
    public static int FrameCount => NintendoLogoFadePaletteFxColorDefinitions.Rows.FrameCount;
}
