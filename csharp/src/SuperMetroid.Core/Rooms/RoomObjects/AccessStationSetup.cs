namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A map, energy or missile station's setup (#2483): the access rule of the station it places, by which Samus uses it from
/// the access blocks either side.
/// </summary>
/// <param name="Station">The access rule of the station the placement places.</param>
internal sealed record AccessStationSetup(StationAccessRule Station) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKindDefinitions.AccessStations[Station];

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpAccessStation(Station);
}
