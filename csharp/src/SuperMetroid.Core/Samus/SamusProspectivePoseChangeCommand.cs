namespace SuperMetroid.Core.Samus;

/// <summary>
/// Prospective pose change commands dispatched by <c>$91:EC16</c> when the prospective
/// pose equals the current pose. Each command is one handler, so this is not a flag set.
/// </summary>
public enum SamusProspectivePoseChangeCommand : byte
{
    /// <summary>Command 0, <c>RTS_91EFC3</c>.</summary>
    None = 0,

    /// <summary>Command 1, <c>ProspectivePoseCmd_1_Decelerate</c>.</summary>
    Decelerate = 1,

    /// <summary>Command 2, <c>ProspectivePoseCmd_2_Stop</c>.</summary>
    Stop = 2,

    /// <summary>Command 6, <c>ProspectivePoseCmd_6_KillXSpeed</c>.</summary>
    KillXSpeed = 6,

    /// <summary>Command 8, <c>ProspectivePoseCmd_8_KillRunSpeed</c>.</summary>
    KillRunSpeed = 8,
}
