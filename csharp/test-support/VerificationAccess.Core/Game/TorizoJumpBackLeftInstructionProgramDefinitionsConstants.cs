using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="TorizoJumpBackLeftInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TorizoJumpBackLeftInstructionProgramDefinitionsConstants
{
    /// <summary>First byte after the left-facing jump-back instruction list at $AA:BD0E.</summary>
    public const ushort End = 0xbd0e;

    /// <summary>Provides the end boundary for Torizo left jump-back program extraction.</summary>
    extension(TorizoJumpBackLeftInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TorizoJumpBackLeftInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => TorizoJumpBackLeftInstructionProgramDefinitionsConstants.End;
    }
}
