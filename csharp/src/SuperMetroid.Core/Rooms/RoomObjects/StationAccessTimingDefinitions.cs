namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The authored timing of a map, energy or missile station's access arm (#1554).</summary>
internal static class StationAccessTimingDefinitions
{
    /// <summary>The updates the arm takes to extend or retract, and each hold around the station's activation.</summary>
    internal const int MovementUpdates = 6;

    /// <summary>The updates the fully extended arm holds before the station activates.</summary>
    internal const int ExtendedHoldUpdates = 96;
}
