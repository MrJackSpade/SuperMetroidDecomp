namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Maridia's glass tube's setup.</summary>
internal sealed record NoobTubeSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpNoobTube();
}
