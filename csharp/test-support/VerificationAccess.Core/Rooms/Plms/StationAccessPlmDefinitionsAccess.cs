namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="StationAccessPlmDefinitions"/> members production does not use.</summary>
internal static class StationAccessPlmDefinitionsAccess
{
    /// <summary>Lists map, energy, and missile station access cases in native side/BTS order.</summary>
    extension(StationAccessPlmDefinitions)
    {
        /// <summary>Enumerate the six named station/side cases in their native BTS order.</summary>
        internal static IEnumerable<StationAccessPlmDefinition> All
        {
            get
            {
                yield return StationAccessPlmDefinitions.MapRight;
                yield return StationAccessPlmDefinitions.MapLeft;
                yield return StationAccessPlmDefinitions.EnergyRight;
                yield return StationAccessPlmDefinitions.EnergyLeft;
                yield return StationAccessPlmDefinitions.MissileRight;
                yield return StationAccessPlmDefinitions.MissileLeft;
            }
        }
    }
}
