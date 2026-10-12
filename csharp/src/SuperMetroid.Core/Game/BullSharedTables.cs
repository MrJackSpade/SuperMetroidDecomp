namespace SuperMetroid.Core.Game;

/// <summary>The per-slot Bull tables shared with the enemy system (#633).</summary>
internal sealed record BullSharedTables
{
    /// <summary>Max speeds.</summary>
    public required ushort[] MaxSpeeds { get; init; }

    /// <summary>Angles to samus.</summary>
    public required ushort[] AnglesToSamus { get; init; }

    /// <summary>Angles.</summary>
    public required ushort[] Angles { get; init; }

    /// <summary>Shot reaction disable flags.</summary>
    public required ushort[] ShotReactionDisableFlags { get; init; }

    /// <summary>Acceleration timer resets.</summary>
    public required ushort[] AccelerationTimerResets { get; init; }

    /// <summary>Deceleration timer resets.</summary>
    public required ushort[] DecelerationTimerResets { get; init; }

    /// <summary>Shot reaction disable timers.</summary>
    public required ushort[] ShotReactionDisableTimers { get; init; }

    /// <summary>Previous health.</summary>
    public required ushort[] PreviousHealth { get; init; }
}
