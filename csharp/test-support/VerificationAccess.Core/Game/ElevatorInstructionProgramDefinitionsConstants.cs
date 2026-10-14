using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="ElevatorInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ElevatorInstructionProgramDefinitionsConstants
{
    /// <summary>The controller-input table immediately after the program, at $A3:94E2.</summary>
    public const ushort FirstAdjacentMechanicsData = 0x94e2;

    /// <summary>Provides the neighboring mechanics-data boundary for elevator instruction program extraction.</summary>
    extension(ElevatorInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ElevatorInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => ElevatorInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}
