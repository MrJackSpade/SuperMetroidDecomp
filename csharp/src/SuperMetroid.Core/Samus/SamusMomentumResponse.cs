namespace SuperMetroid.Core.Samus;

/// <summary>
/// What Samus's pose handling does to her momentum when the pose she would take is the pose she already holds.
/// Each response is one handling, so this is not a flag set.
/// </summary>
public enum SamusMomentumResponse
{
    /// <summary>Leaves her momentum alone.</summary>
    None = 0,

    /// <summary>Decelerates Samus.</summary>
    Decelerate = 1,

    /// <summary>Stops Samus.</summary>
    Stop = 2,

    /// <summary>Kills Samus's X speed.</summary>
    KillXSpeed = 6,

    /// <summary>Kills Samus's run speed.</summary>
    KillRunSpeed = 8,
}
