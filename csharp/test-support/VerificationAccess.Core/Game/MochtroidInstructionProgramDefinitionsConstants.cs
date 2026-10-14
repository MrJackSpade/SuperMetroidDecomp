using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="MochtroidInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MochtroidInstructionProgramDefinitionsConstants
{
    /// <summary>The shake-velocity table immediately after the programs, at $A3:A76D.</summary>
    public const ushort FirstAdjacentMechanicsData = 0xa76d;

    /// <summary>Provides the mechanics-data boundary adjacent to Mochtroid instruction programs.</summary>
    extension(MochtroidInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MochtroidInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => MochtroidInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}
