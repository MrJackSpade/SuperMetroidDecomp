namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A save station's setup (#2483): the load station its placement names, which the station saves at.</summary>
/// <param name="SavesAt">The load station the save station saves at.</param>
internal sealed record SaveStationSetup(SaveStationPlace SavesAt) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpSaveStation(SavesAt);
}
