namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What the downward gate's programs do besides drawing (#1296).</summary>
internal enum DownwardGateCue
{
    /// <summary>Clears the gate's trigger count.</summary>
    ClearTrigger,
    /// <summary>Installs the wake-if-shot pre-instruction.</summary>
    ArmWakeIfTriggered,
    /// <summary>Installs the wake-if-shot-or-Samus-below pre-instruction.</summary>
    ArmWakeIfTriggeredOrSamusBelow,
    /// <summary>Spawns the moving gate projectile that slides the gate down.</summary>
    SpawnMovingGate,
    /// <summary>Wakes the gate projectile so it slides back up.</summary>
    WakeGateProjectile,
    /// <summary>Queues the gate's movement sound.</summary>
    QueueMovementSound,
}
