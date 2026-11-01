namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Chooses a deactivated Work Robot's unpowered pose.</summary>
internal static class WorkRobotPoses
{
    /// <summary>The unpowered pose <paramref name="parameter"/> selects; the overrun at parameter three fails loudly.</summary>
    /// <exception cref="InvalidDataException"><paramref name="parameter"/> is three, which reads code as a list.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parameter"/> is outside the range check.</exception>
    internal static WorkRobotProgram InitialProgram(int parameter)
    {
        if ((uint)parameter < (uint)WorkRobotInitializationDefinitions.InitialPrograms.Length)
            return WorkRobotInitializationDefinitions.InitialPrograms[parameter];
        if (parameter == WorkRobotInitializationDefinitions.InitialPrograms.Length)
            throw new InvalidDataException(
                "Work Robot parameter 3 reads past the three-entry animation table into the robot's behaviour; the four " +
                "retail unpowered robots, in the Wrecked Ship's basement and east super missile room, use poses 0, 1, 0 and 2.");
        throw new ArgumentOutOfRangeException(nameof(parameter));
    }
}
