
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ZebesExplosionForegroundPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionForegroundPaletteFxColorDefinitionsConstants
{
    /// <summary>The palette-FX definition at <c>$8D:E1C8</c>.</summary>
    public const ushort DefinitionPointer = 0xe1c8;

    /// <summary>Provides the Zebes explosion foreground palette definition for effect checks.</summary>
    extension(ZebesExplosionForegroundPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
}
