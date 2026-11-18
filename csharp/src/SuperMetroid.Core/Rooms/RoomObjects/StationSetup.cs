namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A station's setup (#2481): the station it places, a save station saving at the load station its placement names.</summary>
/// <param name="Station">The station the placement places.</param>
/// <param name="SavesAt">The load station a save station saves at, or null for a map or refill station.</param>
internal sealed record StationSetup(StationKind Station, SaveStationArgument? SavesAt) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpStation(Station, SavesAt);
}
