namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Cartridge values of <see cref="CrocomireArenaObjectDrawDefinitions"/> that only verification reads.</summary>
internal static class CrocomireArenaObjectDrawDefinitionsConstants
{
    /// <summary><c>$84:9BF7</c>: first byte of the following eye-door draw region.</summary>
    public const ushort EndExclusive = 0x9bf7;

    /// <summary>Provides the exclusive native draw-data boundary for Crocomire arena PLM checks.</summary>
    extension(CrocomireArenaObjectDrawDefinitions)
    {
        /// <inheritdoc cref="CrocomireArenaObjectDrawDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => CrocomireArenaObjectDrawDefinitionsConstants.EndExclusive;
    }
}
