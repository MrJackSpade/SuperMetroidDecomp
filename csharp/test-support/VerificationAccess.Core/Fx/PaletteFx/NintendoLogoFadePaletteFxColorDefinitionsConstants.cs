
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="NintendoLogoFadePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class NintendoLogoFadePaletteFxColorDefinitionsConstants
{
    /// <summary>The unused boot-logo palette-FX definition at <c>$8D:E198</c>.</summary>
    public const ushort BootLogoDefinitionPointer = 0xe198;
    /// <summary>The copyright palette-FX definition at <c>$8D:E19C</c>.</summary>
    public const ushort CopyrightDefinitionPointer = 0xe19c;
    /// <summary>Either entry runs the shared fade for 24 frames.</summary>
    public const int CycleFrames = NintendoLogoFadePaletteFxColorDefinitions.FrameCount * NintendoLogoFadePaletteFxColorDefinitions.FrameDuration;

    /// <summary>Provides boot logo and copyright palette definitions and fade cycle timing.</summary>
    extension(NintendoLogoFadePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="NintendoLogoFadePaletteFxColorDefinitionsConstants.BootLogoDefinitionPointer"/>
        internal static ushort BootLogoDefinitionPointer => NintendoLogoFadePaletteFxColorDefinitionsConstants.BootLogoDefinitionPointer;
        /// <inheritdoc cref="NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightDefinitionPointer"/>
        internal static ushort CopyrightDefinitionPointer => NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightDefinitionPointer;
        /// <inheritdoc cref="NintendoLogoFadePaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => NintendoLogoFadePaletteFxColorDefinitionsConstants.CycleFrames;
    }
}
