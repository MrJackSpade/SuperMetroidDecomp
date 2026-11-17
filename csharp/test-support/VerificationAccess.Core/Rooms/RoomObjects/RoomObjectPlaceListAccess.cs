namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectPlaceList"/> members production does not use.</summary>
internal static class RoomObjectPlaceListAccess
{
    /// <summary>Exposes <see cref="RoomObjectPlaceList"/> state that only verification reads.</summary>
    extension(RoomObjectPlaceList self)
    {
        /// <summary>How many places hold an object.</summary>
        public int ActiveCount => PrivateState.Field<RoomObjectPlaceList, SuperMetroid.Core.Compatibility.RoomObjectPlaces>(self, "occupancy").HeldCount;
    }
}
