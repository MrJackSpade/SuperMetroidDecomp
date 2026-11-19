namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A permanent item's setup (#2481): the item it places and how it is presented, shown as it is placed, unless the item
/// location it places is already collected.
/// </summary>
/// <param name="Item">The item the placement places and its presentation.</param>
/// <param name="Collected">The item location the placement places.</param>
internal sealed record CollectibleSetup(PermanentCollectibleObject Item, CollectibleIdentity Collected) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpCollectible(Item, Collected);
}
