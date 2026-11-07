namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A PLM whose pre-instruction acts on the next frame after a projectile touches it (#1395).</summary>
internal interface IShotTriggeredObject
{
    /// <summary>Records the touch for the PLM's next pre-instruction, as PLM_Timers does.</summary>
    void RecordShot();
}
