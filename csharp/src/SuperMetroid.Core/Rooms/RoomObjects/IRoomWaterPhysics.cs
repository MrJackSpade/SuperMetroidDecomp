namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Turns on the room's water physics, as the broken n00b tube floods its room (#1395).</summary>
public interface IRoomWaterPhysics
{
    /// <summary>Lets the room's water surface affect Samus, in the room's water and in Samus's copy of it.</summary>
    void EnableWaterPhysics();
}
