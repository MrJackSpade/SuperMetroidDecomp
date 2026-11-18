namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Mother Brain's glass's setup.</summary>
internal sealed record MotherBrainGlassSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpMotherBrainGlass();
}
