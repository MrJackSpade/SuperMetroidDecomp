namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Bomb Torizo statue's hand's setup.</summary>
internal sealed record BombTorizoHandSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKind.BombTorizoHand;

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpBombTorizoHand();
}
