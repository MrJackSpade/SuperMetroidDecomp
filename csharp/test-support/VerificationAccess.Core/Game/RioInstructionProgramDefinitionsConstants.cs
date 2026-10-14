using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="RioInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class RioInstructionProgramDefinitionsConstants
{
    /// <summary>The first mechanics constant after Rio's programs, at $A2:BBBB.</summary>
    public const ushort FirstAdjacentMechanicsData = 0xbbbb;

    /// <summary>Provides adjacent mechanics data that bounds Rio instruction programs.</summary>
    extension(RioInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="RioInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => RioInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}
