namespace SuperMetroid.Core.Samus;

/// <summary>
/// Prospective pose change commands dispatched by Samus's pose handling when the prospective
/// pose equals the current pose. Each command is one handler, so this is not a flag set.
/// </summary>
public enum SamusProspectivePoseChangeCommand
{
    /// <summary>Command 0, which does nothing.</summary>
    None = 0,

    /// <summary>Command 1, which decelerates Samus.</summary>
    Decelerate = 1,

    /// <summary>Command 2, which stops Samus.</summary>
    Stop = 2,

    /// <summary>Command 6, which kills Samus's X speed.</summary>
    KillXSpeed = 6,

    /// <summary>Command 8, which kills Samus's run speed.</summary>
    KillRunSpeed = 8,
}
