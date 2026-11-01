namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Animation selection for the deactivated Work Robot.</summary>
internal static class WorkRobotInitializationDefinitions
{
    /// <summary>
    /// The three authored unpowered poses. The range check also admits parameter three, whose
    /// table read overruns into the first word of the robot's main behaviour and points the interpreter at
    /// unrelated bytes; that corrupt path is outside the port.
    /// </summary>
    internal static readonly WorkRobotProgram[] InitialPrograms =
        [WorkRobotProgram.NoPowerNeutral, WorkRobotProgram.NoPowerLeaningLeft, WorkRobotProgram.NoPowerLeaningRight];
}
