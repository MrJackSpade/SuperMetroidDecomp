using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>$8D:F795: native Maridia1 sand-pit palette-FX definition; subsequent selected definitions occupy four bytes each.</summary>
    public const ushort SandPitDefinition = 0xF795;

    /// <summary>Provides the native sand pit palette definition for Maridia environment checks.</summary>
    extension(MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants.SandPitDefinition"/>
        internal static ushort SandPitDefinition => MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitionsConstants.SandPitDefinition;
    }
}
