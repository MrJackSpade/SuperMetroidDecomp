namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A station's setup, a save station saving at the load station its placement names.</summary>
/// <param name="SavesAt">The load station a save station saves at, or null for a map or refill station.</param>
internal sealed record StationSetup(SaveStationArgument? SavesAt) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpStation(kind, SavesAt);
}
