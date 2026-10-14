using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The unused boot-logo palette-FX definition at <c>$8D:E198</c>.</summary>
    public const ushort BootLogoDefinitionPointer = 0xe198;
    /// <summary>The copyright palette-FX definition at <c>$8D:E19C</c>.</summary>
    public const ushort CopyrightDefinitionPointer = 0xe19c;
    /// <summary>Either entry runs the shared fade for 24 frames.</summary>
    public const int CycleFrames = NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount * NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameDuration;

    /// <summary>Provides boot logo and copyright palette definitions and fade cycle timing.</summary>
    extension(NintendoLogoFadePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.BootLogoDefinitionPointer"/>
        internal static ushort BootLogoDefinitionPointer => NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.BootLogoDefinitionPointer;
        /// <inheritdoc cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CopyrightDefinitionPointer"/>
        internal static ushort CopyrightDefinitionPointer => NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CopyrightDefinitionPointer;
        /// <inheritdoc cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => NintendoLogoFadePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}
