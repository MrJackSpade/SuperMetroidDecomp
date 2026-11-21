namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// What starts a moving shutter (#2560): a closed choice of a timer with its wait, Samus coming near with the distance,
/// starting at once, and a shot's reaction once or every time. Each carries only what it reads.
/// </summary>
/// <param name="Mode">The kind of trigger.</param>
public abstract record ShutterTrigger(ShutterTriggerMode Mode)
{
    /// <summary>The shutter starts at once.</summary>
    public static ShutterTrigger Immediate { get; } = new Plain(ShutterTriggerMode.Immediate);

    /// <summary>A shot starts the shutter once.</summary>
    public static ShutterTrigger OneShotReaction { get; } = new Plain(ShutterTriggerMode.OneShotReaction);

    /// <summary>A shot starts the shutter every time.</summary>
    public static ShutterTrigger RepeatableReaction { get; } = new Plain(ShutterTriggerMode.RepeatableReaction);

    /// <summary>A timer starts the shutter.</summary>
    /// <param name="Wait">The updates it waits.</param>
    public sealed record Timer(int Wait) : ShutterTrigger(ShutterTriggerMode.Timer);

    /// <summary>Samus coming near starts the shutter.</summary>
    /// <param name="Distance">The horizontal distance in pixels Samus must come within.</param>
    public sealed record HorizontalProximity(int Distance) : ShutterTrigger(ShutterTriggerMode.HorizontalProximity);

    /// <summary>A trigger that reads no wait or distance.</summary>
    /// <param name="Mode">The kind of trigger.</param>
    private sealed record Plain(ShutterTriggerMode Mode) : ShutterTrigger(Mode);
}
