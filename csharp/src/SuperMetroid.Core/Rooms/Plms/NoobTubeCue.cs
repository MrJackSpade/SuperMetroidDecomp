namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What the n00b tube's programs do besides drawing (#1296), one member per native instruction it uses.</summary>
internal enum NoobTubeCue
{
    /// <summary>Continues at <see cref="NoobTubeProgram.AlreadyBroken"/> when the tube's event is set.</summary>
    SkipIfBroken,
    /// <summary>Links <see cref="NoobTubeProgram.WaitForInput"/> for the power-bomb wake.</summary>
    LinkInputWait,
    /// <summary>Installs the power-bomb wake pre-instruction.</summary>
    ArmPowerBombWake,
    /// <summary>Links <see cref="NoobTubeProgram.Break"/> for the input wake.</summary>
    LinkBreak,
    /// <summary>Installs the accepted-input wake pre-instruction.</summary>
    ArmInputWake,
    /// <summary>Clears the pre-instruction.</summary>
    ClearPreInstruction,
    /// <summary>Locks Samus for the break sequence.</summary>
    LockSamus,
    /// <summary>Spawns the crack projectile.</summary>
    SpawnCrack,
    /// <summary>Queues the break sound.</summary>
    QueueBreakSound,
    /// <summary>Spawns the ten glass shards and six released-air bubbles.</summary>
    SpawnShardsAndBubbles,
    /// <summary>Starts the break earthquake.</summary>
    TriggerEarthquake,
    /// <summary>Sets the tube's broken event.</summary>
    SetBrokenEvent,
    /// <summary>Enables the room's water physics.</summary>
    EnableWaterPhysics,
    /// <summary>Unlocks Samus.</summary>
    UnlockSamus,
}
