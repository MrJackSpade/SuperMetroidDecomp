namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Cartridge values of <see cref="CrocomireArenaPlmDrawDefinitions"/> that only verification reads.</summary>
internal static class CrocomireArenaPlmDrawDefinitionsConstants
{
    /// <summary><c>$84:9BF7</c>: first byte of the following eye-door draw region.</summary>
    public const ushort EndExclusive = 0x9bf7;

    /// <summary>Provides the exclusive native draw-data boundary for Crocomire arena PLM checks.</summary>
    extension(CrocomireArenaPlmDrawDefinitions)
    {
        /// <inheritdoc cref="CrocomireArenaPlmDrawDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => CrocomireArenaPlmDrawDefinitionsConstants.EndExclusive;
    }
}
