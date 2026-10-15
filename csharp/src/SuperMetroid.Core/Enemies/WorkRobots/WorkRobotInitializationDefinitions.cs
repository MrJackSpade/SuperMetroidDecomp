namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Animation selection for the deactivated Work Robot.</summary>
internal static class WorkRobotInitializationDefinitions
{
    /// <summary>
    /// $A8:CC30..CC35: the three authored unpowered poses. The native range check also admits parameter three, whose
    /// table read overruns into the first code word of <c>MainAI_Robot</c> at $A8:CC36 and points the interpreter at
    /// unrelated bytes; that corrupt path is outside the port and fails loudly here.
    /// </summary>
    internal static WorkRobotProgram InitialProgram(int parameter) => parameter switch
    {
        0 => WorkRobotProgram.NoPowerNeutral,
        1 => WorkRobotProgram.NoPowerLeaningLeft,
        2 => WorkRobotProgram.NoPowerLeaningRight,
        3 => throw new InvalidDataException(
            "Work Robot parameter 3 reads past the three-entry list table into code at $A8:CC36; no retail population uses it."),
        _ => throw new ArgumentOutOfRangeException(nameof(parameter)),
    };
}
