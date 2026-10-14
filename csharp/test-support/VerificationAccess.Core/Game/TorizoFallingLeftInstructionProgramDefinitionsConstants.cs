using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="TorizoFallingLeftInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TorizoFallingLeftInstructionProgramDefinitionsConstants
{
    /// <summary>First byte after the falling-left list, $AA:BC96.</summary>
    public const ushort End = 0xbc96;

    /// <summary>Provides the end boundary for Torizo falling-left program extraction.</summary>
    extension(TorizoFallingLeftInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TorizoFallingLeftInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => TorizoFallingLeftInstructionProgramDefinitionsConstants.End;
    }
}
