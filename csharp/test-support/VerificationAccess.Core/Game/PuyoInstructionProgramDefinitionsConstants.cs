using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="PuyoInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class PuyoInstructionProgramDefinitionsConstants
{
    /// <summary>The first word of <c>PuyoHopTable</c>, immediately after the programs at $A2:9A07.</summary>
    public const ushort FirstAdjacentDefinition = 0x9a07;
    /// <summary>The final airborne-pose sleep opcode at $A2:9A05.</summary>
    public const ushort LastSleepOpcode = 0x9a05;

    /// <summary>Provides Puyo's first adjacent definition and final sleep opcode for program decoding.</summary>
    extension(PuyoInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="PuyoInstructionProgramDefinitionsConstants.FirstAdjacentDefinition"/>
        internal static ushort FirstAdjacentDefinition => PuyoInstructionProgramDefinitionsConstants.FirstAdjacentDefinition;
        /// <inheritdoc cref="PuyoInstructionProgramDefinitionsConstants.LastSleepOpcode"/>
        internal static ushort LastSleepOpcode => PuyoInstructionProgramDefinitionsConstants.LastSleepOpcode;
    }
}
