namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="StationAccessObjectDefinitions"/> members production does not use.</summary>
internal static class StationAccessObjectDefinitionsAccess
{
    /// <summary>Lists map, energy, and missile station access cases in native side/BTS order.</summary>
    extension(StationAccessObjectDefinitions)
    {
        /// <summary>Enumerate the six named station/side cases in their native BTS order.</summary>
        internal static IEnumerable<StationAccessObjectDefinition> All
        {
            get
            {
                yield return StationAccessObjectDefinitions.MapRight;
                yield return StationAccessObjectDefinitions.MapLeft;
                yield return StationAccessObjectDefinitions.EnergyRight;
                yield return StationAccessObjectDefinitions.EnergyLeft;
                yield return StationAccessObjectDefinitions.MissileRight;
                yield return StationAccessObjectDefinitions.MissileLeft;
            }
        }
    }
}
