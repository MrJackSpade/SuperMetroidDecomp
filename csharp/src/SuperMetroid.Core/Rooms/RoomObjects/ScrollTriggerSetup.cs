namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A scroll trigger's setup: the scroll change it runs.</summary>
/// <param name="Change">The scroll change the trigger runs.</param>
internal sealed record ScrollTriggerSetup(RoomScrollChange Change) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpScrollTrigger(Change);
}
