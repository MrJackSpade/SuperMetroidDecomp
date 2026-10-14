namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification access to <see cref="CrocomireArenaPlmProgramDefinitions"/> members production does not use.</summary>
internal static class CrocomireArenaPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates Crocomire arena PLM instruction addresses for retail word comparison.</summary>
    extension(CrocomireArenaPlmProgramDefinitions)
    {
        /// <summary>Visits consecutive compiled words from bridge clearing through the arena program end.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            for (int address = CrocomireArenaPlmProgramDefinitions.ClearBridge; address < CrocomireArenaPlmProgramDefinitions.EndExclusive; address += 2)
                yield return (ushort)address;
        }
    }
}
