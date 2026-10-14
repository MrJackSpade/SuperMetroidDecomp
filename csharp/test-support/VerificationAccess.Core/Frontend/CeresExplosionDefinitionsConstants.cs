using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="CeresExplosionDefinitions"/> that only verification reads.</summary>
internal static class CeresExplosionDefinitionsConstants
{
    /// <summary>Bank containing the native definitions and placement tables.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary>Provides the native Ceres explosion bank used to check cinematic explosion extraction.</summary>
    extension(CeresExplosionDefinitions)
    {
        /// <inheritdoc cref="CeresExplosionDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => CeresExplosionDefinitionsConstants.NativeBank;
    }
}
