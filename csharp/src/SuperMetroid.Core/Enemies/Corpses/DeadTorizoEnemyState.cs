namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Dead Torizo's corpse state: its phase and velocity, rot schedule, its own rows as the rot has left them, contact delay
/// and the rot calls its sand heap forms by.
/// </summary>
public sealed class DeadTorizoEnemyState
{
    /// <summary>Captures the corpse's own rot schedule.</summary>
    internal DeadTorizoEnemyState(RoomEnemySlot slot, CorpseRotSchedule rot)
    {
        Slot = slot;
        Rot = Ensure.NotNull(rot);
    }

    /// <summary>The room's first-listed enemy, owning the corpse's position and collision and animation state.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>The corpse's own rot schedule: the fall of each of its ninety-six pixel rows.</summary>
    internal CorpseRotSchedule Rot { get; }

    /// <summary>The corpse's body rows as its rot calls have left them, which its body frame is drawn whole through (#1539).</summary>
    internal RottingCorpseRows Rows { get; } = new(DeadTorizoCorpseDefinitions.RowCount);
    /// <summary>Contact-delay calls counted before rotting, 0..16: the sixteenth enters rotting and the corpse never returns to the delay. Direct shot or touch triggering bypasses it.</summary>
    public int PreRotDelayCounter { get; internal set; }
    /// <summary>Count of the corpse's rot calls since initialization, including the call that completes its final entry; its sand heap forms by it.</summary>
    public uint ProcessCallCount { get; internal set; }

    /// <summary>The corpse phase.</summary>
    internal DeadTorizoPhase? Phase { get; set; }

    /// <summary>Signed 8.8 velocity per update: X, and Y, which rotting increases by one 256th each update.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
}
