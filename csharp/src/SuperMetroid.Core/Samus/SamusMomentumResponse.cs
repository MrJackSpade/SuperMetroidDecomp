namespace SuperMetroid.Core.Samus;

/// <summary>
/// What Samus's pose handling does to her momentum when the pose she would take is the pose she already holds.
/// Each response is one handling, so this is not a flag set.
/// </summary>
public enum SamusMomentumResponse
{
    /// <summary>Leaves her momentum alone.</summary>
    None,

    /// <summary>Slows Samus down.</summary>
    Decelerate,

    /// <summary>Stops Samus.</summary>
    Stop,

    /// <summary>Ends Samus's horizontal speed.</summary>
    KillXSpeed,

    /// <summary>Ends Samus's running speed.</summary>
    KillRunSpeed,
}
