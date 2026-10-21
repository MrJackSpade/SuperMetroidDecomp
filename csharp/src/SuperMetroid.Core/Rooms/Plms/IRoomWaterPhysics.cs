namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Turns on the room's water physics, as the broken n00b tube floods its room (#1395).</summary>
public interface IRoomWaterPhysics
{
    /// <summary>Enables water physics in the room's liquid options and in Samus's copy of them.</summary>
    void EnableWaterPhysics();
}
