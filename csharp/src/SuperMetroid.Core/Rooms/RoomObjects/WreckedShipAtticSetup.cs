namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Wrecked Ship attic's setup.</summary>
internal sealed record WreckedShipAtticSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpWreckedShipAttic();
}
