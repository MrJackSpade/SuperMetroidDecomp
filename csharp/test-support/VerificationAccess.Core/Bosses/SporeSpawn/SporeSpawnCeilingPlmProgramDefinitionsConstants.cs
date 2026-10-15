namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Cartridge values of <see cref="SporeSpawnCeilingPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class SporeSpawnCeilingPlmProgramDefinitionsConstants
{
    /// <summary><c>$84:AB27</c>: first byte of adjacent Botwoon setup code, not Spore Spawn data.</summary>
    public const ushort EndExclusive = 0xab27;

    /// <summary>Provides the exclusive native program boundary for Spore Spawn ceiling PLM checks.</summary>
    extension(SporeSpawnCeilingPlmProgramDefinitions)
    {
        /// <inheritdoc cref="SporeSpawnCeilingPlmProgramDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => SporeSpawnCeilingPlmProgramDefinitionsConstants.EndExclusive;
    }
}
