namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A permanent item's setup: its kind shown as it is placed, unless the item it places is already collected.</summary>
/// <param name="Collected">The item location the placement places, or null for none.</param>
internal sealed record CollectibleSetup(CollectedItemArgument? Collected) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpCollectible(kind, Collected);
}
