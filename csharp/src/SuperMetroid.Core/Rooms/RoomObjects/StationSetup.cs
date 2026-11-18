namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A map, energy or missile station's setup (#2481, #2484): the station it places.</summary>
/// <param name="Station">The station the placement places.</param>
internal sealed record AccessStationSetup(StationKind Station) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => Ensure.NotNull(setups).SetUpAccessStation(Station);
}

/// <summary>A save station's setup (#2481, #2484): the station saving at the load station its placement names.</summary>
/// <param name="SavesAt">The load station the save station saves at, as its placement names it.</param>
internal sealed record SaveStationSetup(SaveStationArgument? SavesAt) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => Ensure.NotNull(setups).SetUpSaveStation(SavesAt);
}
