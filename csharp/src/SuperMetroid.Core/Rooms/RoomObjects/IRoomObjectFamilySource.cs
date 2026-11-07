namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A PLM family owner that registers its family's step.</summary>
internal interface IRoomObjectFamilySource
{
    /// <summary>Registers the owner's family step in <paramref name="table"/>.</summary>
    void AddObjectFamilies(RoomObjectFamilyTable table);
}
