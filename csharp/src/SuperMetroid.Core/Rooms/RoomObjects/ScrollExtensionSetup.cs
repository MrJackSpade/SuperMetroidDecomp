namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A scroll extension's setup, which extends the trigger beside it in its kind's direction.</summary>
internal sealed record ScrollExtensionSetup : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpScrollExtension(kind);
}
