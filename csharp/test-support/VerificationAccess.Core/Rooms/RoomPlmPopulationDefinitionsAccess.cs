using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomPlmPopulationDefinitions"/> members production does not use.</summary>
internal static class RoomPlmPopulationDefinitionsAccess
{
    /// <summary>Scans bank-$84 header pointers accepted by the room PLM population loader.</summary>
    extension(RoomPlmPopulationDefinitions)
    {
        /// <summary>Scans bank-offset values for PLM headers accepted by the room population loader.</summary>
        internal static IEnumerable<ushort> Pointers
        {
            get
            {
                for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                    if (RoomPlmPopulationDefinitions.TryPlace((ushort)pointer, null))
                        yield return (ushort)pointer;
            }
        }
    }
}
