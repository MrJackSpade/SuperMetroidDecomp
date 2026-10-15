
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The palette-FX definition at <c>$8D:E1C8</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c8;

    /// <summary>Provides the Zebes explosion foreground palette definition for effect checks.</summary>
    extension(ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
