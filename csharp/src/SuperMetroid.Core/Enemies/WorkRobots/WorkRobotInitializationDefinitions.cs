namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Animation selection for the deactivated Work Robot.</summary>
internal static class WorkRobotInitializationDefinitions
{
    /// <summary>
    /// $A8:CC30..CC35: the three authored unpowered poses. The native range check also admits parameter three, whose
    /// table read overruns into the first code word of <c>MainAI_Robot</c> at $A8:CC36 and points the interpreter at
    /// unrelated bytes; that corrupt path is outside the port.
    /// </summary>
    internal static readonly WorkRobotProgram[] InitialPrograms =
        [WorkRobotProgram.NoPowerNeutral, WorkRobotProgram.NoPowerLeaningLeft, WorkRobotProgram.NoPowerLeaningRight];
}
