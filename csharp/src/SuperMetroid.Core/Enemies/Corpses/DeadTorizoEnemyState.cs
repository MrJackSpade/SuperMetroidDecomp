namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Dead Torizo's corpse state: its function and velocity, rot schedule, the rot step each half of its rows shows,
/// contact delay and sand-heap progress.
/// </summary>
public sealed class DeadTorizoEnemyState
{
    /// <summary>Captures the corpse's own rot schedule.</summary>
    internal DeadTorizoEnemyState(RoomEnemySlot slot, CorpseRotSchedule rot)
    {
        Slot = slot;
        Rot = Ensure.NotNull(rot);
    }

    /// <summary>Physical enemy slot zero, owning the corpse's position and collision/instruction words.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>The corpse's own rot schedule: the fall of each of its ninety-six pixel rows.</summary>
    internal CorpseRotSchedule Rot { get; }

    /// <summary>
    /// Whether the half of the corpse's rows shown last is the odd half: body rows 6-11 and the first sand strip. Every
    /// main-AI call flips it before showing that half, even while the corpse is intact or finished. The game
    /// increments a phase word and selects by its low bit, which alternates identically across the word's wrap from 65535
    /// to 0.
    /// </summary>
    public bool ShowsOddHalf { get; internal set; }

    /// <summary>The rot step the even half of the corpse's rows shows, or none before a main-AI call first shows it (#1406).</summary>
    public int? EvenHalfStep { get; internal set; }

    /// <summary>The rot step the odd half of the corpse's rows shows, or none before a main-AI call first shows it (#1406).</summary>
    public int? OddHalfStep { get; internal set; }

    /// <summary>The corpse's body rows as its rot calls have left them (#1531).</summary>
    internal RottingCorpseRows Rows { get; } = new(DeadTorizoCorpseDefinitions.RowCount);

    /// <summary>The body rows as a main-AI call last showed the even half, or none before one first shows it (#1531).</summary>
    internal CorpseRotRows? EvenHalfRows { get; set; }

    /// <summary>The body rows as a main-AI call last showed the odd half, or none before one first shows it (#1531).</summary>
    internal CorpseRotRows? OddHalfRows { get; set; }
    /// <summary>Contact-delay calls counted before rotting, 0..16: the sixteenth enters rotting and the corpse never returns to the delay. Direct shot or touch triggering bypasses it.</summary>
    public int PreRotDelayCounter { get; internal set; }
    /// <summary>Host diagnostic count of shared corpse-rotting scheduler calls since initialization, including the call that completes its final entry.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Host diagnostic count of completed row entries, incremented before their dust callback, including the final entry.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Host diagnostic count of dust effects requested by completed entries, one request per completed-entry hook invocation.</summary>
    public uint DustSpawnCount { get; internal set; }

    /// <summary>The corpse function.</summary>
    internal DeadTorizoFunction? Function { get; set; }

    /// <summary>Signed 8.8 velocity per update: X, and Y, which rotting increases by one 256th each update.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
}
