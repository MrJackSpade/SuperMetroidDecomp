
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="BeaconPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class BeaconPaletteFxColorDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 100;
    /// <summary>The room palette-FX definition at <c>$8D:F781</c>.</summary>
    public const ushort DefinitionPointer = 0xf781;

    /// <summary>Provides the beacon palette cycle length and native definition pointer used to verify its imported program.</summary>
    extension(BeaconPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="BeaconPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => BeaconPaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="BeaconPaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => BeaconPaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
}
