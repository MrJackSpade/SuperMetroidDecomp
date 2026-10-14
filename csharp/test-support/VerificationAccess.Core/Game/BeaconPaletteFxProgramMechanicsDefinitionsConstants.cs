using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="BeaconPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class BeaconPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 100;
    /// <summary>The room palette-FX definition at <c>$8D:F781</c>.</summary>
    public const ushort DefinitionPointer = 0xf781;

    /// <summary>Provides the beacon palette cycle length and native definition pointer used to verify its imported program.</summary>
    extension(BeaconPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="BeaconPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => BeaconPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="BeaconPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => BeaconPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
