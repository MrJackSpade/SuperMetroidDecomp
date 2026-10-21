namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A PLM whose pre-instruction acts on the next frame after a projectile touches it (#1395).</summary>
internal interface IShotTriggeredPlm
{
    /// <summary>Records the touch for the PLM's next pre-instruction, as native PLM_Timers does.</summary>
    void RecordShot();
}
